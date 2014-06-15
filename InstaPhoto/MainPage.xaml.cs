using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

namespace InstaPhoto
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            
            menu = new Menu(this);
            popup = new Popup();
        }

        Menu menu;
        Popup popup;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            popup.IsLightDismissEnabled = true;

            popup.Height = Window.Current.Bounds.Height;
            popup.Width = 175;

            popup.ChildTransitions = new Windows.UI.Xaml.Media.Animation.TransitionCollection();
            popup.ChildTransitions.Add(new Windows.UI.Xaml.Media.Animation.PaneThemeTransition() { Edge = (Windows.UI.ApplicationSettings.SettingsPane.Edge == Windows.UI.ApplicationSettings.SettingsEdgeLocation.Left) ? EdgeTransitionLocation.Left : EdgeTransitionLocation.Left });

            menu.Height = Window.Current.Bounds.Height;
            menu.Width = 125;

            popup.Child = menu;

            popup.IsOpen = true;
        }
    }
}
