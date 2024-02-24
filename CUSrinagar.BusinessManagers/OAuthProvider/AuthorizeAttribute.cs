using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.Enums;
using System.Security.Cryptography;
using System;
using CUSrinagar.BusinessManagers;

namespace CUSrinagar.OAuth
{
    public class OAuthorizeAttribute : AuthorizeAttribute
    {
        private bool isForbidden = false;

        public OAuthorizeAttribute(params AppRoles[] _Roles)
        {
            Roles = _Roles.Select(x => x.ToString().Trim())?.ToSingleString(',')?.ToLower()?.Trim() ?? string.Empty;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated && !isForbidden)
            {
                base.HandleUnauthorizedRequest(filterContext);
                UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.CurrentExecutionFilePath) && HttpContext.Current.Request.RawUrl.ToLower().Contains("custudentzone"))
                    filterContext.HttpContext.Response.Redirect(urlHelper.Action("StudentZone", "Account", new { area = string.Empty, R = HttpContext.Current.Request.CurrentExecutionFilePath }), true);
                else
                    filterContext.HttpContext.Response.Redirect(urlHelper.Action("SignIn", "Account", new { area = string.Empty, R = HttpContext.Current.Request.CurrentExecutionFilePath }), true);

            }
            else
            {
                UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                filterContext.HttpContext.Response.Redirect(urlHelper.Action("Forbidden", "Account", new { area = string.Empty }), true);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                HttpCookie authCookie = httpContext.Request.Cookies[AuthCookie.Name];
                if (authCookie != null)
                {
                    return _OAuthorizes(authCookie.Value);
                }
                else
                {
                    string BDInputStream = HttpUtility.UrlDecode(new System.IO.StreamReader(httpContext.Request.InputStream)?.ReadToEnd() ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(BDInputStream))
                    {
                        string oAuthValDB = new BusinessManagers.AuthenticationManager().GetCustomerID(BDInputStream);
                        if (!string.IsNullOrWhiteSpace(oAuthValDB))
                        {
                            return _OAuthorizes(oAuthValDB);
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                new AuthenticationManager().SignOut();
            }
            catch (ArgumentException)
            {
                new AuthenticationManager().SignOut();
            }
            return false;
        }

        private bool _OAuthorizes(string oAuthVal)
        {
            try
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(oAuthVal);
                AppUserCompact serializeModel = new JavaScriptSerializer().Deserialize<AppUserCompact>(authTicket.UserData.DecryptCookieAndURLSafe());

                PrincipalProvider _AppUser = new PrincipalProvider(serializeModel, serializeModel.FullName);

                //check for security stamp is changed or not aganist DB
                if (serializeModel.UserRoles.Any(x => x == AppRoles.Student))
                {
                    if (new AuthenticationManager().LogMeOutOfOtherDevices(serializeModel.User_ID, serializeModel.SecurityStamp, serializeModel.TableSuffix))
                    {
                        new AuthenticationManager().SignOut();
                        return false;
                    }
                }
                else if (new AuthenticationManager().LogMeOutOfOtherDevices(serializeModel.User_ID, serializeModel.SecurityStamp))
                {
                    new AuthenticationManager().SignOut();
                    return false;
                }

                if (_AppUser.IsInRole(Roles))
                {
                    HttpContext.Current.User = _AppUser;
                }
                else
                {
                    isForbidden = true;
                    return false;
                }
                return true;
            }
            catch (FormatException)
            {
                new AuthenticationManager().SignOut();
                return false;
            }
        }
    }
}