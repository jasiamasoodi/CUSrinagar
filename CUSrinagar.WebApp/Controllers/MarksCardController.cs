using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.Web.Compilation;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using Rotativa;

namespace CUSrinagar.WebApp.Controllers
{
    public class MarksCardController : WebAppBaseController
    {
        //[HttpGet]
        //public ActionResult SemesterMarksCards(PrintProgramme PrintProgramme, short Semester, Guid Student_ID)
        //{
        //    Parameters param = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" }, new SearchFilter() { TableAlias = "Comb", Column = "Semester", Value = Semester.ToString() } }, PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 } };
        //    var listResult = new ResultManager().Get(PrintProgramme, Semester, param) ?? new List<ResultCompact>();
        //    return View(listResult);
        //}

        public ActionResult PrintMarksSheet(PrintProgramme PrintProgramme, Guid Student_ID, Guid ResultNotification_ID)
        {
            var notification = new ResultNotificationManager().Get(ResultNotification_ID);
            if (notification == null || notification.ShowPrintMarksCard == false) return View();
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "CUSRegistrationNo" } };
            parameters.Filters.Add(new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" });
            parameters.Filters.Add(new SearchFilter() { Column = "Semester", Value = notification.Semester.ToString(), TableAlias = "Comb", Operator = SQLOperator.LessThanEqualTo });

            if (notification.Semester > 6)
            {
                List<ResultCompact> model = new ResultManager().GetDetails(PrintProgramme, 6, parameters, true);
                return View("PrintEngMarksSheet", model);
            }
            else
            {

                List<ResultCompact> model = new ResultManager().Get(PrintProgramme, null, parameters);
                if (model == null || model.First().SubjectResults.Max(x => x.SemesterBatch) != notification.Batch) return View();
                return View(model);
            }
        }

        public ActionResult PrintTranscript(PrintProgramme PrintProgramme, Guid Student_ID, Guid ResultNotification_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(new StudentManager().GetStudentCurrentCourse(PrintProgramme, Student_ID));
            var notification = new ResultNotificationManager().Get(ResultNotification_ID);
            if (notification == null || notification.ShowPrintTranscript == false) return View();
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "CUSRegistrationNo" } };
            if (PrintProgramme == PrintProgramme.UG)
            { parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() + "," + ((short)PrintProgramme.BED).ToString(), Operator = SQLOperator.In }); }
            else
            {
                parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });
            }
            parameters.Filters.Add(new SearchFilter() { Column = "SemesterTo", Value = notification.Semester.ToString() });
            parameters.Filters.Add(new SearchFilter() { Column = "Batch", Value = notification.Batch.ToString() });
            parameters.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = aDMCourseMaster?.Course_ID.ToString() });
            parameters.Filters.Add(new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "VW" });
            List<Transcript> model = new TranscriptManager().TranscriptList(PrintProgramme, parameters);
            if (model == null || (model.Max(x => x.Batch) != notification.Batch.ToString() && model.Max(x => x.SemesterBatch) != notification.Batch.ToString())) return View();
            //return View(model);
            if (model.FirstOrDefault().SGPA.Count == 10)
            { ViewBag.CourseFullName = aDMCourseMaster?.CourseFullName; }
            return new PartialViewAsPdf("PrintTranscript", model)
            {
                FileName = $"Transcript_{DateTime.Now.ToString("dd-MMM-yy")}.pdf"
            };
        }

        public ActionResult PrintDegreeCertificate(PrintProgramme PrintProgramme, Guid Student_ID, Guid ResultNotification_ID)
        {
            var notification = new ResultNotificationManager().Get(ResultNotification_ID);
            if (notification == null || notification.ShowPrintDegreeCertificate == false) return View();
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "CUSRegistrationNo" } };
            if (PrintProgramme == PrintProgramme.UG)
            { parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() + "," + ((short)PrintProgramme.BED).ToString(), Operator = SQLOperator.In }); }
            else
            {
                parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });
            }
            parameters.Filters.Add(new SearchFilter() { Column = "Student_ID", Value = Student_ID.ToString(), TableAlias = "S" });
            parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", TableAlias = "DC", Operator = SQLOperator.ISNotNULL });
            List<DegreeCertificate> model = new DegreeCertificateManager().List(parameters, PrintProgramme);
            if (model == null) return View();
            foreach (var item in model.Where(x => x.DegreeCourseTitle.ToLower().Contains("geography")))
            {
                item.DegreeCourseTitle = new StudentManager().GetCoursePrefix(item.DegreeCourseTitle, PrintProgramme, item.Student_ID, "Graduation");
            }
            //return View(model);
            return new PartialViewAsPdf("PrintDegreeCertificate", model)
            {
                FileName = $"DegreeCertificate_{DateTime.Now.ToString("dd-MMM-yy")}.pdf"
            };
        }




    }

}
