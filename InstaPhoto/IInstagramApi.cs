using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace InstaPhoto
{
    interface IInstagramApi
    {
        Task<JsonObject> GetJsonObjectAsync(string api);
        Task<JsonObject> PostDataAsync(string api, string content);
        String GetDate(string created_time);
    }

    interface IStorage
    {
        void AccountInsert(AuthData Item);
        Task<AuthData> GetAccountData();
        void FeedInsert(List<Feed> Items);
        Task<List<Feed>> GetFeedData();
    }

    interface IFeedPage
    {
        void WebAuthenticationBrokerShow();
        void ExceptionMessage(String ExceptionMessage);
        void AuthenticationSuccess();
        void GetFeed();
        void GetFollowers(Boolean parameter);
    }

    interface IUsersPage
    {
        void LoadData();
        void GetFollowers();
        void GetFollowedBy();
    }
}
