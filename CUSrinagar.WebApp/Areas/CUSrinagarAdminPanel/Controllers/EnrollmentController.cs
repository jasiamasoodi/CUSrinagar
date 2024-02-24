using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using IronXL;
using System.Data;
using System.Threading.Tasks;
using CUSrinagar.DataManagers;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.Enrollment)]
    public class EnrollmentController : AdminController
    {
        // GET: CUSrinagarAdminPanel/Enrollment
        public ActionResult Index()
        {
            FillViewBag_College();
            return View();
        }
        public PartialViewResult EnrollmentList(Parameters parameter)
        {
            List<Enrollments> list  =new StudentManager().GetEnrollment(parameter);
            return PartialView(list);
        }
    }
}