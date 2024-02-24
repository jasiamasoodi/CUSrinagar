using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class PaymentController : Controller
    {
        #region New admission
        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult MakePayment()
        {
            try
            {
                List<PrintProgramme> _PrintProgrammeOption = new BillDeskManager().GetPaymentCategory();
                if (!(_PrintProgrammeOption?.Any(x => x == AppUserHelper.TableSuffix) ?? false))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Payment is either closed or will be allowed soon for {AppUserHelper.OrgPrintProgramme.GetEnumDescription()}</a></div>";
                    return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                }
                ARGFormNoMaster aRGFormNoMaster = new RegistrationManager().GetFormNoMaster(AppUserHelper.TableSuffix);
                if (aRGFormNoMaster?.ValidatePaymentByCourse ?? true)
                {
                    List<Guid> OpenAdmissionCourses =
                        new CourseManager().GetCourseListForRegistration(AppUserHelper.TableSuffix)?.Select(c => c.Course_ID)?.ToList();
                    if (OpenAdmissionCourses.IsNullOrEmpty())
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Payment is either closed or will be allowed soon for {AppUserHelper.OrgPrintProgramme.GetEnumDescription()}</a></div>";
                        return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                    }

                    List<Guid> AdmissionCoursesApplied =
                        new RegistrationManager().GetStudentCourses(AppUserHelper.User_ID, AppUserHelper.TableSuffix).Select(c => c.Course_ID).ToList();

                    if (OpenAdmissionCourses.Intersect(AdmissionCoursesApplied).Count() <= 0)
                    {
                        TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Payment is either closed or will be allowed soon for {AppUserHelper.OrgPrintProgramme.GetEnumDescription()}</a></div>";
                        return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                    }
                }
                ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, AppUserHelper.TableSuffix);

                //fee already submitted
                if ((AppUserHelper.OrgPrintProgramme == PrintProgramme.IH || AppUserHelper.OrgPrintProgramme == PrintProgramme.UG) && aRGPersonalInformation.Batch <= 2018)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Old Batch is not eligible for Online Payment. Note : You should be able to print your Form.</a></div>";
                    return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                }
                else if (aRGPersonalInformation.FormStatus != FormStatus.InProcess)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> It seems that you have already paid fee. Note : You should be able to print your Form.</a></div>";
                    return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                }
                return RedirectToAction("Index", "Payment", new { id = AppUserHelper.User_ID + "/", R = ((int)AppUserHelper.TableSuffix).ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
            }catch (Exception) {

                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> It seems that you have incomplete registration form.</a></div>";
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
            }
        }


        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Index(string id, string R)
        {
            if (Request.UrlReferrer == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Refresh is not allowed.</a></div>";
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
            }

            if (!Guid.TryParse(id + "", out Guid result) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(result, programme, true);
            if (aRGPersonalInformation == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
            }
            Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme programme2);
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = aRGPersonalInformation.StudentAddress.Email,
                PhoneNo = aRGPersonalInformation.StudentAddress.Mobile,
                ReturnURL = $"CUStudentZone/Payment/PaymentSuccess",
                PrintProgramme = programme2,
                Entity_ID = result,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                TotalFee = aRGPersonalInformation.TotalFee
            };
            StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.OTH));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }

        public ActionResult PaymentSuccess()
        {
            if (Request.InputStream == null)
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveRegistrationPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
                }
                else
                {
                    new PaymentManager().SendPaymentSMSAndEmailAsyc(billDeskResponse.Item3);
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";
                    return RedirectToAction("Detail", "Registration", new { id = billDeskResponse.Item3.Entity_ID + "/", R = ((int)billDeskResponse.Item3.PrintProgramme).ToString().EncryptCookieAndURLSafe(), area = "" });
                }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });
            }
        }

        #endregion



        #region NextSemesterAdmissionFee
        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult SemAdmissionFee(string S, string F)
        {
            if (Request.UrlReferrer == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Refresh is not allowed.</a></div>";
                return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
            }

            S = Convert.ToString(S).Trim().DecryptCookieAndURLSafe();
            short.TryParse(S, out short AdmSemester);

            F = Convert.ToString(F).Trim().DecryptCookieAndURLSafe();
            decimal.TryParse(F, out decimal SemTotalFee);

            if (AdmSemester <= 0 || SemTotalFee <= 0m)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
            }

            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, AppUserHelper.TableSuffix, true);
            if (aRGPersonalInformation == null)
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'> Details are not valid. Please try again.</a></div>";
                return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
            }

            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = (Convert.ToString(aRGPersonalInformation.StudentAddress.Email).Trim() == string.Empty) ? "shahid@cusrinagar.edu.in" : aRGPersonalInformation.StudentAddress.Email.Trim(),
                PhoneNo = (Convert.ToString(aRGPersonalInformation.StudentAddress.Mobile).Trim() == string.Empty) ? "7006448099" : aRGPersonalInformation.StudentAddress.Mobile.Trim(),
                ReturnURL = $"CUStudentZone/Payment/SemAdmPaymentSuccess",
                PrintProgramme = AppUserHelper.OrgPrintProgramme,
                Entity_ID = aRGPersonalInformation.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                Semester = AdmSemester.ToString(),
                TotalFee = Convert.ToInt32(SemTotalFee)
            };
            StringBuilder sbHTML = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }

        public ActionResult SemAdmPaymentSuccess()
        {
            if (Request.InputStream == null)
                return RedirectToAction("StudentProfile", "Student", new { area = "CUStudentZone" });

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveSemesterAdmissionPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
                }
                else
                {
                    TempData["alert"] = $"Payment received successfully under TxnReferenceNo. :{billDeskResponse.Item3.TxnReferenceNo} Amount (Rs): {billDeskResponse.Item3.TxnAmount}";

                    //if (billDeskResponse.Item3.Semester > 4)
                    //{
                    return RedirectToAction("SemesterAdmForm", "Student", new { S = ((int)billDeskResponse.Item3.Semester).ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
                    //}
                    //else
                    //{
                    //    return RedirectToAction("GetCombination", "Combination", new { semester = ((int)billDeskResponse.Item3.Semester).ToString(), area = "CUStudentZone" });
                    //}
                }
            }
            else
            {
                string ErrorMsg = (string.IsNullOrEmpty(billDeskResponse.Item2) || billDeskResponse.Item2.Trim().ToLower() == "na" || billDeskResponse.Item2.Trim().ToLower() == "na - na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : billDeskResponse.Item2;
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("ApplySemesterAdm", "Student", new { area = "CUStudentZone" });
            }
        }
        #endregion


    }
}