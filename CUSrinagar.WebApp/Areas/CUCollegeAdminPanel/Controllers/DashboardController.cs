using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            new CombinationManager().CloseCombinationSettingsLink();
            return View();
        }
    }
}