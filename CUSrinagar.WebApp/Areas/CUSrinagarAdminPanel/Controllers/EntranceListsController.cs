using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.University_Registrar, AppRoles.University_Notification)]
    public class EntranceListsController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        private void SetViewBags()
        {
            ViewBag.ListType = Helper.GetSelectList<EntranceListType>();
            ViewBag.Programme = Helper.GetSelectList<Programme>();
            ViewBag.Courses = new List<SelectListItem>();
        }

        #region DeleteAndDisplay

        [HttpGet]
        public ActionResult Index()
        {
            return View(new EntranceListManager().GetList());
        }

        [HttpPost]
        public JsonResult Delete(Guid Id, string FileName)
        {
            int result = new EntranceListManager().Delete(Id, FileName);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAll()
        {
            int result = new EntranceListManager().DeleteAll();
            if (result > 0)
            {
                TempData["response"] = errorMsg.Replace("##", "Deleted successfully").Replace("alert-danger", "alert-success");
            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", "Nothing deleted");
            }
            return RedirectToAction("Index");
        }

        #endregion


        #region AddOrEdit

        [HttpGet]
        public ActionResult AddEditList()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditList(ADMEntranceLists entranceLists)
        {
            SetViewBags();
            if (!ModelState.IsValid)
            {
                return View();
            }
            int result = new EntranceListManager().UploadAndSave(entranceLists);
            if (result > 0)
            {
                TempData["response"] = errorMsg.Replace("##", "Saved successfully").Replace("alert-danger", "alert-success");
            }
            else
            {
                TempData["response"] = errorMsg.Replace("##", "Operation was unsuccessfully");
            }
            return RedirectToAction("AddEditList");
        }

        [HttpPost]
        public PartialViewResult GetCourseDDL(Programme programme)
        {            
            List<SelectListItem> _CourseDDL = new EntranceListManager().GetAllCoursesByProgramme(programme);
            return PartialView("GetCourseDDL", _CourseDDL);
        }

        #endregion
    }
}