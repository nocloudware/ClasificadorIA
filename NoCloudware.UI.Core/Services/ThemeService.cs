using System;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace NoCloudware.UI.Core.Services;

public class ThemeService
{
    private ResourceDictionary? _currentThemeResources;

    public bool IsDarkTheme { get; private set; } = true;

    public event EventHandler<bool>? ThemeChanged;

    public void ToggleTheme()
    {
        ApplyTheme(!IsDarkTheme);
    }

    public void ApplyTheme(bool isDark)
    {
        IsDarkTheme = isDark;
        var theme = isDark ? ApplicationTheme.Dark : ApplicationTheme.Light;
        // None: la ventana pinta su propio fondo (WindowBackgroundBrush); Mica (default) lo vuelve negro al cambiar de tema.
        ApplicationThemeManager.Apply(theme, WindowBackdropType.None);

        // Swap custom brush dictionary for Aether theme
        if (Application.Current == null) return;

        if (_currentThemeResources != null)
            Application.Current.Resources.MergedDictionaries.Remove(_currentThemeResources);

        var themeName = isDark ? "AetherColors.Dark.xaml" : "AetherColors.Light.xaml";
        var uri = new Uri($"pack://application:,,,/NoCloudware.UI.Core;component/Themes/Aether/{themeName}", UriKind.Absolute);
        _currentThemeResources = new ResourceDictionary { Source = uri };
        Application.Current.Resources.MergedDictionaries.Add(_currentThemeResources);

        ThemeChanged?.Invoke(this, isDark);
    }
}
