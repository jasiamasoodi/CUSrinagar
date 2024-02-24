using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_OfficeAssistant)]
    public class CombinationSettingController : AdminController
    {
        #region CombinationSetting
        public ActionResult CombinationSetting()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            return View();
        }
        public ActionResult CombinationSettingList(Parameters parameter)
        {
            var list = new CombinationSettingManager().GetCombinationSettings(parameter);
            return View(list);
        }

        public JsonResult ToggleCollegeChangeCombination(List<Guid> CombinationSetting_IDs)
        {
            ResponseData response = new CombinationSettingManager().ToggleCollegeChangeCombination(CombinationSetting_IDs);
            return Json(response);
        }

        public ActionResult CreateEdit(Guid? id)
        {
            CombinationSetting model = null;
            if (id.HasValue && id.Value != Guid.Empty)
                model = (CombinationSetting)TempData["model"] ?? new CombinationSettingManager().GetCombinationSetting(id.Value);
            else
                model = (CombinationSetting)TempData["model"] ?? new CombinationSetting() { Batch = 2017, Semester = 5 };
            GetResponseViewBags();
            FillViewBags(model.Course_ID, model.Semester);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(CombinationSetting model, bool? CreateNew, bool? SaveAsNew)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                if (SaveAsNew.HasValue && SaveAsNew.Value)
                {
                    model.CombinationSetting_ID = Guid.Empty;
                    model.UpdatedBy = null;
                    model.UpdatedOn = null;

                }
                response = model.CombinationSetting_ID == Guid.Empty ? new CombinationSettingManager().Save(model) : new CombinationSettingManager().Update(model);
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());

            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                if (CreateNew.HasValue && CreateNew.Value)
                    return RedirectToAction("CreateEdit", new { @id = "" });
                else
                    TempData["model"] = (CombinationSetting)response.ResponseObject;
            }
            else
                TempData["model"] = model;
            return RedirectToAction("CreateEdit", new { @id = model.CombinationSetting_ID });
        }
        public ActionResult Delete(Guid id)
        {
            var response = new CombinationSettingManager().Delete(id);
            return Json(response);
        }

        #endregion

        #region SubjectCombinationSetting
        public ActionResult SubjectCombinationSetting()
        {
            //FillViewBag_Batches();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            return View();
        }
        public ActionResult SubjectCombinationSettingList(Parameters parameter)
        {
            var list = new CombinationSettingManager().GetSubjectCombinationSettings(parameter);
            return View(list);
        }
        public void SubjectSettingCSV(Parameters parameter)
        {
            parameter.SortInfo = new Sort() { ColumnName = "CourseFullName,BaseSemester,BaseSubjectFullName" };
            var listResult = new CombinationSettingManager().GetSubjectCombinationSettings(parameter)?.Where(x => x.DependentSubjectFullName != null)?.ToList() ?? new List<SubjectCombinationSetting>();
            var list = listResult.Select(
                col => new
                {
                    col.CourseFullName,
                    col.BaseSemester,
                    col.BaseSubjectFullName,
                    col.ForSemester,
                    col.DependentSubjectFullName
                }).ToList();
            ExportToCSV(list, $"Subject Setting ResultReport");
        }

        public ActionResult CreateEditSubjectCombinationSetting(Guid? id)
        {
            List<SubjectCombinationSetting> list = null;
            if (id.HasValue)
                TempData["model"] = new CombinationSettingManager().SubjectCombinationSettingList(id.Value);
            list = (List<SubjectCombinationSetting>)TempData["model"] ?? new List<SubjectCombinationSetting>() { new SubjectCombinationSetting() { BaseSemester = 4, IsActive = true, Dated = DateTime.Now, ForSemester = 6 } };
            GetResponseViewBags();
            FillViewBags();
            if (list.IsNotNullOrEmpty())
            {
                foreach (var item in list)
                {
                    var param = new Parameters()
                    {
                        PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
                        Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", Operator = SQLOperator.EqualTo, Value = item.Course_ID.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = (!item.DependentSubject_ID.HasValue ? item.BaseSemester : item.ForSemester).ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Status", Operator = SQLOperator.EqualTo, Value = "1", TableAlias = "S" } }
                    };
                    ViewData[item._ID.ToString()] = new SubjectsManager().SubjectDDLWithDetail(param);
                }
            }
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEditSubjectCombinationSetting(SubjectCombinationSetting baseSubject)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = baseSubject._ID == Guid.Empty ? new CombinationSettingManager().Save(baseSubject) : new CombinationSettingManager().Update(baseSubject);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                TempData["model"] = response.ResponseObject;
                return RedirectToAction("CreateEditSubjectCombinationSetting", new { @id = baseSubject._ID });
            }
            else
            {
                TempData["model"] = baseSubject;
                return RedirectToAction("CreateEditSubjectCombinationSetting", new { @id = baseSubject._ID });
            }
        }

        [HttpPost]
        public ActionResult SubjectSettingRapidEntry(List<SubjectCombinationSetting> model, Guid MainBaseItem_ID)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = new CombinationSettingManager().SaveUpdate(model, MainBaseItem_ID);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors)?.Select(y => y.ErrorMessage)?.ToArray());
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                model = (List<SubjectCombinationSetting>)response.ResponseObject;
                TempData["model"] = model;
                return RedirectToAction("CreateEditSubjectCombinationSetting", new { @id = MainBaseItem_ID });
            }
            else
            {
                return RedirectToAction("CreateEditSubjectCombinationSetting", new { @id = MainBaseItem_ID });
            }
        }

        public ActionResult DeleteBaseSubjectCombinationSetting(Guid id)
        {
            var response = new CombinationSettingManager().DeleteBaseSubjectCombinationSetting(id);
            return Json(response);
        }

        public ActionResult DeleteSubjectCombinationSetting(Guid id, Guid MainBaseItem_ID)
        {
            var response = new CombinationSettingManager().DeleteSubjectCombinationSetting(id);
            SetResponseViewBags(response);
            return RedirectToAction("CreateEditSubjectCombinationSetting", new { @id = MainBaseItem_ID });
        }



        #endregion

        #region CombinationSettingStructure

        public ActionResult CombinationSettingStructure()
        {
            return View();
        }
        public ActionResult CombinationSettingStructureList(Parameters parameter)
        {
            var list = new CombinationSettingManager().GetCombinationSettingStructureList(parameter);
            return View(list);
        }

        public ActionResult CombinationSettingStructureCreateEdit(Guid? id)
        {
            CombinationSettingStructure model = null;
            if (id.HasValue && id.Value != Guid.Empty)
                model = (CombinationSettingStructure)TempData["model"] ?? new CombinationSettingManager().GetCombinationSettingStructure(id.Value);
            else
                model = (CombinationSettingStructure)TempData["model"] ?? new CombinationSettingStructure() { };
            GetResponseViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CombinationSettingStructureCreateEdit(CombinationSettingStructure model)
        {
            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
                response = model.CombinationSettingStructure_ID == Guid.Empty ? new CombinationSettingManager().Save(model) : new CombinationSettingManager().Update(model);
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            SetResponseViewBags(response);
            if (response.IsSuccess)
                TempData["model"] = (CombinationSettingStructure)response.ResponseObject;
            else
                TempData["model"] = model;
            return RedirectToAction("CombinationSettingStructureCreateEdit", new { @id = model.CombinationSettingStructure_ID });
        }

        #endregion


        private void FillViewBags(Guid? course_id = null, short? semester = null)
        {
            FillViewBag_Batches();
            FillViewBag_CombinationSettingStructure(null);
            ViewBag.Courses = new CourseManager().GetAllCourseList();
            ViewBag.Semesters = new List<SelectListItem>() { new SelectListItem() { Text = "Sem-I", Value = "1" }, new SelectListItem() { Text = "Sem-II", Value = "2" }, new SelectListItem() { Text = "Sem-III", Value = "3" }, new SelectListItem() { Text = "Sem-IV", Value = "4" }, new SelectListItem() { Text = "Sem-V", Value = "5" }, new SelectListItem() { Text = "Sem-VI", Value = "6" }, new SelectListItem() { Text = "Sem-VII", Value = "7" }, new SelectListItem() { Text = "Sem-VIII", Value = "8" }, new SelectListItem() { Text = "Sem-IX", Value = "9" }, new SelectListItem() { Text = "Sem-X", Value = "10" } };
            if (course_id.HasValue && course_id.Value != Guid.Empty && semester.HasValue && semester.Value > 0)
            {
                var param = new Parameters()
                {
                    PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
                    Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", Operator = SQLOperator.EqualTo, Value = course_id.Value.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = semester.Value.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Status", Operator = SQLOperator.EqualTo, Value = "1", TableAlias = "S" } }
                };
                ViewBag.SubjetList = new SubjectsManager().SubjectDDLWithDetail(param);
                ViewBag.CurrentSemSubject = new SubjectsManager().SubjectDDLWithDetail(param);
                param = new Parameters()
                {
                    PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
                    Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", Operator = SQLOperator.EqualTo, Value = course_id.Value.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = (semester.Value - 1).ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Status", Operator = SQLOperator.EqualTo, Value = "1", TableAlias = "S" } }
                };
                ViewBag.PrevSemSubject = new SubjectsManager().SubjectDDLWithDetail(param);
            }
        }


        [HttpGet]
        public JsonResult SetCombinationEndDate(int? batch, DateTime? endDate, PrintProgramme? printProgramme, int? semester)
        {
            return Json(new CombinationManager().SetCombinationEndDate(batch, endDate, printProgramme, semester), JsonRequestBehavior.AllowGet);
        }

    }
}
