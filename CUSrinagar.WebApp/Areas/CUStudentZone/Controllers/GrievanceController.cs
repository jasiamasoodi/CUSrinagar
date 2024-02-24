using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeneralModels;
using CaptchaMvc.HtmlHelpers;
using CUSrinagar.Extensions;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class GrievanceController : StudentBaseController
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        public ActionResult Grievance()
        {
            FillViewBags();
            return View();
        }
        public ActionResult GrievanceList(Parameters parameter)
        {
            if (parameter == null)
                parameter = new Parameters();
            if (parameter.Filters.IsNullOrEmpty())
                parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "Student_ID", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, Value = AppUserHelper.User_ID.ToString() });
            parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };

            List<GrievanceList> list = new GrievanceManager().GetGrievanceListCompact(parameter);
            return PartialView(list);
        }
        public ActionResult GrievanceForm()
        {
            Grievance model = (Grievance)TempData["model"] ?? new Grievance();
            FillViewBags();
            GetResponseViewBags();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GrievanceForm(Grievance model)
        {
            ResponseData response = new ResponseData();
            if (!(this?.IsCaptchaValid("") ?? true))
            {
                TempData["ErrorMessage"] = "Invalid Captcha";
                TempData["model"] = model;
                return RedirectToAction("GrievanceForm");
            }
            if (ModelState.IsValid)
            {
                model.Date = DateTime.Now;
                model.Student_ID = AppUserHelper.User_ID;
                response = new GrievanceManager().SaveGrievance(model, true);
                TempData["model"] = response.ResponseObject;
            }
            else
            {
                TempData["model"] = model;
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                //string invalidDataErrorMessage = ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors).Aggregate("", (current, error) => current + error.ErrorMessage);
            }
            SetResponseViewBags(response);
            if (!response.IsSuccess)
            {
                TempData["model"] = model;
                return RedirectToAction("GrievanceForm");
            }
            TempData["model"] = response.ResponseObject;
            return RedirectToAction("OTPConfirmation", new { @id = model.Grievance_ID });
        }

        [HttpGet]
        public ActionResult OTPConfirmation(string id)
        {
            Guid Grievance_ID;
            Guid.TryParse(id, out Grievance_ID);
            if (Grievance_ID != Guid.Empty)
            {
                var grievance = (Grievance)TempData["model"];
                if (grievance == null)
                    grievance = new GrievanceManager().GetGrievance(Grievance_ID);
                ViewData.Model = grievance;
                GetResponseViewBags();
                return View();
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OTPConfirmation(Guid? Grievance_ID, string OTP)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "Invalid request" };
            if (Grievance_ID.HasValue && !string.IsNullOrEmpty(OTP))
            {
                response = new GrievanceManager().VerifyGrievance(Grievance_ID.Value, OTP);
                SetResponseViewBags(response);
                if (response.IsSuccess)
                {
                    var GrievanceID = ((Grievance)response.ResponseObject).GrievanceID.ToUpper();
                    return RedirectToAction("GrievanceConfirmation", new { @GrievanceID = GrievanceID, @IsSuccess = true });
                }
                else
                {
                    return RedirectToAction("OTPConfirmation", new { @id = Grievance_ID });
                }
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }
        public ActionResult GrievanceConfirmation(string GrievanceID, bool IsSuccess)
        {
            if (!string.IsNullOrEmpty(GrievanceID))
            {
                ViewBag.GrievanceID = GrievanceID;
                ViewBag.IsVerified = IsSuccess;
                GetResponseViewBags();
                return View();
            }
            else
            {
                return RedirectToAction("Grievance");
            }
        }

        public ActionResult Detail(Guid? id)
        {
            if (id.HasValue)
            {
                GetResponseViewBags();
                ViewData.Model = new GrievanceManager().GetGrievance(id.Value);
            }
            return View();
        }
        public ActionResult PostGrievanceReply(GrievanceReply model)
        {
            ResponseData response = new ResponseData();
            model.Date = DateTime.Now;
            response = new GrievanceManager().SaveGrievanceReply(model);
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {//send mail to cadidate                
                var gr = new GrievanceManager().GetGrievance(model.Grievance_ID);
                //MailGrievanceReply(gr, model);
            }
            return RedirectToAction("Detail", "Grievance", new { @id = model.Grievance_ID });
        }

        #region HelperMethods
        void FillViewBags()
        {
            ViewBag.GrievanceCategory = Helper.GetSelectList<GrievanceCategory>();
            ViewBag.GrievanceStatus = Helper.GetSelectList<GrievanceStatus>();
            // ViewBag.GrievanceStatus = Helper.GetSelectList<GrievanceStatus>().OrderBy(x => x.Text);
            // ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
        }
        #endregion


        #region alumni 
        [HttpGet]
        public ActionResult AddAlumni()
        {
            ViewBag.EmployementStatus = Helper.AlominiEmploymentStatusDDL();            
            return View(new GrievanceManager().GetAlomini(AppUserHelper.User_ID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAlumni(Alomini alomini)
        {
            ViewBag.EmployementStatus = Helper.AlominiEmploymentStatusDDL();
            ModelState.Remove(nameof(alomini.LastSavedOn));
            if (!Helper.AlominiEmploymentStatusDDL().Any(x => x.Value == alomini.EmploymentStatus))
            {
                alomini = null;
            }
            if (!ModelState.IsValid)
            return View();

            Tuple<bool, string> result = new GrievanceManager().SaveUpdateAlomini(alomini);
            TempData["response"] = errorMsg.Replace("##", result.Item2).Replace(result.Item1 ? "alert-danger" : "alert-danger", result.Item1 ? "alert-success" : "alert-danger");
            return RedirectToAction("AddAlumni");
        }
        #endregion
    }
}