using CUSrinagar.BusinessManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CUSrinagar.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
#if (!DEBUG)
            BundleTable.EnableOptimizations = true;
#endif
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //app security
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_Error()
        {

#if (!DEBUG)

            Exception ex = Server.GetLastError();
            var StatusCode = ex as HttpException;
            Server.ClearError();
            var isAjaxRequest = new HttpRequestWrapper(Request).IsAjaxRequest();

            if (StatusCode != null && StatusCode.GetHttpCode() == 404)
            {
                Response.Redirect("~/Error/Error404?IsAjaxRequest=" + isAjaxRequest);
            }
            else if (ex.Message.ToLower().Contains("cookie"))
            {
                Response.Redirect("~/Error/CookieDisabled?IsAjaxRequest=" + isAjaxRequest);
            }
            else if (ex is HttpRequestValidationException || ex is HttpException)
            {
                Response.Redirect("~/Error/ApplicationError?IsAjaxRequest=" + isAjaxRequest);
            }
            else if (ex is HttpAntiForgeryException)
            {
                Response.Redirect("~/Error/Error404?IsAjaxRequest=" + isAjaxRequest);
            }
            else
            {

                try
                {
                    if (ex != null)
                        new EmailSystem().LogErrorMail(ex);
                    Response.Redirect("~/Error/ApplicationError?IsAjaxRequest=" + isAjaxRequest);
                }
                catch (Exception)
                {
                    Response.Redirect("~/Error/ApplicationError?IsAjaxRequest=" + isAjaxRequest);
                }
            }
#endif

        }
        protected void Application_PreSendRequestHeaders()
        {
            //app security
            Response.Headers.Set("Server", "CUSrinagar");
            Response.Headers.Set("X-Frame-Options", "SAMEORIGIN");
            Response.Headers.Set("X-Content-Type-Options", "nosniff");
            Response.Headers.Set("Strict-Transport-Security", "max-age=10886400; includeSubDomains");
            Response.Headers.Set("Permissions-Policy", "(\"https://cusrinagar.edu.in\" \"https://*.cusrinagar.edu.in\")");
        }
    }
}
