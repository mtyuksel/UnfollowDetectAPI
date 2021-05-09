using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnfollowDetectAPI.Result;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Logger;
using Microsoft.Extensions.Configuration;

namespace UnfollowDetectAPI
{

    class InstagramManager
    {
        private static UserSessionData _user;

        public static async Task<IDataResult<List<string>>> GetUnfollowers(string username, IConfiguration config)
        {
            try
            {
                InstagramManager.GetUser(config);

                var api = await InstagramManager.Login();
                if (api != null)
                {
                    List<string> result = await InstagramManager.GetUnfollowers(api, username);
                    return new SuccessDataResult<List<string>>(result, "Process Succeeded.");
                }
                else
                {
                    return new ErrorDataResult<List<string>>(message: "An Error Occured: Cannot Connect to Instagram API. Login Information May Not Be Correct!");
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<string>>(message: "An Error Occured: " + ex.Message);
            }

        }

        private static UserSessionData GetUser(IConfiguration config)
        {
            _user = new UserSessionData();
            _user.UserName = config.GetSection("InstagramConfig").GetSection("Username").Value;
            _user.Password = config.GetSection("InstagramConfig").GetSection("Password").Value;

            return _user;
        }

        private static async Task<IInstaApi> Login()
        {
            IInstaApi api = InstaApiBuilder.CreateBuilder()
                            .SetUser(_user)
                            .UseLogger(new DebugLogger(LogLevel.Exceptions))
                            .SetRequestDelay(RequestDelay.FromSeconds(8, 8))
                            .Build();

            var loginRequest = await api.LoginAsync();

            if (loginRequest.Succeeded)
            {
                return api;
            }
            else
            {
                throw new Exception("Error: " + loginRequest.Info.Message);
            }
        }

        private static async Task<List<string>> GetUnfollowers(IInstaApi api, string username)
        {
            var followersListTask = GetFollowersList(api, username);
            var followingListTask = GetFollowingList(api, username);

            await Task.WhenAll(followersListTask, followingListTask);

            var followersList = await followersListTask;
            var followingList = await followingListTask;

            if (followersList.Count == 0 && followingList.Count == 0)
            {
                throw new Exception("Please check your username!");
            }

            HashSet<string> followerUsernameList = new HashSet<string>(followersList.Select(s => s.UserName));

            var unfollowerList = followingList.Where(m => !followerUsernameList.Contains(m.UserName)).Select(f => f.UserName).ToList<string>();

            return unfollowerList;
        }

        private static async Task<InstaUserShortList> GetFollowingList(IInstaApi api, string username)
        {
            var following = await api.GetUserFollowingAsync(username, PaginationParameters.Empty);

            if (following.Info.ResponseType != ResponseType.OK && following.Info.Exception.Message == "Object reference not set to an instance of an object.")
            {
                throw new Exception("Please Check Your Username!");
            }

            var followingList = following.Value;
            return followingList;
        }

        private static async Task<InstaUserShortList> GetFollowersList(IInstaApi api, string username)
        {
            var followers = await api.GetUserFollowersAsync(username, PaginationParameters.Empty);

            if (followers.Info.ResponseType != ResponseType.OK && followers.Info.Exception.Message == "Object reference not set to an instance of an object.")
            {
                throw new Exception("Please Check Your Username!");
            }

            var followersList = followers.Value;
            return followersList;
        }
    }
}
