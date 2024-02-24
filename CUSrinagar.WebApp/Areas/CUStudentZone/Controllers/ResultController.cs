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
using System.Web.Mvc;
namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class ResultController : StudentBaseController
    {
        public ActionResult ResultDiscrepancy(short? sem, string sub)
        {
            var response = new ResponseData() { };
            var subjectdis = TempData["model"] ?? new ResultManager().SubjectResultDiscrepancy(AppUserHelper.User_ID);

            if (subjectdis == null && (!sem.HasValue || string.IsNullOrEmpty(sub)))
            {
                ViewBag.ErrorMessage = "Invalid request";
                return View();
            }
            ARGPersonalInformation STDINFO = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.TableSuffix, true, sem.Value);

            if (subjectdis == null)
            {
                if (STDINFO == null || STDINFO.SelectedCombinations.IsNullOrEmpty() || STDINFO.SelectedCombinations.First().Subjects.IsNullOrEmpty()
                || !STDINFO.SelectedCombinations.First().Subjects.Any(x => sub.ToLower().Contains(x.Subject_ID.ToString().ToLower())))
                {
                    ViewBag.ErrorMessage = "Invalid request.";
                    return View();
                }
            }
            if (subjectdis == null)
            {
                var _d = STDINFO.SelectedCombinations.First().Subjects.First(x => sub.ToLower().Contains(x.Subject_ID.ToString().ToLower()));
                subjectdis = new SubjectResultDiscrepancy()
                {
                    Semester = sem.Value,
                    Dated = DateTime.Now,
                    Student_ID = AppUserHelper.User_ID,
                    SubjectFullName = _d.SubjectFullName,
                    Subject_ID = _d.Subject_ID
                };
            }
            ViewBag.StudentResultDiscrepancy = subjectdis;

            GetResponseViewBags();
            return View(STDINFO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultDiscrepancy(SubjectResultDiscrepancy model)
        {
            ResponseData response = new ResponseData();
            TempData["model"] = model;
            if (ModelState.IsValid)
            {
                response = new ResultManager().Save(model);
                if(response.IsSuccess)
                {
                    TempData["model"] = response.ResponseObject;
                }
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            return RedirectToAction("ResultDiscrepancy", new { sem = model.Semester });
        }

    }
}
