using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace NoCloudware.UI.Core.Controls;

public class LanguageItem(string name, string flag, string cultureCode)
{
    public string Name { get; set; } = name;
    public string Flag { get; set; } = flag;
    public string CultureCode { get; set; } = cultureCode;

    public override string ToString() => Name;
}

public class LanguageChangedEventArgs(string cultureCode, RoutedEvent routedEvent)
    : RoutedEventArgs(routedEvent)
{
    public string CultureCode { get; } = cultureCode;
}

public partial class LanguageSelector : UserControl
{
    private bool _suppressSelectionChanged;

    public static readonly DependencyProperty LanguagesProperty =
        DependencyProperty.Register(
            nameof(Languages),
            typeof(ObservableCollection<LanguageItem>),
            typeof(LanguageSelector),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FlagWidthProperty =
        DependencyProperty.Register(nameof(FlagWidth), typeof(double), typeof(LanguageSelector),
            new PropertyMetadata(18.0));

    public static readonly DependencyProperty FlagHeightProperty =
        DependencyProperty.Register(nameof(FlagHeight), typeof(double), typeof(LanguageSelector),
            new PropertyMetadata(12.0));

    public static readonly DependencyProperty ComboMinWidthProperty =
        DependencyProperty.Register(nameof(ComboMinWidth), typeof(double), typeof(LanguageSelector),
            new PropertyMetadata(60.0));

    public static readonly DependencyProperty ComboMaxWidthProperty =
        DependencyProperty.Register(nameof(ComboMaxWidth), typeof(double), typeof(LanguageSelector),
            new PropertyMetadata(60.0));

    public static readonly DependencyProperty FlagsBaseUrlProperty =
        DependencyProperty.Register(nameof(FlagsBaseUrl), typeof(string), typeof(LanguageSelector),
            new PropertyMetadata("https://flagcdn.com/16x12/"));

    public static readonly DependencyProperty FlagsExtensionProperty =
        DependencyProperty.Register(nameof(FlagsExtension), typeof(string), typeof(LanguageSelector),
            new PropertyMetadata(".png"));

    public static readonly RoutedEvent LanguageChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(LanguageChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(LanguageSelector));

    public ObservableCollection<LanguageItem>? Languages
    {
        get => (ObservableCollection<LanguageItem>?)GetValue(LanguagesProperty);
        set => SetValue(LanguagesProperty, value);
    }

    public double FlagWidth
    {
        get => (double)GetValue(FlagWidthProperty);
        set => SetValue(FlagWidthProperty, value);
    }

    public double FlagHeight
    {
        get => (double)GetValue(FlagHeightProperty);
        set => SetValue(FlagHeightProperty, value);
    }

    public double ComboMinWidth
    {
        get => (double)GetValue(ComboMinWidthProperty);
        set => SetValue(ComboMinWidthProperty, value);
    }

    public double ComboMaxWidth
    {
        get => (double)GetValue(ComboMaxWidthProperty);
        set => SetValue(ComboMaxWidthProperty, value);
    }

    public string FlagsBaseUrl
    {
        get => (string)GetValue(FlagsBaseUrlProperty);
        set => SetValue(FlagsBaseUrlProperty, value);
    }

    public string FlagsExtension
    {
        get => (string)GetValue(FlagsExtensionProperty);
        set => SetValue(FlagsExtensionProperty, value);
    }

    public event RoutedEventHandler LanguageChanged
    {
        add => AddHandler(LanguageChangedEvent, value);
        remove => RemoveHandler(LanguageChangedEvent, value);
    }

    public LanguageSelector()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChanged) return;
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is LanguageItem item)
        {
            RaiseEvent(new LanguageChangedEventArgs(item.CultureCode, LanguageChangedEvent));
        }
    }

    public void SetLanguage(string cultureCode)
    {
        if (Languages is null) return;

        _suppressSelectionChanged = true;
        try
        {
            foreach (var lang in Languages)
            {
                if (lang.CultureCode.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))
                {
                    LanguageCombo.SelectedItem = lang;
                    return;
                }
            }
        }
        finally
        {
            _suppressSelectionChanged = false;
        }
    }
}