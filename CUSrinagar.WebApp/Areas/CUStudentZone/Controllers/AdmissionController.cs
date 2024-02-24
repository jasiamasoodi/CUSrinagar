using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using System;
using CUSrinagar.BusinessManagers;

using System.Collections.Generic;
using CUSrinagar.Extensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeneralModels;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class AdmissionController : StudentBaseController
    {
        #region IH
        public ActionResult UploadCertificateIH(string R)
        {
            ViewBag.R = R;
            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student_IH(AppUserHelper.User_ID.ToString().EncryptCookieAndURLSafe(), null, R, false);
            if (ValidateRequest.Item1)
            {
                return View();
            }
            ViewBag.ARGPersonalInformation = ValidateRequest.Item2;
            List<Certificate> certificates = new StudentManager().GetUploadedCertificates(AppUserHelper.User_ID) ?? new List<Certificate>();
            foreach (CertificateType item in Enum.GetValues(typeof(CertificateType)))
            {
                if (!certificates.Any(x => x.CertificateType == item))
                    certificates.Add(new Certificate() { VerificationStatus = VerificationStatus.Pending, CertificateType = item, Student_ID = ValidateRequest.Item2.Student_ID, UploadingDate = DateTime.Now });
            }
            if (ValidateRequest.Item2.Category == "OM") { certificates.Remove(certificates.First(x => x.CertificateType == CertificateType.Category)); }

            GetResponseViewBags();
            return View(certificates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCertificateIH(List<Certificate> model, string R, Guid Course_ID, string S, bool HasMultimpleCourses = false)
        {
            ResponseData response = new ResponseData();
            if ((string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme printProgramme)) ||
                (string.IsNullOrEmpty(S) || !Enum.TryParse((S + "").DecryptCookieAndURLSafe(), out StudentSelectionStatus studentSelectionStatus)) || model.IsNullOrEmpty())
            {
                response.ErrorMessage = "Invalid request, please try again.";
                SetResponseViewBags(response);
                return RedirectToAction("UploadCertificateIH", new { R = R });
            }
            model = model.Where(x => x.File != null && (x.RecordState == RecordState.New || x.RecordState == RecordState.Dirty)).ToList();
            //TryUpdateModel(model);
            if (model.IsNotNullOrEmpty() || HasMultimpleCourses)
            {
                response = new StudentManager().SaveUpdate(model, printProgramme, Course_ID, studentSelectionStatus);
                if (HasMultimpleCourses)
                {
                    response.NumberOfRecordsEffected += new StudentManager().UpdateSelectionStatus(AppUserHelper.User_ID, Course_ID, studentSelectionStatus);
                    ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(AppUserHelper.User_ID, printProgramme, false, false, true, false);
                    studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF) && x.PrintProgramme == printProgramme && x.Course_ID != Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E") && x.Course_ID != Course_ID).ToList();
                    if (studentPersonalInfo.CoursesApplied.IsNotNullOrEmpty())
                    {
                        foreach (var item in studentPersonalInfo.CoursesApplied)
                        {
                            response.NumberOfRecordsEffected += new StudentManager().UpdateSelectionStatus(AppUserHelper.User_ID, item.Course_ID, item.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded ? StudentSelectionStatus.Provisional : StudentSelectionStatus.Provisional_SF);
                        }
                        response.IsSuccess = response.NumberOfRecordsEffected >= 1;
                        if (response.IsSuccess)
                            response.SuccessMessage = "Details saved successfully.";
                    }
                }

            }
            else
            {
                response.ErrorMessage = "Invalid input or nothing to save.";
            }
            SetResponseViewBags(response);
            if (response.IsSuccess)
            {
                return RedirectToAction("UploadSuccessReponseIH", new { R = R });
            }
            else
            {
                return RedirectToAction("UploadCertificateIH", new { R = R });
            }
        }

        public ActionResult UploadSuccessReponseIH(string R)
        {
            ResponseData response = new ResponseData();
            if (string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme printProgramme))
            {
                response.ErrorMessage = "Invalid request, please try again.";
                SetResponseViewBags(response);
                return RedirectToAction("UploadCertificateIH", new { R = R });
            }
            var certificates = new StudentManager().GetUploadedCertificates(AppUserHelper.User_ID);
            if (certificates.IsNotNullOrEmpty())
            {
                var studentPersonalInfo = new RegistrationManager().GetStudent(AppUserHelper.User_ID, printProgramme, false, false, true, true);
                studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Provisional_SF
                || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF
                || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF
                || x.StudentSelectionStatus == StudentSelectionStatus.Joined || x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF
                     ) && x.PrintProgramme == printProgramme).ToList();
                ViewBag.ARGPersonalInformation = studentPersonalInfo;
            }
            else TempData["ErrorMessage"] = "Result not found.";
            GetResponseViewBags();
            return View(certificates);
        }

        public void CreateBillDeskRequestIH(string Student_ID, string Course_ID, string R)
        {
            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student_IH(Student_ID, Course_ID, R, true);

            if (ValidateRequest.Item1)
            {
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write("<h1>Invalid Request</h1>");
                System.Web.HttpContext.Current.Response.End();
            }
            else
            {
                var studentPersonalInfo = ValidateRequest.Item2;
                var courseSetting = ValidateRequest.Item3;
                decimal amount = courseSetting.AdmissionFee;
                if (courseSetting.AllowOnlySpecificCUSRegnNos)
                    amount += new StudentManager().GetAdditionalFeeSemesterAdm(studentPersonalInfo.StudentFormNo, 1);
                if (ValidateRequest.Item2.CoursesApplied.Any(x => x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF) && courseSetting.SelfFinanceFee.HasValue)
                    amount += courseSetting.SelfFinanceFee.Value;

                BillDeskRequest billDeskRequest = new BillDeskRequest()
                {
                    Entity_ID = Guid.Parse(Course_ID.DecryptCookieAndURLSafe()),
                    Email = studentPersonalInfo.StudentAddress.Email,
                    PhoneNo = studentPersonalInfo.StudentAddress.Mobile,
                    TotalFee = Convert.ToInt32(amount),
                    CustomerID = DateTime.Now.Ticks.ToString(),
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Student_ID = studentPersonalInfo.Student_ID,
                    Semester = "1",
                    ReturnURL = "CUStudentZone/Admission/PaymentResponseIH",
                    AdditionalInfo = studentPersonalInfo.StudentFormNo,
                    NonBillDeskField = studentPersonalInfo.StudentFormNo,
                };
                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
            }
        }
        public ActionResult PaymentResponseIH()
        {
            ResponseData response = new ResponseData();
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billdeskResponse.Item1)
            {
                response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.SemesterAdmission);
                if (response.IsSuccess)
                {
                    var paymentDetails = (PaymentDetails)response.ResponseObject;
                    return RedirectToAction("UploadSuccessReponseIH", new { R = Helper.EncryptCookieAndURLSafe(((short)PrintProgramme.IH).ToString()) });
                }
                else
                {
                    ViewBag.PaymentFailureMessage = "Information: In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking.";
                }
            }
            else
            {
                ViewBag.PaymentFailureMessage = billdeskResponse.Item2;
            }
            return View();
        }

        Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest_Student_IH(string Student_ID, string Course_ID, string R, bool ValidateForPayment = false)
        {
            bool HasError = false;
            if (string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme _P)
                || string.IsNullOrEmpty(Student_ID) || !Guid.TryParse((Student_ID + "").DecryptCookieAndURLSafe(), out Guid _S)
                 || (ValidateForPayment && (string.IsNullOrEmpty(Course_ID) || !Guid.TryParse((Course_ID + "").DecryptCookieAndURLSafe(), out Guid _C))))
            {
                ViewBag.ErrorMessage = " Invalid Request. Please try again.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }
            PrintProgramme printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (R).DecryptCookieAndURLSafe());
            Guid _Student_ID = Guid.Parse((Student_ID).DecryptCookieAndURLSafe()), _Course_ID = ValidateForPayment ? Guid.Parse((Course_ID).DecryptCookieAndURLSafe()) : Guid.Empty;
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(_Student_ID, printProgramme, true, false, true, false);

            if (studentPersonalInfo == null)
            {
                ViewBag.ErrorMessage = "No details found";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }
            else if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
            {
                ViewBag.ErrorMessage = " It seems that you have not paid registration fee , also not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }
            else if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "It seems that you have not applied for any course";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }
            studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Provisional_SF
            || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF
             || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF
            || x.StudentSelectionStatus == StudentSelectionStatus.Joined || x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF) && x.PrintProgramme == printProgramme && x.Course_ID != Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();

            if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "It seems that you are not selected in the entrance";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }

            if (studentPersonalInfo.CATEntrancePoints == null)
            {
                ViewBag.ErrorMessage = "It seems that you have not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }

            List<ARGCoursesApplied> _CoursesApplied_Selected = new List<ARGCoursesApplied>();
            ADMSemesterAdmissionSettings courseSetting = null;
            if (studentPersonalInfo.CoursesApplied.Any(x => x.StudentSelectionStatus == StudentSelectionStatus.Joined || x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF))
            {
                studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Joined || x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF) && x.PrintProgramme == printProgramme && x.Course_ID != Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();
            }
            else
            {
                if (studentPersonalInfo.CoursesApplied.Count == 1)
                {
                    courseSetting = new StudentManager().GetSemesterAdmissionSettings(studentPersonalInfo.CoursesApplied.First().Course_ID, 1);
                    if (courseSetting == null || courseSetting.Status == false)
                    {
                        ViewBag.ErrorMessage = $"Admission for Course({studentPersonalInfo.CoursesApplied.First().CourseName}) not available yet, please contact at admissions@cusrinagar.edu.in in case you have any query.";

                        return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
                    }
                    if (courseSetting.AllowOnlySpecificCUSRegnNos && new StudentManager().IsStudentAllowedInSemesterAdm(studentPersonalInfo.StudentFormNo, 1) == false)
                    {
                        ViewBag.ErrorMessage = $"Student not added under course({studentPersonalInfo.CoursesApplied.First().CourseName}), please contact at admissions@cusrinagar.edu.in";
                        return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
                    }
                }
                else
                {
                    string errorMessage = "";
                    foreach (var item in studentPersonalInfo.CoursesApplied)
                    {
                        courseSetting = new StudentManager().GetSemesterAdmissionSettings(item.Course_ID, 1);

                        if (courseSetting == null || courseSetting.Status == false)
                        {
                            errorMessage += $"Admission for Course({item.CourseName}) not available yet, please contact at admissions@cusrinagar.edu.in in case you have any query.";
                            continue;
                        }
                        if (courseSetting.AllowOnlySpecificCUSRegnNos && new StudentManager().IsStudentAllowedInSemesterAdm(studentPersonalInfo.StudentFormNo, 1) == false)
                        {
                            errorMessage += $"Student not added under course({item.CourseName}), please contact at admissions@cusrinagar.edu.in";
                            continue;
                        }
                        _CoursesApplied_Selected.Add(item);
                    }
                    if (_CoursesApplied_Selected.IsNullOrEmpty())
                    {
                        ViewBag.ErrorMessage = errorMessage;
                        return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
                    }
                    studentPersonalInfo.CoursesApplied = _CoursesApplied_Selected;
                }
            }

            if (ValidateForPayment)
            {
                studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF) && x.PrintProgramme == printProgramme
             && x.Course_ID == _Course_ID).ToList();

                if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty() || studentPersonalInfo.CoursesApplied.Count > 1)
                {
                    return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
                }
                courseSetting = new StudentManager().GetSemesterAdmissionSettings(_Course_ID, 1);
            }


            return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(HasError, studentPersonalInfo, courseSetting);
        }
        #endregion

        #region Counseling
        public ActionResult ApplyForCounseling(string R)
        {
            Tuple<bool, ARGPersonalInformation> ValidateRequest = ValidateRequest_Student_IH_ForCounseling(null, R);
            if (ValidateRequest.Item1)
            {
                return View();
            }
            ViewBag.R = R;
            GetResponseViewBags();
            return View(ValidateRequest.Item2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyForCounseling(string R, List<Guid> Course_ID)
        {
            //return RedirectToAction("ApplyForCounseling", new { R = R });

            ResponseData response = new ResponseData();
            if ((string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme printProgramme)))
            {
                response.ErrorMessage = "Invalid request, please try again.";
                SetResponseViewBags(response);
                return RedirectToAction("ApplyForCounseling", new { R = R });
            }
            if (Course_ID.IsNotNullOrEmpty())
            {
                foreach (var item in Course_ID)
                {
                    response.NumberOfRecordsEffected += new StudentManager().UpdateSelectionStatus(AppUserHelper.User_ID, item, StudentSelectionStatus.AppliedForCounseling);
                }
                if (response.NumberOfRecordsEffected > 0)
                {
                    response.IsSuccess = true;
                    response.SuccessMessage = response.NumberOfRecordsEffected + " records saved successfully.";
                }
            }
            else
            {
                response.ErrorMessage = "No course choosen";
            }
            SetResponseViewBags(response);
            return RedirectToAction("ApplyForCounseling", new { R = R });
        }
        Tuple<bool, ARGPersonalInformation> ValidateRequest_Student_IH_ForCounseling(List<Guid> Course_IDs, string R)
        {
            bool HasError = false;
            if (string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme _P))
            {
                ViewBag.ErrorMessage = " Invalid Request. Please try again.";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }
            PrintProgramme printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), (R).DecryptCookieAndURLSafe());
            Guid _Student_ID = AppUserHelper.User_ID;
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(_Student_ID, printProgramme, true, false, true, false);
            if (studentPersonalInfo == null)
            {
                ViewBag.ErrorMessage = " Student not found.";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }
            if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
            {
                ViewBag.ErrorMessage = " It seems that you have not paid registration fee , also not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }
            else if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "It seems that you have not applied for any course";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }
            studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x =>
                                (x.StudentSelectionStatus == StudentSelectionStatus.AppliedForCounseling
                                || x.StudentSelectionStatus == StudentSelectionStatus.AllowedForCounseling
                                || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded
                                || x.StudentSelectionStatus == StudentSelectionStatus.CertificatesUploaded_SF
                                || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment
                                || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF
                                || x.StudentSelectionStatus == StudentSelectionStatus.Joined
                                || x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF)
                                && x.PrintProgramme == printProgramme
                                && x.Course_ID != Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();

            if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                ViewBag.ErrorMessage = "It seems that you are not selected in the entrance";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }

            if (studentPersonalInfo.CATEntrancePoints == null)
            {
                ViewBag.ErrorMessage = "It seems that you have not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation>(true, null);
            }

            List<ARGCoursesApplied> _CoursesApplied_Selected = new List<ARGCoursesApplied>();

            return Tuple.Create<bool, ARGPersonalInformation>(HasError, studentPersonalInfo);
        }
        #endregion


        #region UG      
        [HttpGet]
        public ActionResult CollegePreference()
        {
            List<StudentCollegePreference> collegePref = new StudentManager().GetCollegePreference(AppUserHelper.User_ID);
            if (collegePref.IsNotNullOrEmpty() && collegePref.Any(x => x.OldStudent_ID.HasValue && x.OldStudent_ID != Guid.Empty))
            {
                return RedirectToAction("UploadSuccessReponseUG", new { R = "1".EncryptCookieAndURLSafe() });
            }

            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student_UG(AppUserHelper.User_ID, null, Guid.Empty, AppUserHelper.TableSuffix, false);
            if (ValidateRequest.Item1 && ValidateRequest.Item2 == null)
            {
                GetResponseViewBags();
                return View(ValidateRequest.Item2);
            }
            if (collegePref.IsNullOrEmpty())
                collegePref = new List<StudentCollegePreference>() { new StudentCollegePreference() { Student_ID = AppUserHelper.User_ID }, new StudentCollegePreference() { Student_ID = AppUserHelper.User_ID }, new StudentCollegePreference() { Student_ID = AppUserHelper.User_ID } };

            for (int i = collegePref.Count; i < 3; i++)
            {
                collegePref.Add(new StudentCollegePreference() { Student_ID = AppUserHelper.User_ID });
            }

            ViewBag.CollegePreference = collegePref;//.OrderBy(x => x.PreferenceOrder).ToList();
            FillViewBags(ValidateRequest.Item2.Gender, collegePref.FirstOrDefault()?.Course_ID ?? null);
            GetResponseViewBags();
            return View(ValidateRequest.Item2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CollegePreference(List<StudentCollegePreference> model, Guid Course_ID, string Gender, string CRmv)
        {
            ResponseData response = new ResponseData();
            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student_UG(AppUserHelper.User_ID, null, Course_ID, AppUserHelper.TableSuffix, false);
            if (ValidateRequest.Item1)
            {
                return RedirectToAction("CollegePreference", new { });
            }

            List<SelectListItem> collegeShiftOirginalList = new CourseManager().GetCollegeList(Course_ID, null, null) ?? new List<SelectListItem>();
            List<SelectListItem> collegeShift = new List<SelectListItem>();

            foreach (var item in collegeShiftOirginalList)
            {
                if (!CRmv.ToLower().Trim().Contains(item.Value.ToLower().Trim()))
                    collegeShift.Add(item);
            }

            int countAllowed = (Gender.ToLower().Trim() == "male" && collegeShift.Any(x => Guid.Parse(x.Value) == Guid.Parse("B5E689E6-75FB-44E9-8ED7-3E649C18B659")))
                ? collegeShift.Count - 1
                : (collegeShift.Count > 3 ? 3 : model.Where(x => x.College_ID != Guid.Empty).Count());
            //int countAlloweds = collegeShift.Count > 3 ? 3 : collegeShift.Count;

            if (model.IsNullOrEmpty() || model.Select(x => x.College_ID).Where(y => y != Guid.Empty).Distinct().Count() != countAllowed)
            {
                TempData["ErrorMessage"] = "Please fill all(distinct) college preferences";
                return RedirectToAction("CollegePreference", new { });
            }
            if (ValidateRequest.Item2.Gender.ToLower() == "male" && model.Any(x => x.College_ID == Guid.Parse("B5E689E6-75FB-44E9-8ED7-3E649C18B659")))
            {
                TempData["ErrorMessage"] = "Male candidates can't opted women college option";
                return RedirectToAction("CollegePreference", new { });
            }
            model[0].IsAllowed = true;
            var studentdb = new StudentManager();
            response.NumberOfRecordsEffected = studentdb.DeleteCollegePreference(model.First().Student_ID);
            foreach (var item in model.Where(x => x.College_ID != Guid.Empty))
            {
                item.Course_ID = Course_ID;
                // if (item.Preference_ID == Guid.Empty)
                response.NumberOfRecordsEffected += new StudentManager().SaveCollegePreference(item);
                //else
                //response.NumberOfRecordsEffected += new StudentManager().UpdateCollegePreference(item);
            }
            response.NumberOfRecordsEffected += studentdb.UpdateSelectionStatus(AppUserHelper.User_ID, Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"), StudentSelectionStatus.Verified_MakePayment);

            if (response.NumberOfRecordsEffected > 0)
            {
                response.IsSuccess = true; response.SuccessMessage = "Preferences saved successfully. You can make payment now.";
                SetResponseViewBags(response);
                //return RedirectToAction("UploadSuccessReponseUG");
            }
            return RedirectToAction("CollegePreference", new { });
        }

        public ActionResult UploadSuccessReponseUG()
        {
            ResponseData response = new ResponseData();
            PrintProgramme printProgramme = PrintProgramme.IH;
            List<StudentCollegePreference> collegePref = new StudentManager().GetCollegePreference(AppUserHelper.User_ID);
            Guid Student_ID = AppUserHelper.User_ID;
            if (collegePref.Any(x => x.OldStudent_ID.HasValue && x.OldStudent_ID != Guid.Empty))
            {
                Student_ID = collegePref.First().Student_ID;
                printProgramme = PrintProgramme.UG; //that is student has been imported 
            }
            var studentPersonalInfo = new RegistrationManager().GetStudent(Student_ID, printProgramme, false, false, true, true);
            studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => (x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Joined
                 || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.CollegePreferenceUpdated) && x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();
            studentPersonalInfo.CourseFullName = collegePref.First().CourseFullName;
            ViewBag.ARGPersonalInformation = studentPersonalInfo;

            GetResponseViewBags();
            return View(collegePref);
        }

        public void CreateBillDeskRequestUG(Guid Preference_ID)
        {
            StudentCollegePreference Preference = new StudentManager().GetPreference(Preference_ID);
            string CourseID = CUSrinagar.Extensions.Helper.EncryptCookieAndURLSafe(Preference.Course_ID.ToString());
            Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest = ValidateRequest_Student_UG(AppUserHelper.User_ID, CourseID, Guid.Empty, PrintProgramme.IH, true);
            if (ValidateRequest.Item1)
            {
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write("<h1>Invalid Request</h1>");
                System.Web.HttpContext.Current.Response.End();
            }
            else
            {
                List<StudentCollegePreference> collegePref = new StudentManager().GetCollegePreference(AppUserHelper.User_ID);
                decimal amount = ValidateRequest.Item3.AdmissionFee;
                if (ValidateRequest.Item3.AllowOnlySpecificCUSRegnNos)
                    amount += new StudentManager().GetAdditionalFeeSemesterAdm(ValidateRequest.Item2.StudentFormNo, 1);
                BillDeskRequest billDeskRequest = new BillDeskRequest()
                {
                    Entity_ID = Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"),
                    Email = ValidateRequest.Item2.StudentAddress.Email,
                    PhoneNo = ValidateRequest.Item2.StudentAddress.Mobile,
                    TotalFee = Convert.ToInt16(amount),
                    CustomerID = DateTime.Now.Ticks.ToString(),
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Student_ID = ValidateRequest.Item2.Student_ID,
                    Semester = "1",
                    ReturnURL = "CUStudentZone/Admission/PaymentResponseUG",
                    AdditionalInfo = ValidateRequest.Item2.StudentFormNo,
                    NonBillDeskField = ValidateRequest.Item2.StudentFormNo,
                };
                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
            }

        }
        public ActionResult PaymentResponseUG()
        {
            ResponseData response = new ResponseData();
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billdeskResponse.Item1)
            {
                Guid course_id = billdeskResponse.Item3.Entity_ID;
                response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.SemesterAdmission);
                if (response.IsSuccess)
                {
                    return RedirectToAction("UploadSuccessReponseUG", new { R = "1".EncryptCookieAndURLSafe() });
                }
                else if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                {
                    ViewBag.PaymentFailureMessage = response.ErrorMessage;
                }
                else
                {
                    ViewBag.PaymentFailureMessage = "Information: In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking";
                }
            }
            else
            {
                ViewBag.PaymentFailureMessage = billdeskResponse.Item2;
            }
            return View(response.ResponseObject);
        }

        [HttpGet]
        public ActionResult ProvAdmFormPrint(string R)
        {
            //if (string.IsNullOrEmpty(R) || !Enum.TryParse((R + "").DecryptCookieAndURLSafe(), out PrintProgramme printProgramme))
            //{
            //    ViewBag.ErrorMessage = "Invalid Request";
            //    return View();
            //}
            Guid Student_ID = AppUserHelper.User_ID;

            ViewBag.UGConsolidatedPoinsts = null;
            Guid student_id = AppUserHelper.User_ID;
            var collegePref = new StudentManager().GetCollegePreference(student_id);
            PrintProgramme printProgramme = PrintProgramme.IH;
            if (collegePref.Any(x => x.OldStudent_ID.HasValue && x.OldStudent_ID != Guid.Empty))
            {
                Student_ID = collegePref.First().Student_ID;
                printProgramme = PrintProgramme.UG; //that is student has been imported 
            }
            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(collegePref.First().Student_ID, printProgramme, true, true, true, true);

            ViewBag.AppliedFor = PrintProgramme.UG.GetEnumDescription();
            if (studentPersonalInfo != null)
            {
                if (studentPersonalInfo.PaymentDetail.IsNotNullOrEmpty())
                {
                    ViewBag.CollegePreferences = collegePref;
                    ARGStudentPreviousQualifications I2thDetails = studentPersonalInfo.AcademicDetails?.FirstOrDefault(x => x.ExamName.Trim().ToLower() == "12th");
                    studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"))?.ToList();
                    if (I2thDetails != null)
                    {
                        ViewBag.UGConsolidatedPoinsts = Convert.ToDecimal(((I2thDetails.MarksObt / I2thDetails.MaxMarks * 100) * 60) / 100) + (studentPersonalInfo.CATEntrancePoints ?? 0);
                        studentPersonalInfo.CoursesApplied.First().SubjectEntrancePoints = ViewBag.UGConsolidatedPoinsts;
                    }
                    return View(studentPersonalInfo);
                }
            }
            ViewBag.PaymentFailureMessage = "Error";
            return View("PaymentResponse", TempData["Payment"]);
        }

        Tuple<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings> ValidateRequest_Student_UG(Guid _Student_ID, string CourseID, Guid selectedCourse, PrintProgramme printProgramme = PrintProgramme.IH, bool ValidateForPayment = false)
        {
            bool HasError = false;
            bool checkAcademicDetails = true;
            if ((ValidateForPayment && (string.IsNullOrEmpty(CourseID) || !Guid.TryParse((CourseID + "").DecryptCookieAndURLSafe(), out Guid _selectedCourse))))
            {
                TempData["ErrorMessage"] = " Invalid Request. Please try again.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }

            Guid Course_ID = ValidateForPayment ? Guid.Parse(CourseID.DecryptCookieAndURLSafe()) : Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E");

            ADMSemesterAdmissionSettings courseSetting = new StudentManager().GetSemesterAdmissionSettings(Course_ID, 1);

            ARGPersonalInformation studentPersonalInfo = new RegistrationManager().GetStudent(_Student_ID, printProgramme, true, true, true, false);

            if (studentPersonalInfo == null)
            {
                TempData["ErrorMessage"] = " Student not found.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
            }
            else if ((studentPersonalInfo.IsProvisional ?? true))
            {
                //ARGCoursesApplied coursesApplied = studentPersonalInfo.CoursesApplied?.FirstOrDefault(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E"));
                //if (coursesApplied != null
                //    && (coursesApplied.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment
                //        || coursesApplied.StudentSelectionStatus == StudentSelectionStatus.Joined
                //        || coursesApplied.StudentSelectionStatus == StudentSelectionStatus.CollegePreferenceUpdated
                //        || coursesApplied.StudentSelectionStatus == StudentSelectionStatus.Provisional
                //        ) && studentPersonalInfo.CATEntrancePoints != null)
                //{
                //    checkAcademicDetails = false;
                //}
                //else
                {
                    TempData["ErrorMessage"] = "Provisional admission is not eligible";
                    return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, null, null);
                }
            }
            else if (studentPersonalInfo.FormStatus == FormStatus.InProcess)
            {
                TempData["ErrorMessage"] = " It seems that you have not paid registration fee , also not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }
            else if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = "It seems that you have not applied for any course";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }
            else if (new RegistrationManager().CheckAlreadyExistsInAffiliated(studentPersonalInfo.BoardRegistrationNo, studentPersonalInfo.Batch, PrintProgramme.UG))
            {
                TempData["ErrorMessage"] = "It seems that you have also applied for admission in affiliated colleges.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }
            if (courseSetting == null)
            {
                TempData["ErrorMessage"] = $"Admission for this Course not available yet, please contact at admissions@cusrinagar.edu.in in case you have any query.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }
            if (studentPersonalInfo.CoursesApplied.IsNotNullOrEmpty() && studentPersonalInfo.
                CoursesApplied.Any(x => x.Course_ID != Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E") &&
                (x.StudentSelectionStatus == StudentSelectionStatus.Joined ||
                x.StudentSelectionStatus == StudentSelectionStatus.Joined_SF ||
                x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment ||
                x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment_SF)))
            {

                TempData["ErrorMessage"] = $"It seems that you have already payment pending for IG/Honor's/Professional.Incase you want to join UG please contact I.T Section";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }

            studentPersonalInfo.CoursesApplied = studentPersonalInfo.CoursesApplied.Where(x => ((x.StudentSelectionStatus == StudentSelectionStatus.Provisional || x.StudentSelectionStatus == StudentSelectionStatus.Joined
                || x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment || x.StudentSelectionStatus == StudentSelectionStatus.CollegePreferenceUpdated)) && x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).ToList();

            if (studentPersonalInfo.CoursesApplied.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = "It seems that you are not selected in the entrance";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }

            if (studentPersonalInfo.CATEntrancePoints == null)
            {
                TempData["ErrorMessage"] = "It seems that you have not appeared in entrance test.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }

            if (studentPersonalInfo.CoursesApplied.Any(x => (x.StudentSelectionStatus == StudentSelectionStatus.Joined)))// && x.Course_ID!=Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
            {
                TempData["ErrorMessage"] = $"It seems that you are already registered under course : {studentPersonalInfo.CoursesApplied.First(x => (x.StudentSelectionStatus == StudentSelectionStatus.Joined)).CourseName} ";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }

            if (courseSetting.Status == false)
            {
                ViewBag.ProcessClosed = true;
                TempData["ErrorMessage"] = $"Admission process has been closed.";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }


            if (checkAcademicDetails)
            {
                string _stream = studentPersonalInfo.AcademicDetails.First(x => x.ExamName.ToUpper() == "12TH").Stream.ToUpper();

                //MEDICAL Elegibility
                if ((selectedCourse == Guid.Parse("03FFF89D-3055-4F0A-A18C-185BEB7512DE")
                    || selectedCourse == Guid.Parse("B18A12B2-9025-4057-8E38-DF723649D941")))
                {
                    if (_stream == "HOME SCIENCE")
                    {
                        TempData["ErrorMessage"] = "It seems that you are not eligible for B.Sc";
                        return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
                    }
                    if (!(_stream.Contains("MED") || _stream.Contains("SCI")))
                    {
                        TempData["ErrorMessage"] = "It seems that you are not eligible for B.Sc";
                        return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
                    }
                }

                //Home science
                if (selectedCourse == Guid.Parse("4D8ADDCB-DB0B-4E39-ABC4-AB2FFF15289D") && _stream != "HOME SCIENCE")
                {
                    TempData["ErrorMessage"] = "It seems that you are not eligible for Home Science";
                    return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
                }
            }


            if (courseSetting.AllowOnlySpecificCUSRegnNos && new StudentManager().IsStudentAllowedInSemesterAdm(studentPersonalInfo.StudentFormNo, 1) == false)
            {
                TempData["ErrorMessage"] = $"Student not added under course (UG), please contact at admissions@cusrinagar.edu.in";
                return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
            }
            if (ValidateForPayment)
            {
                if (!studentPersonalInfo.CoursesApplied.Any(x => (x.StudentSelectionStatus == StudentSelectionStatus.Verified_MakePayment) && x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
                {
                    TempData["ErrorMessage"] = $"Student not allowed for payment, please contact at admissions@cusrinagar.edu.in";
                    return Tuple.Create<bool, ARGPersonalInformation, ADMSemesterAdmissionSettings>(true, studentPersonalInfo, null);
                }
            }
            return Tuple.Create(HasError, studentPersonalInfo, courseSetting);
        }

        void FillViewBags(string Gender, Guid? course_Id)
        {
            ViewBag.College = new List<SelectListItem>() {
                new SelectListItem() { Text = "Amar Singh College", Value = "8B585643-CEB5-4C96-8328-862A9911CD51"},
                 new SelectListItem() { Text = "Abdul Ahad Azad Memorial College ", Value = "67C52693-7029-486F-AA46-798F69A95AFB" },
                   new SelectListItem() { Text = "Sri Pratap College", Value = "90AE5E98-3A8F-4B15-A652-7EAE5A203C42" }
            };
            if (Gender.ToLower() == "female")
                ViewBag.College.Add(new SelectListItem() { Text = "Govt. Women College", Value = "B5E689E6-75FB-44E9-8ED7-3E649C18B659" });

            //ViewBag.Courses = new List<SelectListItem>() {
            //    new SelectListItem() { Text = "B.Sc Medical", Value = "03FFF89D-3055-4F0A-A18C-185BEB7512DE"},
            //     new SelectListItem() { Text = "B.Sc Non-Medical", Value = "B18A12B2-9025-4057-8E38-DF723649D941" }
            //};
            if (!course_Id.IsNullOrEmpty())
                ViewBag.College = new CourseManager().GetCollegeList((Guid)course_Id, null, null) ?? new List<SelectListItem>();

            ViewBag.Courses = new EntranceListManager().GetAllCoursesByProgramme(Programme.UG).Where(x => x.Text.ToLower() != "b.ed");
        }


        #endregion

    }
}