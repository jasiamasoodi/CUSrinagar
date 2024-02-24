using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class AttemptCertificateController : Controller
    {
        // GET: AttemptCertificate
        public ActionResult Index()
        {
            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View();
        }
        [HttpPost]
        public ActionResult GetDetails(AttemptCertificate attemptCertificate)
        {
            ModelState.Remove("ReasonforIssuance");
            if (string.IsNullOrWhiteSpace(attemptCertificate.personalInformationCompact.CUSRegistrationNo))
            {
                ModelState.AddModelError("personalInformationCompact.CusRegistrationNo", "CusRegistrationNo Required");
            }
            else if (attemptCertificate.personalInformationCompact.DOB == DateTime.MinValue)
            {
                ModelState.AddModelError("personalInformationCompact.DOB", "DOB Required");
            }
            else
            {
                attemptCertificate.personalInformationCompact = new StudentManager().GetStudent(attemptCertificate.personalInformationCompact.CUSRegistrationNo, attemptCertificate.personalInformationCompact.DOB, attemptCertificate.personalInformationCompact.PrintProgramme);
            }
            if (attemptCertificate.personalInformationCompact == null)
            {
                ModelState.AddModelError("CusRegistrationNo", "Student Not Found");
            }
            else if (!((attemptCertificate.personalInformationCompact ?? new PersonalInformationCompact()).IsPassout))
            {
                ModelState.AddModelError("CusRegistrationNo", "Student has not been awarded degree yet");
            }
            if (ModelState.IsValid)
            {

                AttemptCertificate attemptCertificate1 = new CertificateManager().GetByStudentID(attemptCertificate.personalInformationCompact.Student_ID);
                if (attemptCertificate1 != null)
                {
                    if (attemptCertificate1.FeeStatus == FormStatus.FeePaid)
                    {
                        return RedirectToAction("Print", new { Certificate_Id = attemptCertificate1.Certificate_ID, Student_Id = attemptCertificate1.Student_Id, PP = attemptCertificate.personalInformationCompact.PrintProgramme });
                    }
                    else
                    { //return RedirectToAction("AttemptCertificate", "Payment", new { id = attemptCertificate1.Student_Id + "/", PP = ((int)attemptCertificate.personalInformationCompact.PrintProgramme).ToString().EncryptCookieAndURLSafe(), TF = attemptCertificate1.TotalFee.ToString().EncryptCookieAndURLSafe(), CID = attemptCertificate1.Certificate_ID.ToString().EncryptCookieAndURLSafe(), area = string.Empty }); }
                        attemptCertificate1 = new CertificateManager().Get(attemptCertificate1.Certificate_ID);
                        attemptCertificate1.personalInformationCompact = attemptCertificate.personalInformationCompact;
                        return View("AttemptCertificate", attemptCertificate1);
                    }
                }
                attemptCertificate.AttemptCertificateDetailsList = new List<AttemptCertificateDetails>();
                for (int i = 0; i < attemptCertificate.personalInformationCompact.Duration; i++)
                { attemptCertificate.AttemptCertificateDetailsList.Add(new AttemptCertificateDetails() { Semester = i + 1 }); }
                return View("AttemptCertificate", attemptCertificate);

            }

            ViewBag.PrintProgrammeList = Helper.GetSelectList<PrintProgramme>().OrderBy(x => x.Value);
            return View("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult SaveDetails(AttemptCertificate attemptCertificate)
        {
            if (attemptCertificate.AttemptCertificateDetailsList.Where(x => x.ActualYearPassing <= 0).Count() > 0)
            {
                ModelState.AddModelError("ActualYearPassing", "Actual Year of Passing is required for each semester");
            }
            if (attemptCertificate.AttemptCertificateDetailsList.Where(x => x.ExpectedYearPassing <= 0).Count() > 0)
            {
                ModelState.AddModelError("ExpectedYearPassing", "Expected Year of Passing is required for each semester");
            }
            if (ModelState.IsValid)
            {

                attemptCertificate.TotalFee = 300;
                attemptCertificate.Student_Id = attemptCertificate.personalInformationCompact.Student_ID;
                if ((attemptCertificate.personalInformationCompact.Student_ID == null) || (attemptCertificate.personalInformationCompact.Student_ID == Guid.Empty))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Form does not exist.</a></div>";
                    ViewBag.PrintProgrammeIs = attemptCertificate.personalInformationCompact.PrintProgramme;
                    return View("Index");
                }
                if (attemptCertificate.Certificate_ID == null || attemptCertificate.Certificate_ID == Guid.Empty)
                {
                    attemptCertificate.Certificate_ID = Guid.NewGuid();
                    attemptCertificate.UpdatedOn = DateTime.Now;
                    attemptCertificate.CreatedOn = DateTime.Now;
                    attemptCertificate.FeeStatus = FormStatus.InProcess;
                    if (new CertificateManager().SaveAttemptCertificate(attemptCertificate))
                    { return RedirectToAction("AttemptCertificate", "Payment", new { id = attemptCertificate.personalInformationCompact.Student_ID + "/", PP = ((int)attemptCertificate.personalInformationCompact.PrintProgramme).ToString().EncryptCookieAndURLSafe(), TF = attemptCertificate.TotalFee.ToString().EncryptCookieAndURLSafe(), CID = attemptCertificate.Certificate_ID.ToString().EncryptCookieAndURLSafe(), area = string.Empty }); }
                }
                else
                {
                    attemptCertificate.UpdatedOn = DateTime.Now;
                    if (new CertificateManager().EditAttemptCertificate(attemptCertificate))
                    { return RedirectToAction("AttemptCertificate", "Payment", new { id = attemptCertificate.personalInformationCompact.Student_ID + "/", PP = ((int)attemptCertificate.personalInformationCompact.PrintProgramme).ToString().EncryptCookieAndURLSafe(), TF = attemptCertificate.TotalFee.ToString().EncryptCookieAndURLSafe(), CID = attemptCertificate.Certificate_ID.ToString().EncryptCookieAndURLSafe(), area = string.Empty }); }
                }
            }
            return View("AttemptCertificate", attemptCertificate);
        }


        //public void RePrint(string CusRegistrationNo, DateTime DOB, PrintProgramme programme, MigrationE formType)
        //{
        //    var studentInfo = new MigrationManager().GetMigrationStudent(CusRegistrationNo, DOB, programme);

        //    if (studentInfo != null && studentInfo.SelectedCombinations != null)
        //    {
        //        RedirectToAction("Print", new { Student_Id = studentInfo.Student_ID, TypeIs = formType.ToString(), PP = programme });
        //    }

        //}
        public ActionResult Print(Guid Certificate_Id, Guid Student_Id, PrintProgramme PP)
        {
            AttemptCertificate attemptCertificate = new CertificateManager().Get(Certificate_Id);
            if (attemptCertificate != null && attemptCertificate.FeeStatus != FormStatus.FeePaid)
            { return new EmptyResult(); }
            attemptCertificate.personalInformationCompact = new StudentManager().GetStudentC(attemptCertificate.Student_Id, PP);
            PaymentDetails payment = new CertificateManager().GetPayment(attemptCertificate);
            attemptCertificate.TransactionDate = payment.TxnDate;
            attemptCertificate.TransactionNo = payment.TxnReferenceNo;
            return View("Print", attemptCertificate);
        }
    }
}