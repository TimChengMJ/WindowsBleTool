using Microsoft.UI.Xaml;
using System;

namespace BleTool.Gui;

partial class MainWindow
{
    private bool _contentLoaded;

    public void InitializeComponent()
    {
        if (_contentLoaded) return;
        _contentLoaded = true;
        Application.LoadComponent(this, new Uri("ms-appx:///MainWindow.xaml"));
    }
}
