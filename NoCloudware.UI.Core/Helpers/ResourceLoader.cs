using System.Globalization;
using System.Reflection;
using System.Resources;

namespace NoCloudware.UI.Core.Helpers;

public static class ResourceLoader
{
    private static ResourceManager? _resourceManager;

    public static void Initialize(string baseName, Assembly assembly)
    {
        _resourceManager = new ResourceManager(baseName, assembly);
    }

    public static string GetString(string name, CultureInfo? culture = null)
    {
        try
        {
            return _resourceManager?.GetString(name, culture) ?? name;
        }
        catch
        {
            return name;
        }
    }

    public static string GetString(string name, string cultureCode)
    {
        return GetString(name, new CultureInfo(cultureCode));
    }
}