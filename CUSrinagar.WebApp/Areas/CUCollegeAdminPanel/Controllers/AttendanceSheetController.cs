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
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class AttendanceSheetController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ViewStudents()
        {
            ViewBag.PrintProgrammeOption = new AttendanceSheetManager().GetPrintProgrammeDDL();
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();
            ViewBag.Centers = new List<SelectListItem>();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> DownloadSheets(string id, Guid? id1, string sem, string ExamRollNo, PrintProgramme? programme, Guid? c)
        {
            if (!(Guid.TryParse(Convert.ToString(id1), out Guid Course_ID)) || !int.TryParse(id, out int Batch) || !int.TryParse(sem, out int Semester) || programme == null)
                return RedirectToAction("ViewStudents", "AttendanceSheet", new { area = "CUCollegeAdminPanel" });
            Guid Center_ID = Guid.Empty;
            if (c != null)
            {
                if (!Guid.TryParse(Convert.ToString(c), out Center_ID))
                    return RedirectToAction("ViewStudents", "AttendanceSheet", new { area = "CUCollegeAdminPanel" });
            }

            ViewBag.College = AppUserHelper.AppUsercompact.FullName;
            ViewBag.CollegeCode = AppUserHelper.CollegeCode;

            List<AttendanceSheetForm> AttendanceSheets = await new AttendanceSheetManager().GetAttendanceSheetsAsync((PrintProgramme)programme, Course_ID, Batch, Semester, ExamRollNo, Center_ID,null);
            return View(AttendanceSheets);
        }

        public PartialViewResult _GetChildDDL(string id, string Type, string childType, string childSubType)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = new AttendanceSheetManager().FetchChildDDlValues(id, Type);
            return PartialView();
        }

        public PartialViewResult _GetCenterList(Guid Course_ID, PrintProgramme printProgramme, short examYear, short semester)
        {
            if (Course_ID == Guid.Empty || printProgramme == 0 || examYear < 2017 || semester < 1)
                ViewBag.CenterList = new List<SelectListItem>();
            else
            {
                ViewBag.CenterList = new SemesterCentersManager().GetCenterList(AppUserHelper.College_ID.Value, Course_ID, printProgramme, examYear, semester);
            }
            return PartialView();
        }


    }
}