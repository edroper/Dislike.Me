using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dislike.Me.Models
{
    public class Stats
    {
        public userPost MostPopularPost;
        public userPost LeastPopularPost;
        public User LeastLikedUser;
        public User MostLikedUser;
        public List<User> Top4DislikedUsers;
        public List<DailyPosts> PostsAndLikesByDay;

        public Stats()
        {
            Top4DislikedUsers = new List<User>();
            PostsAndLikesByDay = new List<DailyPosts>();
        }



        public bool populateStats(List<userPost> up, FriendsList uInfo)
        {

            var likesByFriendsStats = new Dictionary<string, int>();
            var postLikeCountStats = new Dictionary<string, int>();
            var likesByDayStats = new SortedDictionary<DateTime, int>(); // not currently in use
            var postLikesByDayStats = new SortedDictionary<DateTime, Dictionary<string, int>>();

            //loop on posts obtained from the facebook calls
            foreach (userPost post in up)
            {
                string shortdate = post.postDate;

                //only counting posts with likes
                if (post.likes.Count != 0)
                {
                    postLikeCountStats.Add(post.id, 0);

                    if (!likesByDayStats.ContainsKey(DateTime.Parse(shortdate)))
                    {
                        likesByDayStats.Add(DateTime.Parse(shortdate), 0);
                    }

                    if (!postLikesByDayStats.ContainsKey(DateTime.Parse(shortdate)))
                    {
                        postLikesByDayStats.Add(DateTime.Parse(shortdate), new Dictionary<string, int>());
                        postLikesByDayStats[DateTime.Parse(shortdate)].Add(post.id, 0);
                    }
                    else
                    {
                        Dictionary<string, int> daypost;
                        daypost = postLikesByDayStats[DateTime.Parse(shortdate)];

                        if (!daypost.ContainsKey(post.id))
                        {
                            postLikesByDayStats[DateTime.Parse(shortdate)].Add(post.id, 0);
                        }
                    }

                    foreach (Like l in post.likes)
                    {

                        postLikesByDayStats[DateTime.Parse(shortdate)][post.id] = postLikesByDayStats[DateTime.Parse(shortdate)][post.id] + 1;
                        postLikeCountStats[post.id] = postLikeCountStats[post.id] + 1;
                        likesByDayStats[DateTime.Parse(shortdate)] = likesByDayStats[DateTime.Parse(shortdate)] + 1;

                        //
                        if (!likesByFriendsStats.ContainsKey(l.uid))
                        {
                            likesByFriendsStats.Add(l.uid, 1);
                        }
                        else
                        {
                            likesByFriendsStats[l.uid] = likesByFriendsStats[l.uid] + 1;
                        }

                    }

                }
            }

            //find most popular post based on pre-calculated counts
            var items = (from pair in postLikeCountStats
                         orderby pair.Value descending
                         select pair).Take(1);

            foreach (KeyValuePair<string, int> pair in items)
            {
                MostPopularPost = up.Find(x => x.id == pair.Key);
            }


            //find first instance least popular post. Not very scientific since there couple be multiple
            //posts with only 1 like, but whatever.
            items = (from pair in postLikeCountStats
                     orderby pair.Value ascending
                     select pair).Take(1);

            foreach (KeyValuePair<string, int> pair in items)
            {
                LeastPopularPost = up.Find(x => x.id == pair.Key);
            }

            //find which friend has liked your posts the most
            items = (from pair in likesByFriendsStats
                     orderby pair.Value descending
                     select pair).Take(1);

            foreach (KeyValuePair<string, int> pair in items)
            {
                User usr = new User();
                usr.uid = pair.Key;
                usr.UserName = uInfo.Friends[pair.Key];
                usr.likes = pair.Value;
                MostLikedUser = usr;
            }

            //find which user has liked your posts the least ( based off of who has given you at least more than 1 link, to try to rule out transient likes )
            // also not scientific since there could be multiple people who only ever like one or two posts. The more likes a user gets overall, the more accurate
            items = (from pair in likesByFriendsStats
                     where pair.Value > 1
                     orderby pair.Value ascending
                     select pair).Take(5);

            int iteration = 0;
            foreach (KeyValuePair<string, int> pair in items)
            {
                User usr = new User();
                usr.uid = pair.Key;
                usr.UserName = uInfo.Friends[pair.Key];
                usr.likes = pair.Value;
                iteration += 1;

                if (iteration == 1)
                {
                    LeastLikedUser = usr;
                    continue;
                }
                Top4DislikedUsers.Add(usr);
            }




            //create a sorted list of likes from last year in order by date, with likes per day, and objects containing the post data itself
            var list = postLikesByDayStats.Keys.ToList();

            foreach (var key in postLikesByDayStats.Keys)
            {

                DailyPosts dp = new DailyPosts();
                dp.postDate = key.ToShortDateString();

                foreach (KeyValuePair<string, int> pair in postLikesByDayStats[key])
                {
                    userPost _post = new userPost();
                    _post = up.Find(x => x.id == pair.Key);
                    dp.userPosts.Add(_post);
                }

                PostsAndLikesByDay.Add(dp);
            }

            return true;
        }

    }
}