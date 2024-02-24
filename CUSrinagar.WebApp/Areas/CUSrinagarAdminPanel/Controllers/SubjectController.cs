using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.SubjectList)]
    public class SubjectController : AdminController
    {
        public ActionResult Subjects(string id = null, string id1 = null, int id2 = 0)
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_SubjectTypes();
            FillViewBag_Semesters();
            return View();
        }

       
        public ActionResult SubjectList(Parameters parameter)
        {
            List<ADMSubjectMasterCompact> listSubjects = new SubjectsManager().List(parameter);
            return View(listSubjects);
        }

        public void ListCSV(Parameters parameter)
        {
            List<ADMSubjectMasterCompact> list = new SubjectsManager().List(parameter);
            var reportList = list.Select(x => new
            {
                Programe = Helper.GetEnumDescription(x.Programme),
                x.CollegeCode,
                x.CourseFullName,
                x.Semester,
                x.DepartmentFullName,
                x.SubjectFullName,
                SubjectType = Helper.GetEnumDescription<SubjectType>(x.SubjectType),
                Status = x.Status ? "Active" : "InActive",
            }).ToList();
            ExportToCSV(reportList, "Subject List-" + DateTime.Now.ToString("dd/mm/yyyy"));
        }

        public ActionResult CreateEdit(Guid? id)
        {
            ADMSubjectMaster model = null;
            if (id.HasValue)
                model = (ADMSubjectMaster)TempData["model"] ?? new SubjectsManager().Get(id.Value);
            model = model ?? new ADMSubjectMaster() { Programme = Programme.UG, Status = true, FromBatch = (short)DateTime.Now.Year, ToBatch = (short)(DateTime.Now.Year + 10) };
            GetResponseViewBags();
            SetViewBags(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ADMSubjectMaster model, bool SaveAndCreateNew = false)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = model.Subject_ID == Guid.Empty ? new SubjectsManager().Save(model) : new SubjectsManager().Update(model);
                SetResponseViewBags(response);
                if (response.IsSuccess)
                {
                    TempData["model"] = response.ResponseObject;
                    if (SaveAndCreateNew)
                        return RedirectToAction("CreateEdit", new { id = "" });
                    else
                        return RedirectToAction("CreateEdit", new { id = model.Subject_ID });
                }
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
                SetResponseViewBags(response);
            }
            GetResponseViewBags();
            SetViewBags(model);
            return View(model);
        }

        public JsonResult Delete(Guid id)
        {
            ResponseData response = new SubjectsManager().Delete(id);
            return Json(response);
        }

        public JsonResult GetCourse(Programme programme, Guid? Course_ID, short Semester, SubjectType subjectType)
        {
            var list = new SubjectsManager().Get(programme, Course_ID, Semester, subjectType) ?? new ADMSubjectMaster();
            return Json(list);
        }


        public string GetEnumText(string ColType, int Value)
        {
            string val;
            if (ColType == "SubjectType")
                val = ((Enums.SubjectType)Value).ToString().Substring(0, 1);
            else
                val = ((Enums.Programme)Value).ToString();
            return val;
        }
        public string GetSubjectNumber(Guid CourseId, int Semester)
        {
            string SubjectNumber = new SubjectsManager().GetSubjectNumber(CourseId, Semester) ?? "";
            return SubjectNumber;
        }

        public string GetSubjectNumberCount(Guid? course_Id, int semester, SubjectType subjectType, Programme programme, Guid? College_ID = null)
        {
            string SubjectNumber = new SubjectsManager().GetSubjectNumber(course_Id, semester, subjectType, programme, College_ID) ?? "";
            return SubjectNumber;
        }
        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new SubjectsManager().ChangeStatus(id);
            return msg;
        }

        void SetViewBags(ADMSubjectMaster _ADMSubjectMaster = null)
        {
            FillViewBag_Course(_ADMSubjectMaster?.Programme ?? Programme.UG);
            FillViewBag_Semesters();
            FillViewBag_SubjectTypes();
            FillViewBag_Programmes();
            FillViewBag_College();
            FillViewBag_Schools(_ADMSubjectMaster?.College_ID);
            FillViewBag_Departments();
            FillViewBag_SubjectMarksStructures();
            ViewBag.MarksIsPartOf = Helper.GetSelectList<MarksIsPartOf>();
            ViewBag.AwardModuleType = Helper.GetSelectList<AwardModuleType>();
            if (_ADMSubjectMaster != null)
            {
                var _param = new Parameters();
                _param.Filters = new List<SearchFilter>();
                _param.Filters.Add(new SearchFilter() { Column = "Semester", Value = ((short)_ADMSubjectMaster.Semester).ToString() });
                if (_ADMSubjectMaster.Programme == Programme.PG)
                    _param.Filters.Add(new SearchFilter() { Column = "Programme", Value = ((short)Programme.PG).ToString(), TableAlias = "C" });
                else
                    _param.Filters.Add(new SearchFilter() { Column = "Programme", Operator = SQLOperator.NotEqualTo, Value = ((short)Programme.PG).ToString(), TableAlias = "C" });
                _param.SortInfo = new Sort { ColumnName = "SubjectFullName" };
                _param.PageInfo = new Paging { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "SubjectFullName" };
                var list = new SubjectsManager().SubjectDDLWithDetail(_param) ?? new List<SelectListItem>();
                ViewBag.Subjects = list;
            }
        }

    }
}