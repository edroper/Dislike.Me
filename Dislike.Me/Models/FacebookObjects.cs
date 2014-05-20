using System.Collections.Generic;

namespace Dislike.Me.Models
{
    public class UserPost
    {
        public string Id;
        public List<Like> Likes;
        public string Message;
        public string PostDate;

        public UserPost()
        {
            Likes = new List<Like>();
        }
    }

    public class DailyPosts
    {
        public string PostDate;
        public List<UserPost> UserPosts;

        public DailyPosts()
        {
            UserPosts = new List<UserPost>();
        }
    }

    public class Like
    {
        public string UserName;
        public string Uid;
    }

    public class User
    {
        public string UserName;
        public int Likes;
        public string Uid;
    }

    public class FriendsList
    {
        public Dictionary<string, string> Friends;

        public FriendsList()
        {
            Friends = new Dictionary<string, string>();
        }
    }
}