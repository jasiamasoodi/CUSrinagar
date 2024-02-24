using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University)]
    public class SubjectMarksStructureController : AdminController
    {
        public ActionResult SubjectMarksStructure()
        {
            return View();
        }
        public ActionResult SubjectMarksStructureList(Parameters parameter)
        {
            List<MSSubjectMarksStructure> listSubjects = new SubjectMarksStructureManager().List(parameter);
            return View(listSubjects);
        }

        public ActionResult CreateEdit(Guid? id)
        {
            FillViewBag_SGPATypes();
            FillViewBag_MarksIsPartOfs();
            FillViewBag_AwardModuleTypes();

            MSSubjectMarksStructure model = null;
            if (id.HasValue)
                model = (MSSubjectMarksStructure)TempData["model"] ?? new SubjectMarksStructureManager().Get(id.Value);
            model = model ?? new MSSubjectMarksStructure() { };
            GetResponseViewBags();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(MSSubjectMarksStructure model)
        {
            FillViewBag_SGPATypes();
            FillViewBag_MarksIsPartOfs();
            FillViewBag_AwardModuleTypes();

            ResponseData response = new ResponseData();
            if (ModelState.IsValid)
            {
                response = model.SubjectMarksStructure_ID == Guid.Empty ? new SubjectMarksStructureManager().Save(model) : new SubjectMarksStructureManager().Update(model);
            }
            else
            {
                response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
            }
            SetResponseViewBags(response);
            GetResponseViewBags();
            if (response.IsSuccess)
            {
                TempData["model"] = response.ResponseObject;
                return RedirectToAction("CreateEdit", new { id = model.SubjectMarksStructure_ID });
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Delete(Guid id)
        {
            ResponseData response = new SubjectMarksStructureManager().Delete(id);
            if (!response.IsSuccess)
                return RedirectToAction("CreateEdit", new { id });

            SetResponseViewBags(response);
            return RedirectToAction("SubjectMarksStructure");
        }


    }
}