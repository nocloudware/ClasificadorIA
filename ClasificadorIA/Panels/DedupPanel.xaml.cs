using System.Windows.Controls;
using ClasificadorIA.Services;

namespace ClasificadorIA.Panels;

public partial class DedupPanel : UserControl
{
    public DedupPanel()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyLanguage();
        Translations.LanguageChanged += (_, _) => ApplyLanguage();
    }

    public bool SameNameChecked => SameNameCheck.IsChecked == true;
    public bool MinSizeChecked => MinSizeCheck.IsChecked == true;
    public bool MinDateChecked => MinDateCheck.IsChecked == true;

    public void ApplyLanguage()
    {
        TitleText.Text = Translations.Get("DedupTitle");
        SameNameCheck.Content = Translations.Get("DedupSameName");
        MinSizeCheck.Content = Translations.Get("DedupMinSize");
        MinDateCheck.Content = Translations.Get("DedupMinDate");
    }
}