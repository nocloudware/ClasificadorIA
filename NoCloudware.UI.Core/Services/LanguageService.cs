using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace NoCloudware.UI.Core.Services;

public class LanguageService
{
    public event EventHandler<CultureInfo>? LanguageChanged;

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

    public List<CultureInfo> AvailableCultures { get; } = new()
    {
        new CultureInfo("en"),
        new CultureInfo("es"),
        new CultureInfo("fr"),
        new CultureInfo("de"),
        new CultureInfo("pt"),
        new CultureInfo("it"),
        new CultureInfo("zh"),
        new CultureInfo("ja")
    };

    public void SetLanguage(CultureInfo culture)
    {
        CurrentCulture = culture;
        LanguageChanged?.Invoke(this, culture);
    }

    public void SetLanguage(string cultureCode)
    {
        SetLanguage(new CultureInfo(cultureCode));
    }

    public CultureInfo DetectSystemLanguage()
    {
        var systemCulture = CultureInfo.CurrentUICulture;
        if (AvailableCultures.Any(c => c.TwoLetterISOLanguageName == systemCulture.TwoLetterISOLanguageName))
            return systemCulture;
        return new CultureInfo("en");
    }
}
