#nullable disable
namespace NoCloudware.UI.Core.Resources;

using System.Globalization;
using System.Resources;

public class Strings
{
    private static ResourceManager _resourceManager;
    private static CultureInfo _resourceCulture;

    private static ResourceManager ResourceManager
    {
        get
        {
            if (_resourceManager == null)
            {
                _resourceManager = new ResourceManager(
                    "NoCloudware.UI.Core.Resources.Strings",
                    typeof(Strings).Assembly);
            }
            return _resourceManager;
        }
    }

    public static CultureInfo Culture
    {
        get => _resourceCulture ?? CultureInfo.CurrentUICulture;
        set => _resourceCulture = value;
    }

    public static string ButtonAcercaDe => ResourceManager.GetString("Button.AcercaDe", Culture)!;
    public static string ButtonDonar => ResourceManager.GetString("Button.Donar", Culture)!;
    public static string ButtonSalir => ResourceManager.GetString("Button.Salir", Culture)!;
    public static string ButtonAction => ResourceManager.GetString("Button.Action", Culture)!;
    public static string ButtonSelectFiles => ResourceManager.GetString("Button.SelectFiles", Culture)!;
    public static string ButtonChange => ResourceManager.GetString("Button.Change", Culture)!;
    public static string DropZoneDefaultText => ResourceManager.GetString("DropZone.DefaultText", Culture)!;
    public static string DropZoneClickToSelect => ResourceManager.GetString("DropZone.ClickToSelect", Culture)!;
    public static string StatusTotal => ResourceManager.GetString("Status.Total", Culture)!;
    public static string StatusProcessed => ResourceManager.GetString("Status.Processed", Culture)!;
    public static string StatusPending => ResourceManager.GetString("Status.Pending", Culture)!;
    public static string StatusErrors => ResourceManager.GetString("Status.Errors", Culture)!;
    public static string DialogAboutTitle => ResourceManager.GetString("Dialog.About.Title", Culture)!;
    public static string DialogAboutVersion => ResourceManager.GetString("Dialog.About.Version", Culture)!;
    public static string DialogAboutCopyright => ResourceManager.GetString("Dialog.About.Copyright", Culture)!;
    public static string DialogAboutThirdParty => ResourceManager.GetString("Dialog.About.ThirdParty", Culture)!;
    public static string DialogAboutCheckUpdates => ResourceManager.GetString("Dialog.About.CheckUpdates", Culture)!;
    public static string DialogAboutClose => ResourceManager.GetString("Dialog.About.Close", Culture)!;
    public static string LanguageChanged => ResourceManager.GetString("Language.Changed", Culture)!;
    public static string ThemeChanged => ResourceManager.GetString("Theme.Changed", Culture)!;
    public static string UpdateAvailable => ResourceManager.GetString("Update.Available", Culture)!;
    public static string UpdateUpToDate => ResourceManager.GetString("Update.UpToDate", Culture)!;
    public static string OutputFolderDefaultText => ResourceManager.GetString("OutputFolder.DefaultText", Culture)!;
}