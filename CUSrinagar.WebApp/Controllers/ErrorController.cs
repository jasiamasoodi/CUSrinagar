using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        #region Error-404
        [HttpGet]
        public ActionResult Error404(bool IsAjaxRequest=false)
        {
            ViewBag.IsAjaxRequest = IsAjaxRequest;
            return View();
        }
        #endregion

        #region Application Error
        [HttpGet]
        public ActionResult ApplicationError(bool IsAjaxRequest=false)
        {
            ViewBag.IsAjaxRequest = IsAjaxRequest;
            return View();
        }

        [HttpGet]
        public ActionResult CookieDisabled(bool IsAjaxRequest = false)
        {
            ViewBag.IsAjaxRequest = IsAjaxRequest;
            return View();
        }
        #endregion

        #region BlockBrowsers
        [HttpGet]
        public ActionResult BrowserBlocked()
        {
            return View();
        }
        #endregion

    }
}