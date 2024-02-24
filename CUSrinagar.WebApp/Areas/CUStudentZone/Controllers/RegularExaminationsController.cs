using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class RegularExaminationsController : Controller
    {
        // GET: CUStudentZone/RegularExaminations
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetSemesterOpenForExaminationType()
        {
            List<SelectListItem> list = new RegularExaminationsManager().GetAllowedSemesterExaminations();
            return Json(list);
        }

        [HttpGet]
        public ViewResult RegularExaminationForm(short semester)
        {
            return View(new RegularExaminationsManager().GetExaminationFormViewModel(semester));
        }

        [HttpPost]
        public ActionResult RegularExaminationForm(RegularExaminationViewModel examinationViewModel)
        {
            if (string.IsNullOrEmpty(examinationViewModel.Email) || string.IsNullOrEmpty(examinationViewModel.PhoneNumber))
            {
                ViewBag.Response = "Please enter your Email and Phone Number";
                return RedirectToAction("RegularExaminationForm", new { @semester = examinationViewModel.ApplyingForSemester });
            }

            var examinationForm = new RegularExaminationsManager().GetExaminationForm(examinationViewModel);

            if (examinationForm.Status == FormStatus.Accepted && examinationForm.AmountPaid > 0)
                RedirectToAction("PrintExaminationFormReceipt", examinationForm);

            return RedirectToAction("RegularExaminationPayment", "RegularExaminations", new object() { });
        }

        [HttpGet]
        public ActionResult RegularExaminationPayment(RegularExaminationForm examinationForm, string Email, string PhoneNumber)
        {
            if (examinationForm != null && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(PhoneNumber))
                CreateBillDeskRequest(examinationForm, Email, PhoneNumber);

            return View(examinationForm);
        }

        private void CreateBillDeskRequest(RegularExaminationForm examinationForm, string email, string phoneNumber)
        {
            var amount = examinationForm.AmountToBePaid + examinationForm.LateFeeAmount;

            BillDeskRequest billDeskRequest = new BillDeskRequest()
            {
                Entity_ID = examinationForm.StudentExamForm_ID,
                Email = email,
                PhoneNo = phoneNumber,
                TotalFee = Convert.ToInt16(amount),
                CustomerID = DateTime.Now.Ticks.ToString(),
                PrintProgramme = AppUserHelper.TableSuffix,
                Student_ID = examinationForm.Student_ID,
                Semester = examinationForm.Semester.ToString(),
                ReturnURL = "CUStudentZone/Examination/PaymentResponse",
                AdditionalInfo = examinationForm.FormNumber
            };

            var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.EXM);
            var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
            System.Web.HttpContext.Current.Response.End();
        }
    }
}