using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.Models.ValidationAttrs;
using CUSrinagar.OAuth;
using CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class ExamFormSettingController : AdminController
    {
        public ActionResult ExaminationSetting()
        {
            ViewBag.Notifications = new NotificationManager().GetAllNotificationList(new Parameters()
            {
                Filters = new List<SearchFilter>()
                {
                    new SearchFilter()
                {
                    Column=nameof(Notification.NotificationType),
                    Operator =SQLOperator.EqualTo,
                    Value =((int)NotificationType.Examination).ToString()
                },
                     new SearchFilter()
                {
                    Column=nameof(Notification.Status),
                    Operator =SQLOperator.EqualTo,
                    Value = "1"
                }

                },
                SortInfo = new Sort()
                {
                    ColumnName = nameof(Notification.CreatedOn),
                    OrderBy = System.Data.SqlClient.SortOrder.Descending
                }
            }).Take(50).ToList();


            return View();
        }
        public ActionResult ExaminationSettingList(Parameters parameter)
        {
            var list = new ExaminationSettingManager().GetExaminationSettings(parameter);
            return View(list);
        }

        [HttpPost]
        public JsonResult UpdateNotificationParam(ExaminationSetting model)
        {
            ResponseData response = new ResponseData();
            if (model == null || model?.RegularBatch < 2016)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Regular Batch is invalid";
                return Json(response);
            }
            response = new ExaminationSettingManager().Update(model);
            return Json(response);
        }



        [HttpPost]
        public JsonResult AllowExamFormApplications(DateTime StartDate, DateTime EndDate, Guid Notification_ID, Guid ARGExamForm_ID)
        {
            return Json(new ExaminationSettingManager().AllowExamFormApplications(StartDate, EndDate, Notification_ID, ARGExamForm_ID));
        }

        public ActionResult Create()
        {
            GetResponseViewBags();
            FillViewBag_Semesters();
            FillViewBags();
            ExaminationSetting model = new ExaminationSetting() { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(10) };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExaminationSetting model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = new ExaminationSettingManager().Save(model);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                model = (ExaminationSetting)response.ResponseObject;
                TempData["model"] = model;
                return RedirectToAction("Edit", new { @id = model.ARGExamForm_ID });
            }
            else
            {
                TempData["model"] = model;
                FillViewBags();
                return RedirectToAction("Create");
            }
        }

        public ActionResult Edit(Guid id)
        {
            var model = (ExaminationSetting)TempData["model"] ?? new ExaminationSettingManager().GetExaminationSetting(id);
            GetResponseViewBags();
            FillViewBags();
            FillViewBag_Semesters();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExaminationSetting model)
        {
            FillViewBags();
            FillViewBag_Semesters();
            ResponseData response = new ResponseData();
            SetResponseViewBags(response);
            if (ModelState.IsValid)
            {
                response = new ExaminationSettingManager().Update(model);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                model = (ExaminationSetting)response.ResponseObject;
                TempData["model"] = model;
                return RedirectToAction("Edit", new { @id = model.ARGExamForm_ID });
            }
            else
            {
                TempData["model"] = model;
                return RedirectToAction("Edit", new { @id = model.ARGExamForm_ID });
            }
        }

        public ActionResult Delete(Guid id)
        {
            var response = new ExaminationSettingManager().Delete(id);
            return Json(response);
        }


        private void FillViewBags()
        {
            ViewBag.CourseCategoryList = Helper.GetSelectList<ExaminationCourseCategory>().OrderBy(x => x.Value);
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);

            ViewBag.SemesterList = Helper.GetSelectList<Semester>().OrderBy(x => x.Value);
            ViewBag.FeeStructure = new ExaminationSettingManager().GetExamFeeStructure();
        }

        [HttpGet]
        public ActionResult ExaminationForms()
        {
            IEnumerable<SelectListItem> ProgrammeDDL = Helper.GetSelectList<PrintProgramme>();

            var Properties = typeof(StudentExamFormList).GetProperties();

            List<SelectListItem> Columns = new List<SelectListItem>();

            foreach (var Property in Properties)
            {
                SelectListItem item = new SelectListItem()
                {
                    Text = Property.Name
                };

                var DbColumnAttribute = Property.GetCustomAttributes(typeof(DBColumnNameAttribute), false).Cast<DBColumnNameAttribute>().FirstOrDefault();

                item.Value = DbColumnAttribute == null ? Property.Name : DbColumnAttribute.name;

                if (Property.CustomAttributes.Any(x => x.AttributeType == typeof(DescriptionAttribute)))
                {
                    item.Value = Property.CustomAttributes
                        .First(x => x.AttributeType == typeof(DescriptionAttribute))
                        .ConstructorArguments.First().Value + item.Value;
                }

                Columns.Add(item);
            }

            Columns.RemoveAll(i => i.Text.Contains("ID"));

            int ProgrammeId = Convert.ToInt32(ProgrammeDDL.FirstOrDefault().Value);
            List<SelectListItem> CourseDDL = new CourseManager().GetAllCoursesByProgramme(ProgrammeId);

            ViewBag.ProgrammeDDLList = ProgrammeDDL == null ? new List<SelectListItem>() : ProgrammeDDL;
            ViewBag.CourseDDLList = CourseDDL == null ? new List<SelectListItem>() : CourseDDL;
            ViewBag.Columns = Columns == null ? new List<SelectListItem>() : Columns;

            ViewBag.PrintProgrammeDropDownList = Helper.GetSelectList<PrintProgramme>();
            return View();
        }

        [HttpGet]
        public ActionResult ExaminationFormsList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExaminationFormsList(Parameters parameter)
        {
            PrintProgramme printProgramme = PrintProgramme.UG;
            var programmeFilter = parameter.Filters.FirstOrDefault(i => i.Column.ToUpper() == "PROGRAMME");

            if (programmeFilter != null)
            {
                printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), programmeFilter.Value);
            }

            var ColumnsFilter = parameter.Filters.FirstOrDefault(i => i.Column.ToUpper() == "COLUMNS");
            string Columns = null;

            if (ColumnsFilter != null)
                Columns = ColumnsFilter.Value;
            ViewBag.Columns = Columns;

            return View(new ExaminationFormManager().GetExaminationForms(parameter, printProgramme, Columns));
        }

    }

}