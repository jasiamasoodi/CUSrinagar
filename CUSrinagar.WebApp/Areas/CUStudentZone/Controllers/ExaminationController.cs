using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class ExaminationController : StudentBaseController
    {
        private int _CurrentSemester = 0;
        private bool _IsRegular = false;
        private static string msg = "<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        #region Examination
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult Examination()
        {
            ResponseData responseData = new ResponseData();
            Guid Student_ID = AppUserHelper.User_ID;
            PrintProgramme printProgramme = AppUserHelper.TableSuffix;

            _CurrentSemester = new StudentManager().GetStudentCurrentSemester(Student_ID, printProgramme);

            ViewBag.CurrentSemester = _CurrentSemester;
            List<SelectListItem> allowedSemestersList = GetListItems(true);
            ViewBag.ExamForSemester = allowedSemestersList;

            ViewBag.IsEligibleForDivisionImprovement = new ExaminationFormManager().IsEligibleForDivisionImprovement(Student_ID, printProgramme).Item1;
            GetResponseViewBags();
            return View(responseData);
        }

        public JsonResult GetSemesterOpenForExaminationType(bool isRegular)
        {
            List<SelectListItem> list = GetListItems(isRegular);
            return Json(list);
        }


        [HttpGet]
        [OutputCache(Duration = 0, Location = System.Web.UI.OutputCacheLocation.None, NoStore = true)]
        public ActionResult ExaminationForm(short? semester)
        {
            ViewBag.SemesterChoosen = semester ?? 0;
            ViewBag.RedirectedFormExaminantionForm = true;
            PrintProgramme printProgramme = AppUserHelper.TableSuffix;

            //check Daily lecture feedback for semester added or not

            bool feedbackreult = new FeedbackManager().HasSubmittedDailyLectureFeedback(AppUserHelper.User_ID, semester ?? 0, printProgramme);

            if (!feedbackreult)
            {
                TempData["response"] = msg.Replace("##", "<strong>Please submit atleast one <a href='/CUStudentZone/Feedback/'>Daily lecture Feedback here</a> before submitting Exam form</strong>");
                return RedirectToAction("Examination", "Examination", new { area = "CUStudentZone" });
            }


            ResponseData response = new ExaminationFormManager().GetRegularStudentExamForm(semester ?? 0);
            if (response.IsSuccess)
            {
                var studentExamForm = (ARGStudentExamFormSubjects)response.ResponseObject;

                if (studentExamForm.Status == FormStatus.Accepted)
                    return RedirectToAction("PrintExaminationFormReceipt", new { semester });

                ViewBag.FeeStructure = new ExaminationFormManager().GetFeeStructure(((ARGExamFormDownloadable)response?.SecondaryResponseObject).FeeStructure_ID);

                ViewBag.PrintProgramme = printProgramme;
                ViewBag.StudentExamForm = response.ResponseObject;
                ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;

                GetResponseViewBags();
                return View(studentExamForm);
            }

            ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;
            ViewBag.Response = response;
            GetResponseViewBags();
            return View();

        }

        #endregion

        public ActionResult BacklogForm(short? semester)
        {
            var response = new ResponseData();
            GetResponseViewBags();
            ViewBag.SemesterChoosen = semester;

            response = new ExaminationFormManager().GetStudentBacklogExamForm(null, semester ?? 0);

            ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;
            ViewBag.Response = response;

            if (response.IsSuccess)
            {
                var _StudentExamForm = (ARGStudentExamFormSubjects)response.ResponseObject;
                ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;

                ViewBag.FeeStructure = new ExaminationFormManager().GetFeeStructure(((ARGExamFormDownloadable)response.SecondaryResponseObject).FeeStructure_ID);
                ViewBag.PrintProgramme = AppUserHelper.TableSuffix;
                GetResponseViewBags();
                ViewBag.StudentExamForm = response.ResponseObject;
                return View(_StudentExamForm);
            }

            SetResponseViewBags(response);
            GetResponseViewBags();
            return View();
        }

        [HttpGet]
        public ActionResult DivisionImprovement(short? semester)
        {
            if (semester == null)
                return RedirectToAction("GetInfo", "StudentInfo");

            var response = new ResponseData();
            var examinationFormManager = new ExaminationFormManager();

            ViewBag.PrintProgramme = AppUserHelper.TableSuffix;
            Guid Student_ID = AppUserHelper.User_ID;

            GetResponseViewBags();
            ViewBag.SemesterChoosen = semester;

            response = examinationFormManager.GetStudentBacklogExamForm(null, semester ?? 0, true);

            ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;
            ViewBag.Response = response;

            if (response.ResponseObject == null)
                return RedirectToAction("GetInfo", "StudentInfo");

            int totalSubjects = ((ARGStudentExamFormSubjects)response.ResponseObject).ReAppearSubjects.Count;

            // ToDo: semester wise subjects or not
            DivImprovementResponse improvementResponse =
                examinationFormManager.HasAlreadyAppliedForDivisionImprovement(Student_ID, ViewBag.PrintProgramme, semester ?? 0, totalSubjects);
            if (improvementResponse.hasAlreadyApplied)
            {
                response = new ResponseData()
                {
                    ErrorMessage = $@"You has already applied for division improvement or max allowed subjects in division improvement is reached.
                                      Indeed you can reprint."
                };
            }

            if (response.IsSuccess)
            {
                var _StudentExamForm = response.ResponseObject as ARGStudentExamFormSubjects;
                ViewBag.ARGExamFormDownloadable = response.SecondaryResponseObject;

                ViewBag.FeeStructure = examinationFormManager.GetFeeStructure(((ARGExamFormDownloadable)response.SecondaryResponseObject).FeeStructure_ID);

                GetResponseViewBags();
                ViewBag.StudentExamForm = response.ResponseObject;
                return View(_StudentExamForm);
            }

            SetResponseViewBags(response);
            GetResponseViewBags();
            return View();
        }


        #region Examination payment & Print
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(ARGStudentExamFormSubjects _StudentExamForm, string email, string mobileNumber, short semester, string secretCode)
        {
            var response = new ResponseData();

            if (email.IsNullOrEmpty() || mobileNumber.IsNullOrEmpty())
                response.ErrorMessage = " Email Address/Mobile Number not provided";

            else if (!_StudentExamForm.IsRegular && (_StudentExamForm.ReAppearSubjects == null
                       || (_StudentExamForm.ReAppearSubjects.Count == 0 || _StudentExamForm.ReAppearSubjects.Where(i => i.IsApplied).Count() == 0)))
            {
                response.IsSuccess = false;
                response.ErrorMessage = " Please select atleast one subject to make a payment or if you continue to see this please change your browser and use updated latest Google Chrome preferably.";
            }
            else if (_StudentExamForm.IsDivisionImprovement && _StudentExamForm.ReAppearSubjects.Where(i => i.IsApplied).Count() > 1)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Only one core subject is allowed in one semester in-case of division improvement.";
            }
            else
            {
                if (_StudentExamForm.IsRegular)
                    RegularExaminationFormPayment(ref response, _StudentExamForm, email, mobileNumber, semester, secretCode);
                else
                    BacklogFormPayment(ref response, _StudentExamForm, email, mobileNumber, semester, secretCode);
            }
            SetResponseViewBags(response);
            TempData["Response"] = response;
            return RedirectToAction(_StudentExamForm.IsRegular ? "ExaminationForm" : _StudentExamForm.IsDivisionImprovement ? "DivisionImprovement" : "BacklogForm", new { semester });
        }

        public ActionResult PaymentResponse()
        {
            ResponseData response = new ResponseData();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            ViewBag.IsRegular = billdeskResponse?.Item3?.AdditionalInfo?.Length == 1;
            ViewBag.ExamForm_ID = billdeskResponse?.Item3?.Entity_ID;
            response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.Examination);
            if (response.IsSuccess)
            {
                var paymentDetails = (PaymentDetails)response.ResponseObject;
                ViewData.Model = paymentDetails;
                ViewBag.Semester = paymentDetails.Semester;
            }
            else
            {
                ViewBag.PaymentFailureMessage = "In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking";
            }
            return View();
        }

        public ActionResult PrintExaminationFormReceipt(short? semester, bool? RedirectFromPaymentGateWay)
        {
            ViewBag.StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.TableSuffix, false, semester ?? 0);
            ResponseData response = new ExaminationFormManager().GetRegularStudentExamForm(semester ?? 0);
            SetResponseViewBags(response);
            GetResponseViewBags();
            if (response.IsSuccess)
            {
                var studentexamForm = (ARGStudentExamFormSubjects)response.ResponseObject;
                if (studentexamForm != null)
                {
                    ViewBag.StudentExamFormDetail = studentexamForm;
                    ViewBag.SemesterChoosen = semester;
                    ViewBag.RedirectFromPaymentGateWay = RedirectFromPaymentGateWay;
                    var _GetPaymentDetail = new PaymentManager().GetPaymentDetails(studentexamForm.StudentExamForm_ID, PaymentModuleType.Examination, AppUserHelper.OrgPrintProgramme);
                    return View(_GetPaymentDetail);
                }
            }
            return RedirectToAction("ExaminationForm", "Examination", new { area = "CUStudentZone", semester = semester });
        }

        public ActionResult PrintRegularExaminationFormReceipt(short? semester)
        {
            var studentExamForm = new ExaminationFormManager().GetExaminationFormForPrint(AppUserHelper.User_ID, semester ?? 0);
            ViewBag.SemesterChoosen = semester;
            if (studentExamForm != null)
            {
                ViewBag.StudentInfo = studentExamForm.StudentInfo;
                ViewBag.StudentExamFormDetail = studentExamForm;
                ViewBag.SemesterChoosen = semester;
                List<PaymentDetails> _GetPaymentDetail = new PaymentManager().GetPaymentDetails(studentExamForm.StudentExamForm_ID, PaymentModuleType.Examination, AppUserHelper.OrgPrintProgramme);

                if (_GetPaymentDetail.IsNotNullOrEmpty())
                {
                    if (_GetPaymentDetail.Last().TxnDate.AddMonths(50).Date < DateTime.Now.Date)
                    {
                        var response = new ResponseData
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Re-Print of old examination form for Semester-{semester} has been closed."
                        };
                        SetResponseViewBags(response);
                        return RedirectToAction("Examination");
                    }
                }
                return View("PrintExaminationFormReceipt", _GetPaymentDetail);
            }
            return View("PrintExaminationFormReceipt");
        }

        public ActionResult PrintBacklogFormReceipt(short? semester)
        {
            var studentExamForm = new ExaminationFormManager().GetBacklogExaminationFormForPrint(AppUserHelper.User_ID, semester ?? 0);
            ViewBag.SemesterChoosen = semester;
            if (studentExamForm != null)
            {
                ViewBag.StudentInfo = studentExamForm.StudentInfo;
                ViewBag.StudentExamFormDetail = studentExamForm;
                var _GetPaymentDetail = new PaymentManager().GetPaymentDetails(studentExamForm.StudentExamForm_ID, PaymentModuleType.Examination, AppUserHelper.OrgPrintProgramme);

                //check whether only skill only
                if (_GetPaymentDetail == null)
                {
                    List<ADMSubjectMaster> subjectMasterCompact = new SubjectsManager()
                        .GetList(studentExamForm?.ReAppearSubjects
                        ?.Where(x => x.FeeStatus != FormStatus.InProcess)
                        ?.Select(x => x.Subject_ID).ToList());

                    if (subjectMasterCompact?.All(x => x.HasExaminationFee == false) ?? false)
                    {
                        _GetPaymentDetail = new List<PaymentDetails>();
                    }
                }
                else
                {
                    if (_GetPaymentDetail.Last().TxnDate.AddMonths(3).Date < DateTime.Now.Date)
                    {
                        var response = new ResponseData
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Re-Print of old examination form for Semester-{semester ?? 0} has been closed."
                        };
                        SetResponseViewBags(response);
                        return RedirectToAction("Examination");
                    }
                }
                return View("PrintBacklogExaminationFormReceipt", _GetPaymentDetail);
            }
            return View("PrintBacklogExaminationFormReceipt");
        }

        public ActionResult PrintBacklogExaminationFormReceipt(Guid? StudentExamForm_ID, short? semester)
        {
            ViewBag.StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.TableSuffix, false, semester ?? 0);

            ResponseData responseData = new ExaminationFormManager().GetStudentBacklogExamForm(null, semester ?? 0);

            SetResponseViewBags(responseData);
            GetResponseViewBags();

            if (responseData.IsSuccess)
            {
                var studentexamForm = (ARGStudentExamFormSubjects)responseData.ResponseObject;
                if (studentexamForm != null)
                {
                    ViewBag.StudentExamFormDetail = studentexamForm;
                    ViewBag.SemesterChoosen = semester;
                    ViewBag.RedirectFromPaymentGateWay = false;
                    var _GetPaymentDetail = new PaymentManager().GetPaymentDetails(studentexamForm.StudentExamForm_ID, PaymentModuleType.Examination, AppUserHelper.OrgPrintProgramme);
                    if (_GetPaymentDetail == null)
                    {
                        List<ADMSubjectMaster> subjectMasterCompact = new SubjectsManager().GetList(studentexamForm?.ReAppearSubjects?.Where(x => x.FeeStatus != FormStatus.InProcess)?.Select(x => x.Subject_ID).ToList());
                        if (subjectMasterCompact?.All(x => x.HasExaminationFee == false) ?? false)
                        {
                            _GetPaymentDetail = new List<PaymentDetails>();
                        }
                    }
                    return View(_GetPaymentDetail);
                }
            }
            return RedirectToAction("ExaminationForm", "Examination", new { area = "CUStudentZone", semester });
        }

        //public ActionResult GetCombination(short? semester)
        //{
        //    GetResponseViewBags();
        //    ViewBag.SemesterChoosen = semester;
        //    ARGPersonalInformation stdinfo = null;
        //    if (semester.HasValue)
        //    {
        //        stdinfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.TableSuffix, false, semester.Value);
        //        ARGSelectedCombination _PrevSemCombination = new CombinationManager().GetSelectedCombination(stdinfo.Student_ID, AppUserHelper.TableSuffix, (short)(semester - 1 > 1 ? semester - 1 : 1)) ?? new CombinationManager().GetSelectedCombination(stdinfo.Student_ID, AppUserHelper.TableSuffix, 1);

        //        if (stdinfo != null && _PrevSemCombination != null)
        //        {
        //            var _CombinationSetting = new CombinationSettingManager().GetCombinationSetting(_PrevSemCombination.Course_ID, semester.Value, stdinfo.Batch);
        //            if (_CombinationSetting != null)
        //            {
        //                ViewBag.CombinationSetting = _CombinationSetting;
        //                ViewBag.PreviousSemCombination = _PrevSemCombination;
        //                Parameters parameter;
        //                if (_CombinationSetting.SEC_Count > 0)
        //                {
        //                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = GetPrintProgrammes(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = semester.Value.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Value = stdinfo.AcceptCollege_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.SEC).ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
        //                    ViewBag.SkillEnhanceSubjects = new SubjectsManager().SubjectListCompact(parameter);
        //                }
        //                if (_CombinationSetting.GE_OE_Count > 0)
        //                {
        //                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = GetPrintProgrammes(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = semester.Value.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "College_ID", Value = stdinfo.AcceptCollege_ID.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.GE).ToString() + "," + ((short)SubjectType.OE).ToString(), Operator = SQLOperator.In } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
        //                    ViewBag.GeneralElectiveSubjects = new SubjectsManager().SubjectListCompact(parameter);
        //                }
        //                if (_CombinationSetting.DCE_DSE_Count > 0)
        //                {
        //                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = GetPrintProgrammes(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = semester.Value.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.DCE).ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "ADMCollegeCourseMapping", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
        //                    ViewBag.DesciplineCentricElectiveCourses = new SubjectsManager().SubjectListCompact(parameter);
        //                }
        //                if (_CombinationSetting.OC_Count > 0)
        //                {
        //                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = semester.Value.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = ((short)SubjectType.OC).ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { TableAlias = "ADMCollegeCourseMapping", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
        //                    ViewBag.OptionalCoreCourseCount = new SubjectsManager().SubjectListCompact(parameter);
        //                }
        //                if (_CombinationSetting.AdditionalCourseCount > 0)
        //                {
        //                    parameter = new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = "Programme", Value = GetPrintProgrammes(_PrevSemCombination.Course_ID), Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "ADMSubjectMaster", Column = "Status", Value = "1", Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "Semester", Value = semester.Value.ToString(), Operator = SQLOperator.EqualTo }, new SearchFilter() { Column = "SubjectType", Value = $"{((short)SubjectType.Core).ToString()},{((short)SubjectType.OC).ToString()}", Operator = SQLOperator.In }, new SearchFilter() { TableAlias = "", Column = "Course_ID", Value = _PrevSemCombination.Course_ID.ToString(), Operator = SQLOperator.EqualTo } }, PageInfo = new Paging() { DefaultOrderByColumn = "SubjectFullName", PageNumber = -1, PageSize = -1 }, SortInfo = new Sort() { ColumnName = "SubjectFullName", OrderBy = System.Data.SqlClient.SortOrder.Ascending } };
        //                    ViewBag.AdditionalCourses = new SubjectsManager().SubjectListCompact(parameter);
        //                    List<StudentAdditionalSubject> studentAddionalSubjects = new StudentManager().GetStudentAdditionalSubjects(stdinfo.Student_ID, semester.Value, AppUserHelper.TableSuffix);
        //                    ViewBag.StudentAddionalSubjects = studentAddionalSubjects;
        //                }
        //                return View(stdinfo);
        //            }
        //            else
        //            {
        //                TempData["ErrorMessage"] = $"Combination Setting not available. Please contact Cluster University I.T Section.";
        //            }
        //        }
        //    }
        //    return RedirectToAction("Examination", new { @semester = semester });
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult PostCombination(short? Semester, List<Guid> Subject, List<Guid> AdditionalSubject)
        //{
        //    ResponseData response = new ResponseData();

        //    List<ADMSubjectMaster> subjectmasterlist = new List<ADMSubjectMaster>();
        //    List<ADMSubjectMaster> addionalSubjectmasterlist = new List<ADMSubjectMaster>();
        //    if (Subject != null)
        //        foreach (var item in Subject.Where(x => x != Guid.Empty))
        //            subjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });
        //    if (AdditionalSubject != null)
        //        foreach (var item in AdditionalSubject.Where(x => x != Guid.Empty))
        //            addionalSubjectmasterlist.Add(new ADMSubjectMaster() { Subject_ID = item });

        //    //response = new AssignCombinationManager().SaveUpdateStudentCombination(AppUserHelper.User_ID, Semester.Value, AppUserHelper.TableSuffix, subjectmasterlist, addionalSubjectmasterlist);
        //    response = new AssignCombinationManager().SaveUpdateStudentCombination(AppUserHelper.TableSuffix, AppUserHelper.User_ID, Semester.Value, subjectmasterlist, addionalSubjectmasterlist);
        //    SetResponseViewBags(response);
        //    if (response.IsSuccess)
        //        return RedirectToAction("ExaminationForm", new { @semester = Semester });
        //    else
        //        return RedirectToAction("GetCombination", new { @semester = Semester });
        //}

        private string GetPrintProgrammes(Guid course_ID)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCourseById(course_ID);
            string printPrg = "";
            switch (aDMCourseMaster.PrintProgramme)
            {
                case PrintProgramme.IH:
                case PrintProgramme.UG:
                    printPrg = ((short)Programme.UG).ToString() + "," + ((short)Programme.IG).ToString();
                    break;
                case PrintProgramme.PG:
                    printPrg = ((short)Programme.PG).ToString();
                    break;
                case PrintProgramme.BED:
                    printPrg = ((short)PrintProgramme.BED).ToString();
                    break;
            }
            return printPrg;
        }

        public ActionResult HardCodeFee()
        {

            if (new PaymentManager().PaymentExistsUG(AppUserHelper.User_ID, 9))
            {
                TempData["testmessage"] = "Payment already made ... ur subjects will be updated soon";
                return RedirectToAction("HardCodeFeeResponse");
            }
            else
            {


                BillDeskRequest billDeskRequest = new BillDeskRequest()
                {
                    Entity_ID = AppUserHelper.User_ID,
                    Email = "faheem.r@cusrinagar.edu.in",
                    PhoneNo = "7006774481",
                    TotalFee = 275,
                    CustomerID = DateTime.Now.Ticks.ToString(),
                    AdditionalInfo = "HardCodeFee",
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Student_ID = AppUserHelper.User_ID,
                    Semester = "9",
                    ReturnURL = "CUStudentZone/Examination/HardCodeFeeResponse"
                };

                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.EXM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
            }

            return new EmptyResult();
        }

        public ActionResult HardCodeFeeResponse()
        {
            ResponseData response = new ResponseData();
            System.Web.HttpContext.Current.Response.Cache.SetNoStore();
            Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponse = new BillDeskManager().BillDeskResponse(Request.InputStream);
            response = new PaymentManager().SavePayment(billdeskResponse, AppUserHelper.TableSuffix, billdeskResponse.Item1, PaymentModuleType.Examination);
            ViewBag.PaymentSuccess = TempData["testmessage"];
            if (response.IsSuccess)
            {
                var paymentDetails = (PaymentDetails)response.ResponseObject;
                ViewData.Model = paymentDetails;
                ViewBag.Semester = paymentDetails.Semester;
            }
            else
            {
                ViewBag.PaymentFailureMessage = "In-case Amount is not deducted try to enable E-COM in your card using your bank App or ATM like M-Pay, or try to use other methods of online banking";
            }
            return View();
        }

        #endregion

        #region Private Methods
        private List<SelectListItem> GetListItems(bool isRegular)
        {
            var list = new ExaminationFormManager().ExamForSemester(AppUserHelper.OrgPrintProgramme, isRegular) ?? new List<SelectListItem>();

            List<SelectListItem> allowedSemestersList = new List<SelectListItem>();

            Parameters parameters = new Parameters()
            {
                Filters = new List<SearchFilter>()
                    {
                        new SearchFilter()
                        {
                            Column = nameof(ARGPersonalInformation.Student_ID),
                            Operator = SQLOperator.EqualTo,
                            Value = AppUserHelper.User_ID.ToString(),
                            TableAlias = nameof(ARGPersonalInformation)
                        },
                        new SearchFilter()
                        {
                            Column = nameof(ExaminationWhiteListCompact.ExaminationYear),
                            Operator = SQLOperator.EqualTo,
                            Value = DateTime.Now.Year.ToString()
                        }
                    }
            };

            List<ExaminationWhiteList> whiteListedEntries = new ExaminationWhiteListManager().GetAllApplications(parameters);

            //allow to one week for previous year after New Year
            if (whiteListedEntries.IsNullOrEmpty() && DateTime.Now.Month == 02 && DateTime.Now.Day <= 28)
            {
                parameters.Filters.First(x => x.Column == nameof(ExaminationWhiteListCompact.ExaminationYear)).Value = (DateTime.Now.Year - 1).ToString();
                whiteListedEntries = new ExaminationWhiteListManager().GetAllApplications(parameters);
            }


            if (whiteListedEntries.IsNotNullOrEmpty())
            {
                foreach (ExaminationWhiteList whiteListedEntry in whiteListedEntries)
                {
                    allowedSemestersList = new List<SelectListItem>()
                    {
                        new SelectListItem()
                        {
                            Text =  (whiteListedEntry.Semester).ToString(),
                            Value = (whiteListedEntry.Semester).ToString()
                        }
                    };

                    if (isRegular == !whiteListedEntry.IsBacklog)
                    {
                        list.AddRange(allowedSemestersList);
                    }
                }
            }
            return list;
        }

        private ActionResult BacklogFormPayment(ref ResponseData response, ARGStudentExamFormSubjects _StudentExamForm, string Email, string MobileNumber, short semester, string secretCode)
        {
            // Print the Form if all the subjects have been applied for. Else add the subject.
            if (_StudentExamForm.ReAppearSubjects.Where(i => i.FeeStatus == FormStatus.FeePaid).ToList().Count == _StudentExamForm.ReAppearSubjects.Count)
            {
                if (_StudentExamForm.Status == FormStatus.Accepted)
                    return PrintExaminationFormReceipt(semester, false);
            }

            response = new ExaminationFormManager().GetStudentBacklogExamForm(_StudentExamForm, semester);

            var studentExamForm = (ARGStudentExamFormSubjects)response.ResponseObject;
            studentExamForm.ReAppearSubjects = _StudentExamForm.ReAppearSubjects;
            studentExamForm.IsDivisionImprovement = _StudentExamForm.IsDivisionImprovement;

            if (response.IsSuccess)
            {
                var settingsForm = new ExaminationFormManager().GetBacklogExamFormDownloadable(AppUserHelper.TableSuffix, studentExamForm.Semester);

                if (new ExaminationFormManager().SaveExaminationFormAndSubjectsForBacklogStudents(studentExamForm, settingsForm, AppUserHelper.User_ID, semester) > 0)
                {
                    response.IsSuccess = true;
                    response.SuccessMessage = " Examination Form and Subjects saved successfully.";
                }

                string SubjectCodes = null;
                SubjectCodes = string.Join("@", studentExamForm.ReAppearSubjects.Where(x => x.IsApplied && x.FeeStatus == FormStatus.InProcess)?.Select(i => i.SubjectCode.ToString()));
                SubjectCodes += $"#{studentExamForm.LateFeeAmount}";

                //check if Specialled Abled
                if (new ExaminationFormManager().IsSpecialledAbledStudent(AppUserHelper.User_ID))
                {
                    studentExamForm.AmountToBePaid = 0;
                }

                if (studentExamForm.AmountToBePaid == 0)
                {
                    new ExaminationFormManager().UpdateExaminationFormManually(studentExamForm.AmountToBePaid, studentExamForm.StudentExamForm_ID, AppUserHelper.OrgPrintProgramme, SubjectCodes, false);
                    PaymentDetails dummyPayment = new ExaminationFormManager().GetDummyPayment(studentExamForm.StudentExamForm_ID, PaymentModuleType.Examination, studentExamForm.Semester, 0);
                    dummyPayment.TxnReferenceNo += "_SA";
                    dummyPayment.PhoneNumber = MobileNumber;
                    dummyPayment.Email = Email;
                    new PaymentManager().SavePaymentDetails(dummyPayment, AppUserHelper.OrgPrintProgramme);
                    return PrintExaminationFormReceipt(semester, false);
                }
                else
                    CreateBillDeskRequest(studentExamForm, Email, MobileNumber, semester, false, SubjectCodes, secretCode);

            }
            return new EmptyResult();
        }

        private ActionResult RegularExaminationFormPayment(ref ResponseData response, ARGStudentExamFormSubjects _StudentExamForm, string Email, string MobileNumber, short semester, string secretCode)
        {
            response = new ExaminationFormManager().GetRegularStudentExamForm(semester);
            var settingsForm = (ARGExamFormDownloadable)response.SecondaryResponseObject;

            if (settingsForm == null)
            {
                return RedirectToAction("ExaminationForm");
            }

            if (!response.IsSuccess)
                return new EmptyResult();

            var studentExamForm = (ARGStudentExamFormSubjects)response.ResponseObject;
            if (studentExamForm.Status == FormStatus.Accepted)
                return PrintExaminationFormReceipt(semester, false);


            if (settingsForm.IsHardCodeFeeSet && studentExamForm.Status != FormStatus.BlockSemesterAdmitCard && ((studentExamForm.AmountToBePaid + studentExamForm.AmountPaid) > settingsForm.HardCodeFeeAmount))
            {
                response.IsSuccess = false;
                response.ErrorMessage =
                    "You total payment against this form is exceeding the total form amount. Please visit I.T Section Cluster University.";
            }
            else
            {
                //Receive More Payment
                if (studentExamForm.Status == FormStatus.BlockSemesterAdmitCard)
                {
                    studentExamForm.AmountToBePaid = studentExamForm.PendingFeeAmount;
                    studentExamForm.AmountPaid = studentExamForm.PendingFeeAmount;
                }
                else if (studentExamForm.AmountPaid >= studentExamForm.AmountToBePaid)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage =
                        "You have already made payment against this form. Please visit I.T Section Cluster University for Reconciliation.";
                }
            }

            if (studentExamForm.RecordState == RecordState.New)
                new ExaminationFormManager().GetAndUpdateFormNumber(studentExamForm, settingsForm);


            //check if Specialled Abled
            if (new ExaminationFormManager().IsSpecialledAbledStudent(AppUserHelper.User_ID))
            {
                studentExamForm.AmountToBePaid = 0;
                new ExaminationFormManager().UpdateExaminationFormManually(studentExamForm.AmountToBePaid, studentExamForm.StudentExamForm_ID, AppUserHelper.OrgPrintProgramme, secretCode, false);
                PaymentDetails dummyPayment = new ExaminationFormManager().GetDummyPayment(studentExamForm.StudentExamForm_ID, PaymentModuleType.Examination, studentExamForm.Semester, 0);
                dummyPayment.TxnReferenceNo += "_SA";
                dummyPayment.PhoneNumber = MobileNumber;
                dummyPayment.Email = Email;
                new PaymentManager().SavePaymentDetails(dummyPayment, AppUserHelper.OrgPrintProgramme);
                return PrintExaminationFormReceipt(semester, true);
            }
            else
            {
                CreateBillDeskRequest(studentExamForm, Email, MobileNumber, semester, true, null, secretCode);
            }

            return new EmptyResult();
        }

        private void CreateBillDeskRequest(ARGStudentExamFormSubjects StudentExamForm, string Email, string MobileNumber, short? semester, bool isRegular, string subjectCodes, string secretCode)
        {
            List<string> secretCodes = new List<string>() { "AQIB_786", "GULSHAN_786", "TANVEER_786", "YASMEEN_786" };

            _IsRegular = isRegular;

            var amount = StudentExamForm.AmountToBePaid + StudentExamForm.LateFeeAmount;

            BillDeskRequest billDeskRequest = new BillDeskRequest()
            {
                Entity_ID = StudentExamForm.StudentExamForm_ID,
                Email = Email,
                PhoneNo = MobileNumber,
                TotalFee = Convert.ToInt16(amount),
                CustomerID = DateTime.Now.Ticks.ToString(),
                PrintProgramme = AppUserHelper.TableSuffix,
                Student_ID = StudentExamForm.Student_ID,
                Semester = semester.Value.ToString(),
                ReturnURL = "CUStudentZone/Examination/PaymentResponse",
                AdditionalInfo = StudentExamForm.FormNumber,
                NonBillDeskField = StudentExamForm.FormNumber
            };

            billDeskRequest.AdditionalInfo = isRegular ? "1" : subjectCodes;

            if (secretCodes.Contains(secretCode?.ToUpper()))
            {
                PaymentDetails payment = new PaymentDetails()
                {
                    Payment_ID = Guid.NewGuid(),
                    Entity_ID = billDeskRequest.Entity_ID,
                    Email = billDeskRequest.Email,
                    PhoneNumber = billDeskRequest.PhoneNo,
                    TxnAmount = billDeskRequest.TotalFee,
                    ModuleType = PaymentModuleType.Examination,
                    PaymentType = PaymentType.Offline,
                    TxnReferenceNo = StudentExamForm.FormNumber,
                    PrintProgramme = AppUserHelper.TableSuffix,
                    Semester = StudentExamForm.Semester,
                    TxnDate = DateTime.Now,
                    Student_ID = StudentExamForm.Student_ID,
                    EntityID = subjectCodes
                };

                Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponseforSave
                        = new Tuple<bool, string, PaymentDetails, Guid, Guid>(true, "Payment Made Locally", payment, Guid.Empty, Guid.Empty);

                var responseData = new PaymentManager().SavePayment(billdeskResponseforSave, AppUserHelper.TableSuffix, true, PaymentModuleType.Examination);
                if (responseData.IsSuccess)
                {
                    PaymentDetails paymentDetails = ((PaymentDetails)responseData.ResponseObject);
                    new ExaminationFormManager().UpdateExaminationFormManually(paymentDetails.TxnAmount, paymentDetails.Entity_ID, AppUserHelper.TableSuffix, subjectCodes, true);
                }
            }
            else
            {
                var request = new BillDeskManager().GenerateRequestString(billDeskRequest, BillDeskPaymentType.EXM);
                var htmlForm = new BillDeskManager().GenerateHTMLForm(request);
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.Write(htmlForm.ToString());
                System.Web.HttpContext.Current.Response.End();
            }
        }

        #endregion

        #region SemesterExamAdmitCard routines
        private void SetViewBags()
        {
            var Manager = new ExaminationFormManager();
            List<PrintProgramme> _PrintProgrammeOption = Manager.GetAdmitPrintProgrammeDDL();
            if (_PrintProgrammeOption.IsNullOrEmpty())
            {
                ViewBag.PrintProgrammeOption = null;
            }
            else
                ViewBag.PrintProgrammeOption = Helper.GetSelectForAdmission(_PrintProgrammeOption).OrderBy(x => x.Text);

            List<SelectListItem> SemestersDDL = Manager.GetSemesterDDL(AppUserHelper.OrgPrintProgramme);
            if (SemestersDDL.IsNullOrEmpty())
                ViewBag.PrintProgrammeOption = null;

            ViewBag.Semester = SemestersDDL;
        }

        [HttpGet]
        public ActionResult PrintAdmitCard()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrintAdmitCard(PrintExamAdmitCards printExamAdmitCards)
        {
            SetViewBags();
            var Manager = new ExaminationFormManager();
            if (!ModelState.IsValid)
                return View(printExamAdmitCards);
            Guid Student_ID = Manager.GetStudentID(printExamAdmitCards.Semester);
            if (Student_ID == Guid.Empty || (bool)Manager.GetAdmitSemesterDDL()?.All(s => s != printExamAdmitCards.Semester))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>Discrepancies : <br/>Examination Form not submitted.<br/>Or Exam Fee not paid.</a></div>";
                return RedirectToAction("PrintAdmitCard", "Examination", new { area = "CUStudentZone" });
            }
            return RedirectToAction("PrintPriview", "Examination", new { S = printExamAdmitCards.Semester.ToString().EncryptCookieAndURLSafe(), area = "CUStudentZone" });
        }


        [HttpGet]
        public ActionResult PrintPriview(string S)
        {
            if (!short.TryParse(S.DecryptCookieAndURLSafe(), out short sem))
            {
                return RedirectToAction("PrintAdmitCard", "Examination", new { area = "CUStudentZone" });
            }
            var ExaminationFormManager = new ExaminationFormManager();
            List<PrintProgramme> _PrintProgrammeOption = ExaminationFormManager.GetAdmitPrintProgrammeDDL();
            if (_PrintProgrammeOption.IsNullOrEmpty())
            {
                return RedirectToAction("PrintAdmitCard", "Examination", new { area = "CUStudentZone" });
            }
            ARGPersonalInformation aRGPersonalInformation = new RegistrationManager().GetStudentByID(AppUserHelper.User_ID, AppUserHelper.TableSuffix, sem);
            if (aRGPersonalInformation == null)
            {
                return RedirectToAction("PrintAdmitCard", "Examination", new { area = "CUStudentZone" });
            }
            Tuple<string, string, List<ADMSubjectMaster>, ARGStudentExamForm, SemesterExamCenterDetails> AdmitCardDetails = ExaminationFormManager.GetAdmitCardDetails(AppUserHelper.User_ID, sem, AppUserHelper.OrgPrintProgramme, aRGPersonalInformation.Batch);
            ViewBag.AdmitCardDetails = AdmitCardDetails;

            string ErrorMsg = AdmitCardDetails.Item1;
            if (string.IsNullOrEmpty(AdmitCardDetails.Item4.ExamRollNumber))
            {
                TempData["response"] = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>{ErrorMsg}</a></div>";
                return RedirectToAction("PrintAdmitCard", "Examination", new { area = "CUStudentZone" });
            }

            return View(aRGPersonalInformation);
        }
        #endregion
    }
}