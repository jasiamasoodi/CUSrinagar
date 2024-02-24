using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.Web.Compilation;
using System.IO;
using CUSrinagar.Enums;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Extensions;
using GeneralModels;
using CUSrinagar.OAuth;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_OfficeAssistant, AppRoles.University_Dean)]
    public class SyllabusController : AdminController
    {

        public ActionResult Index()
        {
            //SetViewBags();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Departments();
            FillViewBag_Semesters();
            GetResponseViewBags();
            return View();
        }

       

        public ActionResult SyllabusListPartial(Parameters parameter)
        {
            List<Syllabus> listSyllabus = new SyllabusManager().List(parameter);
            return View(listSyllabus);
        }

        public void CSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<Syllabus> collection = new SyllabusManager().List(parameter);
            var list = collection.Select(
                col => new
                {
                    col.Session,
                    col.CourseFullName,
                    col.Semester,
                    SubjectType = col.SubjectType.ToString(),
                    col.SubjectFullName,
                    Status = col.Status ? "Active" : "InActive",
                    col.Remark
                }).ToList();
            ExportToCSV(list, $"Syllabus_{DateTime.Now.ToShortDateString()}");
        }

        public ActionResult Create(Guid? id)
        {
            Syllabus model = null;
            if (id.HasValue && id.Value != Guid.Empty)
                model = (Syllabus)TempData["model"] ?? new SyllabusManager().GetSyllabusById(id.Value);
            else
                model = (Syllabus)TempData["model"] ?? new Syllabus() { };
            GetResponseViewBags();
            //FillViewBags(model.Course_ID, model.Semester);
            FillViewBag_Programmes();
            FillViewBag_Semesters();
            //FillViewBag_Course(new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "Programme,CourseFullName", PageNumber = -1, PageSize = -1 } });
            FillViewBag_Departments(new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "DepartmentFullName", PageNumber = -1, PageSize = -1 } });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Syllabus model)
        {
            ResponseData response = new ResponseData();
            var fileName = "";
            if (model.Subject_ID.HasValue)
            {
                ADMSubjectMaster subject = new SubjectsManager().Get(model.Subject_ID.Value);
                model.Semester = (short)subject.Semester;
                if (new SyllabusManager().ExistSyllabus(model))
                    ModelState.AddModelError("Syllabus", "Syllabus Already Exist");
                if (model.Files != null && (new SyllabusManager().checkFileExists(model, subject.SubjectFullName)))
                    ModelState.AddModelError("Files", "File Name Already Exist");
                fileName = subject.SubjectFullName;
                model.Course_ID = null;
            }
            #region MyRegion
            //else if (model.Course_ID.HasValue)
            //{
            //    ADMCourseMaster course = new CourseManager().GetCourseById(model.Course_ID.Value);
            //    if (new SyllabusManager().ExistSyllabus(model))
            //        ModelState.AddModelError("Syllabus", "Syllabus Already Exist");
            //    if (model.Files != null && (new SyllabusManager().checkFileExists(model, course.CourseFullName)))
            //        ModelState.AddModelError("Files", "File Name Already Exist");
            //    fileName = course.CourseFullName;
            //    model.Subject_ID = null;
            //} 
            #endregion


            else
            {
                ModelState.AddModelError("Syllabus", "Missing required parameters/Subject/Course");
            }

            if (model.Syllabus_ID == Guid.Empty && model.Files == null)
                ModelState.AddModelError("Files", "no file selected");

            if (ModelState.IsValid)
            {
                response = new SyllabusManager().AddSyllabus(model, fileName.Trim());
            }
            else
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());

            SetResponseViewBags(response);
            if (response.IsSuccess)
                return RedirectToAction("Index");

            FillViewBag_Semesters();
            FillViewBag_Programmes();
            //FillViewBag_Course(new Parameters() { PageInfo = new Paging() { DefaultOrderByColumn = "Programme,CourseFullName", PageNumber = -1, PageSize = -1 } });
            FillViewBag_Departments();
            GetResponseViewBags();
            return View(model);
        }

        #region Commented

        //public ActionResult Create()
        //{
        //    GetResponseViewBags();
        //    FillViewBags();
        //    SetViewBags();
        //    return View();
        //}


        //private void FillViewBags(Guid? course_id = null, short? semester = null)
        //{
        //    ViewBag.Courses = new CourseManager().GetCourseList();
        //    ViewBag.Semesters = new List<SelectListItem>() { new SelectListItem() { Text = "Sem-I", Value = "1" }, new SelectListItem() { Text = "Sem-II", Value = "2" }, new SelectListItem() { Text = "Sem-III", Value = "3" }, new SelectListItem() { Text = "Sem-IV", Value = "4" }, new SelectListItem() { Text = "Sem-V", Value = "5" }, new SelectListItem() { Text = "Sem-VI", Value = "6" }, new SelectListItem() { Text = "Sem-VII", Value = "7" }, new SelectListItem() { Text = "Sem-VIII", Value = "8" } };
        //    //if (course_id.HasValue && course_id.Value != Guid.Empty && semester.HasValue && semester.Value > 0)
        //    //{
        //    //    var param = new Parameters()
        //    //    {
        //    //        PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 },
        //    //        Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Course_ID", Operator = SQLOperator.EqualTo, Value = course_id.Value.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Semester", Operator = SQLOperator.EqualTo, Value = semester.Value.ToString(), TableAlias = "S" }, new SearchFilter() { Column = "Status", Operator = SQLOperator.EqualTo, Value = "1", TableAlias = "S" } }
        //    //    };
        //    //    ViewBag.SubjetList = new SubjectsManager().SubjectDDLWithDetail(param);
        //    //}
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Syllabus input)
        //{
        //    try
        //    {
        //        var fileName = "";
        //        if (input.Subject_ID.HasValue)
        //        {
        //            ADMSubjectMaster subject = new SubjectsManager().Get(input.Subject_ID.Value);
        //            if (new SyllabusManager().ExistSyllabus(input))
        //                ModelState.AddModelError("Syllabus", "Syllabus Already Exist");
        //            if (input.Files != null && (new SyllabusManager().checkFileExists(input, subject.SubjectFullName)))
        //                ModelState.AddModelError("Files", "File Name Already Exist");
        //            fileName = subject.SubjectFullName;
        //        }
        //        else if (input.Course_ID.HasValue)
        //        {
        //            ADMCourseMaster course = new CourseManager().GetCourseById(input.Course_ID.Value);
        //            if (new SyllabusManager().ExistSyllabus(input))
        //                ModelState.AddModelError("Syllabus", "Syllabus Already Exist");
        //            if (input.Files != null && (new SyllabusManager().checkFileExists(input, course.CourseFullName)))
        //                ModelState.AddModelError("Files", "File Name Already Exist");
        //            fileName = course.CourseFullName;
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("Syllabus", "Missing required parameters/Subject/Course");
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            new SyllabusManager().AddSyllabus(input, fileName.Trim());
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("ErrorMessage", ex.Message);
        //    }
        //    SetViewBags(input);
        //    return View(input);
        //}


        //// GET: CUSrinagarAdminPanel/Syllabus/Edit/5
        //public ActionResult Edit(Guid id)
        //{
        //    SetViewBags();
        //    Syllabus model = new SyllabusManager().GetSyllabusById(id);
        //    return View(model);
        //}


        //public void SetViewBags(Syllabus syllabus = null)
        //{
        //    IEnumerable<SelectListItem> ProgrammeDDL = new List<SelectListItem>();
        //    ProgrammeDDL = Helper.GetSelectList<Programme>();
        //    List<SelectListItem> CourseDDL = new List<SelectListItem>();
        //    List<SelectListItem> SemesterDDL = new List<SelectListItem>();
        //    List<SelectListItem> SubjectDDL = new List<SelectListItem>();
        //    if (syllabus != null && syllabus.Course_ID != null)
        //    {
        //        int ProgrammeId = Convert.ToInt32(ProgrammeDDL.FirstOrDefault().Value);
        //        CourseDDL = new CourseManager().GetAllCoursesByProgramme(ProgrammeId);
        //        SemesterDDL = new SyllabusManager().GetAllSemesters(syllabus.Course_ID.Value);
        //        SubjectDDL = new SubjectsManager().GetAllSubjects(syllabus.Course_ID.Value);
        //    }
        //    ViewBag.ProgrammeDDLList = ProgrammeDDL == null ? new List<SelectListItem>() : ProgrammeDDL;
        //    ViewBag.CourseDDLList = CourseDDL == null ? new List<SelectListItem>() : CourseDDL;
        //    ViewBag.SemesterDDLList = SemesterDDL == null ? new List<SelectListItem>() : SemesterDDL;
        //    ViewBag.SubjectDDLList = SubjectDDL == null ? new List<SelectListItem>() : SubjectDDL;
        //} 

        /// <summary>
        /// Partial view for child dropdown
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        //public PartialViewResult _GetChildDDL(string id, string Type, string childType, string childSubType, int Semester = 0)
        //{
        //    ViewBag.Type = Type;
        //    ViewBag.ChildType = childType;
        //    ViewBag.ChildSubType = childSubType;
        //    ViewBag.ChildValues = new SyllabusManager().FetchChildDDlValues(id, Semester, Type, false);
        //    return PartialView();
        //}


        #endregion

        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            msg = new SyllabusManager().ChangeStatus(id);
            return msg;
        }
        [HttpPost]
        public string Delete(string id)
        {
            Guid ID = new Guid(id);
            bool IsDeleted = new SyllabusManager().Delete(ID);
            return IsDeleted ? "Deleted Successfully" : "Some error occured";
        }
    }

}
