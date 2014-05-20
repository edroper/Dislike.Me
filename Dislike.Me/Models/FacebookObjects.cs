using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dislike.Me.Models
{
    public class userPost
    {
        public string id;
        public string message;
        public string postDate;
        public List<Like> likes;

        public userPost()
        {
            likes = new List<Like>();
        }

    }

    public class DailyPosts
    {
        public string postDate;
        public List<userPost> userPosts;
        public DailyPosts()
        {
            userPosts = new List<userPost>();
        }
    }

    public class Like
    {
        public string UserName;
        public string uid;

    }

    public class User
    {
        public string UserName;
        public string uid;
        public int likes;

    }



    public class FriendsList
    {

       public Dictionary<string, string> Friends;

        public FriendsList()
        {
            Friends = new Dictionary<string,string>();
        }

    }
}
