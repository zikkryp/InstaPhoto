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
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace InstaPhoto
{
    public sealed partial class Menu : UserControl
    {
        public Menu(Page page)
        {
            this.InitializeComponent();

            this.page = page;
        }

        Page page;

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            if (listView.SelectedIndex.Equals(0))
            {
                page.Frame.Navigate(typeof(FeedPage));
            }
            else if (listView.SelectedIndex.Equals(1))
            {
                page.Frame.Navigate(typeof(FeedPage));
            }
            else if (listView.SelectedIndex.Equals(2))
            {
                CameraCaptureUI camera = new CameraCaptureUI();

                var storageFile = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);

                if (storageFile != null)
                {
                    var bitmapImage = new BitmapImage();

                    using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read))
                    {
                        bitmapImage.SetSource(stream);
                    }

                    await new Windows.UI.Popups.MessageDialog(storageFile.Path).ShowAsync();

                    await storageFile.DeleteAsync();
                }
            }
            else if (listView.SelectedIndex.Equals(3))
            {
                page.Frame.Navigate(typeof(FeedPage));
            }
            else if (listView.SelectedIndex.Equals(4))
            {
                //User user = new User();
                page.Frame.Navigate(typeof(UsersPage));
            }
            else
            {

            }
        }
    }
}
