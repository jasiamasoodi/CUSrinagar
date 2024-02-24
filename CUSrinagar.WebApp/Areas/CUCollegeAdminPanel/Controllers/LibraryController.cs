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
    public class LibraryController : Controller
    {
        public ActionResult Students()
        {
            ViewBag.ProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            ViewBag.CourseList = new List<SelectListItem>();
            ViewBag.CombinationList = new List<SelectListItem>();
            ViewBag.SemesterList = new List<SelectListItem>();
            return View();
        }

        public async Task<ActionResult> LibraryForm(string id, Guid? id1, Guid? id2, string sem, PrintProgramme? programme)
        {
            if (!Guid.TryParse(Convert.ToString(id1), out Guid Course_ID) || !int.TryParse(Convert.ToString(id), out int batch) || !int.TryParse(Convert.ToString(sem), out int Semester) || !Enum.TryParse(programme?.ToString(), out PrintProgramme Pprogramme))
                return RedirectToAction("Students", "Library", new { area = "CUCollegeAdminPanel" });

            ViewBag.College = AppUserHelper.AppUsercompact.FullName;
            ViewBag.CollegeCode = AppUserHelper.CollegeCode;

            Parameters parameter = new Parameters();
            var LibraryManager = new LibraryManager();


            parameter = LibraryManager.AddPageFilters("Course_Id", Course_ID.ToString(), parameter);

            if (Guid.TryParse(Convert.ToString(id2), out Guid Combination_ID))
                parameter = LibraryManager.AddPageFilters("Combination_Id", Combination_ID.ToString(), parameter);

            parameter = LibraryManager.AddPageFilters("College_Id", AppUserHelper.College_ID.ToString(), parameter);
            parameter = LibraryManager.AddPageFilters("SemesterBatch", batch.ToString(), parameter);
            parameter = LibraryManager.AddPageFilters("Semester", Semester.ToString(), parameter);
            parameter = LibraryManager.GetDefaultParameter(parameter);

            List<LibraryForm> list = await LibraryManager.GetStudentsAsync(Pprogramme, parameter);
            return View(list);
        }
        public PartialViewResult _GetChildDDL(string id, string Type, string childType, string childSubType,string Semester)
        {
            ViewBag.Type = Type;
            ViewBag.ChildType = childType;
            ViewBag.ChildSubType = childSubType;
            ViewBag.ChildValues = new LibraryManager().FetchChildDDlValues(id, Type,Semester);
            return PartialView();
        }


    }
}