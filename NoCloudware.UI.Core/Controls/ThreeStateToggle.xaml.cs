using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NoCloudware.UI.Core.Controls;

public enum ToggleState
{
    High,
    Medium,
    Low
}

public partial class ThreeStateToggle : UserControl
{
    public static readonly RoutedEvent StateChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(StateChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ThreeStateToggle));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(ToggleState), typeof(ThreeStateToggle),
            new PropertyMetadata(ToggleState.Medium, OnStateChanged));

    public static readonly DependencyProperty ToggleHeightProperty =
        DependencyProperty.Register(nameof(ToggleHeight), typeof(double), typeof(ThreeStateToggle),
            new PropertyMetadata(36.0));

    public static readonly DependencyProperty ThumbMarginProperty =
        DependencyProperty.Register(nameof(ThumbMargin), typeof(Thickness), typeof(ThreeStateToggle),
            new PropertyMetadata(new Thickness(2)));

    public static readonly DependencyProperty ThumbCornerRadiusProperty =
        DependencyProperty.Register(nameof(ThumbCornerRadius), typeof(CornerRadius), typeof(ThreeStateToggle),
            new PropertyMetadata(new CornerRadius(6)));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(ThreeStateToggle),
            new PropertyMetadata(150.0));

    public static readonly DependencyProperty HighLabelProperty =
        DependencyProperty.Register(nameof(HighLabel), typeof(string), typeof(ThreeStateToggle),
            new PropertyMetadata("High", OnLabelChanged));

    public static readonly DependencyProperty MediumLabelProperty =
        DependencyProperty.Register(nameof(MediumLabel), typeof(string), typeof(ThreeStateToggle),
            new PropertyMetadata("Medium", OnLabelChanged));

    public static readonly DependencyProperty LowLabelProperty =
        DependencyProperty.Register(nameof(LowLabel), typeof(string), typeof(ThreeStateToggle),
            new PropertyMetadata("Low", OnLabelChanged));

    public event RoutedEventHandler StateChanged
    {
        add => AddHandler(StateChangedEvent, value);
        remove => RemoveHandler(StateChangedEvent, value);
    }

    public ToggleState State
    {
        get => (ToggleState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public double ToggleHeight
    {
        get => (double)GetValue(ToggleHeightProperty);
        set => SetValue(ToggleHeightProperty, value);
    }

    public Thickness ThumbMargin
    {
        get => (Thickness)GetValue(ThumbMarginProperty);
        set => SetValue(ThumbMarginProperty, value);
    }

    public CornerRadius ThumbCornerRadius
    {
        get => (CornerRadius)GetValue(ThumbCornerRadiusProperty);
        set => SetValue(ThumbCornerRadiusProperty, value);
    }

    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public string HighLabel
    {
        get => (string)GetValue(HighLabelProperty);
        set => SetValue(HighLabelProperty, value);
    }

    public string MediumLabel
    {
        get => (string)GetValue(MediumLabelProperty);
        set => SetValue(MediumLabelProperty, value);
    }

    public string LowLabel
    {
        get => (string)GetValue(LowLabelProperty);
        set => SetValue(LowLabelProperty, value);
    }

    private double _segmentWidth;

    public ThreeStateToggle()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateSegmentWidth();
        UpdateThumbPosition(false);
        UpdateLabels();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSegmentWidth();
        UpdateThumbPosition(false);
    }

    private void UpdateSegmentWidth()
    {
        if (RootGrid != null && RootGrid.ActualWidth > 0)
            _segmentWidth = RootGrid.ActualWidth / 3.0;
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThreeStateToggle toggle)
        {
            toggle.UpdateThumbPosition(true);
            toggle.RaiseEvent(new RoutedEventArgs(StateChangedEvent));
        }
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThreeStateToggle toggle)
            toggle.UpdateLabels();
    }

    private void UpdateThumbPosition(bool animate)
    {
        if (Thumb == null) return;

        UpdateSegmentWidth();
        double targetX = State switch
        {
            ToggleState.High => 0,
            ToggleState.Medium => _segmentWidth,
            ToggleState.Low => _segmentWidth * 2,
            _ => 0
        };

        if (animate)
        {
            var animation = new DoubleAnimation(targetX, TimeSpan.FromMilliseconds(AnimationDuration))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            (Thumb.RenderTransform as TranslateTransform)?.BeginAnimation(
                TranslateTransform.XProperty, animation);
        }
        else
        {
            if (Thumb.RenderTransform is TranslateTransform tt)
                tt.X = targetX;
        }
    }

    private void UpdateLabels()
    {
    }

    private void HighButton_Click(object sender, RoutedEventArgs e)
    {
        SetValue(StateProperty, ToggleState.High);
    }

    private void MediumButton_Click(object sender, RoutedEventArgs e)
    {
        SetValue(StateProperty, ToggleState.Medium);
    }

    private void LowButton_Click(object sender, RoutedEventArgs e)
    {
        SetValue(StateProperty, ToggleState.Low);
    }
}
