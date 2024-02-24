using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CUSrinagar.Models;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.UI;
using CUSrinagar.OAuth;
using GeneralModels;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class MigrationController : Controller
    {
        #region Migration
        //public ActionResult FormList()
        //{
        //    ViewBag.ProgrammeDDLList = Helper.GetSelectList<PrintProgramme>();
        //    return View();
        //}
        //// List all Forms  
        //public ActionResult FormListTable(Parameters parameter)
        //{
        //    List<MigrationDetail> listforms = new MigrationManager().GetMigrationFormList(parameter);
        //    return View(listforms);
        //}



        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult UpdateForm(Guid Form_ID, FormStatus AcceptReject, string REMARKSBYCOLLEGE)
        //{
        //    //string val = Request["Form_ID"].ToString();
        //    //string HDName = fc["FormId"];
        //    //string ID = Convert.ToString(Request.Params["Form_ID"]);
        //    // string  ID = Convert.ToString(Request.Forms["Form_ID"]);
        //    if (ModelState.IsValid)
        //    {
        //        if (new MigrationManager().UpdateForm(Form_ID, (int)AcceptReject, REMARKSBYCOLLEGE))
        //        {
        //            if (AcceptReject == FormStatus.Accepted)
        //            {
        //                MigrationDetail formsToDownload = new MigrationManager().GetData(Form_ID);
        //                if (formsToDownload != null)
        //                {
        //                    return RedirectToAction("FormList", "Migration");
        //                }
        //            }
        //            else
        //            {
        //                return RedirectToAction("FormList", "Migration");
        //            }
        //        }
        //    }
        //    return Content("Some error occured");
        //}
        #endregion
    }
}