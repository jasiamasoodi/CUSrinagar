using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_CAO)]
    public class FeeStructureController : AdminController
    {
        // GET: CUSrinagarAdminPanel/FeeStructure
        public ActionResult FeeStructureList(int programme = 3, int year = 2019)
        {
            ViewBag.Prog = programme;
            ViewBag.Yaer = year;
            return View(new FeeStructureManager().GetFeeStructureAdmissionForIntegratedAndPGCourses(programme, year));
        }

        //public ActionResult FeeStructureListPartial(int programme,int year)
        //{
        //    return View(new FeeStructureManager().GetFeeStructureAdmissionForIntegratedAndPGCourses(programme,year));
        //}
    }
}