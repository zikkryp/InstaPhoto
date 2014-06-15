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
using Windows.Security.Authentication.Web;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Bing.Maps;

namespace InstaPhoto
{
    public sealed partial class FeedPage : Page
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

        public FeedPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

            popup = new Popup();
            menu = new Menu(this);

            storage = new Storage();
            feedpage = this;
        }

        public static FeedPage feedpage;

        public async void Logout()
        {
            await storage.Logout();
            WebAuthenticationBrokerShow();
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            try
            {
                WebAuthenticationBrokerShow();
            }
            catch (Exception)
            {
                //new Windows.UI.Popups.MessageDialog(ex.Message).ShowAsync();
            }
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

        #region WebAuthentication

        private async void WebAuthenticationBrokerShow()
        {
            AuthData authData = await storage.GetAccountData();

            if (authData != null)
            {
                Authentication.Token = authData.OAuthToken;

                Authentication.Userid = authData.UserId;

                List<Feed> feedData = new List<Feed>();

                //var storageData = storage.GetFeedData();

                //if (storageData != null)
                //{

                //}

                AuthenticationSuccess();
            }
            else
            {
                try
                {
                    WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, Authentication.AuthenticationUri, Authentication.RedirectUri);

                    if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                    {
                        var tokenUri = WebAuthenticationResult.ResponseData;

                        Authentication.Token = tokenUri.Split('=')[1].Split('&')[0];
                        Authentication.Userid = Authentication.Token.Split('.')[0];

                        storage.AccountInsert(new AuthData { UserId = Authentication.Userid, OAuthToken = Authentication.Token, IsActive = true });

                        AuthenticationSuccess();
                    }
                    else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                    {
                        throw new Exception(WebAuthenticationResult.ResponseErrorDetail.ToString());
                    }
                    else
                    {
                        throw new Exception("Вийти?");
                    }
                }
                catch (Exception e)
                {
                    ExceptionMessage(e.Message);
                }
            }
        }

        private async void ExceptionMessage(String ExceptionMessage)
        {
            var md = new Windows.UI.Popups.MessageDialog(String.Format("{0}", ExceptionMessage));

            md.Commands.Add(new Windows.UI.Popups.UICommand("Ні", (UICommandInvokedHandler) => { WebAuthenticationBrokerShow(); }));
            md.Commands.Add(new Windows.UI.Popups.UICommand("Так", (UICommandInvokedHandler) => { Application.Current.Exit(); }));

            await md.ShowAsync();
        }

        #endregion

        private async void AuthenticationSuccess()
        {
            progressRing.IsActive = true;

            try
            {
                var userTask = UserInfoSource.GetUserAsync(Authentication.Userid);
                var feedTask = FeedDataSource.GetFeedAsync(String.Empty);

                await System.Threading.Tasks.Task.WhenAll(userTask, feedTask);

                this.defaultViewModel["Feed"] = feedTask.Result;
                this.defaultViewModel["User"] = userTask.Result;

                CurrentUser = userTask.Result.FirstOrDefault<User>();
                user = userTask.Result.FirstOrDefault<User>();

                userInfoGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        private void gridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.feedItem = (Feed)e.ClickedItem;
            this.defaultViewModel["FeedItem"] = new System.Collections.ObjectModel.ObservableCollection<Feed> { this.feedItem };

            MediaItemPopup.IsOpen = true;

            this.user = new User(feedItem.username, string.Empty, string.Empty, feedItem.profile_picture, feedItem.full_name, 0, 0, 0, feedItem.user_id);
        }

        #region Followers Click

        private void Follower_Click(object sender, RoutedEventArgs e)
        {
            GetFollowers(false);
        }

        private void Follows_Click(object sender, RoutedEventArgs e)
        {
            GetFollowers(true);
        }

        private async void GetFollowers(Boolean parameter)
        {
            followerPopup.IsOpen = true;

            progressBarFollowers.Visibility = Windows.UI.Xaml.Visibility.Visible;

            var list = await FollowersDataSource.GetFollowersAsync(Authentication.Userid, parameter);

            followerList.ItemsSource = list;

            progressBarFollowers.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (parameter == true)
            {
                FollowersTitle.Text = "Following (" + list.Count().ToString() + ")";
            }
            else
            {
                FollowersTitle.Text = "Followed by (" + list.Count().ToString() + ")";
            }
        }

        private void FollowerItem_Click(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(UsersPage), (User)e.ClickedItem);
        }

        private void FollowersPage_Navigate(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FollowersPage), user);
        }

        #endregion

        #region MediaItem Popup

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

        #endregion

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

        private void Menu_Click(object sender, RoutedEventArgs e)
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
