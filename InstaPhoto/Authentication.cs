using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite;

namespace InstaPhoto
{
    class Storage
    {
        StorageFile storageFile;

        public async void AccountInsert(AuthData Item)
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Storage.db", CreationCollisionOption.OpenIfExists);

            using (SQLiteConnection connection = new SQLiteConnection(storageFile.Path))
            {
                connection.CreateTable<AuthData>();
                connection.InsertOrReplace(Item);
            }
        }

        public async Task<AuthData> GetAccountData()
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Storage.db", CreationCollisionOption.OpenIfExists);
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(storageFile.Path))
                {
                    return connection.Query<AuthData>("select * from AuthData where IsActive = 1").FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }

        public async void FeedInsert(List<Feed> Items)
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Storage.db", CreationCollisionOption.OpenIfExists);

            using (SQLiteConnection connection = new SQLiteConnection(storageFile.Path))
            {
                connection.CreateTable<Feed>();
                connection.DeleteAll<Feed>();
                connection.InsertAll(Items);
            }
        }

        public async Task<List<Feed>> GetFeedData()
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Storage.db", CreationCollisionOption.OpenIfExists);
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(storageFile.Path))
                {
                    return connection.Table<Feed>().ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task Logout()
        {
            storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Storage.db", CreationCollisionOption.OpenIfExists);
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(storageFile.Path))
                {
                    connection.Delete<AuthData>(Authentication.Userid);
                }
            }
            catch (Exception)
            {
                
            }
        }
    }

    public class AuthData
    {
        [PrimaryKey]
        public String UserId { get; set; }

        public String OAuthToken { get; set; }

        public Boolean IsActive { get; set; }
    }

    class hui { }

    public static class Authentication
    {
        public static String Token { get; set; }
        public static String Userid { get; set; }

        public static string Client_id { get { return "039d35ec6aef455592c2e073e70b2e70"; } }

        private static string scope { get { return "likes"; } }

        public static Uri AuthenticationUri = new Uri("https://api.instagram.com/oauth/authorize/?client_id=039d35ec6aef455592c2e073e70b2e70&scope=" + scope + "&redirect_uri=http://krypapp.com/&response_type=token");

        public static Uri RedirectUri = new Uri("http://krypapp.com/");
    }
}
