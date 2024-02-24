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
using CUSrinagar.DataManagers;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class MigrationController : Controller
    {
        #region Migration
        //public ActionResult FormList()
        //{
        //    ViewBag.ProgrammeDDLList = Helper.GetSelectList<PrintProgramme>();
        //    ViewBag.MigrationEDDLList = Helper.GetSelectList<MigrationE>();
        //    return View();
        //}
        //// List all Forms  
        //public ActionResult FormListTable(Parameters parameter)
        //{
        //    List<MigrationDetail> listforms = new MigrationManager().GetALLMigrationFormList(parameter);
        //    return View(listforms);
        //}
        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public ActionResult UpdateForm(Guid Form_ID, FormStatus AcceptReject, string REMARKSBYCOLLEGE)
        //{
     
        //    if (ModelState.IsValid)
        //    {
        //        if (new MigrationManager().UpdateForm(Form_ID, (int)AcceptReject, REMARKSBYCOLLEGE))
        //        {
        //            if (AcceptReject == FormStatus.Accepted)
        //            {
        //                MigrationDetail formsToDownload = new MigrationManager().GetData(Form_ID);
        //                if (formsToDownload != null)
        //                {
        //                    UpdateStudentStatus(formsToDownload);
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

        //private void UpdateStudentStatus(MigrationDetail student)
        //{
        //    PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(student.Programme);

        //    if (student.FormType == MigrationE.CancelRegistration)
        //    {
        //        new StudentManager().CancelAdmission(printProgramme, student.Student_ID);
        //    }
        //    else if (student.FormType == MigrationE.IntraCollegeMigration)
        //    {
        //        new StudentManager().IntraMigration(printProgramme, student.Student_ID,Guid.Parse(student.NewCollege));
        //    }
        //    else if (student.FormType == MigrationE.PassoutMigration)
        //    {
        //        new StudentManager().InterMigration(printProgramme, student.Student_ID);
        //    }
        //}


        #endregion
    }
}