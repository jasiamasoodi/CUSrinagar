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
    [OAuthorize(AppRoles.University_StudentInfo, AppRoles.University_StudentProfile)]
    public class StudentInfoController : AdminController
    {
        public ActionResult SearchStudent(string ErrorMessage = "")
        {
            ViewBag.ErrorMessage = ErrorMessage;
            List<PersonalInformationCompact> personalInformation = new List<PersonalInformationCompact>();
            return View(personalInformation);
        }

        [HttpPost]
        public ActionResult GetList(string SearchQuery)
        {
            List<PersonalInformationCompact> personalInformation = new StudentManager().GetStudentInfoBySearchQuery(SearchQuery) ?? new List<PersonalInformationCompact>();

            return View("SearchStudent", personalInformation);
        }
        public ActionResult GetInfo(string PrintProgramme, string Student_ID)
        {

            ViewBag.PrintProgramme = PrintProgramme;
            ViewBag.Student_ID = Student_ID;
            return View();

        }
        public PartialViewResult PersonalDetail(Guid student_Id, PrintProgramme printProgramme)
        {
            ARGPersonalInformation pinfo = new StudentManager().GetStudent(student_Id, printProgramme);
            ViewBag.PrintProgramme = printProgramme;
            ViewBag.MigrationCollege = new CollegeManager().GetADMCollegeMasterList().Where(x => x.Value != pinfo.AcceptCollege_ID.ToString().ToUpper());
            return PartialView(pinfo);
        }
        public PartialViewResult ExaminationDetail(Guid student_Id, PrintProgramme printProgramme)
        {
            List<Semesters> ExamForms = new StudentManager().GetStudentExaminationHistory(student_Id, printProgramme, false);
            return PartialView(ExamForms);
        }

        public PartialViewResult AdmissionDetail(Guid student_Id, PrintProgramme printProgramme)
        {
            List<AdmissionForm> AdmissionForms = new StudentManager().GetStudentAdmissionFormHistory(student_Id, printProgramme);
            return PartialView(AdmissionForms ?? new List<AdmissionForm>());
        }
        public PartialViewResult PaymentDetail(Guid student_Id, PrintProgramme printProgramme)
        {
            List<StudentPayment> StudentPayment = new StudentManager().GetStudentPaymentHistory(student_Id, printProgramme);
            return PartialView(StudentPayment ?? new List<StudentPayment>());
        }

        #region Migration
        public JsonResult CancelAdmission(PrintProgramme PrintProgramme, Guid Student_ID)
        {
            ResponseData response = new StudentManager().CancelAdmission(PrintProgramme, Student_ID);
            return Json(response);
        }
        public JsonResult IntraMigration(PrintProgramme PrintProgramme, Guid Student_ID, Guid MigrationCollege_id)
        {
            Guid College_id = MigrationCollege_id;
            ResponseData response = new StudentManager().IntraMigration(PrintProgramme, Student_ID, College_id);

            return Json(response);
        }
        public JsonResult InterMigration(PrintProgramme PrintProgramme, Guid Student_ID)
        {
            ResponseData response = new StudentManager().InterMigration(PrintProgramme, Student_ID);
            return Json(response);
        }

        public JsonResult IssueMigration(PrintProgramme PrintProgramme, Guid Student_ID)
        {
            ResponseData response 
                = new StudentManager().IssueMigration(PrintProgramme, Student_ID);

            return Json(response);
        }

        public JsonResult ReceiveMigration(PrintProgramme PrintProgramme, Guid Student_ID)
        {
            ResponseData response 
                = new StudentManager().ReceiveMigration(PrintProgramme, Student_ID);
            return Json(response);
        }
        #endregion
    }
}