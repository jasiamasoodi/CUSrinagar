using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI;
using System.Data;


namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class CollegeClassController : Controller
    {
        private void ViewBags()
        {
            ViewBag.PrintProgamme = Helper.GetSelectList<PrintProgramme>().OrderBy(x=>x.Value);
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AssignClassRollNo assignClassRollNo)
        {
            ViewBags();
            if (!ModelState.IsValid)
                return View(assignClassRollNo);

            Tuple<int, string> response = new AssignClassRollNoManager().Update(assignClassRollNo);
            if (response.Item1 <= 0)
            {
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Error ! </strong> {response.Item2}<br></div><div class='col-sm-1'></div>";
            }
            else
            {
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome! </strong> Updated Successfully<br></div><div class='col-sm-1'></div>";
            }
            return View(new AssignClassRollNo());
        }
    }
}