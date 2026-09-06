using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NoCloudware.UI.Core.Controls;

public partial class AboutDialog : Window
{
    public static readonly RoutedEvent CheckUpdatesClickEvent =
        EventManager.RegisterRoutedEvent(nameof(CheckUpdatesClick), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(AboutDialog));

    public static readonly DependencyProperty AppNameProperty =
        DependencyProperty.Register(nameof(AppName), typeof(string), typeof(AboutDialog),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    public static readonly DependencyProperty AppVersionProperty =
        DependencyProperty.Register(nameof(AppVersion), typeof(string), typeof(AboutDialog),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    public static readonly DependencyProperty AppLogoProperty =
        DependencyProperty.Register(nameof(AppLogo), typeof(ImageSource), typeof(AboutDialog),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty DialogTitleProperty =
        DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("About"));

    public static readonly DependencyProperty CheckUpdatesTextProperty =
        DependencyProperty.Register(nameof(CheckUpdatesText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Check for updates"));

    public static readonly DependencyProperty CloseButtonTextProperty =
        DependencyProperty.Register(nameof(CloseButtonText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Close"));

    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(520.0));

    public static readonly DependencyProperty DialogHeightProperty =
        DependencyProperty.Register(nameof(DialogHeight), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(560.0));

    public static readonly DependencyProperty CreditsHeaderProperty =
        DependencyProperty.Register(nameof(CreditsHeader), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Credits", OnHeaderChanged));

    public static readonly DependencyProperty DevelopedByTextProperty =
        DependencyProperty.Register(nameof(DevelopedByText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Developed by", OnHeaderChanged));

    public static readonly DependencyProperty DeveloperNameProperty =
        DependencyProperty.Register(nameof(DeveloperName), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("NoCloudware", OnHeaderChanged));

    public static readonly DependencyProperty DeveloperUrlProperty =
        DependencyProperty.Register(nameof(DeveloperUrl), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("https://www.nocloudware.com", OnHeaderChanged));

    public static readonly DependencyProperty ThirdPartyLibrariesTextProperty =
        DependencyProperty.Register(nameof(ThirdPartyLibrariesText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Third-party libraries", OnHeaderChanged));

    public static readonly DependencyProperty WpfUiDescProperty =
        DependencyProperty.Register(nameof(WpfUiDesc), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("UI framework", OnHeaderChanged));

    public static readonly DependencyProperty MvvmDescProperty =
        DependencyProperty.Register(nameof(MvvmDesc), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("MVVM toolkit", OnHeaderChanged));

    public static readonly DependencyProperty SpecialThanksTextProperty =
        DependencyProperty.Register(nameof(SpecialThanksText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Special thanks", OnHeaderChanged));

    public static readonly DependencyProperty SpecialThanksMessageProperty =
        DependencyProperty.Register(nameof(SpecialThanksMessage), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("", OnHeaderChanged));

    public static readonly DependencyProperty TechnologiesUsedTextProperty =
        DependencyProperty.Register(nameof(TechnologiesUsedText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Technologies used", OnHeaderChanged));

    public static readonly DependencyProperty TechListProperty =
        DependencyProperty.Register(nameof(TechList), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("", OnHeaderChanged));

    public static readonly DependencyProperty LicenseTextProperty =
        DependencyProperty.Register(nameof(LicenseText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("License", OnHeaderChanged));

    public static readonly DependencyProperty LicenseInfoProperty =
        DependencyProperty.Register(nameof(LicenseInfo), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("", OnHeaderChanged));

    public bool IsCheckUpdatesEnabled
    {
        get => CheckUpdatesButton?.IsEnabled ?? true;
        set { if (CheckUpdatesButton != null) CheckUpdatesButton.IsEnabled = value; }
    }

    public event RoutedEventHandler CheckUpdatesClick
    {
        add => AddHandler(CheckUpdatesClickEvent, value);
        remove => RemoveHandler(CheckUpdatesClickEvent, value);
    }

    public string AppName
    {
        get => (string)GetValue(AppNameProperty);
        set => SetValue(AppNameProperty, value);
    }

    public string AppVersion
    {
        get => (string)GetValue(AppVersionProperty);
        set => SetValue(AppVersionProperty, value);
    }

    public ImageSource? AppLogo
    {
        get => (ImageSource?)GetValue(AppLogoProperty);
        set => SetValue(AppLogoProperty, value);
    }

    public string DialogTitle
    {
        get => (string)GetValue(DialogTitleProperty);
        set => SetValue(DialogTitleProperty, value);
    }

    public string CheckUpdatesText
    {
        get => (string)GetValue(CheckUpdatesTextProperty);
        set => SetValue(CheckUpdatesTextProperty, value);
    }

    public string CloseButtonText
    {
        get => (string)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    public double DialogHeight
    {
        get => (double)GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    public string CreditsHeader
    {
        get => (string)GetValue(CreditsHeaderProperty);
        set => SetValue(CreditsHeaderProperty, value);
    }

    public string DevelopedByText
    {
        get => (string)GetValue(DevelopedByTextProperty);
        set => SetValue(DevelopedByTextProperty, value);
    }

    public string DeveloperName
    {
        get => (string)GetValue(DeveloperNameProperty);
        set => SetValue(DeveloperNameProperty, value);
    }

    public string DeveloperUrl
    {
        get => (string)GetValue(DeveloperUrlProperty);
        set => SetValue(DeveloperUrlProperty, value);
    }

    public string ThirdPartyLibrariesText
    {
        get => (string)GetValue(ThirdPartyLibrariesTextProperty);
        set => SetValue(ThirdPartyLibrariesTextProperty, value);
    }

    public string WpfUiDesc
    {
        get => (string)GetValue(WpfUiDescProperty);
        set => SetValue(WpfUiDescProperty, value);
    }

    public string MvvmDesc
    {
        get => (string)GetValue(MvvmDescProperty);
        set => SetValue(MvvmDescProperty, value);
    }

    public string SpecialThanksText
    {
        get => (string)GetValue(SpecialThanksTextProperty);
        set => SetValue(SpecialThanksTextProperty, value);
    }

    public string SpecialThanksMessage
    {
        get => (string)GetValue(SpecialThanksMessageProperty);
        set => SetValue(SpecialThanksMessageProperty, value);
    }

    public string TechnologiesUsedText
    {
        get => (string)GetValue(TechnologiesUsedTextProperty);
        set => SetValue(TechnologiesUsedTextProperty, value);
    }

    public string TechList
    {
        get => (string)GetValue(TechListProperty);
        set => SetValue(TechListProperty, value);
    }

    public string LicenseText
    {
        get => (string)GetValue(LicenseTextProperty);
        set => SetValue(LicenseTextProperty, value);
    }

    public string LicenseInfo
    {
        get => (string)GetValue(LicenseInfoProperty);
        set => SetValue(LicenseInfoProperty, value);
    }

    public AboutDialog()
    {
        InitializeComponent();
        UpdateDisplay();
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AboutDialog dialog)
            dialog.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (AppNameTextBlock != null)
            AppNameTextBlock.Text = AppName;
        if (AppVersionTextBlock != null)
            AppVersionTextBlock.Text = AppVersion;
        if (LogoImage != null)
        {
            LogoImage.Source = AppLogo;
            LogoImage.Visibility = AppLogo != null ? Visibility.Visible : Visibility.Collapsed;
        }
        if (CreditsHeaderTextBlock != null)
            CreditsHeaderTextBlock.Text = CreditsHeader;
        if (DevelopedByTextBlock != null)
            DevelopedByTextBlock.Text = DevelopedByText;
        if (DeveloperHyperlink != null)
        {
            DeveloperHyperlink.NavigateUri = new Uri(DeveloperUrl);
            DeveloperHyperlink.Inlines.Clear();
            DeveloperHyperlink.Inlines.Add(new Run { Text = DeveloperName });
        }
        if (ThirdPartyLibrariesTextBlock != null)
            ThirdPartyLibrariesTextBlock.Text = ThirdPartyLibrariesText;
        if (WpfUiDescTextBlock != null)
            WpfUiDescTextBlock.Text = WpfUiDesc;
        if (MvvmDescTextBlock != null)
            MvvmDescTextBlock.Text = MvvmDesc;
        if (SpecialThanksTextBlock != null)
            SpecialThanksTextBlock.Text = SpecialThanksText;
        if (SpecialThanksMessageTextBlock != null)
            SpecialThanksMessageTextBlock.Text = SpecialThanksMessage;
        if (TechnologiesUsedTextBlock != null)
            TechnologiesUsedTextBlock.Text = TechnologiesUsedText;
        if (TechListTextBlock != null)
            TechListTextBlock.Text = TechList;
        if (LicenseTextBlock != null)
            LicenseTextBlock.Text = LicenseText;
        if (LicenseInfoTextBlock != null)
            LicenseInfoTextBlock.Text = LicenseInfo;
    }

    private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CheckUpdatesClickEvent));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch { }
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
            Close();
    }
}