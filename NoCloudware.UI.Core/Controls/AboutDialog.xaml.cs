using System.Windows;
using System.Windows.Media;

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

    public static readonly DependencyProperty AppCopyrightProperty =
        DependencyProperty.Register(nameof(AppCopyright), typeof(string), typeof(AboutDialog),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    public static readonly DependencyProperty ThirdPartyLicensesProperty =
        DependencyProperty.Register(nameof(ThirdPartyLicenses), typeof(string), typeof(AboutDialog),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    public static readonly DependencyProperty AppLogoProperty =
        DependencyProperty.Register(nameof(AppLogo), typeof(ImageSource), typeof(AboutDialog),
            new PropertyMetadata(null, OnHeaderChanged));

    public static readonly DependencyProperty DialogTitleProperty =
        DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("About"));

    public static readonly DependencyProperty ThirdPartyHeaderProperty =
        DependencyProperty.Register(nameof(ThirdPartyHeader), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Third-Party Licenses"));

    public static readonly DependencyProperty CheckUpdatesTextProperty =
        DependencyProperty.Register(nameof(CheckUpdatesText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Check for updates"));

    public static readonly DependencyProperty CloseButtonTextProperty =
        DependencyProperty.Register(nameof(CloseButtonText), typeof(string), typeof(AboutDialog),
            new PropertyMetadata("Close"));

    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(480.0));

    public static readonly DependencyProperty DialogHeightProperty =
        DependencyProperty.Register(nameof(DialogHeight), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(520.0));

    public static readonly DependencyProperty LogoFrameWidthProperty =
        DependencyProperty.Register(nameof(LogoFrameWidth), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(64.0));

    public static readonly DependencyProperty LogoFrameHeightProperty =
        DependencyProperty.Register(nameof(LogoFrameHeight), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(64.0));

    public static readonly DependencyProperty LogoCornerRadiusProperty =
        DependencyProperty.Register(nameof(LogoCornerRadius), typeof(CornerRadius), typeof(AboutDialog),
            new PropertyMetadata(new CornerRadius(16)));

    public static readonly DependencyProperty LogoImageWidthProperty =
        DependencyProperty.Register(nameof(LogoImageWidth), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(48.0));

    public static readonly DependencyProperty LogoImageHeightProperty =
        DependencyProperty.Register(nameof(LogoImageHeight), typeof(double), typeof(AboutDialog),
            new PropertyMetadata(48.0));

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

    public string AppCopyright
    {
        get => (string)GetValue(AppCopyrightProperty);
        set => SetValue(AppCopyrightProperty, value);
    }

    public string ThirdPartyLicenses
    {
        get => (string)GetValue(ThirdPartyLicensesProperty);
        set => SetValue(ThirdPartyLicensesProperty, value);
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

    public string ThirdPartyHeader
    {
        get => (string)GetValue(ThirdPartyHeaderProperty);
        set => SetValue(ThirdPartyHeaderProperty, value);
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

    public double LogoFrameWidth
    {
        get => (double)GetValue(LogoFrameWidthProperty);
        set => SetValue(LogoFrameWidthProperty, value);
    }

    public double LogoFrameHeight
    {
        get => (double)GetValue(LogoFrameHeightProperty);
        set => SetValue(LogoFrameHeightProperty, value);
    }

    public CornerRadius LogoCornerRadius
    {
        get => (CornerRadius)GetValue(LogoCornerRadiusProperty);
        set => SetValue(LogoCornerRadiusProperty, value);
    }

    public double LogoImageWidth
    {
        get => (double)GetValue(LogoImageWidthProperty);
        set => SetValue(LogoImageWidthProperty, value);
    }

    public double LogoImageHeight
    {
        get => (double)GetValue(LogoImageHeightProperty);
        set => SetValue(LogoImageHeightProperty, value);
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
        if (CopyrightTextBlock != null)
            CopyrightTextBlock.Text = AppCopyright;
        if (LicensesTextBlock != null)
            LicensesTextBlock.Text = ThirdPartyLicenses.Replace("\\n", "\n");
        if (LogoImage != null)
        {
            LogoImage.Source = AppLogo;
            LogoBorder.Visibility = AppLogo != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CheckUpdatesClickEvent));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
