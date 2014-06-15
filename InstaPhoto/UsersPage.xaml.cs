using InstaPhoto.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.Web.Http;
using Bing.Maps;

namespace InstaPhoto
{
    public sealed partial class UsersPage : Page
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

        public UsersPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            //this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        User user;
        Feed feedItem;
        Pushpin p;

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            progressRing.IsActive = true;

            if (e.NavigationParameter != null)
            {
                this.user = e.NavigationParameter as User;

                this.defaultViewModel["User"] = new ObservableCollection<User>() { user };

                var userInfoDataTask = UserInfoSource.GetUserAsync(this.user.id);
                var userFeedDataTask = FeedDataSource.GetFeedAsync(this.user.id);

                await System.Threading.Tasks.Task.WhenAll(userInfoDataTask, userFeedDataTask);

                if (userFeedDataTask.Result.Count == 0)
                {
                    //якщо ніхуя нема
                }

                this.defaultViewModel["Feed"] = userFeedDataTask.Result;
                this.defaultViewModel["User"] = userInfoDataTask.Result;
            }
            else
            {
                this.user = FeedPage.CurrentUser;

                this.defaultViewModel["User"] = new ObservableCollection<User>() { FeedPage.CurrentUser };
                
                var userFeed = await FeedDataSource.GetFeedAsync(this.user.id);
                this.defaultViewModel["Feed"] = userFeed;
            }

            progressRing.IsActive = false;
        }

        #region Регистрация NavigationHelper

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            //if (e.NavigationMode == NavigationMode.New)
            {
                //new Windows.UI.Popups.MessageDialog(e.NavigationMode.ToString()).ShowAsync();
                /*var cacheSize = this.Frame.CacheSize;
                this.Frame.CacheSize = 0;
                this.Frame.CacheSize = cacheSize;*/
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Follows_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FollowersPage), this.user);
        }

        private void Followed_by_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FollowersPage), this.user);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //DataPackage dataPackage = new DataPackage();
            //dataPackage.SetText(((Feed)e.ClickedItem).standart_resolution);
            //Clipboard.SetContent(dataPackage);

            this.feedItem = (Feed)e.ClickedItem;
            this.defaultViewModel["FeedItem"] = new System.Collections.ObjectModel.ObservableCollection<Feed> { this.feedItem };

            MediaItemPopup.IsOpen = true;

            //this.user = new User(feedItem.username, string.Empty, string.Empty, feedItem.profile_picture, feedItem.full_name, 0, 0, 0, feedItem.user_id);
        }

        private async void MediaItemPopup_Opened(object sender, object e)
        {
            ButtonMap.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            progressBarComments.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (feedItem.locationBool)
            {
                ButtonMap.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bingMap.SetView(feedItem.location, 15.0F);
                p = new Pushpin();
                p.Style = this.Resources["PushPinStyle"] as Style;
                Location locationOfPin = feedItem.location;
                MapLayer.SetPositionAnchor(p, new Point(25 / 2, 39));
                MapLayer.SetPosition(p, locationOfPin);
                bingMap.Children.Add(p);
            }

            if (feedItem.userHasLiked)
            {
                Like_Button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
            }
            else
            {
                Like_Button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
            }

            commentView.ItemsSource = null;
            commentView.ItemsSource = await CommentDataSource.GetCommentsAsync(feedItem.mediaId);

            progressBarComments.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void username_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UsersPage), user);
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            MediaItemPopup.IsOpen = false;
        }

        private void CommentPost_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BingMapLocation_Click(object sender, RoutedEventArgs e)
        {
            mapPopup.IsOpen = true;
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            progressRingLike.IsActive = true;

            try
            {
                feedItem = await InstagramApi.Like(feedItem);

                if (feedItem.userHasLiked)
                {
                    Like_Button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                }
                else
                {
                    Like_Button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                }
            }
            finally
            {
                progressRingLike.IsActive = false;
            }
        }

        private async void Follow_Click(object sender, RoutedEventArgs e)
        {
            //var result = await InstagramApi.Follow(user);
            //var result = await InstagramApi.GetRelationShip(user);
            var postContent = new Windows.Web.Http.HttpStringContent("&action=follow");
            Uri resourceAddress = new Uri("https://api.instagram.com/v1/" + "users/" + user.id + "/relationship?access_token=" + Authentication.Token);
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(resourceAddress, postContent);

            var result = response.Content.ToString();

            await new MessageDialog(result).ShowAsync();
        }
    }
}
