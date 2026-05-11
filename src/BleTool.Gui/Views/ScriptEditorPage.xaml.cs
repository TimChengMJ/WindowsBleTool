using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScriptEditorPage : Page
{
    public ScriptEditorViewModel ViewModel { get; }

    public ScriptEditorPage()
    {
        ViewModel = new ScriptEditorViewModel(App.Scanner);
        this.InitializeComponent();
    }
}
