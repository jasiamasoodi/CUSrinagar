using CUSrinagar.BusinessManagers;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_OfficeAssistant)]
    public class ReEvaluationSettingController : AdminController
    {
        public ActionResult ReEvaluationSetting()
        {
            GetResponseViewBags();
            FillViewBag_ExaminationCourseCategories();
            return View();
        }
        public ActionResult ReEvaluationSettingList(Parameters parameter)
        {
            var list = new ReEvaluationSettingManager().List(parameter);
            return View(list);
        }

        [HttpGet]
        public ActionResult CreateEdit(Guid? id)
        {
            ReEvaluationSetting model = null;
            if (id.HasValue && id.Value != Guid.Empty)
                model = (ReEvaluationSetting)TempData["model"] ?? new ReEvaluationSettingManager().Get(id.Value);
            else
                model = (ReEvaluationSetting)TempData["model"] ?? new ReEvaluationSetting() { Semester = 1, CourseCategory = ExaminationCourseCategory.UG, FormNumberPrefix = "-XRX-", SubmittedYear = (short)DateTime.Now.Year, FeePerSubject = 100, StartDate = DateTime.Now, AllowDownloadForm = true, EndDate = DateTime.Now.AddDays(5) };
            GetResponseViewBags();
            FillViewBag_ExaminationCourseCategories();
            Parameters parameters = null;
            if (model == null)
                parameters = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = "1" }, new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme.UG).ToString() } }, SortInfo = new Sort() { ColumnName = "Dated", OrderBy = SortOrder.Descending }, PageInfo = new Paging() { DefaultOrderByColumn = "Dated", PageNumber = -1, PageSize = -1 } };
            else
                parameters = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = model.Semester.ToString() }, new SearchFilter() { Column = "PrintProgramme", Value = ((short)new GeneralFunctions().GetPrintProgrammeForExaminationCourseCategory(model.CourseCategory)).ToString() } }, SortInfo = new Sort() { ColumnName = "Dated", OrderBy = SortOrder.Descending }, PageInfo = new Paging() { DefaultOrderByColumn = "Dated", PageNumber = -1, PageSize = -1 } };
            FillViewBag_ResultNotifications(parameters);
            FillViewBag_Semesters();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ReEvaluationSetting model, bool? CreateNew, bool? SaveAsNew)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                if (SaveAsNew.HasValue && SaveAsNew.Value)
                {
                    model.ReEvaluationSetting_ID = Guid.Empty;
                }
                response = model.ReEvaluationSetting_ID == Guid.Empty ? new ReEvaluationSettingManager().Save(model) : new ReEvaluationSettingManager().Update(model);
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());

            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                if (CreateNew.HasValue && CreateNew.Value)
                    return RedirectToAction("CreateEdit", new { @id = "" });
                else
                    TempData["model"] = (ReEvaluationSetting)response.ResponseObject;
            }
            else
                TempData["model"] = model;
            return RedirectToAction("CreateEdit", new { @id = model.ReEvaluationSetting_ID });
        }

        public ActionResult Delete(Guid id)
        {
            ResponseData response = new ReEvaluationSettingManager().Delete(id);
            if (!response.IsSuccess)
                return RedirectToAction("CreateEdit", new { id });

            SetResponseViewBags(response);
            return RedirectToAction("ReEvaluationSetting");

        }

        public JsonResult ChangeStatus(Guid id)
        {
            ResponseData response = new ReEvaluationSettingManager().ChangeStatus(id);
            return Json(response);
        }
    }
}
