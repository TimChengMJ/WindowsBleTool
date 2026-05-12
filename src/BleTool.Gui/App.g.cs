using Microsoft.UI.Xaml;
using System;

namespace BleTool.Gui;

partial class App
{
    private bool _contentLoaded;

    public void InitializeComponent()
    {
        if (_contentLoaded) return;
        _contentLoaded = true;
        Application.LoadComponent(this, new Uri("ms-appx:///App.xaml"));
    }

    [STAThread]
    static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(p => new App());
    }
}
