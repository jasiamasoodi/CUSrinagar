using CaptchaMvc.HtmlHelpers;
using CUSrinagar.BusinessManagers;
using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class RegnCoursesController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";
        private Tuple<ARGPersonalInformation, ARGFormNoMaster> SetViewBags(PrintProgramme printProgramme)
        {
            RegnCoursesManager _RegnCoursesManager = new RegnCoursesManager();
            bool FormMaster = _RegnCoursesManager.IsRegistrationOpen();
            ViewBag.PrintProgrammeOption = FormMaster;
            List<ARGCoursesApplied> _CoursesApplied = _RegnCoursesManager.GetCoursesApplied();
            ViewBag.CoursesApplied = _CoursesApplied;

            ViewBag.HasPayment = (_CoursesApplied.IsNotNullOrEmpty() && _CoursesApplied[0].PaymentDetail != null);

            ARGPersonalInformation personalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, printProgramme, true);
            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(printProgramme);
            ViewBag.FormNoMaster = formNoMaster;
            ViewBag.IsJkResident = personalInformation?.StudentAddress?.District?.ToLower()?.Trim() ?? "other";

            ViewBag.TotalFee = (ViewBag.IsJkResident == "other") ? personalInformation.TotalFee - formNoMaster.AdditionalFeeForNonJK : personalInformation.TotalFee;
            return Tuple.Create(personalInformation, formNoMaster);
        }


        [NonAction]
        private IEnumerable<SelectListItem> SetPreferenceCount(int TotalCourses)
        {
            for (int i = 1; i <= TotalCourses; i++)
                yield return new SelectListItem { Value = i.ToString(), Text = i.ToString() };
        }


        #region Add More For IHP
        [HttpGet]
        public ActionResult AddIHP()
        {
            ViewBag.IHPreferences = SetPreferenceCount(2);

            if (!AppUserHelper.College_ID.IsNullOrEmpty() || AppUserHelper.TableSuffix != PrintProgramme.IH)
                return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });

            SetViewBags(PrintProgramme.IH);
            List<ARGCoursesApplied> AdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.IH)?.ToList();
            if (AdmissionCoursesApplied.IsNullOrEmpty())
            {
                ViewBag.PrintProgrammeOption = false;
                return View(new List<ARGCoursesApplied>());
            }

            ARGCoursesApplied HasIntBedMed = ((List<ARGCoursesApplied>)ViewBag.CoursesApplied).FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));

            if (HasIntBedMed != null)
            {
                HasIntBedMed = AdmissionCoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                AdmissionCoursesApplied = new List<ARGCoursesApplied> { HasIntBedMed };
            }
            else
            {
                HasIntBedMed = AdmissionCoursesApplied.FirstOrDefault(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3"));
                AdmissionCoursesApplied.Remove(HasIntBedMed);
            }
            if (AdmissionCoursesApplied.IsNullOrEmpty())
                ViewBag.PrintProgrammeOption = false;

            return View(AdmissionCoursesApplied);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIHP(List<ARGCoursesApplied> AdmissionCoursesApplied, bool HasPayment)
        {
            ViewBag.IHPreferences = SetPreferenceCount(2);

            Tuple<ARGPersonalInformation, ARGFormNoMaster> Bags = SetViewBags(PrintProgramme.IH);
            if ((AdmissionCoursesApplied.IsNullOrEmpty() || AdmissionCoursesApplied.All(c => c.IsClicked == false) && !HasPayment))
            {
                TempData["response"] = errorMsg.Replace("##", "Please select the courses");
                return RedirectToAction("AddIHP");
            }

            #region PreferenceCode
            if (!Bags.Item1.IsLateralEntry)//to ignore preferences
            {
                List<ARGCoursesApplied> tempValidationCourse = AdmissionCoursesApplied.Where(x => x.IsClicked || x.AlreadyAppliedWithFee).ToList();

                if (tempValidationCourse.Any(x => x.Preference == null || x.Preference <= 0))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button><strong></strong> <a href='#' class='alert-link'> Please select preference(s) for all your selected course(s) and try again.</a></div>";
                    return RedirectToAction("AddIHP");
                }
                else if (tempValidationCourse.Any(x => x.Preference > 2))
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'></button> <a href='#' class='alert-link'> Invalid Course preferences found. Please check carefully and try again.</a></div>";
                    return RedirectToAction("AddIHP");
                }
            }
            #endregion



            if (!HasPayment)
                new RegistrationManager().AddGraduationCourse(AdmissionCoursesApplied);

            List<ARGCoursesApplied> TempAdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.IH).ToList();

            AdmissionCoursesApplied.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.GroupName))
                {
                    x.GroupName = x.Course_ID.ToString();
                }
            });

            foreach (var item in AdmissionCoursesApplied.Where(c => c.IsClicked))
            {
                if (TempAdmissionCoursesApplied.FirstOrDefault(c => c.Course_ID == item.Course_ID) == null)
                {
                    TempData["response"] = errorMsg.Replace("##", "Invalid courses found");
                    TempAdmissionCoursesApplied = null;//dispose
                    return RedirectToAction("AddIHP");
                }

                if (!Bags.Item1.IsLateralEntry)//to ignore preferences
                {
                    if (AdmissionCoursesApplied.Where(c => c.IsClicked).Count(x => x.GroupName.ToLower().Trim() == item.GroupName.ToLower().Trim()) <= 1)
                    {
                        item.Preference = 1;
                    }
                }

            }

            TempAdmissionCoursesApplied = AdmissionCoursesApplied.Where(c => c.IsClicked).ToList();
            Bags.Item1.CoursesApplied = TempAdmissionCoursesApplied;
            ARGPersonalInformation personalInformation = Bags.Item1;

            new RegistrationManager().AddCommonCourses(ref personalInformation);
            TempAdmissionCoursesApplied = personalInformation.CoursesApplied;

            //make payment 
            ARGStudentAddress _StudentAddress = new RegistrationManager().GetStudentAddress(AppUserHelper.User_ID, PrintProgramme.IH);


            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = _StudentAddress.Email,
                PhoneNo = _StudentAddress.Mobile,
                ReturnURL = $"CUStudentZone/RegnCourses/Success",
                PrintProgramme = PrintProgramme.IH,
                Entity_ID = _StudentAddress.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),

                TotalFee = new RegnCoursesManager().CalculateFeeFor_IHP_Only(TempAdmissionCoursesApplied,
                                                    AdmissionCoursesApplied.Where(x => x.AlreadyAppliedWithFee)?.Select(x => x.GroupName)?.ToList() ?? new List<string>(),
                                                    Bags.Item2, (Bags.Item1.StudentAddress.State.ToLower().Trim() != "other"), HasPayment),

                AdditionalInfo = TempAdmissionCoursesApplied.Select(x => x.CourseCode + "_" + (x.Preference ?? 0).ToString()).ToSingleString('@')
            };

            //update already applied course Prefs
            if (HasPayment)
                new PaymentManager().UpdateAlreadyAppliedCoursePref(AdmissionCoursesApplied.Where(x => x.AlreadyAppliedWithFee)?.ToList() ?? new List<ARGCoursesApplied>());

            if (billDeskRequest.TotalFee == 0)
            {
                PaymentDetails paymentDetails = new PaymentDetails
                {
                    AdditionalInfo = billDeskRequest.AdditionalInfo,
                    Entity_ID = _StudentAddress.Student_ID,
                    PrintProgramme = PrintProgramme.IH
                };
                if (AdmissionCoursesApplied.Where(x => x.AlreadyAppliedWithFee || x.IsClicked).All(x => x.AlreadyAppliedWithFee))
                {
                    //do nothing
                }
                else
                {
                    new PaymentManager().SaveAdmMoreCoursesPaymentDetails(paymentDetails, false);
                }
                TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href = '#' class='alert-link'>Course Added / Updated successfully.Please print your Regn.Form again.</a></div>";
                return RedirectToAction("AddIHP");
            }
            else
            {
                StringBuilder builder = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM));
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(builder.ToString());
                System.Web.HttpContext.Current.Response.End();
                return new EmptyResult();
            }
        }

        public ActionResult Success()
        {
            if (Request.InputStream == null)
                return RedirectToAction("AddIHP");

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveAdmMoreCoursesPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("AddIHP");
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href = '#' class='alert-link'>Payment received successfully.Please print your Regn.Form again.</a></div>";
                    return RedirectToAction("AddIHP");
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{billDeskResponse.Item2}</a></div>";
                return RedirectToAction("AddIHP");
            }
        }
        #endregion


        #region Add More For PG
        [HttpGet]
        public ActionResult AddPG()
        {
            if (!AppUserHelper.College_ID.IsNullOrEmpty() || AppUserHelper.TableSuffix != PrintProgramme.PG)
                return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });

            SetViewBags(PrintProgramme.PG);
            List<ARGCoursesApplied> AdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.PG)?.OrderByDescending(x => x.Programme).ToList();

            if (AdmissionCoursesApplied.IsNullOrEmpty())
                ViewBag.PrintProgrammeOption = false;

            return View(AdmissionCoursesApplied);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPG(List<ARGCoursesApplied> AdmissionCoursesApplied, bool HasPayment)
        {
            if (AdmissionCoursesApplied.IsNullOrEmpty() || AdmissionCoursesApplied.All(c => c.IsClicked == false))
            {
                TempData["response"] = errorMsg.Replace("##", "Please select the courses");
                return RedirectToAction("AddPG");
            }
            Tuple<ARGPersonalInformation, ARGFormNoMaster> Bags = SetViewBags(PrintProgramme.PG);

            List<ARGCoursesApplied> TempAdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.PG)?.OrderByDescending(x => x.Programme).ToList();
            foreach (var item in AdmissionCoursesApplied.Where(c => c.IsClicked))
            {
                if (TempAdmissionCoursesApplied.FirstOrDefault(c => c.Course_ID == item.Course_ID) == null)
                {
                    TempData["response"] = errorMsg.Replace("##", "Invalid courses found");
                    TempAdmissionCoursesApplied = null;//dispose
                    return RedirectToAction("AddPG");
                }
            }
            TempAdmissionCoursesApplied = AdmissionCoursesApplied.Where(c => c.IsClicked).ToList();
            //make payment 
            ARGStudentAddress _StudentAddress = new RegistrationManager().GetStudentAddress(AppUserHelper.User_ID, PrintProgramme.PG);
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = _StudentAddress.Email,
                PhoneNo = _StudentAddress.Mobile,
                ReturnURL = $"CUStudentZone/RegnCourses/SuccessPG",
                PrintProgramme = PrintProgramme.PG,
                Entity_ID = _StudentAddress.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                TotalFee = new RegnCoursesManager().CalculateFeeFor_PG_Only(TempAdmissionCoursesApplied.Select(x => x.Course_ID).ToList(), Bags.Item2, (Bags.Item1.StudentAddress.State.ToLower().Trim() != "other")),
                AdditionalInfo = TempAdmissionCoursesApplied.Select(x => x.CourseCode).ToSingleString('@')
            };
            StringBuilder builder = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM));
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(builder.ToString());
            System.Web.HttpContext.Current.Response.End();
            return new EmptyResult();
        }

        public ActionResult SuccessPG()
        {
            if (Request.InputStream == null)
                return RedirectToAction("AddPG");

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveAdmMoreCoursesPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("AddPG");
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href = '#' class='alert-link'>Payment received successfully.Please print your Regn.Form again.</a></div>";
                    return RedirectToAction("AddPG");
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{billDeskResponse.Item2}</a></div>";
                return RedirectToAction("AddPG");
            }
        }
        #endregion


        #region Add More For UG
        [HttpGet]
        public ActionResult AddUG()
        {
            if (!AppUserHelper.College_ID.IsNullOrEmpty() || AppUserHelper.TableSuffix != PrintProgramme.UG)
                return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });

            SetViewBags(PrintProgramme.UG);
            List<ARGCoursesApplied> AdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.UG)?.OrderByDescending(x => x.Programme).ToList();

            if (AdmissionCoursesApplied.IsNullOrEmpty())
                ViewBag.PrintProgrammeOption = false;

            return View(AdmissionCoursesApplied);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUG(List<ARGCoursesApplied> AdmissionCoursesApplied, bool HasPayment)
        {
            if (AdmissionCoursesApplied.IsNullOrEmpty() || AdmissionCoursesApplied.All(c => c.IsClicked == false))
            {
                TempData["response"] = errorMsg.Replace("##", "Please select the courses");
                return RedirectToAction("AddUG");
            }
            Tuple<ARGPersonalInformation, ARGFormNoMaster> Bags = SetViewBags(PrintProgramme.UG);

            List<ARGCoursesApplied> TempAdmissionCoursesApplied = new CourseManager().GetCourseListForRegistration(PrintProgramme.UG)?.OrderByDescending(x => x.Programme).ToList();
            foreach (var item in AdmissionCoursesApplied.Where(c => c.IsClicked))
            {
                if (TempAdmissionCoursesApplied.FirstOrDefault(c => c.Course_ID == item.Course_ID) == null)
                {
                    TempData["response"] = errorMsg.Replace("##", "Invalid courses found");
                    TempAdmissionCoursesApplied = null;//dispose
                    return RedirectToAction("AddUG");
                }
            }
            ARGPersonalInformation personalInformation = new RegistrationManager().GetStudentPersonalInfoOnly(AppUserHelper.User_ID, PrintProgramme.UG);

            #region TempCode should be removed
            if (personalInformation.CreatedOn <= new DateTime(2020, 6, 6))
            {
                TempData["response"] = errorMsg.Replace("##", "You are already enrolled");
                return RedirectToAction("AddUG");
            }
            #endregion

            TempAdmissionCoursesApplied = AdmissionCoursesApplied.Where(c => c.IsClicked).ToList();
            //make payment 
            ARGStudentAddress _StudentAddress = new RegistrationManager().GetStudentAddress(AppUserHelper.User_ID, PrintProgramme.UG);
            BillDeskRequest billDeskRequest = new BillDeskRequest
            {
                Email = _StudentAddress.Email,
                PhoneNo = _StudentAddress.Mobile,
                ReturnURL = $"CUStudentZone/RegnCourses/SuccessUG",
                PrintProgramme = PrintProgramme.UG,
                Entity_ID = _StudentAddress.Student_ID,
                CustomerID = DateTime.UtcNow.Ticks.ToString() + new Random().Next(1, 99),
                TotalFee = new RegnCoursesManager().CalculateFeeFor_UG_Only((Bags.Item1.StudentAddress.State.ToLower().Trim() != "other"), personalInformation.BoardRegistrationNo, personalInformation.Batch, Bags.Item2, HasPayment, TempAdmissionCoursesApplied.Select(x => x.Course_ID).ToList()),
                AdditionalInfo = TempAdmissionCoursesApplied.Select(x => x.CourseCode).ToSingleString('@')
            };

            if (billDeskRequest.TotalFee == 0)
            {
                PaymentDetails paymentDetails = new PaymentDetails
                {
                    AdditionalInfo = TempAdmissionCoursesApplied.Select(x => x.CourseCode).ToSingleString('@'),
                    Entity_ID = _StudentAddress.Student_ID,
                    PrintProgramme = PrintProgramme.UG
                };

                new PaymentManager().SaveAdmMoreCoursesPaymentDetails(paymentDetails, false);
                TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href = '#' class='alert-link'>Course Added successfully.Please print your Regn.Form again.</a></div>";
                return RedirectToAction("AddUG");
            }
            else
            {
                StringBuilder builder = new BillDeskManager().GenerateHTMLForm(new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.ADM));
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(builder.ToString());
                System.Web.HttpContext.Current.Response.End();
                return new EmptyResult();
            }
        }

        public ActionResult SuccessUG()
        {
            if (Request.InputStream == null)
                return RedirectToAction("AddUG");

            Tuple<bool, string, PaymentDetails, Guid, Guid> billDeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            if (billDeskResponse.Item1)
            {
                int result = new PaymentManager().SaveAdmMoreCoursesPaymentDetails(billDeskResponse.Item3);
                if (result <= 0)
                {
                    TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Something went wrong. If your account is debited, Please visit University I.T Section.</a></div>";
                    return RedirectToAction("AddUG");
                }
                else
                {
                    TempData["response"] = $"<div class='alert alert-success alert-dismissable'><button type = 'button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href = '#' class='alert-link'>Payment received successfully.Please print your Regn.Form again.</a></div>";
                    return RedirectToAction("AddUG");
                }
            }
            else
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{billDeskResponse.Item2}</a></div>";
                return RedirectToAction("AddUG");
            }
        }
        #endregion


        #region AddEditQualificationDetails IH UG
        [NonAction]
        private void SetViewBags()
        {
            ViewBag.UniversitiesDDL = Helper.UniversitiesDDL();
            bool IsBedMed = new RegistrationDB().GetStudentCoursesApplied(AppUserHelper.User_ID, AppUserHelper.TableSuffix)?.Any(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) ?? false;
            ViewBag.isLateralEntry = new RegistrationDB().IsLateralEntry(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
            if (ViewBag.isLateralEntry)
            {
                ViewBag.Boards = Helper.BoardsDDL();
                ((List<SelectListItem>)ViewBag.Boards).AddRange(Helper.UniversitiesDDL());
            }
            else
            {
                ViewBag.Boards = IsBedMed ? Helper.UniversitiesDDL() : Helper.BoardsDDL();
            }
            ViewBag.Session = Helper.SessionDDL();
            ViewBag.I2thStreamDDL = IsBedMed ? null : Helper.I2thStreamDDL();

        }

        [HttpGet]
        public ActionResult AddEditQualificationDetails()
        {
            SetViewBags();
            ARGStudentPreviousQualifications qualifications = null;
            if (AppUserHelper.OrgPrintProgramme == PrintProgramme.IH || AppUserHelper.OrgPrintProgramme == PrintProgramme.UG)
            {
                qualifications = new RegistrationDB().GetStudentAcademicDetails(AppUserHelper.User_ID, AppUserHelper.TableSuffix)?.FirstOrDefault();
                List<ARGCoursesApplied> _CoursesApplied = new RegistrationDB().GetStudentCoursesApplied(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
                if (qualifications != null)
                {
                    //provisional Admission
                    qualifications.ReadOnly = true;
                }
                else
                {
                    //non provisional admission
                    if (_CoursesApplied?.Any(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) ?? false)
                    {
                        //is int b.ed-m.ed
                        qualifications = new ARGStudentPreviousQualifications
                        {
                            ExamName = "Post-Graduation",
                            ReadOnly = true
                        };
                    }
                    else
                    {
                        if (new RegistrationDB().IsLateralEntry(AppUserHelper.User_ID, AppUserHelper.TableSuffix))
                        {
                            if (_CoursesApplied.All(x => x.Programme == Programme.Engineering))
                            {
                                qualifications = new ARGStudentPreviousQualifications
                                {
                                    ExamName = "Diploma/B.Sc",
                                    ReadOnly = true
                                };
                            }
                            else
                            {
                                qualifications = new ARGStudentPreviousQualifications
                                {
                                    ExamName = "GRADUATION",
                                    ReadOnly = true
                                };
                            }
                        }
                        else
                        {
                            qualifications = new ARGStudentPreviousQualifications
                            {
                                ExamName = "12TH",
                                ReadOnly = true
                            };
                        }
                    }
                }

            }
            else
            {

                return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });
            }
            qualifications.CreatedOn = qualifications.CreatedOn == DateTime.MinValue ? DateTime.Now : qualifications.CreatedOn;
            return View(qualifications);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditQualificationDetails(ARGStudentPreviousQualifications previousQualifications)
        {
            SetViewBags();
            if (!ModelState.IsValid)
                return View(previousQualifications);

            Tuple<bool, string> response = new RegnCoursesManager().AddEditQualificationDetails(previousQualifications);

            if (response.Item1)
            {
                TempData["response"] = errorMsg.Replace("alert-danger", "alert-success").Replace("##", response.Item2);
                return RedirectToAction("AddEditQualificationDetails");
            }
            else
                TempData["response"] = errorMsg.Replace("##", response.Item2);

            previousQualifications.CreatedOn = previousQualifications.CreatedOn == DateTime.MinValue ? DateTime.Now : previousQualifications.CreatedOn;
            return View(previousQualifications);
        }
        #endregion

        #region AddEditQualificationDetails PG
        [NonAction]
        private void SetViewBagsPG()
        {
            ViewBag.UniversitiesDDL = Helper.UniversitiesDDL();
            ViewBag.Boards = Helper.BoardsDDL();
            ViewBag.Session = Helper.SessionDDL();
            ViewBag.I2thStreamDDL = Helper.I2thStreamDDL();

        }
        [HttpGet]
        public ActionResult AddEditQualificationDetailsPG()
        {
            SetViewBagsPG();
            List<ARGStudentPreviousQualifications> qualifications = null;
            if (AppUserHelper.OrgPrintProgramme == PrintProgramme.PG)
            {
                qualifications = new RegistrationDB().GetStudentAcademicDetails(AppUserHelper.User_ID, AppUserHelper.TableSuffix);
                if (qualifications != null)
                {
                    //provisional Admission
                    qualifications.ForEach(x => { x.ReadOnly = true; });
                }
                else
                {
                    qualifications = new List<ARGStudentPreviousQualifications>
                        {
                            new   ARGStudentPreviousQualifications {
                                ExamName = "GRADUATION",
                                ReadOnly=true
                                }
                        };
                }
            }
            else
            {

                return RedirectToAction("GetInfo", "StudentInfo", new { area = "CUStudentZone" });
            }
            return View(qualifications);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditQualificationDetailsPG(List<ARGStudentPreviousQualifications> previousQualifications)
        {
            SetViewBagsPG();
            if (!ModelState.IsValid)
                return View(previousQualifications);
            Tuple<bool, string> response = null;
            foreach (var item in previousQualifications)
            {
                response = new RegnCoursesManager().AddEditQualificationDetails(item);
            }

            if (response.Item1)
            {
                TempData["response"] = errorMsg.Replace("alert-danger", "alert-success").Replace("##", response.Item2);
                return RedirectToAction("AddEditQualificationDetailsPG");
            }
            else
                TempData["response"] = errorMsg.Replace("##", response.Item2);

            return View(previousQualifications);
        }
        #endregion

        #region ChangeMarksJPG and CGPA
        [HttpGet]
        public ActionResult UpdateCertificates()
        {
            Guid Student_ID = AppUserHelper.User_ID;
            List<ARGCoursesApplied> alreadyAppliedCourses = new RegistrationManager().GetStudentCourses(Student_ID, AppUserHelper.OrgPrintProgramme);
            UpdateCertificates OlduploadedDetails = new StudentManager().GetCertificateDetails(Student_ID) ?? new UpdateCertificates();
            OlduploadedDetails.coursesApplied = alreadyAppliedCourses;

            return View(OlduploadedDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCertificates(UpdateCertificates newUploadedCertificate)
        {
            if (newUploadedCertificate.CertificateMarksCard == null)
            {
                ModelState.Remove(nameof(newUploadedCertificate.CertificateMarksCard));
            }
            if (!ModelState.IsValid)
            {
                TempData["response"] = errorMsg.Replace("##", "Invalid details. Try again.");
                return RedirectToAction("UpdateCertificates");
            }
            if (newUploadedCertificate?.coursesApplied?.All(x => x.SubjectCGPA != null) ?? false)
            {
                if (newUploadedCertificate.coursesApplied.Any(x => (x.SubjectCGPA ?? 0) <= 0))
                {
                    TempData["response"] = errorMsg.Replace("##", "Please enter Over All Subject CGPA Points. Try again.");
                    return RedirectToAction("UpdateCertificates");
                }
            }

            string MSG = new RegnCoursesManager().UpdateCertificates(newUploadedCertificate) > 0 ? "Saved successfully" : "No change made.";

            TempData["response"] = errorMsg.Replace("alert-danger", "alert-success").Replace("##", MSG);
            return RedirectToAction("UpdateCertificates");
        }

        #endregion
    }
}