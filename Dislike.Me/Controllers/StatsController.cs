using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dislike.Me.Models;
using System.Web.Caching;
using Dislike.Me.Common;


namespace Dislike.Me.Controllers
{
    public class StatsController : Controller
    {

        private static IDictionary<Guid, string> tasks = new Dictionary<Guid, string>();

        //
        // GET: /Stats/

        public ActionResult Index()
        {
          

            if (string.IsNullOrEmpty(Session["AccessToken"] as string))
            {
                return RedirectToAction("Error", "Stats");
            }

            return View();
        }

        public ActionResult GetData()
        {
            var taskId = Guid.NewGuid();
            tasks.Add(taskId, "Starting...");

            if (string.IsNullOrEmpty(Session["AccessToken"] as string))
            {
                return RedirectToAction("Error", "Stats");
            }

            string AccessToken = Session["AccessToken"].ToString();

            Task.Factory.StartNew(() =>
            {
                
                List<userPost> userposts = new List<userPost>();
                FriendsList friends = new FriendsList();

                GetFacebookData(taskId, AccessToken, userposts, friends);

                tasks[taskId] = "Generating Statistics..";
                
                //generate our statistics
                Stats stats = new Stats();
                stats.populateStats(userposts, friends);

                try
                {
                    //insert stats object into cache for later retrieval, since it's being created by a stateless / contextless thread
                    //AccessToken is unique per user, so no risk of a different user getting it.
                    HttpRuntime.Cache.Insert(AccessToken, stats, null, DateTime.UtcNow.AddMinutes(5), Cache.NoSlidingExpiration);
                }
                catch (Exception)
                {
                    
                    
                }
                
                tasks.Remove(taskId);
            });

            return Json(taskId, JsonRequestBehavior.AllowGet);
        }

        private static void GetFacebookData(Guid taskId, string AccessToken, List<userPost> up, FriendsList friends)
        {
            bool pullingData = true;
            bool hasData = false;


            string jsonURL = @"https://graph.facebook.com/me/posts/?limit=50&access_token=" + AccessToken;

            //keep looping while 'next' links are present, or until empty or an error
            while (pullingData)
            {

                string jsonResult = Utilities.GetJSONFromWeb(jsonURL);
                dynamic dynObj = JsonConvert.DeserializeObject(jsonResult);

                tasks[taskId] = "Getting more data...";

                //catch RuntimeBinderException if the data returned is invalid or empty
                try
                {
                    foreach (var data in dynObj.data)
                    {
                        hasData = true;
                        //only pulling status updates that are friends only, a very rough way to ensure people who arent friends dont count
                        //and avoid 'transient' likes in public statuses from strangers
                        if (data.type == "status" && data.privacy.value == "ALL_FRIENDS")
                        {
                            userPost p = new userPost();
                            p.id = data.id;
                            p.message = data.message;
                            string shortdate = data.created_time;

                            //if (data.created_time < DateTime.Now.AddYears(-1))
                            if (data.created_time < DateTime.Now.AddDays(-365))
                            {
                                pullingData = false;
                            }

                            p.postDate = DateTime.Parse(shortdate).ToShortDateString();

                            tasks[taskId] = "Pulling Data From " + p.postDate;
                            if (data.likes != null)
                            {
                                foreach (var likes in data.likes.data)
                                {

                                    Like _like = new Like();
                                    _like.uid = likes.id;
                                    _like.UserName = likes.name;
                                    p.likes.Add(_like);

                                    friends.Friends[_like.uid] = _like.UserName;

                                }
                            }
                            up.Add(p);
                        }
                    }
                }
                catch (RuntimeBinderException)
                {
                    pullingData = false;
                }

                //see if we have any next links, if not, we're at the end probably
                try
                {
                    if (dynObj.paging.next != null)
                    {
                        jsonURL = dynObj.paging.next;
                    }
                }
                catch (RuntimeBinderException)
                {
                    pullingData = false;
                }

                dynObj = null;
            }
        }


        public ActionResult Progress(Guid id)
        {
            return Json(tasks.Keys.Contains(id) ? tasks[id] : "Done!");
        }

        public ActionResult ShowResults()
        {
            if (string.IsNullOrEmpty(Session["AccessToken"] as string))
            {
                return RedirectToAction("Error", "Stats");
            }


            //try to retrieve stats object from cache. if its null then something bad happened.
            Stats stats; 
            var tempstats = HttpRuntime.Cache.Get(Session["AccessToken"].ToString());
            if(tempstats!=null)
            {
                stats = (Stats)tempstats;

            }
            else
            {
                 return RedirectToAction("Error", "Stats");
            }

            
            return View(stats);

        }

        public ActionResult Error()
        {
            return View();
        }



    }


}
