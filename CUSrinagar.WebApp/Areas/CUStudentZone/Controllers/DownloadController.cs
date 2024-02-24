using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class DownloadController : Controller
    {
        // GET: CUStudentZone/Download
        public ActionResult Download()
        {
            return View();
        }


        

    }
}