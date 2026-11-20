using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MakimaDesk;

public sealed record MarketSnapshot(double Global, double GlobalChange, double Local,
    double LocalChange, double Fx, double[] Points, DateTime Updated);

public sealed class MarketService : IDisposable
{
    private readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(12) };

    public MarketService() => client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

    private async Task<(double price, double previous, double[] points)> FetchChart(string symbol, CancellationToken token)
    {
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(symbol)}?interval=1d&range=1mo";
        using var response = await client.GetAsync(url, token);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(token);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);
        var result = doc.RootElement.GetProperty("chart").GetProperty("result");
        if (result.ValueKind != JsonValueKind.Array || result.GetArrayLength() == 0)
            throw new InvalidOperationException("行情服务暂无数据");
        var item = result[0];
        var meta = item.GetProperty("meta");
        double price = meta.GetProperty("regularMarketPrice").GetDouble();
        double previous = meta.TryGetProperty("chartPreviousClose", out var p) ? p.GetDouble() :
            meta.TryGetProperty("previousClose", out p) ? p.GetDouble() : price;
        var values = new List<double>();
        if (item.TryGetProperty("indicators", out var indicators) &&
            indicators.TryGetProperty("quote", out var quotes) && quotes.GetArrayLength() > 0 &&
            quotes[0].TryGetProperty("close", out var closes))
        {
            foreach (var close in closes.EnumerateArray())
                if (close.TryGetDoubleSafe(out double value) && double.IsFinite(value))
                    values.Add(value);
        }
        if (!double.IsFinite(price) || price <= 0) throw new InvalidOperationException("无效价格");
        // 月范围的 chartPreviousClose 是月初基准，不能用于当日涨跌幅。
        // 日线最后一点为最新交易日，倒数第二点为上一交易日收盘价。
        if (values.Count >= 2) previous = values[^2];
        else if (meta.TryGetProperty("previousClose", out var dailyPrevious) &&
                 dailyPrevious.TryGetDoubleSafe(out var dailyClose)) previous = dailyClose;
        else previous = price;
        return (price, previous, values.TakeLast(15).ToArray());
    }

    public async Task<MarketSnapshot> FetchAsync(CancellationToken token)
    {
        var goldTask = FetchChart("GC=F", token);
        var fxTask = FetchChart("CNY=X", token);
        await Task.WhenAll(goldTask, fxTask);
        var gold = await goldTask;
        var fx = await fxTask;
        double change = gold.previous > 0 ? (gold.price / gold.previous - 1) * 100 : 0;
        double localChange = gold.previous > 0 && fx.previous > 0
            ? (gold.price * fx.price / (gold.previous * fx.previous) - 1) * 100 : 0;
        return new(gold.price, change, gold.price * fx.price / 31.1034768,
            localChange, fx.price, gold.points, DateTime.Now);
    }

    public void Dispose() => client.Dispose();
}

internal static class JsonExtensions
{
    public static bool TryGetDoubleSafe(this JsonElement element, out double value)
    {
        value = 0;
        return element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out value);
    }
}
