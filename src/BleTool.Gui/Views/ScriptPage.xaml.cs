using BleCore;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScriptPage : Page
{
    public ScriptPage() => InitializeComponent();
    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger) { }
    public void SetDevMode(bool isDev) { }
}
