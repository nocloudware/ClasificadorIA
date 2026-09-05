using System.Windows;
using System.Windows.Controls;

namespace NoCloudware.UI.Core.Controls;

public partial class StatusBar : UserControl
{
    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(StatusBar),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty ProcessedCountProperty =
        DependencyProperty.Register(nameof(ProcessedCount), typeof(int), typeof(StatusBar),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty PendingCountProperty =
        DependencyProperty.Register(nameof(PendingCount), typeof(int), typeof(StatusBar),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty ErrorCountProperty =
        DependencyProperty.Register(nameof(ErrorCount), typeof(int), typeof(StatusBar),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty TotalLabelProperty =
        DependencyProperty.Register(nameof(TotalLabel), typeof(string), typeof(StatusBar),
            new PropertyMetadata("files", OnCountChanged));

    public static readonly DependencyProperty ProcessedLabelProperty =
        DependencyProperty.Register(nameof(ProcessedLabel), typeof(string), typeof(StatusBar),
            new PropertyMetadata("processed", OnCountChanged));

    public static readonly DependencyProperty PendingLabelProperty =
        DependencyProperty.Register(nameof(PendingLabel), typeof(string), typeof(StatusBar),
            new PropertyMetadata("pending", OnCountChanged));

    public static readonly DependencyProperty ErrorsLabelProperty =
        DependencyProperty.Register(nameof(ErrorsLabel), typeof(string), typeof(StatusBar),
            new PropertyMetadata("errors", OnCountChanged));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public int ProcessedCount
    {
        get => (int)GetValue(ProcessedCountProperty);
        set => SetValue(ProcessedCountProperty, value);
    }

    public int PendingCount
    {
        get => (int)GetValue(PendingCountProperty);
        set => SetValue(PendingCountProperty, value);
    }

    public int ErrorCount
    {
        get => (int)GetValue(ErrorCountProperty);
        set => SetValue(ErrorCountProperty, value);
    }

    public string TotalLabel
    {
        get => (string)GetValue(TotalLabelProperty);
        set => SetValue(TotalLabelProperty, value);
    }

    public string ProcessedLabel
    {
        get => (string)GetValue(ProcessedLabelProperty);
        set => SetValue(ProcessedLabelProperty, value);
    }

    public string PendingLabel
    {
        get => (string)GetValue(PendingLabelProperty);
        set => SetValue(PendingLabelProperty, value);
    }

    public string ErrorsLabel
    {
        get => (string)GetValue(ErrorsLabelProperty);
        set => SetValue(ErrorsLabelProperty, value);
    }

    public StatusBar()
    {
        InitializeComponent();
        UpdateDisplay();
    }

    private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBar bar)
            bar.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (TotalTextBlock != null)
            TotalTextBlock.Text = $"{TotalCount} {TotalLabel}";
        if (ProcessedTextBlock != null)
            ProcessedTextBlock.Text = $"{ProcessedCount} {ProcessedLabel}";
        if (PendingTextBlock != null)
            PendingTextBlock.Text = $"{PendingCount} {PendingLabel}";
        if (ErrorTextBlock != null)
            ErrorTextBlock.Text = $"{ErrorCount} {ErrorsLabel}";
    }
}
