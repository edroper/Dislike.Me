using Dislike.Me.Private;
using Facebook;
using System;
using System.Web.Mvc;

namespace Dislike.Me.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public ActionResult Facebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = PrivateSettings.client_id,
                client_secret = PrivateSettings.client_secret,
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email,user_friends,user_about_me,read_friendlists,read_stream" // Add other permissions as needed
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            string accessToken;

            try
            {
                dynamic result = fb.Post("oauth/access_token", new
                {
                    client_id = PrivateSettings.client_id,
                    client_secret = PrivateSettings.client_secret,
                    redirect_uri = RedirectUri.AbsoluteUri,
                    code = code
                });

                accessToken = result.access_token;
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Account");
            }

            if (accessToken == null)
            {
                return RedirectToAction("Error", "Account");
            }
            else
            {
                Session["AccessToken"] = accessToken;
            }

            return RedirectToAction("Index", "Stats");
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}