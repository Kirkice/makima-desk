using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace Petal;
public partial class App : Application
{
    private TrayIcon? tray;
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            var show = new NativeMenuItem("打开");
            show.Click += (_, _) => { window.Show(); window.Activate(); };
            var quit = new NativeMenuItem("退出");
            quit.Click += (_, _) => window.Close();
            var menu = new NativeMenu();
            menu.Items.Add(show);
            menu.Items.Add(quit);
            using var iconStream = AssetLoader.Open(new Uri("avares://Petal/Assets/tray.png"));
            tray = new TrayIcon { Icon = new WindowIcon(iconStream), ToolTipText = "Petal", Menu = menu, IsVisible = true };
            tray.Clicked += (_, _) => { window.Show(); window.Activate(); };
            TrayIcon.SetIcons(this, new TrayIcons { tray });
            desktop.Exit += (_, _) => { tray.Dispose(); };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
