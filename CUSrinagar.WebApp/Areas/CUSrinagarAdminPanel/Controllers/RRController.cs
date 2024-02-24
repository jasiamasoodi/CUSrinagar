using CUSrinagar.BusinessManagers;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_RR)]
    public class RRController : UniversityAdminBaseController
    {
        [HttpGet]
        public ActionResult Report()
        {
            ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.CombinationList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();
            ViewBag.Colleges = new CollegeManager().GetADMCollegeMasterList();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ViewReport(string id, Guid? id1, string sem, PrintProgramme? programme, string College_ID)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !Int32.TryParse(id, out int Batch)) || !int.TryParse(sem, out int semester) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("Report", "RR", new { area = "CUSrinagarAdminPanel" });

            ADMCollegeMaster college = new CollegeManager().GetItem(Guid.Parse(College_ID ?? Guid.Empty.ToString()));

            ViewBag.College = college.CollegeFullName;
            ViewBag.CollegeCode = college.CollegeCode;

            Parameters parameter = new Parameters();
            var RRManager = new RRManager();

            parameter = RRManager.AddPageFilters("Course_ID", Course_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("College_ID", College_ID ?? Guid.Empty.ToString(), parameter);
            parameter = RRManager.AddPageFilters("SemesterBatch", id, parameter);
            parameter = RRManager.AddPageFilters("Semester", semester.ToString(), parameter);
            ViewBag.CourseFullName = new CourseManager().GetItem(Course_ID).CourseFullName;
            List<LibraryForm> list = await new LibraryManager().GetStudentsAsync(Pprogramme, parameter, true);
            return View(list);
        }

        public PartialViewResult _GetRRDDL(string College_ID, string id, string Type, string childType, string childSubType, string Semester)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = FetchChildDDlValues(College_ID, id, Type, Semester);
            return PartialView();
        }

        private List<SelectListItem> FetchChildDDlValues(string College_ID, string id, string Type, string Semester)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Course":
                    {
                        PrintProgramme programme = (PrintProgramme)Convert.ToInt32(id);
                        childDDL = new CourseManager().GetCourseList(Guid.Parse(College_ID ?? Guid.Empty.ToString()), programme);
                        break;
                    }
                case "Semester":
                    {
                        Guid CourseId = new Guid(id);
                        childDDL = new LibraryManager().GetAllSemesters(CourseId);
                        break;
                    }
            }

            return childDDL;
        }

        [HttpGet]
        public async Task<ActionResult> DownloadInExcel(string id, Guid? id1, string sem, PrintProgramme? programme, string College_ID)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !Int32.TryParse(id, out int Batch)) || !int.TryParse(sem, out int semester) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("Report", "RR", new { area = "CUSrinagarAdminPanel" });


            ADMCollegeMaster college = new CollegeManager().GetItem(Guid.Parse(College_ID ?? Guid.Empty.ToString()));

            ViewBag.College = college.CollegeFullName;
            ViewBag.CollegeCode = college.CollegeCode;

            Parameters parameter = new Parameters();
            var RRManager = new RRManager();

            parameter = RRManager.AddPageFilters("Course_ID", Course_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("College_ID", College_ID ?? Guid.Empty.ToString(), parameter);
            parameter = RRManager.AddPageFilters("Batch", id, parameter);
            parameter = RRManager.AddPageFilters("Semester", semester.ToString(), parameter);
            ViewBag.CourseFullName = new CourseManager().GetItem(Course_ID).CourseFullName;
            List<LibraryForm> list = await new LibraryManager().GetStudentsAsync(Pprogramme, parameter, true);

            var reportList = list.Select(x => new
            {
                college.CollegeFullName,
                x.SemesterBatch,
                x.StudentFormNo,
                x.CUSRegistrationNo,
                x.BoardRegistrationNo,
                x.ClassRollNo,
                x.FullName,
                x.FathersName,
                x.MothersName,
                DOB = x.DOB.ToLongDateString(),
                x.Gender,
                x.Category,
                x.Religion,
                x.StudentAddress.PermanentAddress,
                x.StudentAddress.District,
                x.StudentAddress.Tehsil,
                x.StudentAddress.Block,
                x.StudentAddress.AssemblyConstituency,
                x.StudentAddress.Mobile,
                x.StudentAddress.Email,
                ABCID = string.IsNullOrWhiteSpace(x.ABCID) ? "" : string.Join("-", Enumerable.Range(0, x.ABCID.Length / 3).Select(i => x.ABCID.Substring(i * 3, 3))),
                x.StudentSubjects
            }).ToList();
            ExportToCSV(reportList, $"{id}_{ViewBag.CourseFullName}_Sem_{semester.ToString()}");
            return null;
        }

    }
}