using System.Windows;
using System.Windows.Controls;
using ClasificadorIA.Models;
using ClasificadorIA.Services;

namespace ClasificadorIA.Panels;

public partial class OptionsPanel : UserControl
{
    public OptionsPanel()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ApplyLanguage();
            ApplyMethodState();
            ApplyDedupRowState();
        };
        Translations.LanguageChanged += (_, _) => ApplyLanguage();

        MethodLocal.Checked += (_, _) => ApplyMethodState();
        MethodIa.Checked += (_, _) => ApplyMethodState();
        DedupMasterCheck.Checked += (_, _) => ApplyDedupRowState();
        DedupMasterCheck.Unchecked += (_, _) => ApplyDedupRowState();
    }

    public bool SameNameChecked => DedupMasterCheck.IsChecked == true;
    public bool SizeEnabled => SizeCheck.IsChecked == true;
    public bool SizeMenor => SizeMenorRadio.IsChecked == true;
    public bool DateEnabled => DateCheck.IsChecked == true;
    public bool DateMenor => DateMenorRadio.IsChecked == true;

    public int BatchSize
    {
        get
        {
            int tag = (BatchSizeCombo.SelectedItem as ComboBoxItem)?.Tag as int? ?? 20;
            return tag;
        }
    }

    public bool IsIaMethod => MethodIa.IsChecked == true;

    private void ApplyMethodState()
    {
        bool ia = IsIaMethod;
        ModeCombo.IsEnabled = true;
        CriterionCombo.IsEnabled = true;
        BatchSizeCombo.IsEnabled = ia;
        ByokButton.Visibility = ia ? Visibility.Visible : Visibility.Collapsed;
        PanelHintText.Text = ia ? Translations.Get("AutoByokHint") : "";
        LocalDisclaimerText.Visibility = ia ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ApplyDedupRowState()
    {
        DedupRowContainer.IsEnabled = DedupMasterCheck.IsChecked == true;
    }

    private void ApplyLanguage()
    {
        PanelTitleText.Text = Translations.Get("OptionsPanelTitle");
        DedupMasterCheck.Content = Translations.Get("DedupMaster");
        SizeCheck.Content = Translations.Get("DedupSize");
        SizeMenorRadio.Content = Translations.Get("DedupMin");
        SizeMayorRadio.Content = Translations.Get("DedupMax");
        DateCheck.Content = Translations.Get("DedupDate");
        DateMenorRadio.Content = Translations.Get("DedupMin");
        DateMayorRadio.Content = Translations.Get("DedupMax");
        MethodLabel.Text = Translations.Get("Method");
        MethodLocal.Content = Translations.Get("MethodLocal");
        MethodIa.Content = Translations.Get("MethodIA");
        ByokButton.Content = Translations.Get("Byok");
        ModeLabel.Text = Translations.Get("Mode");
        CriterionLabel.Text = Translations.Get("Criterion");
        DepthLabel.Text = Translations.Get("Depth");
        BatchSizeLabel.Text = Translations.Get("BatchSize");
        OutputModeLabel.Text = Translations.Get("OutputMode");
        CopyRadio.Content = Translations.Get("CopyFiles");
        MoveRadio.Content = Translations.Get("MoveFiles");
        ClassifyBtn.Content = Translations.Get("Classify");
        CancelClassifyLink.Content = Translations.Get("Cancel");
        LocalDisclaimerText.Text = Translations.Get("LocalDisclaimer");

        int modeIndex = ModeCombo.SelectedIndex;
        ModeCombo.ItemsSource = ClassificationModes.All
            .Select(m => new ComboBoxItem { Content = Translations.ModeLabel(m.Key, Translations.Current), Tag = m.Key })
            .ToList<ComboBoxItem>();
        ModeCombo.SelectedIndex = modeIndex >= 0 && modeIndex < ModeCombo.Items.Count ? modeIndex : 0;

        int depthIndex = DepthCombo.SelectedIndex;
        DepthCombo.ItemsSource = new[]
            {
                (Content: Translations.Get("Categorias5"), Tag: 5),
                (Content: Translations.Get("Categorias10"), Tag: 10),
                (Content: Translations.Get("Categorias15"), Tag: 15),
            }
            .Select(x => new ComboBoxItem { Content = x.Content, Tag = x.Tag })
            .ToList<ComboBoxItem>();
        DepthCombo.SelectedIndex = depthIndex >= 0 && depthIndex < DepthCombo.Items.Count ? depthIndex : 0;

        int batchIndex = BatchSizeCombo.SelectedIndex;
        BatchSizeCombo.ItemsSource = new[]
            {
                (Content: Translations.Get("BatchAll"), Tag: int.MaxValue),
                (Content: "10", Tag: 10),
                (Content: "20", Tag: 20),
                (Content: "50", Tag: 50),
            }
            .Select(x => new ComboBoxItem { Content = x.Content, Tag = x.Tag })
            .ToList<ComboBoxItem>();
        BatchSizeCombo.SelectedIndex = batchIndex >= 0 && batchIndex < BatchSizeCombo.Items.Count ? batchIndex : 1;
    }

    private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string? modeKey = (ModeCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        var mode = ClassificationModes.Find(modeKey ?? "");
        if (mode == null) return;
        int idx = CriterionCombo.SelectedIndex;
        CriterionCombo.ItemsSource = ClassificationModes.GetCriteria(mode, Translations.Current)
            .Select(c => new ComboBoxItem { Content = c, Tag = ClassificationModes.GetCriterionKey(c, Translations.Current) })
            .ToList<ComboBoxItem>();
        CriterionCombo.SelectedIndex = idx >= 0 && idx < CriterionCombo.Items.Count ? idx : 0;
    }
}