using CUSrinagar.BusinessManagers;
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

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College_RR)]
    public class RRController : CollegeAdminBaseController
    {
        [HttpGet]
        public ActionResult Report()
        {
            ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.CombinationList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ViewReport(string id, Guid? id1, string sem, PrintProgramme? programme, bool IsLateralEntry = false, string cno = null)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !int.TryParse(id, out int Batch)) || !int.TryParse(sem, out int semester) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("Report", "RR", new { area = "CUCollegeAdminPanel" });

            ViewBag.College = AppUserHelper.AppUsercompact.FullName;
            ViewBag.CollegeCode = AppUserHelper.CollegeCode;

            Parameters parameter = new Parameters();
            var RRManager = new RRManager();

            parameter = RRManager.AddPageFilters("Course_ID", Course_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("College_ID", AppUserHelper.College_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("SemesterBatch", id, parameter);
            parameter = RRManager.AddPageFilters("Semester", semester.ToString(), parameter);
            if (IsLateralEntry)
            {
                parameter = RRManager.AddPageFilters("IsLateralEntry", "1", parameter);
            }

            if (!string.IsNullOrWhiteSpace(cno))
            {
                List<SearchFilter> filters = new List<SearchFilter>
                {
                    new SearchFilter{
                         Column="CUSRegistrationNo",
                         Value=cno,
                         Operator=SQLOperator.EqualTo,

                    },
                    new SearchFilter{
                         Column="StudentFormNo",
                         Value=cno,
                         Operator=SQLOperator.EqualTo,
                         GroupOperation=LogicalOperator.OR,
                         IsSibling=true
                    }
                };
                parameter.Filters.AddRange(filters);
            }

            ViewBag.CourseFullName = new CourseManager().GetCompactItem(Course_ID).CourseFullName;
            List<LibraryForm> list = await new LibraryManager().GetStudentsAsync(Pprogramme, parameter, true);
            return View(list);
        }

        public PartialViewResult _GetRRDDL(string id, string Type, string childType, string childSubType, string Semester)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = new LibraryManager().FetchChildDDlValues(id, Type, Semester);
            return PartialView();
        }

        [HttpGet]
        public async Task<ActionResult> DownloadInExcel(string id, Guid? id1, string sem, PrintProgramme? programme, bool IsLateralEntry = false)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !Int32.TryParse(id, out int Batch)) || !int.TryParse(sem, out int semester) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("Report", "RR", new { area = "CUCollegeAdminPanel" });

            ViewBag.College = AppUserHelper.AppUsercompact.FullName;
            ViewBag.CollegeCode = AppUserHelper.CollegeCode;

            Parameters parameter = new Parameters();
            var RRManager = new RRManager();

            parameter = RRManager.AddPageFilters("Course_ID", Course_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("College_ID", AppUserHelper.College_ID.ToString(), parameter);
            parameter = RRManager.AddPageFilters("SemesterBatch", id, parameter);
            parameter = RRManager.AddPageFilters("Semester", semester.ToString(), parameter);
            ViewBag.CourseFullName = new CourseManager().GetCompactItem(Course_ID).CourseFullName;
            if (IsLateralEntry)
            {
                parameter = RRManager.AddPageFilters("IsLateralEntry", "1", parameter);
            }
            List<LibraryForm> list = await new LibraryManager().GetStudentsAsync(Pprogramme, parameter, true);

            if (list.IsNotNullOrEmpty())
            {
                var reportList = list.Select(x => new
                {
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
                    ABCID = string.IsNullOrWhiteSpace(x.ABCID)
                    ? "" 
                    : string.Join("-", Enumerable.Range(0, x.ABCID.Length / 3).Select(i => x.ABCID.Substring(i * 3, 3))),
                    x.StudentSubjects
                }).ToList();
                ExportToCSV(reportList, $"{id}_{ViewBag.CourseFullName}_Sem_{semester.ToString()}");
            }
            return null;
        }



        [HttpGet]
        public ActionResult DownloadSemesterAdmissionDetails()
        {
            ViewBag.PrintProgamme = Helper.GetSelectList<PrintProgramme>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadSemesterAdmissionDetails(SemesterAdmissionFilter semesterAdmissionFilter)
        {
            ViewBag.PrintProgamme = Helper.GetSelectList<PrintProgramme>();
            if (!ModelState.IsValid)
                return View();
            DataTable data = new RRManager().GetSemesterAdmissionDetails(semesterAdmissionFilter);
            if (data != null)
            {
                return DownloadExcel(data, "SemesterDetails");
            }
            return View();
        }
    }
}