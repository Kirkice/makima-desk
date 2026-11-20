using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MakimaDesk;

namespace Petal;
public partial class MainWindow : Window
{
    private readonly MarketService market = new();
    private readonly CancellationTokenSource lifetime = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(30) };
    private bool refreshing, closed;

    public MainWindow()
    {
        InitializeComponent();
        timer.Tick += async (_, _) => await RefreshAsync();
        Opened += async (_, _) =>
        {
            var area = Screens.Primary?.WorkingArea;
            if (area is { } rect)
                Position = new Avalonia.PixelPoint(rect.Right - (int)(Width * RenderScaling) - 24,
                    rect.Bottom - (int)(Height * RenderScaling) - 24);
            timer.Start();
            await RefreshAsync();
        };
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Hide(); };
        Closed += (_, _) =>
        {
            closed = true;
            timer.Stop();
            lifetime.Cancel();
            market.Dispose();
        };
    }

    private void DragHeader(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }
    private void HideClicked(object? sender, RoutedEventArgs e) => Hide();
    private void QuitClicked(object? sender, RoutedEventArgs e) => Close();
    private async void RefreshClicked(object? sender, RoutedEventArgs e) => await RefreshAsync();

    private async Task RefreshAsync()
    {
        if (refreshing || closed) return;
        refreshing = true;
        RefreshButton.IsEnabled = false;
        StatusDot.Fill = Brush.Parse("#7589AE");
        try
        {
            var data = await market.FetchAsync(lifetime.Token);
            if (closed) return;
            GlobalValue.Text = data.Global.ToString("F2");
            LocalValue.Text = data.Local.ToString("F2");
            GlobalChange.Text = data.GlobalChange.ToString("+0.00;-0.00;0.00") + "%";
            LocalChange.Text = data.LocalChange.ToString("+0.00;-0.00;0.00") + "%";
            FxValue.Text = "↔ " + data.Fx.ToString("F4");
            UpdateTime.Text = data.Updated.ToString("HH:mm:ss");
            Chart.SetPoints(data.Points);
            if (data.Points.Length > 0)
            {
                HighLabel.Text = data.Points.Max().ToString("F1");
                LowLabel.Text = data.Points.Min().ToString("F1");
            }
            StatusDot.Fill = Brush.Parse("#9C8AB7");
            ToolTip.SetTip(StatusDot, null);
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception error)
        {
            if (!closed)
            {
                StatusDot.Fill = Brush.Parse("#9E9EA8");
                ToolTip.SetTip(StatusDot, error.Message);
            }
        }
        finally
        {
            refreshing = false;
            if (!closed) RefreshButton.IsEnabled = true;
        }
    }
}
