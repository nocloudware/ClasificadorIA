using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NoCloudware.UI.Core.Controls;

namespace ClasificadorIA;

public partial class App : System.Windows.Application
{
    private ShellWindow? _window;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _window = new ShellWindow
        {
            Title = "Clasificador IA",
            WindowWidth = 1200,
            WindowHeight = 760,
            WindowMinWidth = 900,
            WindowMinHeight = 600,
            WindowIcon = LoadIcon(),
            DropText = "Selecciona una carpeta para clasificar sus archivos",
            AcceptedFormatsText = "",
            ActionButtonText = "Organizar",
            AboutButtonText = "Acerca de",
            DonateButtonText = "Donar",
            ExitButtonText = "Salir",
            OutputFolderText = ""
        };
        _window.MainControl.AppTitle = "Clasificador IA";
        _window.MainControl.AppTagline = "Organizador de archivos con IA";
        _window.MainControl.OptionsPanelMinWidth = 320;
        _window.MainControl.FileListHeader = "Archivos";
        _window.MainControl.OptionsContent.Content = new TextBlock
        {
            Text = "Panel de opciones",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _window.Show();
    }

    private static BitmapImage LoadIcon()
    {
        var icon = new BitmapImage();
        icon.BeginInit();
        icon.UriSource = new Uri("pack://application:,,,/ClasificadorIA;component/Assets/clasificadoria.png");
        icon.CacheOption = BitmapCacheOption.OnLoad;
        icon.EndInit();
        return icon;
    }
}