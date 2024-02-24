using CUSrinagar.BusinessManagers;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_FileTracking)]
    public class FileTrackingController : UniversityAdminBaseController
    {
        [HttpGet]
        public ActionResult AllFiles()
        {
            FileTrackingManager _FileTrackingManager = new FileTrackingManager();
            ViewBag.FileTrackingUsers = _FileTrackingManager.GetFileTrackingActiveUsers();
            ViewBag.Files = _FileTrackingManager.GetFilesCompact(null);
            return View();
        }


        [HttpPost]
        public JsonResult CreateNewFile(FTFileTracking fileTracking)
        {
            if (!ModelState.IsValid)
                return Json(0, JsonRequestBehavior.DenyGet);
            int result = new FileTrackingManager().CreateNewFile(fileTracking);
            return Json(result, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public PartialViewResult GetFiles(FTFilters filters)
        {
            return PartialView(new FileTrackingManager().GetFilesCompact(filters));
        }


        [HttpPost]
        public async Task<JsonResult> DeleteFile(Guid File_ID)
        {
            int result = await new FileTrackingManager().DeleteAync(File_ID);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> CloseFile(FTFileTrackingHistory fileTrackingHistory)
        {
            int result = await new FileTrackingManager().CloseFileAsync(fileTrackingHistory);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> RevertFile(FTFileTrackingHistory fileTrackingHistory)
        {
            int result = await new FileTrackingManager().RevertFileAsync(fileTrackingHistory);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> ReOpenFile(FTFileTrackingHistory fileTrackingHistory)
        {
            int result = await new FileTrackingManager().ReOpenFileAsync(fileTrackingHistory);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> ForwardFile(FTFileTrackingHistory fileTrackingHistory)
        {
            int result = await new FileTrackingManager().ForwardFileAsync(fileTrackingHistory);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> AcceptFile(FTFileTrackingHistory fileTrackingHistory)
        {
            int result = await new FileTrackingManager().AcceptFileAsync(fileTrackingHistory);
            return Json(result > 0, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<PartialViewResult> TrackFileDetails(FTFileTrackingHistory fileTrackingHistory)
        {
            return PartialView(await new FileTrackingManager().GetFileAsync(fileTrackingHistory.File_ID));
        }

        [HttpGet]
        public async Task<ActionResult> PrintHistory(Guid id)
        {
            return View(await new FileTrackingManager().GetFileAsync(id));
        }
    }
}