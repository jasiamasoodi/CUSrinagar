using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using CUSrinagar.Models;
using System.IO;
using CUSrinagar.BusinessManagers;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.OAuth;
using CUSrinagar.Extensions;
using IronXL;
using System.Data;
using System.Threading.Tasks;
using CUSrinagar.DataManagers;
using System.Web.UI.WebControls;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;




namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University, AppRoles.MarksSheetList)]
    public class MarksSheetController : AdminController
    {
        #region ResultListSection
        public ActionResult Result()
        {
            FillViewBag_College();
            FillViewBag_Programmes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Semesters();
            FillViewBag_Batches();
            return View();
        }
        public PartialViewResult ResultListPartial(Parameters parameter, PrintProgramme? otherParam1) 
        {
            List<ResultCompact> resultlist = null;
            short? sem = null;
            var semester = parameter.Filters.FirstOrDefault(x => x.Column.ToLower() == "semester" && !string.IsNullOrEmpty(x.Value))?.Value;
            if (semester != null) sem = short.Parse(semester);
            resultlist = new ResultManager().Get(otherParam1.Value, sem, parameter, true);
            return PartialView(resultlist);
        }


        [HttpPost]
        public ActionResult MarksSheet(Parameters parameter,PrintProgramme printProgramme)//Generate MarksSheets for B.tech
        {
            SearchFilter coursefilter = parameter.Filters.Where(x => x.Column == "Course_ID").FirstOrDefault();
            string CourseId = string.Empty;
            if (coursefilter != null)
            {
                CourseId = coursefilter.Value;
            }


            //Check if data not exists in MSFullDetails table, then insert
            short semester = 0;     
             short.TryParse(parameter.Filters?.FirstOrDefault(x => x.Column == "Semester")?.Value, out semester);
            
            List<ResultCompact> resultlist = null;

            if (semester > 0)
            {
                parameter.PageInfo = new Paging() { PageNumber = -1, PageSize = -1, DefaultOrderByColumn = "ExamRollNumber" };
                resultlist = new ResultManager().GetDetailsForEngForInsertion(parameter, printProgramme,semester,false);

                //Chk here whether this resultlist exist in MSFullDetails Table or not, if not then insert Student_Id
                
               if(resultlist.IsNotNullOrEmpty())
                {
                    if (CourseId == "8497707B-1E6B-4E40-9B89-469E79012E5E")//Electrical Engineering 2017 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 69).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }
                    else if(CourseId == "BEF12A56-16B9-443A-9282-01FE86440FC1")//Electronics and Communication Engineering 2017 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 72).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }

                    else if (CourseId == "0C470F58-6507-494C-9A36-FEBB77E93E68")//Computer Science and Engineering 2018 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 72).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }
                    else if (CourseId == "CF6B1904-8BBD-45C0-BC33-7B2B8606A60F")//Mechanicle Engineering 2018 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 74).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }
                    //else if (CourseId == "4C96F29A-635E-42ED-B9FD-A0385F0F83B1")//Civil Engineering 2018 batch
                    //{
                    //    resultlist = resultlist.Where(x => x.SubjectResults.Count > 73).ToList();// 73 retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    //}
                    else if (CourseId == "FCFD3241-3436-4537-AA17-0EDAE9733395")//BioMedical Engineering 2018 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 69).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }
                    else if (CourseId == "691D6D86-7DD8-4FCD-85DB-300C49AF9F2D")//Electrical and Electronics Engineering 2018 batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 72).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)

                    }
                    else//Civil Engineering 2017 Batch
                    {
                        resultlist = resultlist.Where(x => x.SubjectResults.Count > 70).ToList();//retrieve  only those students who have passed all subjects from the list (Minimum Subjects 71)
                    }

                    List<Guid> StudentIDs = resultlist.Select(x => x.Student_ID).Distinct().ToList();
                    List<ResultCompact> MSFullDetailsList =
                        new ResultManager().ChkDataMSFullDetails(StudentIDs);//Checks whether the Student IDs exist in new table or not
                    if (MSFullDetailsList == null)
                    {
                        //then insert all data into MSFullDetails table and MSStatus Table.                        
                        new ResultManager().Save(StudentIDs);
                    }
                    else
                    {
                        List<ResultCompact> studIDList = new ResultManager().ChkDatastudIDList();//Check all StudentIds already stored in MSFullDetails
                        var NewList = resultlist.Except(studIDList).ToList();//Students who have passed and have not yet generated marksheet.(For Backlog and other students)
                        if (NewList.Count > 0)
                        {
                            List<Guid> NewStudentIdsList = NewList.Select(x => x.Student_ID).Distinct().ToList();
                            new ResultManager().Save(NewStudentIdsList);
                            
                        }

                    
                    }

                }
                var college_id = parameter.Filters?.FirstOrDefault(x => x.Column == "AcceptCollege_ID")?.Value;
                ViewBag.Batch = parameter.Filters?.FirstOrDefault(x => x.Column == "batch")?.Value;
                if (college_id != null)
                    ViewBag.CollegeFullName = new CollegeManager().GetItem(Guid.Parse(college_id)).CollegeFullName;
                var course_Id = parameter.Filters?.FirstOrDefault(x => x.Column == "Course_ID")?.Value;
                if (course_Id.IsNotNullOrEmpty())
                    ViewBag.CourseFullName = new CourseManager().GetCompactItem(Guid.Parse(course_Id)).CourseFullName;
                ViewBag.Semester = semester;
                ViewBag.PrintProgramme = printProgramme;
            }

            
            return View(resultlist);
        }

        #endregion

        #region MarksSheetList
        [OAuthorize(AppRoles.MarksSheetList, AppRoles.University)]
        public ActionResult List()
        {
            FillViewBag_College();
            FillViewBag_PrintProgrammes();
            FillViewBag_Course(Programme.UG);
            FillViewBag_Batches();
           // ViewBag.Batches = Helper.GetYearsDDL().OrderByDescending(x => x.Value);

            ViewData["PageSize"] = -1;
            return View();
        }
       
        

        [OAuthorize(AppRoles.MarksSheetList, AppRoles.University)]
        public PartialViewResult  MarksSheetStatus(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<MarksSheet> list = new ResultManager().MarksSheetStatus(otherParam1.Value, parameter);
            ViewBag.PrintProgramme = otherParam1;
            return PartialView(list);
        }
 


        [OAuthorize(AppRoles.MarksSheetList, AppRoles.University)]
        public ActionResult PrintMarksSheet(int? SemesterBatch, PrintProgramme PrintProgramme, Guid? Course_ID, Guid? AcceptCollege_ID, string CUSRegistrationNo
       , string GreaterThanDate, string LessThanDate, short? PrintedOn, short? ValidatedOn, short? HandedOverOn)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>(), PageInfo = new Paging() { DefaultOrderByColumn = "ExamRollNumber", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort { ColumnName = "ExamRollNumber" } };
            //parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Value = ((short)PrintProgramme).ToString() });
            
            SearchFilter programefilter = parameters.Filters.Where(x => x.Column == "Programme").FirstOrDefault();
            string Programme = string.Empty;
            if (programefilter == null)
            {
                Programme = "5";
            }

            if (AcceptCollege_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = AcceptCollege_ID.ToString() });
            if (Course_ID.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = Course_ID.ToString(), TableAlias = "C" });
            if (SemesterBatch.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "Batch", Value = SemesterBatch.ToString() });
            if (!string.IsNullOrEmpty(GreaterThanDate) && DateTime.TryParse(GreaterThanDate, out DateTime date1))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date1.ToString(), TableAlias = "MSCGPA", Operator = SQLOperator.GreaterThanEqualTo });
            if (!string.IsNullOrEmpty(LessThanDate) && DateTime.TryParse(LessThanDate, out DateTime date2))
                parameters.Filters.Add(new SearchFilter() { Column = "CreatedOn", Value = date2.ToString(), TableAlias = "MSCGPA", Operator = SQLOperator.LessThanEqualTo });
            if (ValidatedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "ValidatedOn", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (PrintedOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "PrintedOn", Operator = (PrintedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (HandedOverOn.HasValue)
                parameters.Filters.Add(new SearchFilter() { Column = "ValidatedOn", Operator = (ValidatedOn.Value == 1 ? SQLOperator.ISNotNULL : SQLOperator.ISNULL) });
            if (!string.IsNullOrEmpty(CUSRegistrationNo))
                parameters.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = CUSRegistrationNo });
            List<ResultCompact> MarksSheetList  = new ResultManager().FinalData(PrintProgramme, parameters);
            return View(MarksSheetList);
        }

        [OAuthorize(AppRoles.MarksSheetList, AppRoles.University)]
        public JsonResult Printed(Guid Student_ID)
        {
            ResponseData response = new ResultManager().Printed(Student_ID);
            return Json(response);
        }

        #endregion

    }

}
 