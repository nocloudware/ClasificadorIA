using System.Windows;
using System.Windows.Controls;
using ClasificadorIA.Services;

namespace ClasificadorIA.Panels;

public partial class AddFolderButton : UserControl
{
    public event RoutedEventHandler? Clicked;

    public AddFolderButton()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyLanguage();
        Translations.LanguageChanged += (_, _) => ApplyLanguage();
    }

    public void ApplyLanguage()
    {
        Button.Content = Translations.Get("AddFolder");
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}