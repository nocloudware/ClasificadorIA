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
        Loaded += (_, _) => ApplyLanguage();
        Translations.LanguageChanged += (_, _) => ApplyLanguage();

        MethodManual.Checked += (_, _) =>
        {
            ByokButton.Visibility = Visibility.Collapsed;
            PanelHintText.Text = "";
        };
        MethodAuto.Checked += (_, _) =>
        {
            ByokButton.Visibility = Visibility.Visible;
            PanelHintText.Text = Translations.Get("AutoByokHint");
        };
    }

    private void ApplyLanguage()
    {
        PanelTitleText.Text = Translations.Get("OptionsPanelTitle");
        PanelSubtitleText.Text = Translations.Get("AppTagline");
        SourceFolderLabel.Text = Translations.Get("SourceFolder");
        ChangeBtn.Content = Translations.Get("Browse");
        MethodLabel.Text = Translations.Get("Method");
        MethodManual.Content = Translations.Get("ManualMode");
        MethodAuto.Content = Translations.Get("AutoMode");
        ByokButton.Content = Translations.Get("Byok");
        ModeLabel.Text = Translations.Get("Mode");
        CriterionLabel.Text = Translations.Get("Criterion");
        DepthLabel.Text = Translations.Get("Depth");
        PromptSectionLabel.Text = Translations.Get("GeneratePrompt");
        GeneratePromptBtn.Content = Translations.Get("GeneratePrompt");
        CopyPromptBtn.Content = Translations.Get("Copy");
        ResponseSectionLabel.Text = Translations.Get("PasteResponse");
        PasteResponseBtn.Content = Translations.Get("PasteResponse");
        LoadResponseBtn.Content = Translations.Get("Load");
        OutputModeLabel.Text = Translations.Get("OutputMode");
        CopyRadio.Content = Translations.Get("CopyFiles");
        MoveRadio.Content = Translations.Get("MoveFiles");
        ClassifyBtn.Content = Translations.Get("Classify");
        if (PathText.Text.Length == 0)
            PathText.Text = Translations.Get("NoFolderSelected");

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