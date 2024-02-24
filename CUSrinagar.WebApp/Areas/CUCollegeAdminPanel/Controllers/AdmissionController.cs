using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI;
using System.Data;
using Newtonsoft.Json;

namespace CUSrinagar.WebApp.CUCollegeAdminPanel.Controllers
{
    [OAuthorize(AppRoles.College)]//temporary changed college to university-test
    public class AdmissionController : CollegeAdminBaseController
    {
        public ActionResult UploadedCertificateIH()
        {
            FillViewBag_Course(AppUserHelper.College_ID, PrintProgramme.IH, null);
            FillViewBag_SelectionStatus();
            return View();
        }
        public PartialViewResult UploadCertificateIHList(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<SelectedStudent> list = new StudentManager().GetSelectedStudentsList(parameter, otherParam1.Value);
            return PartialView(list);
        }


        public void UploadCertificateIHCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<SelectedStudent> list = new StudentManager().GetSelectedStudentsList(parameter, printProgramme.Value);
            var reportList = list.Select(x => new
            {
                x.CollegeFullName,
                Programe = Helper.GetEnumDescription(x.Programme),
                x.CourseFullName,
                x.TotalPoints,
                x.StudentFormNo,
                x.FullName,
                x.Parentage,
                x.Category,
                x.Gender,
                Status = Helper.GetEnumDescription<StudentSelectionStatus>(x.StudentSelectionStatus),
                x.NumberOfCertificate
            }).ToList();
            ExportToCSV(reportList, "Selection List" + printProgramme.ToString() + DateTime.Now.ToString("dd/mm/yyyy"));
        }

        public ActionResult UploadedCertificate(Guid Student_ID, PrintProgramme printProgramme)
        {
            FillViewBag_SelectionStatus();
            FillViewBag_VerificationStatusOfCertificates();
            var certificates = new StudentManager().GetUploadedCertificates(Student_ID);
            if (certificates.IsNotNullOrEmpty())
            {
                var studentPersonalInfo = new RegistrationManager().GetStudent(Student_ID, printProgramme, false,true, true, true);
                studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Joined
                     || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded) && x.PrintProgramme == printProgramme).ToList();
                ViewBag.ARGPersonalInformation = studentPersonalInfo;
            }
            GetResponseViewBags();
            return View(certificates);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadedCertificate(List<Certificate> model, Guid Course_ID, StudentSelectionStatus StudentSelectionStatus, PrintProgramme printProgramme)
        {
            ResponseData response = new ResponseData();
            response = new StudentManager().SaveUpdate(model, printProgramme, Course_ID, StudentSelectionStatus);

            SetResponseViewBags(response);
            return RedirectToAction("UploadedCertificate", new { Student_ID = model.First().Student_ID, printProgramme = printProgramme });

        }





        //[HttpPost]
        //public JsonResult PostCertificateListItem(Certificate model)
        //{
        //    ResponseData response = new ResponseData();
        //    //if (ModelState.IsValid)
        //    //    response = new ResultManager().SaveUpdate(model);
        //    //else
        //    //    response.ErrorMessage = string.Join(", ", ModelState.Values.SelectMany(s => s.Errors).Select(y => y.ErrorMessage).ToArray());
        //    return Json(response);
        //}

        public ActionResult NewAdmission()
        {
            FillViewBag_Course(AppUserHelper.College_ID, PrintProgramme.UG, Programme.UG);
            return View();
        }

        public PartialViewResult NewAdmissionListPartial(Parameters parameter, PrintProgramme? otherParam1)
        {
            List<StudentCollegePreferenceList> list = new StudentManager().GetCollegePreference(parameter);
            return PartialView(list);
        }

        public void NewAdmissionCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            List<StudentCollegePreferenceList> list = new StudentManager().GetCollegePreference(parameter);
            var reportList = list.Select(x => new
            {
                x.Batch,
                x.CourseFullName,
                x.Category,
                x.StudentFormNo,
                x.FullName,
                x.Gender,
                x.PreferenceOrder,
                x.EntranceRollNo,
                x.CATEntrancePoints
            }).ToList();
            ExportToCSV(reportList, "New Admission List" + DateTime.Now.ToString("dd/MMM/yyyy"));
        }

        public ActionResult NewUGAdmissionForms()
        {
            return View();
        }


        public void NewUGAdmissionFormsCSV(Parameters parameter, PrintProgramme? printProgramme)
        {
            var AdmissionFormsCSV = new StudentManager().NewUGAdmissionFormsCSV(parameter);
            DownloadExcel(AdmissionFormsCSV,((AdmissionFormsCSV?.Rows?.Count??0)==0?"No Record Found-": "New UG Admission List_") + DateTime.Now.ToString("dd-MMM-yy"));
        }
    }
}