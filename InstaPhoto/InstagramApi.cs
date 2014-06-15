using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Data.Json;
using System.Collections.ObjectModel;
using Windows.Web.Http;
using Bing.Maps;

namespace InstaPhoto
{
    public class InstagramApi : IInstagramApi
    {
        public async Task<JsonObject> GetJsonObjectAsync(string api)
        {
            var client = new System.Net.Http.HttpClient();
            var jsonString = await client.GetStringAsync("https://api.instagram.com/v1" + api + "?access_token=" + Authentication.Token);

            if (jsonString.Contains("APINotAllowedError"))
            {
                return null;
            }

            return JsonObject.Parse(jsonString);
        }

        public async Task<JsonObject> PostDataAsync(String api, String content)
        {
            var postContent = new Windows.Web.Http.HttpStringContent("content=" + content + "access_token=" + Authentication.Token);
            Uri resourceAddress = new Uri("https://api.instagram.com/v1/" + api);
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PostAsync(resourceAddress, postContent);
            string s;
            return JsonObject.Parse(response.Content.ToString());
        }

        public static async Task<Feed> Like(Feed feedItem)
        {
            try
            {
                if (!feedItem.userHasLiked)
                {
                    var content = new Windows.Web.Http.HttpStringContent("access_token=" + Authentication.Token);
                    Uri resourceAddress = new Uri("https://api.instagram.com/v1/media/" + feedItem.mediaId + "/likes");
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.PostAsync(resourceAddress, content);

                    feedItem.userHasLiked = true;
                }
                else
                {
                    Uri resourceAddress = new Uri("https://api.instagram.com/v1/media/" + feedItem.mediaId + "/likes?access_token=" + Authentication.Token);
                    HttpClient httpClient = new HttpClient();
                    HttpResponseMessage response = await httpClient.DeleteAsync(resourceAddress);

                    feedItem.userHasLiked = false;
                }

                return feedItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> Follow(User user)
        {
            try
            {
                var content = new Windows.Web.Http.HttpStringContent("access_token=" + Authentication.Token);
                Uri resourceAddress = new Uri("https://api.instagram.com/v1/users/" + user.id + "/relationship");
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.PostAsync(resourceAddress, content);

                return response.Content.ToString();
            }
            catch
            {
                return "Error";
            }
        }

        public static async Task<string> GetRelationShip(User user)
        {
            try
            {
                Uri resourceAddress = new Uri("https://api.instagram.com/v1/users/" + user.id + "/relationship?access_token=" + Authentication.Token);
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(resourceAddress);

                return response.Content.ToString();
            }
            catch
            {
                return "Error";
            }
        }

        public String GetDate(string created_time)
        {
            var PubDate = UnixTimeStampToDateTime(Convert.ToDouble(created_time));
            var Subtract = DateTime.Now.Subtract(UnixTimeStampToDateTime(Convert.ToDouble(created_time)));

            string timeAgo;
            if (Subtract.Days == 0)
            {
                if (Subtract.Hours > 0)
                {
                    switch (Subtract.Hours)
                    {
                        case 1:
                            timeAgo = "год.";
                            break;
                        case 2:
                        case 3:
                        case 4:
                            timeAgo = "год.";
                            break;
                        default:
                            timeAgo = "год.";
                            break;
                    }

                    return String.Format("{0} {1} тому", Subtract.Hours, timeAgo);
                }
                if (Subtract.Minutes > 0)
                {
                    switch (Subtract.Minutes)
                    {
                        case 1:
                            timeAgo = "хв.";
                            break;
                        case 2:
                        case 3:
                        case 4:
                            timeAgo = "хв.";
                            break;
                        default:
                            timeAgo = "хв.";
                            break;
                    }
                    return String.Format("{0} {1} тому", Subtract.Minutes, timeAgo);
                }
                return String.Format("{0} сек тому", Subtract.Seconds);
            }

            return String.Format("{0} {1} {2}", PubDate.Day, month[PubDate.Month - 1], PubDate.Year, Subtract.Days);
        }

        private string[] month = { "січня", "лютого", "березня", "квітня", "травня", "червня", "липня", "серпня", "вересня", "жовтня", "листопада", "грудня" };

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    public class Relationships
    {
        public Relationships(String outgoing_status, String incomming_status, Boolean target_user_is_private)
        {
            this.outgoing_status = outgoing_status;
            this.incomming_status = incomming_status;
            this.target_user_is_private = target_user_is_private;
        }

        public String outgoing_status { get; set; }
        public String incomming_status { get; set; }
        public Boolean target_user_is_private { get; set; }
    }

    public class RelationshipStatusDataSource : InstagramApi
    {
        private static RelationshipStatusDataSource _status;

        private Relationships Relationship { get; set; }

        public static async Task<Relationships> GetStatusAsync(User user)
        {
            _status = new RelationshipStatusDataSource();
            await _status.GetStatus(user);
            return _status.Relationship;
        }

        private async Task SetStatus(User user)
        {
            JsonObject jsonObject = await PostDataAsync("users/" + user.id + "/relationship", "follow");
            JsonObject jsonDataObject = jsonObject["data"].GetObject();

            Relationship = new Relationships(
                jsonDataObject["outgoing_status"].GetString(),
                jsonDataObject["incoming_status"].GetString(),
                jsonDataObject["target_user_is_private"].GetBoolean());
        }

        private async Task GetStatus(User user)
        {
            JsonObject jsonObject = await GetJsonObjectAsync("users/" + user.id + "/relationship");
            JsonObject jsonDataObject = jsonObject["data"].GetObject();

            Relationship = new Relationships(
                jsonDataObject["outgoing_status"].GetString(),
                jsonDataObject["incoming_status"].GetString(),
                jsonDataObject["target_user_is_private"].GetBoolean());
        }
    }

    #region Liked

    public class Liked
    {
        public Liked(String id, String username, String full_name, String profile_picture)
        {
            this.id = id;
            this.username = username;
            this.full_name = full_name;
            this.profile_picture = profile_picture;
        }

        public String username { get; set; }
        public String full_name { get; set; }
        public String id { get; set; }
        public String profile_picture { get; set; }
    }

    #endregion

    #region Feed

    public class Feed : InstagramApi
    {
        public Feed() { }

        public Feed(String user_id,
            String username,
            String full_name,
            String profile_picture,
            String mediaId,
            String created_time,
            String link,
            String type,
            String low_resolution,
            String thumbnail,
            String standart_resolution,
            String video,
            Int32 likes_count,
            Int32 comments_count,
            Boolean userHasLiked,
            Location location
            )
        {
            this.user_id = user_id;
            this.username = username;
            this.full_name = full_name;
            this.profile_picture = profile_picture;

            this.mediaId = mediaId;
            this.created_time = created_time;
            this.link = link;
            this.type = type;
            this.low_resolution = low_resolution;
            this.thumbnail = thumbnail;
            this.standart_resolution = standart_resolution;
            this.video = video;
            this.likes_count = likes_count;
            this.comments_count = comments_count;
            this.userHasLiked = userHasLiked;
            this.location = location;
            
            if (location != null)
            {
                locationBool = true;
            }

            liked = new ObservableCollection<Liked>();

            IsVideo = true;

            if (type == "image")
            {
                IsVideo = false;
                visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        public String user_id { get; set; }
        public String username { get; set; }
        public String full_name { get; set; }
        public String profile_picture { get; set; }

        public String mediaId { get; set; }
        public String created_time { get; set; }
        public String link { get; set; }
        public String type { get; set; }
        public String low_resolution { get; set; }
        public String thumbnail { get; set; }
        public String standart_resolution { get; set; }
        public String video { get; set; }

        public Int32 likes_count { get; set; }
        public ObservableCollection<Liked> liked { get; set; }

        public Int32 comments_count { get; set; }
        public Boolean userHasLiked { get; set; }
        public Location location { get; set; }
        public Boolean locationBool { get; set; }

        public Windows.UI.Xaml.Visibility visibility { get; set; }
        public Boolean IsVideo { get; set; }

        public String Date { get { return GetDate(created_time); } }
    }

    public class FeedDataSource : InstagramApi
    {
        private static FeedDataSource _sampleDataSource;

        private ObservableCollection<Feed> _feed = new ObservableCollection<Feed>();

        public ObservableCollection<Feed> Feed
        {
            get { return this._feed; }
        }

        public static async Task<ObservableCollection<Feed>> GetFeedAsync(String userid)
        {
            _sampleDataSource = new FeedDataSource();
            await _sampleDataSource.GetSampleDataAsync(userid);
            return _sampleDataSource.Feed;
        }

        private async Task GetSampleDataAsync(string userid)
        {
            //Uri dataUri = new Uri("ms-appx:///Lana.json");

            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);

            //string jsonText = await FileIO.ReadTextAsync(file);

            //JsonObject jsonObject = JsonObject.Parse(jsonText);

            String api;

            if (userid == String.Empty)
            {
                api = "/users/self/feed";
            }
            else
            {
                api = "/users/" + userid + "/media/recent";
            }

            JsonObject jsonObject = null;

            try
            {
                jsonObject = await GetJsonObjectAsync(api);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                var ex = e;
                return;
            }

            JsonArray jsonArray = jsonObject["data"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject itemObject = groupValue.GetObject();

                String type = itemObject["type"].GetString();

                String video = String.Empty;

                if (type == "video")
                {
                    video = itemObject["videos"].GetObject()["standard_resolution"].GetObject()["url"].GetString();
                }

                var imagesObject = itemObject["images"].GetObject();
                var userObject = itemObject["user"].GetObject();
                var likesObject = itemObject["likes"].GetObject();
                var commentsObject = itemObject["comments"].GetObject();

                Location location = null;
                try
                {
                    var locationObject = itemObject["location"];
                    location = new Location(itemObject["location"].GetObject()["latitude"].GetNumber(), itemObject["location"].GetObject()["longitude"].GetNumber());
                }
                catch { }
                

                var item = new Feed(
                    userObject["id"].GetString(),
                    userObject["username"].GetString(),
                    userObject["full_name"].GetString(),
                    userObject["profile_picture"].GetString(),
                    itemObject["id"].GetString(),
                    itemObject["created_time"].GetString(),
                    itemObject["link"].GetString(),
                    itemObject["type"].GetString(),
                    imagesObject["low_resolution"].GetObject()["url"].GetString(),
                    imagesObject["thumbnail"].GetObject()["url"].GetString(),
                    imagesObject["standard_resolution"].GetObject()["url"].GetString(),
                    video,
                    (Int32)likesObject["count"].GetNumber(),
                    (Int32)commentsObject["count"].GetNumber(),
                    itemObject["user_has_liked"].GetBoolean(),
                    location
                    );

                if ((Int32)likesObject["count"].GetNumber() > 0)
                {
                    foreach (JsonValue likeValue in likesObject["data"].GetArray())
                    {
                        JsonObject likeObject = likeValue.GetObject();
                        item.liked.Add(new Liked(
                            likeObject["id"].GetString(),
                            likeObject["username"].GetString(),
                            likeObject["full_name"].GetString(),
                            likeObject["profile_picture"].GetString()));
                    }
                }

                this.Feed.Add(item);
            }
        }
    }

    #endregion    

    #region UserInfo

    public class User
    {
        public User(String username, String bio, String website, String profile_picture, String full_name, Int32 media_count, Int32 followed_by, Int32 follows, String id)
        {
            this.username = username;
            this.bio = bio;
            this.website = website;
            this.profile_picture = profile_picture;
            this.full_name = full_name;
            this.media_count = media_count;
            this.followed_by = followed_by;
            this.follows = follows;
            this.id = id;
        }

        public String username { get; set; }
        public String bio { get; set; }
        public String website { get; set; }
        public String profile_picture { get; set; }
        public String full_name { get; set; }
        public Int32 media_count { get; set; }
        public Int32 followed_by { get; set; }
        public Int32 follows { get; set; }
        public String id { get; set; }
    }

    public class UserInfoSource : InstagramApi
    {
        private static UserInfoSource _sampleDataSource;

        private ObservableCollection<User> _user = new ObservableCollection<User>();

        public ObservableCollection<User> User
        {
            get { return this._user; }
        }

        public static async Task<ObservableCollection<User>> GetUserAsync(string userid)
        {
            _sampleDataSource = new UserInfoSource();
            await _sampleDataSource.GetSampleDataAsync(userid);
            return _sampleDataSource.User;
        }

        private async Task GetSampleDataAsync(string userid)
        {
            //Uri dataUri = new Uri("ms-appx:///User.json");

            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);

            //string jsonText = await FileIO.ReadTextAsync(file);

            //JsonObject jsonObject = JsonObject.Parse(jsonText);

            JsonObject jsonObject = await GetJsonObjectAsync("/users/" + userid);

            var itemObject = jsonObject["data"].GetObject();

            var item = new User(
                itemObject["username"].GetString(),
                itemObject["bio"].GetString(),
                itemObject["website"].GetString(),
                itemObject["profile_picture"].GetString(),
                itemObject["full_name"].GetString(),
                (Int32)itemObject["counts"].GetObject()["media"].GetNumber(),
                (Int32)itemObject["counts"].GetObject()["followed_by"].GetNumber(),
                (Int32)itemObject["counts"].GetObject()["follows"].GetNumber(),
                itemObject["id"].GetString());

            User.Add(item);

            //new Windows.UI.Popups.MessageDialog(e.Message).ShowAsync();
        }
    }

    #endregion
        
    #region Followers

    public sealed class FollowersDataSource : InstagramApi
    {
        private static FollowersDataSource _sampleDataSource;

        private ObservableCollection<User> _followers = new ObservableCollection<User>();

        public ObservableCollection<User> Followers
        {
            get { return this._followers; }
        }

        public static async Task<IEnumerable<User>> GetFollowersAsync(String id, Boolean follows)
        {
            _sampleDataSource = new FollowersDataSource();
            await _sampleDataSource.GetSampleDataAsync(id, follows);
            return _sampleDataSource.Followers;
        }

        private async Task GetSampleDataAsync(String id, Boolean follows)
        {
            String api = "followed-by";

            if (follows)
            {
                api = "follows";
            }

            JsonObject jsonObject = await GetJsonObjectAsync("/users/" + id + "/" + api);
            JsonArray jsonArray = jsonObject["data"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject itemObject = groupValue.GetObject();

                var item = new User(
                    itemObject["username"].GetString(),
                    itemObject["bio"].GetString(),
                    itemObject["website"].GetString(),
                    itemObject["profile_picture"].GetString(),
                    itemObject["full_name"].GetString(),
                    0, 0, 0,
                    itemObject["id"].GetString());

                this.Followers.Add(item);
            }
        }
    }

    #endregion

    #region Comments

    public class Comment
    {
        public Comment(String created_time, String text, String commentId, String username, String profile_picture, String userid, String full_name)
        {
            this.created_time = created_time;
            this.text = text;
            this.commentId = commentId;

            this.username = username;
            this.profile_picture = profile_picture;
            this.userid= userid;
            this.full_name = full_name;
        }

        public string created_time { get; set; }
        public string text { get; set; }
        public string commentId { get; set; }

        public string username { get; set; }
        public string profile_picture { get; set; }
        public string userid { get; set; }
        public string full_name { get; set; }
    }

    public class CommentDataSource : InstagramApi
    {
        private static CommentDataSource _commentDataSource;

        private ObservableCollection<Comment> _comments = new ObservableCollection<Comment>();

        public ObservableCollection<Comment> Comments
        {
            get { return this._comments; }
        }

        public static async Task<IEnumerable<Comment>> GetCommentsAsync(String mediaId)
        {
            _commentDataSource = new CommentDataSource();
            await _commentDataSource.GetDataAsync(mediaId);
            return _commentDataSource._comments;
        }

        private async Task GetDataAsync(String mediaId)
        {
            JsonObject jsonObject = await GetJsonObjectAsync("/media/" + mediaId + "/comments");
            JsonArray jsonArray = jsonObject["data"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject itemObject = groupValue.GetObject();

                Comment comment = new Comment(
                    itemObject["created_time"].GetString(),
                    itemObject["text"].GetString(),
                    itemObject["id"].GetString(),
                    itemObject["from"].GetObject()["username"].GetString(),
                    itemObject["from"].GetObject()["profile_picture"].GetString(),
                    itemObject["from"].GetObject()["id"].GetString(),
                    itemObject["from"].GetObject()["full_name"].GetString()
                    );

                this.Comments.Add(comment);
            }
        }
    }

#endregion

    //public class CurrentUser
    //{
    //    private Task<ObservableCollection<User>> user;

    //    public static void SetCurrentUser(User user)
    //    {
    //        username = user.username;
    //        bio = user.bio;
    //        website = user.website;
    //        profile_picture = user.profile_picture;
    //        full_name = user.full_name;
    //        media_count = user.media_count;
    //        followed_by = user.followed_by;
    //        follows = user.follows;
    //        id = user.id;
    //    }

    //    public static String username { get; set; }
    //    public static String bio { get; set; }
    //    public static String website { get; set; }
    //    public static String profile_picture { get; set; }
    //    public static String full_name { get; set; }
    //    public static Int32 media_count { get; set; }
    //    public static Int32 followed_by { get; set; }
    //    public static Int32 follows { get; set; }
    //    public static String id { get; set; }
    //}
}
