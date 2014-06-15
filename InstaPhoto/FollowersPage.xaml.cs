using InstaPhoto.Common;
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

namespace InstaPhoto
{
    public sealed partial class FollowersPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public FollowersPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            User user = e.NavigationParameter as User;
            var followers = await FollowersDataSource.GetFollowersAsync(user.id, true);
            this.GridView.ItemsSource = followers;

            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #region Регистрация NavigationHelper

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content != null)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(((Button)sender).Content.ToString()));
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var user = (User)e.ClickedItem;

            this.Frame.Navigate(typeof(UsersPage), user);
        }
    }
}
