using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Bing.Maps;

namespace InstaPhoto
{
    public sealed partial class FeedPage
    {
        Menu menu;
        Popup popup;

        Pushpin p;
        Storage storage;
        public static User CurrentUser;
        User user;
        Feed feedItem;
    }
}
