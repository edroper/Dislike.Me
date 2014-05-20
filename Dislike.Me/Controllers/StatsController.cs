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

        //status container for the spawned threads. contains a unique guid and a message indicating current process
        //while polling facebook graph api over and over.
        private static IDictionary<Guid, string> tasks = new Dictionary<Guid, string>();

  
        //forwarded to after successful facebook login
        //present a jquery script which then calls GetData and monitors the loop
        public ActionResult Index()
        {
          

            if (string.IsNullOrEmpty(Session["AccessToken"] as string))
            {
                return RedirectToAction("Error", "Stats");
            }

            return View();
        }


        //called by jQuery
        public ActionResult GetData()
        {
            var taskId = Guid.NewGuid();
            tasks.Add(taskId, "Starting...");

            //something bad happened, abort
            if (string.IsNullOrEmpty(Session["AccessToken"] as string))
            {
                return RedirectToAction("Error", "Stats");
            }

            //assign our session to a variable, since session state doesnt exist in spawned threads
            string AccessToken = Session["AccessToken"].ToString();

            Task.Factory.StartNew(() =>
            {
                
                //our raw list of posts to be summarized soon
                List<userPost> userposts = new List<userPost>();

                //facebook is dumb, and won't let you grab a users entire friends list anymore, so we gotta build our own and assume
                //friendship
                FriendsList friends = new FriendsList();

                //go get the data
                GetFacebookData(taskId, AccessToken, userposts, friends);

                tasks[taskId] = "Generating Statistics..";
                
                //generate our statistics
                Stats stats = new Stats();
                stats.populateStats(userposts, friends);

                try
                {
                    //insert stats object into cache for later retrieval, since it's being created by a stateless / contextless thread
                    //AccessToken is unique per user, so no risk of a different user getting it.
                    //this could probably be replace with SQL/ORM, but benchmarking should be done to see if its any better or worse.
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

            //using raw JSON, since Facebook Client SDK doesn't handle pagination. Probably quicker to use JSON.net anyway.
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
                    //probably a result of no more data
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
                 
                    //we probably hit the end of the stream for X number of days of data, so no paging links are
                    //returned. or something bad happened
                    pullingData = false;
                }

                dynObj = null;
            }
        }


        //called by jQuery to see where the processing is at
        public ActionResult Progress(Guid id)
        {
            return Json(tasks.Keys.Contains(id) ? tasks[id] : "Done!");
        }

        //our collection and analysis is done, grab the object out of cache and present the results.
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
