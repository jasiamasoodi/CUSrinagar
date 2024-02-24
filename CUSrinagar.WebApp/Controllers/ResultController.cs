using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{

    public class ResultController : WebAppBaseController
    {
        public ActionResult ResultNotification(PrintProgramme? program)
        {
            ViewBag.printProgramme = program.HasValue ? program : PrintProgramme.UG;
            return View();
        }
        public ActionResult ResultNotificationList(Parameters parameter)
        {
            if (parameter.Filters == null) parameter.Filters = new List<SearchFilter>();
            parameter.Filters.Add(new SearchFilter() { Column = "ParentNotification_ID", Operator = SQLOperator.ISNULL, GroupOperation = LogicalOperator.AND, Value = "NULL Value" });
            parameter.Filters.Add(new SearchFilter() { Column = "IsActive", Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND, Value = "1" });

            var resultNotificationList = new ResultNotificationManager().ListCompact(parameter);
            return PartialView(resultNotificationList);
        }

        public ActionResult Result(Guid? id)
        {
            ResultNotification model = null;
            if (id.HasValue)
                model = new ResultNotificationManager().Get(id.Value);
            if (model == null) return RedirectToAction("Error404", "Error");
            GetResponseViewBags();
            return View(model);
        }

        //[HttpPost]
        //[ChildActionOnly]
        //public PartialViewResult ResultPartial(ResultNotification model, string searchvalue)
        //{
        //    ViewBag.ResultNotification = model;
        //    ResultCompact studentResult = null;
        //    if (!string.IsNullOrEmpty(searchvalue) && model != null)
        //    {
        //        searchvalue = searchvalue.Trim();
        //        studentResult = new ResultManager().GetResult(searchvalue, model);
        //    }
        //    return PartialView(studentResult);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ViewResult(Guid? id, string searchvalue)
        {
            #region validateRequest
            ResponseData response = new ResponseData();
            if (!(this?.IsCaptchaValid("") ?? true))
            {
                response.ErrorMessage = "Invalid Captcha";
                SetResponseViewBags(response);
                return RedirectToAction("Result", new { id = id });
            }
            else
            {
                if (!id.HasValue)
                {
                    response.ErrorMessage = "invalid request.";
                    return Redirect("/");
                }
                if (string.IsNullOrEmpty(searchvalue))
                {
                    response.ErrorMessage = "result not found.";
                    return RedirectToAction("Result", new { id = id });
                }
                #endregion

                

                ResultNotification resultNotification = await new ResultNotificationManager().GetAsync(id.Value);

                if (resultNotification != null)
                {
                    PrintProgramme printProgramme= resultNotification.PrintProgramme;//If Result Declared as a Backlog, then Remarks of ResultNotification table will be shown accordingly
                    if (printProgramme == PrintProgramme.BED)
                    {
                        printProgramme = PrintProgramme.UG;
                    }
                    short semester =   resultNotification.Semester;                    
                    ResultNotification backlogNotification =  new ResultNotificationManager().CheckAnyBacklog(id.Value,searchvalue, printProgramme, semester);
                    if (backlogNotification != null)
                    { 
                        resultNotification.Remark= backlogNotification.Remark;
                    }


                    ResultCompact model = await new ResultManager().GetResultAsync(searchvalue.Trim(), resultNotification);

                    int Semester = resultNotification.Semester;
                    List<ADMSubjectMaster> Subjectlist = new SubjectsManager().GetAllSubjectsSemester(searchvalue.Trim(), Semester, printProgramme);

                    if (model != null)
                    {
                        ViewBag.ResultNotification = resultNotification;
                        model.SubjectList = Subjectlist;
                        return View(model);
                    }
                }
                response.ErrorMessage = "result not found";
                SetResponseViewBags(response);
                return RedirectToAction("Result", new { id = id });
            }
        }
    }
}