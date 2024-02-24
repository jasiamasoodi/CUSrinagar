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

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Dean)]
    public class CombinationController : AdminController
    {
        #region Add New Combination
        [HttpGet]
        public ActionResult Index()
        {
            FillViewBags(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ADMCombinationMaster aDMCombinationMaster, FormCollection formCollection)
        {
            FillViewBags(null);

            FormCollection _formCollection = new DynamicControls().FillModel(formCollection, nameof(aDMCombinationMaster.CombinationHelper.FinalCombinations));
            aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetListDDL((Guid)aDMCombinationMaster.Course_ID);
            if (_formCollection != null)
            {
                aDMCombinationMaster.CombinationHelper.FinalCombinations = new List<BaseCombinationHelper>();
                List<BaseCombinationHelper> list = new List<BaseCombinationHelper>();
                TryUpdateModel(aDMCombinationMaster.CombinationHelper, _formCollection.ToValueProvider());
            }
            if (!ModelState.IsValid)
                return View(aDMCombinationMaster);
            try
            {
                int result = new CombinationManager().Save(aDMCombinationMaster);
                if (result > 0)
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Combination saved successfully.<br></div><div class='col-sm-1'></div>";
                else
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Combination not saved.<br></div><div class='col-sm-1'></div>";
            }
            catch (CUSException msg)
            {
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> {msg.Message}.<br></div><div class='col-sm-1'></div>";
                return View(aDMCombinationMaster);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public PartialViewResult Subjects(Guid Course_ID, int Semester)
        {
            ADMCombinationMaster aDMCombinationMaster = new ADMCombinationMaster { CombinationHelper = new CombinationManager().GetCombinationsHelper(Course_ID, Semester) };
            var coursemaster = new CourseManager().GetCourseById(Course_ID);
            //string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            var subjectDDL = new SubjectsManager().GetAllSubjects(Course_ID, Semester, true) ?? new List<SelectListItem>();
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            var printProgramme = new CourseManager().GetCourseById(Course_ID).PrintProgramme;
            if (printProgramme == PrintProgramme.PG)
            {
                var parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = Semester.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
                var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
                if (childDDL2.IsNotNullOrEmpty())
                    subjectDDL.AddRange(childDDL2);
            }
            aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");

            return PartialView("_SubjectsHelper", aDMCombinationMaster);
        }

        [HttpPost]
        public PartialViewResult GetSubjectsOf(Guid Course_ID, int semester, Guid College_ID)
        {
            ADMCombinationMaster aDMCombinationMaster = new ADMCombinationMaster { CombinationHelper = new CombinationManager().GetCombinationsHelper(Course_ID, semester), Course_ID = Course_ID, Semester = (short)semester, College_ID = College_ID };

            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(Course_ID);
            // aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.College_ID,aDMCombinationMaster.Semester.Value ,coursemaster.Programme,null);
            Parameters parameter = null;

            //string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            //var subjectDDL = new SubjectsManager().GetAllSubjects(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value, false) ?? new List<SelectListItem>();
            //parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", TableAlias = "C", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Course_ID.Value.ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            var subjectDDL = new List<SelectListItem>();
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);

            PrintProgramme printProgramme = aDMCourseMaster.PrintProgramme;
            Programme programme = aDMCourseMaster.Programme;
            List<Guid> skillGECourseIDs = new CourseManager().GetSkillGECourseIDsByProgramme(programme) ?? new List<Guid>();
            skillGECourseIDs.Add(aDMCourseMaster.Course_ID);

            if (printProgramme == PrintProgramme.UG)
                parameter = new Parameters()
                {
                    Filters = new List<SearchFilter>() {
                        new SearchFilter() { Column = "Programme",TableAlias="S", Operator = SQLOperator.EqualTo,
                            Value = ((short)programme).ToString() },                       

                        new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo,
                            Value = aDMCombinationMaster.Semester.Value.ToString() } },

                    PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }
                };
            else
                parameter = new Parameters()
                {
                    Filters = new List<SearchFilter>() {
                        new SearchFilter() { Column = "Programme",TableAlias="S", Operator = SQLOperator.EqualTo, Value = ((short)programme).ToString() },

                          new SearchFilter() { Column = "Course_ID",TableAlias="S", Operator = SQLOperator.In,
                            Value = string.Join(",", skillGECourseIDs)},

                        new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo,
                            Value = aDMCombinationMaster.Semester.Value.ToString() }

                        },
                    PageInfo = new Paging()
                    {
                        DefaultOrderByColumn = "SubjectFullName",
                        PageNumber = -1,
                        PageSize = -1
                    }
                };

            subjectDDL = new SubjectsManager().SubjectDDLWithDetail(parameter);
            aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");

            //var coursemaster = new CourseManager().GetCourseById(Course_ID);
            ////string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            //var subjectDDL = new SubjectsManager().GetAllSubjects(Course_ID, semester, true) ?? new List<SelectListItem>();
            ////aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            //var printProgramme = new CourseManager().GetCourseById(Course_ID).PrintProgramme;
            //if (printProgramme == PrintProgramme.PG)
            //{
            //    var parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = semester.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //    var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
            //    if (childDDL2.IsNotNullOrEmpty())
            //        subjectDDL.AddRange(childDDL2);
            //}
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");

            return PartialView("_SubjectsHelper", aDMCombinationMaster);

            //ADMCombinationMaster aDMCombinationMaster = new ADMCombinationMaster { CombinationHelper = new CombinationManager().GetCombinationsHelper(Course_ID, semester),Course_ID=Course_ID,Semester=(short)semester,College_ID=College_ID };
            //var coursemaster = new CourseManager().GetCourseById(Course_ID);
            //// aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.College_ID,aDMCombinationMaster.Semester.Value ,coursemaster.Programme,null);

            ////string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            //var subjectDDL = new SubjectsManager().GetAllSubjects(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value, false) ?? new List<SelectListItem>();
            ////aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            //var printProgramme = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value).PrintProgramme;
            //Parameters parameter = null;
            ////if (printProgramme == PrintProgramme.PG)
            //{
            //    if (printProgramme == PrintProgramme.PG)
            //        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //    else
            //        parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.NotEqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.College_ID.ToString() }, new SearchFilter() { Column = "Course_ID", TableAlias = "ADMSubjectMaster", Operator = SQLOperator.NotEqualTo, Value = aDMCombinationMaster.Course_ID.ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //    var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
            //    if (childDDL2.IsNotNullOrEmpty())
            //        subjectDDL.AddRange(childDDL2);
            //}
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");
            //var coursemaster = new CourseManager().GetCourseById(Course_ID);
            ////string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            //var subjectDDL = new SubjectsManager().GetAllSubjects(Course_ID, semester, true) ?? new List<SelectListItem>();
            ////aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            //var printProgramme = new CourseManager().GetCourseById(Course_ID).PrintProgramme;
            //if (printProgramme == PrintProgramme.PG)
            //{
            //    var parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = semester.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //    var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
            //    if (childDDL2.IsNotNullOrEmpty())
            //        subjectDDL.AddRange(childDDL2);
            //}
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");

            // return PartialView("_SubjectsHelper", aDMCombinationMaster);
        }

        #endregion


        #region Search Combinations
        public ActionResult Combination()
        {
            FillViewBag_College();
            ViewBag.Courses = new CourseManager().CoursesSelectListItem();
            FillViewBag_Semesters();
            return View();
        }
        public ActionResult CombinationListPartial(Parameters parameter)
        {
            List<ADMCombinationMaster> list = new CombinationManager().List(parameter);
            return View(list);
        }

        public void CombinationCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<ADMCombinationMaster> list = new CombinationManager().List(parameter);
            var superlist = list.Select(
                col => new
                {
                    col.CourseFullName,
                    col.Semester,
                    col.Status,
                    Subjects = col.SubjectsDetails.IsNotNullOrEmpty() ? string.Join("|", col.SubjectsDetails.Select(x => x.SubjectFullName)) : ""
                }).ToList();
            ExportToCSV(superlist, $"CombinationList_{DateTime.Now.ToShortDateString()}");
        }

        public void UsedCombinationCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<ADMCombinationMaster> list = new CombinationManager().UsedCombinationList(parameter);
            var superlist = list.Select(
                col => new
                {
                    col.CourseFullName,
                    col.CombinationCode,
                    col.Semester,
                    col.Status,
                    Subjects = col.SubjectsDetails.IsNotNullOrEmpty() ? string.Join("|", col.SubjectsDetails.Select(x => x.SubjectFullName)) : ""
                }).ToList();
            ExportToCSV(superlist, $"CombinationList_{DateTime.Now.ToShortDateString()}");
        }

        [HttpGet]
        public ActionResult Combinations()
        {
            FillViewBags(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Combinations(ADMCourseMaster _ADMCourseMaster, string btnclicked, string Semester)
        {
            FillViewBags(_ADMCourseMaster);
            if (!ModelState.IsValid)
            {
                _ADMCourseMaster.CombinationDetails = null;
                return View(_ADMCourseMaster);
            }
            if (btnclicked?.ToLower().Trim() == "search")
            {
                _ADMCourseMaster = new CourseManager().GetItem(_ADMCourseMaster, false, _ADMCourseMaster.College_ID, Semester);
                return View(_ADMCourseMaster);
            }
            else
            {
                var resultsExcel = new CombinationManager().DownloadExcel(_ADMCourseMaster, Semester);
                try
                {
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment; filename=Combinations_For_" + resultsExcel.Item2 + "__" + DateTime.Now.ToString("ddMMyyss") + ".xls");
                    Response.ContentType = "application/ms-excel";
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    resultsExcel.Item1.HeaderRow.Style.Add("background-color", "#438EB9");
                    resultsExcel.Item1.HeaderRow.Style.Add("color", "white");
                    Response.Charset = "";
                    resultsExcel.Item1.RenderControl(htw);

                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                    return new EmptyResult();
                }
                catch (NullReferenceException)
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i></strong> No Combinations found.<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("Combinations", "Combination", new { area = "CUSrinagarAdminPanel" });
                }
            }
        }
        #endregion


        #region Change Combination Status
        [HttpPost]
        public JsonResult MakeCombinationsActive(List<Guid> Combination_IDs)
        {
            return Json($"{new CombinationManager().ChangeStatus(Combination_IDs, Status.Active)} Combination(s) Updated Successfully", JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public JsonResult MakeCombinationsInActive(List<Guid> Combination_IDs)
        {
            return Json($"{new CombinationManager().ChangeStatus(Combination_IDs, Status.InActive)} Combination(s) Updated Successfully", JsonRequestBehavior.DenyGet);
        }
        #endregion



        #region RemoteValidation
        public JsonResult CombinationCodeExists(string CombinationCode, Guid College_ID, Guid? Combination_ID)
        {
            if (Regex.IsMatch(Convert.ToString(CombinationCode), "^([a-zA-Z0-9]*){1,5}$"))
                return Json(!new CombinationManager().CombinationCodeAlreadyExists(CombinationCode, College_ID, Combination_ID), JsonRequestBehavior.AllowGet);
            else
                return Json(true, JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region NonAction Methods
        [NonAction]
        private void FillViewBags(ADMCourseMaster aDMCourseMaster, ADMCombinationMaster aDMCombinationMaster = null)
        {
            List<SelectListItem> CollegeList = new CollegeManager().GetADMCollegeMasterList();
            ViewBag.Colleges = CollegeList;
            if (aDMCourseMaster == null)
            {
                ViewBag.Courses = new GeneralDDLManager().GetCourseOptLabelDDL(true, new Guid(CollegeList.FirstOrDefault().Value));
            }
            else
            { ViewBag.Courses = new GeneralDDLManager().GetCourseOptLabelDDL(true, aDMCourseMaster.College_ID); }
            if (aDMCombinationMaster != null)
            {
                SelectList subjects = new SubjectsManager().GetListDDL(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value, true);
                foreach (ADMSubjectMaster subject in new SubjectsManager().GetSkillSubjectsByCollege(aDMCombinationMaster.College_ID, (int)aDMCombinationMaster.Semester) ?? new List<ADMSubjectMaster>())
                    subjects.ToList().Add(new SelectListItem { Text = subject.SubjectFullName, Value = subject.Subject_ID.ToString() });
                foreach (ADMSubjectMaster subject in new SubjectsManager().GetSubjectByType(aDMCombinationMaster.College_ID, SubjectType.GE, aDMCombinationMaster.Semester) ?? new List<ADMSubjectMaster>())
                    subjects.ToList().Add(new SelectListItem { Text = subject.SubjectFullName, Value = subject.Subject_ID.ToString() });
                ViewBag.Subjects = subjects;
            }
            ViewBag.Status = Helper.StatusDDL();
            ViewBag.Semester = Helper.GetSelectList<Semester>();
        }
        #endregion

        [HttpPost]
        public PartialViewResult GetCourseList(string College_ID)
        {
            ViewBag.Courses = new GeneralDDLManager().GetCourseOptLabelDDL(true, new Guid(College_ID));

            return PartialView();
        }

        [HttpGet]
        public ActionResult Edit(Guid Combination_ID)
        {
            ADMCombinationMaster aDMCombinationMaster = new CombinationManager().GetCombinationByID(Combination_ID);
            aDMCombinationMaster.CombinationHelper = new CombinationManager().GetCombinationsHelper(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value);
            for (int i = 0; i < aDMCombinationMaster.SubjectsDetails.Count; i++)
            {
                if (aDMCombinationMaster.CombinationHelper.FinalCombinations.Count <= i)
                    aDMCombinationMaster.CombinationHelper.FinalCombinations.Add(new BaseCombinationHelper() { });
                aDMCombinationMaster.CombinationHelper.FinalCombinations[i] = new BaseCombinationHelper() { Subject_ID = aDMCombinationMaster.SubjectsDetails[i].Subject_ID, IsCompulsary = aDMCombinationMaster.SubjectsDetails[i].IsCompulsory, SubjectsName = aDMCombinationMaster.SubjectsDetails[i].SubjectFullName };
            }
            var coursemaster = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value);
            // aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.College_ID,aDMCombinationMaster.Semester.Value ,coursemaster.Programme,null);

            //string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            var subjectDDL = new SubjectsManager().GetAllSubjects(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value, false) ?? new List<SelectListItem>();
            //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            var printProgramme = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value).PrintProgramme;
            Parameters parameter = null;
            //if (printProgramme == PrintProgramme.PG)
            {
                if (printProgramme == PrintProgramme.PG)
                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "ADMCourseMaster", Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
                else
                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "ADMCourseMaster", Column = "Programme", Operator = SQLOperator.NotEqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.College_ID.ToString() }, new SearchFilter() { Column = "Course_ID", TableAlias = "ADMSubjectMaster", Operator = SQLOperator.NotEqualTo, Value = aDMCombinationMaster.Course_ID.ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
                var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
                if (childDDL2.IsNotNullOrEmpty())
                    subjectDDL.AddRange(childDDL2);
            }
            aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");

            // var coursemaster = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value);
            //// aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.College_ID,aDMCombinationMaster.Semester.Value ,coursemaster.Programme,null);

            // //string[] _SubjectType = new string[] { ((short)SubjectType.GE).ToString(), ((short)SubjectType.OE).ToString() };
            // var subjectDDL = new SubjectsManager().GetAllSubjects(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value, false) ?? new List<SelectListItem>();
            // //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(Course_ID, AppUserHelper.College_ID.Value, (short)semester, coursemaster.Programme, null);
            // var printProgramme = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value).PrintProgramme;
            // Parameters parameter = null;
            // //if (printProgramme == PrintProgramme.PG)
            // {
            //     if (printProgramme == PrintProgramme.PG)
            //         parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.EqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.ISNotNULL }, new SearchFilter() { Column = "SubjectType", Operator = SQLOperator.In, Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //     else
            //         parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Operator = SQLOperator.NotEqualTo, Value = ((short)Programme.PG).ToString() }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Operator = SQLOperator.EqualTo,Value=aDMCombinationMaster.College_ID.ToString() }, new SearchFilter() { Column = "Course_ID", TableAlias="ADMSubjectMaster",Operator = SQLOperator.NotEqualTo, Value =aDMCombinationMaster.Course_ID.ToString() }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = aDMCombinationMaster.Semester.Value.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 } };
            //     var childDDL2 = new SubjectsManager().SubjectDDLWithDetail(parameter);
            //     if (childDDL2.IsNotNullOrEmpty())
            //         subjectDDL.AddRange(childDDL2);
            // }
            // aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SelectList(subjectDDL, "Value", "Text");




            coursemaster.College_ID = aDMCombinationMaster.College_ID;
            FillViewBags(coursemaster, aDMCombinationMaster);
            return View(aDMCombinationMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ADMCombinationMaster aDMCombinationMaster, FormCollection formCollection)
        {
            FillViewBags(null);

            FormCollection _formCollection = new DynamicControls().FillModel(formCollection, nameof(aDMCombinationMaster.CombinationHelper.FinalCombinations));
            aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetListDDL((Guid)aDMCombinationMaster.Course_ID);
            if (_formCollection != null)
            {
                aDMCombinationMaster.CombinationHelper.FinalCombinations = new List<BaseCombinationHelper>();
                List<BaseCombinationHelper> list = new List<BaseCombinationHelper>();
                TryUpdateModel(aDMCombinationMaster.CombinationHelper, _formCollection.ToValueProvider());
            }
            if (!ModelState.IsValid)
                return View(aDMCombinationMaster);
            try
            {
                int result = new CombinationManager().Edit(aDMCombinationMaster);
                if (result > 0)
                {
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Awesome!</strong> Combination saved successfully.<br></div><div class='col-sm-1'></div>";
                    return RedirectToAction("Index");

                }
                else
                    TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-warning col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> Combination not saved.<br></div><div class='col-sm-1'></div>";
            }
            catch (CUSException msg)
            {
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> Oh snap!</strong> {msg.Message}.<br></div><div class='col-sm-1'></div>";
                return View(aDMCombinationMaster);
            }
            return RedirectToAction("Edit", new { @Combination_ID = aDMCombinationMaster.Combination_ID });
        }

        public JsonResult BatchUpdateCombinationMaster(BatchUpdateCombinationMaster model)
        {
            ResponseData response = new CombinationManager().BatchUpdateCombinationMaster(model);
            return Json(response);
        }
        [HttpGet]
        public ActionResult ReplaceSubject()
        {
            SetViewBags();
            return View(new ReplaceSubject());
        }

        private void SetViewBags()
        {
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            FillViewBag_Batches();
        }

        [HttpPost]
        public ActionResult ReplaceSubject(ReplaceSubject model)
        {
            ResponseData response = new CombinationManager().ReplaceSubject(model);
            if (response.IsSuccess)
                TempData["response"] = "<div class='col-sm-1'></div><div class='alert alert-success col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i> " + response.SuccessMessage + "<br> </div><div class='col-sm-1'></div>";
            else
                TempData["response"] = $"<div class='col-sm-1'></div><div class='alert alert-danger col-sm-10'><button type='button' class='close' data-dismiss='alert'><i class='ace-icon fa fa-times'></i></button><strong><i class='ace-icon fa fa-times'></i>{response.ErrorMessage}<br> </div><div class='col-sm-1'></div>";
            SetViewBags();

            return View(model);
        }

        public JsonResult BatchUpdateCombinationMasterByIDs(ADMCombinationMaster model)
        {
            ADMCombinationMaster _ADMCombinationMaster = new CombinationManager().GetCombinationByID(model.Combination_ID);
            _ADMCombinationMaster.CombinationSubjects = model.CombinationSubjects;
            ResponseData response = new CombinationManager().UpdateByIDs(_ADMCombinationMaster);
            return Json(response);
        }


        public ActionResult Delete(Guid id)
        {
            ResponseData response = new CombinationManager().Delete(id);
            return Json(response);
        }

        [HttpPost]
        public PartialViewResult _UserActivity(Guid Id)
        {
            return PartialView(new CombinationManager().GetUserActivity(Id));
        }
    }
}