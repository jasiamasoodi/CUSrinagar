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
using CUSrinagar.BusinessManagers;
using Newtonsoft.Json;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]
    public class ExaminationController : CollegeAdminBaseController
    {

        public ActionResult BulkExamaminationForm()
        {
            Parameters parameter = new Parameters()
            {
                SortInfo = new Sort() { ColumnName = "CourseFullName,CUSRegistrationNo", OrderBy = System.Data.SqlClient.SortOrder.Ascending },
                PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 },
                Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = "4", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, TableAlias = "COMB" }
                }
            };
            List<string> listOfEligibleStudents = ListOfEligibleStudents();
            parameter.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = string.Join(",", listOfEligibleStudents.ToList()), Operator = SQLOperator.In });

            //parameter.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = "F1FC5214-041F-4B5A-92CC-F585F7174527,9D03A374-4398-4A48-BE2A-FD9911EC6F82", Operator = SQLOperator.NotIn });

            List<CourseWiseList> list = new AssignCombinationManager().GetStudentDetails(parameter, PrintProgramme.UG);
            return View(list);
        }
        public ActionResult BulkExamaminationFormWithSubjects()
        {
            Parameters parameter = new Parameters()
            {
                SortInfo = new Sort() { ColumnName = "AcceptCollege_ID,CourseFullName,ClassRollNo,CUSRegistrationNo", OrderBy = System.Data.SqlClient.SortOrder.Ascending },
                PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 },
                Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = "3", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, TableAlias = "COMB" }
                 }
            };

            List<string> listOfEligibleStudents = ListOfEligibleStudents();
            parameter.Filters.Add(new SearchFilter() { Column = "CUSRegistrationNo", Value = string.Join(",", listOfEligibleStudents.ToList()), Operator = SQLOperator.In });

            //parameter.Filters.Add(new SearchFilter() { Column = "AcceptCollege_ID", Value = "90AE5E98-3A8F-4B15-A652-7EAE5A203C42", Operator = SQLOperator.In });
            //parameter.Filters.Add(new SearchFilter() { Column = "Course_ID", Value = "48887D19-F0C3-41CB-AC7A-22EA7B65494A", Operator = SQLOperator.In });
            parameter.Filters.Add(new SearchFilter() { Column = "Batch", Value = "2017", Operator = SQLOperator.In });

            List<CourseWiseList> list = new AssignCombinationManager().GetStudentDetails(parameter, PrintProgramme.UG);
            List<ARGPersonalInformation> masterList = new List<ARGPersonalInformation>();
            if (list.IsNotNullOrEmpty())
            {
                foreach (var item in list)
                {
                    ARGPersonalInformation std = new StudentManager().GetStudentWithCombination(item.Student_ID, PrintProgramme.UG, false, 3);
                    if (std != null && std.SelectedCombinations.IsNotNullOrEmpty())
                        masterList.Add(std);
                }
            }
            //return View(masterList);
            // return View("BulkExamaminationFormWithSubjectsPG1B", masterList);
            //return View("BulkExamaminationFormWithSubjectsPG2B", masterList);
            //return View("BulkExamaminationFormWithSubjectsPG3B", masterList);
            return View("BackLogFormWithPayment", masterList);
            //return View("RegularFormWithPaymentWithSubjects", masterList);
            //return View("BackLogFormWithPayment", masterList);
            //return View("BulkExamaminationFormWithSubjectsPGMED2B", masterList);
            //return View("BulkExamaminationFormWithSubjectsUG2B", masterList);
            //return View("BulkExamaminationFormWithSubjectsIH2", masterList);
            //return View("BulkExamaminationFormWithSubjectsUG2", masterList);
            //return View("BulkExamaminationFormWithSubjectsIH5", masterList);
            //return View("BulkExamaminationFormWithSubjectsUG5", masterList);
            //return View("BulkExamaminationFormWithSubjectsENG2", masterList);
            //return View("BulkExamaminationFormWithSubjectsENG2B", masterList);
        }

        private List<string> ListOfEligibleStudents()
        {
            List<string> listOfEligibleStudents = new List<string>();
            listOfEligibleStudents.Add("CUS-17-ISE-10355");
            return listOfEligibleStudents;
        }



        public ActionResult SingleExamaminationForm(PrintProgramme printProgramme, string CUSRegistrationNo)
        {
            Parameters parameter = new Parameters()
            {
                SortInfo = new Sort() { ColumnName = "CourseFullName,CUSRegistrationNo", OrderBy = System.Data.SqlClient.SortOrder.Ascending },
                PageInfo = new Paging() { DefaultOrderByColumn = "CUSRegistrationNo", PageNumber = -1, PageSize = -1 },
                Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Semester", Value = "1", GroupOperation = LogicalOperator.AND, Operator = SQLOperator.EqualTo, TableAlias = "COMB" },
                 new SearchFilter() { Column = "CUSRegistrationNo", Value = CUSRegistrationNo.ToString(), GroupOperation = LogicalOperator.AND, Operator = SQLOperator.In }
                }
            };
            List<CourseWiseList> list = new AssignCombinationManager().GetStudentDetails(parameter, printProgramme);
            return View(list);
        }


        public ActionResult ExaminationForm(PrintProgramme? programme)
        {
            //FillViewBag_Batches();
            FillViewBag_Programmes(AppUserHelper.College_ID);
            FillViewBag_Course(AppUserHelper.College_ID, null, Programme.UG);
            FillViewBag_Semesters();
            GetResponseViewBagsForExaminationWidget();
            return View();
        }
        public ActionResult ExaminationFormListPartail(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<StudentExamFormList> list = null;
            if (otherParam1.HasValue)
            {
                list = new ExaminationFormManager().GetExaminationFormList(parameter, otherParam1.Value);
            }
            return View(list);
        }
        public String GetExaminationSummary(Parameters parameter, PrintProgramme? otherParam1)
        {
            ExaminationFormSummary summary = new ExaminationFormSummary();
            if (otherParam1.HasValue)
            {
                summary = new ExaminationFormManager().GetExaminationFormSummary(parameter, otherParam1.Value) ?? new ExaminationFormSummary();
            }
            return JsonConvert.SerializeObject(summary);
        }

        public ActionResult AcceptExaminationForm()
        {
            FillViewBags();
            return View();
        }
        public String PostExamForm(ARGStudentExamForm StudentExamForm, PrintProgramme? printProgramme)
        {
            try
            {
                StudentExamForm.Status = FormStatus.Accepted;
                ResponseData response = new ExaminationFormManager().UpdateStudentExamForm(StudentExamForm, printProgramme.Value);
                return JsonConvert.SerializeObject(response);
            }
            catch (InvalidCastException)
            {
                return JsonConvert.SerializeObject(new ResponseData { IsSuccess = false, ErrorMessage = "User is not authorized, please logout and login again." });
            }
        }

        void FillViewBags()
        {
            ViewBag.College_ID = AppUserHelper.College_ID;
            ViewBag.PrintProgrammeList = Helper.GetSelectList(PrintProgramme.BED).OrderBy(x => x.Text);
            ViewBag.CollegeCourses = new CourseManager().GetCourseList(AppUserHelper.College_ID.Value, PrintProgramme.UG) ?? new List<SelectListItem>();
        }

        private void GetResponseViewBagsForExaminationWidget()
        {
            ViewBag.PrintProgrammes = Helper.GetSelectList<PrintProgramme>();
            ViewBag.Semesters = new SelectList(
                                                new List<SelectListItem>
                                                {
                                                    new SelectListItem { Selected = false, Text = "Semester I", Value = 1.ToString()},
                                                    new SelectListItem { Selected = true, Text = "Semester II", Value = 2.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester III", Value = 3.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester IV", Value = 4.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester V", Value = 5.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VI", Value = 6.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VII", Value = 7.ToString()},
                                                    new SelectListItem { Selected = false, Text = "Semester VIII", Value = 8.ToString()},

                                                }, "Value", "Text", 1);

            var ExaminationYears = new List<SelectListItem>();
            for (int item = 2017; item <= DateTime.Now.Year; item++)
            {
                ExaminationYears.Add(new SelectListItem { Selected = false, Text = item.ToString(), Value = item.ToString() });
            }

            ViewBag.ExaminationYears = new SelectList(ExaminationYears, "Value", "Text", 1);
        }

        public void ExamFormCSV(Parameters parameter, PrintProgramme printProgramme)
        {
            if (parameter.PageInfo == null)
                parameter.PageInfo = new Paging();
            parameter.PageInfo.PageNumber = parameter.PageInfo.PageSize = -1;

            List<StudentExamFormList> list = new ExaminationFormManager().GetExaminationFormList(parameter, printProgramme);
            var reportList = list.Select(x => new
            {
                x.SemesterBatch,
                Programe = Helper.GetEnumDescription((PrintProgramme)printProgramme),
                x.CourseFullName,
                x.CUSRegistrationNo,
                x.Semester,
                x.Year,                
                x.FullName,
                x.FormNumber,
                x.ExamRollNumber,
                x.ClassRollNo,                
                Type = x.IsRegular == true ? "Regular" : "Backlog",
                IsVerified = x.IsVerified == true ? "Verified" : "Not Verified",
                ExamFormStatus =x.Status==FormStatus.Accepted ? "Applied" : "Not Applied"
            }).ToList();
        }

    }
}