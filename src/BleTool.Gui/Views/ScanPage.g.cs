using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace BleTool.Gui.Views;

partial class ScanPage
{
    private bool _contentLoaded;

    public void InitializeComponent()
    {
        if (_contentLoaded) return;
        _contentLoaded = true;
        Application.LoadComponent(this, new Uri("ms-appx:///Views/ScanPage.xaml"));
    }
}
