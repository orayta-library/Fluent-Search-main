using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MySearchApp
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // start MBTiles server (serves www and tiles)
                    try
                    {
                        var baseDir = AppContext.BaseDirectory;
                        var www = Path.Combine(baseDir, "www");
                        var mbtiles = Path.Combine(baseDir, "data", "map.mbtiles");
                        var server = new Services.MbTilesServer("http://127.0.0.1:5000/", www, mbtiles);
                        server.Start();
                        // store on App properties if needed later
                        Avalonia.Application.Current?.Resources.Add("TileServerUrl", server.Url);
                    }
                    catch { }

                    desktop.MainWindow = new MainWindow();
                }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
