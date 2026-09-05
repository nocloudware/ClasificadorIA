using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace NoCloudware.UI.Core.Controls;

public partial class ThemeToggle : UserControl
{
    public static readonly DependencyProperty IsDarkThemeProperty =
        DependencyProperty.Register(nameof(IsDarkTheme), typeof(bool), typeof(ThemeToggle),
            new PropertyMetadata(true, OnThemeChanged));

    public static readonly DependencyProperty DarkIconSymbolProperty =
        DependencyProperty.Register(nameof(DarkIconSymbol), typeof(SymbolRegular), typeof(ThemeToggle),
            new PropertyMetadata(SymbolRegular.WeatherMoon20));

    public static readonly DependencyProperty LightIconSymbolProperty =
        DependencyProperty.Register(nameof(LightIconSymbol), typeof(SymbolRegular), typeof(ThemeToggle),
            new PropertyMetadata(SymbolRegular.WeatherSunny20));

    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(ThemeToggle),
            new PropertyMetadata(28.0));

    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(ThemeToggle),
            new PropertyMetadata(28.0));

    public static readonly RoutedEvent ThemeToggledEvent =
        EventManager.RegisterRoutedEvent(nameof(ThemeToggled), RoutingStrategy.Bubble,
            typeof(EventHandler<ThemeToggledEventArgs>), typeof(ThemeToggle));

    public bool IsDarkTheme
    {
        get => (bool)GetValue(IsDarkThemeProperty);
        set => SetValue(IsDarkThemeProperty, value);
    }

    public SymbolRegular DarkIconSymbol
    {
        get => (SymbolRegular)GetValue(DarkIconSymbolProperty);
        set => SetValue(DarkIconSymbolProperty, value);
    }

    public SymbolRegular LightIconSymbol
    {
        get => (SymbolRegular)GetValue(LightIconSymbolProperty);
        set => SetValue(LightIconSymbolProperty, value);
    }

    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    public event EventHandler<ThemeToggledEventArgs> ThemeToggled
    {
        add => AddHandler(ThemeToggledEvent, value);
        remove => RemoveHandler(ThemeToggledEvent, value);
    }

    public ThemeToggle()
    {
        InitializeComponent();
        UpdateIcon();
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThemeToggle toggle)
            toggle.UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (ThemeIcon != null)
        {
            ThemeIcon.Symbol = IsDarkTheme
                ? DarkIconSymbol
                : LightIconSymbol;
        }
    }

    public void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleTheme();
        RaiseEvent(new ThemeToggledEventArgs(IsDarkTheme, ThemeToggledEvent));
    }
}

public class ThemeToggledEventArgs(bool isDark, RoutedEvent routedEvent)
    : RoutedEventArgs(routedEvent)
{
    public bool IsDarkTheme { get; } = isDark;
}
