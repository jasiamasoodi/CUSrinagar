using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web;
using GeneralModels;
using CUSrinagar.Enums;
using System.Transactions;
using CUSrinagar.Extensions;
using System.ComponentModel.Design;

namespace CUSrinagar.BusinessManagers
{
    public class ExaminationFormManager
    {
        public DivImprovementResponse HasAlreadyAppliedForDivisionImprovement(Guid Student_ID, PrintProgramme printProgramme, short semester, int totalSubjects)
        {
            return new ExaminationFormDB().HasAlreadyAppliedForDivisionImprovement(Student_ID, printProgramme, semester, totalSubjects);
        }

        public Tuple<bool, string> IsEligibleForDivisionImprovement(Guid Student_ID, PrintProgramme printProgramme)
        {
            ResultCompact _resultCompact = new ResultManager().GetResult(printProgramme, null, Student_ID, true);

            if (_resultCompact == null)
                return Tuple.Create(false, "No result found");

            if (_resultCompact.SubjectResults.IsNullOrEmpty())
                return Tuple.Create(false, "No result found");

            bool isFullyPass = _resultCompact.SubjectResults.All(x => x.OverallResultStatus == ResultStatus.P && x.ShowPublic);
            DateTime notificationDate = _resultCompact.SubjectResults.Max(x => x.NotificationDate) ?? DateTime.MinValue;

            if (notificationDate == DateTime.MinValue)
                return Tuple.Create(false, "Result Completion date not available");

            DateTime dateOfExpiryforDivImprovement = notificationDate.AddDays(806);//730 = 2 years

            bool isDateOfExpiryforDivImprovementNotOver = dateOfExpiryforDivImprovement.Date >= DateTime.Now.Date;

            //ADMCourseMaster courseOfStudent = new CourseManager().GetCourseById(_resultCompact.Course_ID);
            //bool hasAllSemesterResults = courseOfStudent.Duration == _resultCompact.SubjectResults.Max(x => x.Semester);

            return Tuple.Create(isFullyPass && isDateOfExpiryforDivImprovementNotOver, "");
        }

        #region General Methods
        public ARGExamFormDownloadable GetRegularExamFormDownloadable(PrintProgramme printProgramme, short semester, bool IsBacklogSetting = true)
        {
            ARGExamFormDownloadable settingsForm = null;
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);

            if (ExaminationAllowedForStudentCourse(AppUserHelper.User_ID, printProgramme, semester))
                settingsForm = new ExaminationFormDB().GetRegularExamFormDownloadable(courseCategory, semester);

            return settingsForm;
        }


        public ARGExamFormDownloadable GetRegularExamFormDownloadable(PrintProgramme printProgramme, short semester)
        {
            ARGExamFormDownloadable settingsForm = null;
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);

            if (ExaminationAllowedForStudentCourse(AppUserHelper.User_ID, printProgramme, semester))
                settingsForm = new ExaminationFormDB().GetRegularExamFormDownloadableWithoutDateValidation(courseCategory, semester);

            return settingsForm;
        }

        public ARGExamFormDownloadable GetBacklogExamFormDownloadable(PrintProgramme printProgramme, short semester)
        {
            ARGExamFormDownloadable settingsForm = null;
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);

            if (ExaminationAllowedForStudentCourse(AppUserHelper.User_ID, printProgramme, semester))
                settingsForm = new ExaminationFormDB().GetBacklogExamFormDownloadableWithoutDateValidation(courseCategory, semester);

            return settingsForm;
        }


        public ARGExamFormDownloadable GetBacklogExamFormDownloadable(PrintProgramme printProgramme, short semester, bool IsBacklogSetting = true)
        {
            ARGExamFormDownloadable settingsForm = null;
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(printProgramme);

            if (ExaminationAllowedForStudentCourse(AppUserHelper.User_ID, printProgramme, semester))
                settingsForm = new ExaminationFormDB().GetBacklogExamFormDownloadable(courseCategory, semester);

            return settingsForm;
        }

        private bool ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new ExaminationFormDB().ExaminationAllowedForStudentCourse(user_ID, printProgramme, semester);
        }

        public List<StudentExamFormList> GetExaminationFormList(short semester, PrintProgramme printProgramme, bool getUnpaidStudents)
        {
            return new ExaminationFormDB().GetExaminationFormList(semester, printProgramme, getUnpaidStudents);
        }

        public List<SelectListItem> ExamForSemester(PrintProgramme printProgramme, bool isRegular)
        {
            new ExaminationFormDB().CloseExaminationForm();
            if (isRegular)
                return new ExaminationFormDB().RegularExamForSemester(printProgramme);
            else
                return new ExaminationFormDB().BacklogExamForSemester(printProgramme);
        }

        public FeeStructure GetFeeStructure(Guid feeStructure_ID)
        {
            return new ExaminationFormDB().GetFeeStructure(feeStructure_ID);
        }

        public ResponseData GetRegularStudentExamForm(short semester)
        {
            var responseData = new ResponseData();

            ARGExamFormDownloadable settingsForm = null;

            // Allow if the student for the applying semester is whitelisted.
            if (CheckStudentIsWhiteListed(semester))
            {
                settingsForm = GetRegularExamFormDownloadable(AppUserHelper.TableSuffix, semester);

                //temporary change to allow examination forms without Internals.
                responseData = new ResponseData() { IsSuccess = true };//CheckForMissingInternals(settingsForm.Semester);
                responseData.SecondaryResponseObject = settingsForm;

                if (!responseData.IsSuccess)
                {
                    return responseData;
                }
            }
            else
            {
                settingsForm = GetRegularExamFormDownloadable(AppUserHelper.TableSuffix, semester);

                // Check if Examination Form Application should be allowed.
                responseData = ValidateExaminationForm(settingsForm, semester, true);
                responseData.SecondaryResponseObject = settingsForm;

                if (!responseData.IsSuccess)
                    return responseData;
            }

            if (settingsForm == null)
                return new ResponseData() { IsSuccess = false, ErrorMessage = "The process of applying for exam form is either closed or will be allowed soon. Please refer to University Notification." };

            #region Check is applicable for Regular or not
            short CurrentBatchOrSemesterBatch = new ExaminationFormDB().GetStudentCurrentBatch(AppUserHelper.User_ID, settingsForm.Semester, settingsForm.PrintProgramme);

            if (CurrentBatchOrSemesterBatch <= 0)
                return new ResponseData() { IsSuccess = false, ErrorMessage = "You donot have any college or combination assigned. Visit your respective college." };

            if (settingsForm.RegularBatch != CurrentBatchOrSemesterBatch)
                return new ResponseData() { IsSuccess = false, ErrorMessage = "You are not eligible for Regular Exam, please apply as Backlog." };
            #endregion

            var _StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester);
            _StudentInfo.Batch = CurrentBatchOrSemesterBatch;
            if (_StudentInfo == null || (_StudentInfo?.SelectedCombinations.IsNullOrEmpty() ?? true))
            {
                return new ResponseData() { ErrorMessage = $"Subject Combination not ASSIGNED by College. Please visit your College to ASSIGN subject combination of Semester-{settingsForm.Semester} immediately." };
            }
            else if (_StudentInfo.SelectedCombinations.Any(x => x.IsVerified != true))
            {
                return new ResponseData() { ErrorMessage = $"Subject Combination not VERIFIED by College. Please visit your College to VERIFY subject combination of Semester-{settingsForm.Semester} immediately." };
            }


            Tuple<bool, string, List<ADMSubjectMaster>> blockExamForm = BlockSemesterAdmitCardOrNot(_StudentInfo.Student_ID, semester, settingsForm.PrintProgramme, _StudentInfo.Batch, _StudentInfo?.SelectedCombinations?.First()?.Subjects ?? new List<ADMSubjectMaster>(), "", false);


            // If the Student has applied for the form return back the form else check if form can be applied.
            var studentExamForm = GetRegularExaminationForm(settingsForm);

            if (studentExamForm == null && blockExamForm.Item1 == false)
            {
                if (responseData.IsSuccess)
                {
                    studentExamForm = new ARGStudentExamFormSubjects
                    {
                        Student_ID = AppUserHelper.User_ID,
                        StudentExamForm_ID = Guid.NewGuid(),
                        Semester = settingsForm.Semester,
                        Year = settingsForm.Year,
                        ExamRollNumber = null,
                        Notification_ID = settingsForm.Notification_ID,
                        Status = FormStatus.InProcess,
                        RecordState = RecordState.New,
                        IsRegular = true,
                        ReAppearSubjects = null,
                        StudentInfo = _StudentInfo
                    };
                    studentExamForm.AdditionalSubjects = new StudentManager().GetStudentAdditionalSubjects(studentExamForm.Student_ID, settingsForm.Semester, settingsForm.PrintProgramme);
                    studentExamForm.AmountToBePaid = GetAmountForStudent(studentExamForm, settingsForm);

                    if (studentExamForm.AmountToBePaid == -1)
                    {
                        responseData.ResponseObject = studentExamForm;
                        responseData.IsSuccess = false;
                        responseData.ErrorMessage = "An error in mismatch in subject fee calculation. Please contact Cluster University I.T Section immediately";
                        return responseData;
                    }

                    responseData.ResponseObject = studentExamForm;
                    responseData.IsSuccess = true;
                    return responseData;
                }
                responseData.ResponseObject = null;
                return responseData;
            }
            else
            {
                responseData.ResponseObject = studentExamForm;
                responseData.IsSuccess = true;

                //In-case of block admit card
                if (blockExamForm.Item1)
                {
                    responseData.IsSuccess = false;
                    responseData.ErrorMessage = blockExamForm.Item2;
                }

                return responseData;
            }

        }

        public ARGStudentExamFormSubjects GetRegularExaminationForm(ARGExamFormDownloadable settingsForm)
        {
            var studentExamForm = new ExaminationFormDB().GetStudentExamForm(AppUserHelper.User_ID, settingsForm, settingsForm.PrintProgramme, true);

            if (studentExamForm == null)
                return null;

            studentExamForm.StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester);
            studentExamForm.AdditionalSubjects = new StudentManager().GetStudentAdditionalSubjects(studentExamForm.Student_ID, settingsForm.Semester, settingsForm.PrintProgramme);
            studentExamForm.AmountToBePaid = GetAmountForStudent(studentExamForm, settingsForm);
            studentExamForm.RecordState = RecordState.Old;
            return studentExamForm;
        }

        public void UpdateExaminationFormManually(decimal txnAmount, Guid entity_ID, PrintProgramme printProgramme, string subjectCodesString, bool isBillDeskPayment)
        {

            var lateFeeAmountString = string.Empty; var lateFeeAmount = 0;

            if (subjectCodesString.IsNotNullOrEmpty())
            {
                var startIndex = subjectCodesString.IndexOf('#');
                var length = subjectCodesString.Length - subjectCodesString.IndexOf('#');

                if (startIndex >= 0)
                {
                    lateFeeAmountString = subjectCodesString.Substring(startIndex, length)?.Replace("#", "");
                    int.TryParse(lateFeeAmountString, out lateFeeAmount);
                }

                //Remove the Late fee Amount and Separator to fetch the subjects properly.
                subjectCodesString = subjectCodesString.Replace($"#{lateFeeAmountString}", "");
            }

            string[] subjectCodes = subjectCodesString?.Split('@');

            using (TransactionScope transaction = new TransactionScope())
            {
                new ExaminationFormDB().UpdateExaminationFormStatus(txnAmount, lateFeeAmount, entity_ID, printProgramme, true);

                if (subjectCodes.IsNotNullOrEmpty())
                    new ExaminationFormManager().UpdateFeeStatusForBackLogSubject(entity_ID, subjectCodes, isBillDeskPayment);

                transaction.Complete();
            }

        }

        public PaymentDetails GetDummyPayment(Guid Entity_ID, PaymentModuleType paymentModuleType, short semester, int amount)
        {
            return new PaymentDetails
            {
                Payment_ID = Guid.NewGuid(),
                Entity_ID = Entity_ID,
                PaymentType = PaymentType.Offline,
                ModuleType = paymentModuleType,
                TxnAmount = amount,
                TxnDate = DateTime.Now,
                TxnReferenceNo = DateTime.Now.Ticks.ToString(),
                PhoneNumber = "0000000000",
                Email = "offlinePayment@cusrinagar.edu.in",
                Semester = semester,
                BankID = "JKCUS",
                TxnType = "INR",
                CurrencyName = "INR"
            };
        }

        public bool IsSpecialledAbledStudent(Guid Student_ID)
        {
            return new ExaminationFormDB().IsSpecialledAbledStudent(Student_ID);
        }


        internal void UpdateFeeStatusForBackLogSubject(Guid studentReExamFormId, string[] subjectCodes, bool isBillDeskPayment)
        {
            string inClause = subjectCodes.ToIN();
            List<Guid> Subject_IDs = new SubjectDB().GetSubjectIdentifiersFromSubjectCodes(inClause);
            if (Subject_IDs.IsNotNullOrEmpty())
                new ExaminationFormDB().UpdateFeeStatusForBackLogSubjects(studentReExamFormId, Subject_IDs, isBillDeskPayment);
        }

        public ARGStudentExamFormSubjects GetExaminationFormForPrint(Guid student_ID, short semester)
        {
            var studentExamForm = new ExaminationFormDB().GetExaminationFormForPrint(student_ID, semester);
            if (studentExamForm != null)
            {
                studentExamForm.StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, true, semester);
                studentExamForm.ReAppearSubjects = new ResultDB().GetAppliedReAppearSubjects(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme);
                studentExamForm.AdditionalSubjects = new StudentManager().GetStudentAdditionalSubjects(studentExamForm.Student_ID, semester, AppUserHelper.OrgPrintProgramme);
            }
            return studentExamForm;
        }


        public ARGStudentExamFormSubjects GetBacklogExaminationFormForPrint(Guid student_ID, short semester)
        {
            var studentExamForm = new ExaminationFormDB().GetBacklogExaminationFormForPrint(student_ID, semester);
            if (studentExamForm != null)
            {
                studentExamForm.StudentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, true, semester);
                studentExamForm.ReAppearSubjects = new ResultDB().GetAppliedReAppearSubjects(studentExamForm.StudentExamForm_ID, AppUserHelper.OrgPrintProgramme);
                studentExamForm.AdditionalSubjects = new StudentManager().GetStudentAdditionalSubjects(studentExamForm.Student_ID, semester, AppUserHelper.OrgPrintProgramme);
            }
            return studentExamForm;
        }


        public ResponseData GetStudentBacklogExamForm(ARGStudentExamFormSubjects studentExamFormSubjects, short semester, bool isDivisionImprovement = false)
        {
            ARGExamFormDownloadable settingsForm = null;
            ResponseData responseData = new ResponseData() { IsSuccess = true };

            // Allow if the student for the applying semester is whitelisted.
            if (CheckStudentIsWhiteListed(semester))
            {
                settingsForm = GetBacklogExamFormDownloadable(AppUserHelper.TableSuffix, semester);
                responseData.SecondaryResponseObject = settingsForm;
            }
            else
            {
                settingsForm = GetBacklogExamFormDownloadable(AppUserHelper.TableSuffix, semester);
                responseData = ValidateExaminationForm(settingsForm, semester, false);
                responseData.SecondaryResponseObject = settingsForm;
            }

            StudentManager studentManager = new StudentManager();

            #region Check is applicable for Backlog or not
            short CurrentBatchOrSemesterBatch = new ExaminationFormDB().GetStudentCurrentBatch(AppUserHelper.User_ID, settingsForm?.Semester ?? 0, settingsForm?.PrintProgramme ?? PrintProgramme.UG);
            if (CurrentBatchOrSemesterBatch <= 0)
                return new ResponseData() { IsSuccess = false, ErrorMessage = "You donot have any college or combination assigned. Visit your respective college." };

            if (!(CurrentBatchOrSemesterBatch < settingsForm.RegularBatch))
                return new ResponseData() { IsSuccess = false, ErrorMessage = "You are not eligible for Backlog Exam, please apply as Regular." };

            #region For UG NEP only
            if (settingsForm.RegularBatch >= 2022 && (settingsForm?.PrintProgramme ?? PrintProgramme.UG) == PrintProgramme.UG && settingsForm.IsBacklogSetting)
            {
                if (CurrentBatchOrSemesterBatch < 2022 && settingsForm.RegularBatch > 2022)
                    return new ResponseData() { IsSuccess = false, ErrorMessage = "Only UG NEP students can apply for now." };
            }
            #endregion

            #endregion

            if (!responseData.IsSuccess)
                return responseData;

            Tuple<bool, string, List<ADMSubjectMaster>> blockExamForm = null;
            if (studentExamFormSubjects == null)
            {
                var _StudentInfo = studentManager.GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester);
                _StudentInfo.Batch = CurrentBatchOrSemesterBatch;

                if (_StudentInfo == null || (_StudentInfo?.SelectedCombinations.IsNullOrEmpty() ?? true))
                {
                    return new ResponseData() { ErrorMessage = $"Subject Combination not ASSIGNED by College. Please visit your College to ASSIGN subject combination of Semester-{settingsForm.Semester} immediately." };
                }
                else if (_StudentInfo.SelectedCombinations.Any(x => x.IsVerified != true))
                {
                    return new ResponseData() { ErrorMessage = $"Subject Combination not VERIFIED by College. Please visit your College to VERIFY subject combination of Semester-{settingsForm.Semester} immediately." };
                }

                blockExamForm = BlockSemesterAdmitCardOrNot(_StudentInfo.Student_ID, semester, settingsForm.PrintProgramme, CurrentBatchOrSemesterBatch, _StudentInfo?.SelectedCombinations?.First()?.Subjects ?? new List<ADMSubjectMaster>(), "", false);
            }

            var studentExamForm = GetExistingBacklogExamForm(studentExamFormSubjects, settingsForm);

            if (studentExamForm == null)
            {
                studentExamForm = new ARGStudentExamFormSubjects
                {
                    Student_ID = AppUserHelper.User_ID,
                    StudentExamForm_ID = Guid.NewGuid(),
                    Semester = settingsForm.Semester,
                    Year = settingsForm.Year,
                    ExamRollNumber = null,
                    Notification_ID = settingsForm.Notification_ID,
                    Status = FormStatus.InProcess,
                    RecordState = RecordState.New,
                    IsRegular = false,
                    IsDivisionImprovement = isDivisionImprovement,
                    StudentInfo = studentManager.GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester),
                    AdditionalSubjects = studentManager.GetStudentAdditionalSubjects(AppUserHelper.User_ID, settingsForm.Semester, settingsForm.PrintProgramme)
                };
            }

            if (studentExamFormSubjects != null)
                studentExamForm.ReAppearSubjects = studentExamFormSubjects.ReAppearSubjects;
            else
            {
                studentExamForm.ReAppearSubjects = GetReAppearSubjects(studentExamForm.Student_ID,
                studentExamForm.StudentExamForm_ID, settingsForm.PrintProgramme, studentExamForm.Semester, settingsForm.Year, blockExamForm.Item3, studentExamForm.IsDivisionImprovement);
            }

            studentExamForm.AmountToBePaid = GetAmountForStudent(studentExamForm, settingsForm);

            //Show only eligible subjects as per DivisionImprovement
            if (studentExamForm.IsDivisionImprovement)
            {
                if (studentExamForm.ReAppearSubjects.IsNotNullOrEmpty())
                {
                    studentExamForm.ReAppearSubjects = studentExamForm.ReAppearSubjects
                                                        .Where(x => x.SubjectType == SubjectType.Core
                                                        || x.SubjectType == SubjectType.DCE
                                                        || x.SubjectType == SubjectType.DSE)?.ToList() ?? new List<ARGStudentReExamForm>();
                }
            }
            responseData.ResponseObject = studentExamForm;
            return responseData;
        }

        public ARGStudentExamFormSubjects GetExistingBacklogExamForm(ARGStudentExamFormSubjects studentExamFormSubjects, ARGExamFormDownloadable settingsForm)
        {
            var studentExamForm = new ExaminationFormDB().GetStudentExamForm(AppUserHelper.User_ID, settingsForm, settingsForm.PrintProgramme, false);

            if (studentExamForm == null)
                return null;

            StudentManager studentManager = new StudentManager();
            studentExamForm.StudentInfo = studentManager.GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester);
            studentExamForm.AdditionalSubjects = studentManager.GetStudentAdditionalSubjects(studentExamForm.Student_ID, settingsForm.Semester, settingsForm.PrintProgramme);
            studentExamForm.RecordState = RecordState.Old;

            return studentExamForm;
        }

        public ResponseData ValidateExaminationForm(ARGExamFormDownloadable settingsForm, short semester, bool isRegular)
        {
            ResponseData responseData = new ResponseData() { IsSuccess = true };

            // if no settings form were fetched because of start and end dates mismatch
            if (settingsForm == null)
            {
                responseData.IsSuccess = false;
                responseData.ErrorMessage = $" The Examination Forms Application is not available because of non availablity of the configuration options for the selected Course [{AppUserHelper.OrgPrintProgramme} and Semster = {semester}].";
                return responseData;
            }

            //Check if Examination Form is allowed to download.
            //if (!settingsForm.AllowDownloadForm)
            //{
            //    responseData.IsSuccess = false;
            //    responseData.ErrorMessage = " The Examination Form Application has been disabled.";
            //}
            //else
            {
                if (!responseData.IsSuccess)
                    return responseData;

                // Check If the student is applying for semester greater than his Current Semester.
                var studentInfo = new StudentManager().GetStudentWithCombination(AppUserHelper.User_ID, settingsForm.PrintProgramme, true, settingsForm.Semester);

                int currentSemster = studentInfo.CurrentSemesterOrYear;
                int studentBatch = studentInfo.Batch;

                if (isRegular)
                {
                    #region old code
                    //if (currentSemester % 2 == 0)
                    //{
                    //    // Check applying for semester is same or 1 semester greater as current semester for semesters 2,4,6,8,10
                    //    if (currentSemester != settingsForm.Semester)
                    //    {
                    //        if ((currentSemester + 1) != settingsForm.Semester)
                    //        {
                    //            responseData.IsSuccess = false;
                    //            responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemester} and you are trying to apply for Semester {settingsForm.Semester} which is not possible.Regular Application Forms cannot be less than your current semester.<br/>Please make sure the college has updated your admission data.</i>";
                    //            return responseData;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    // Check applying for semester is same as current semester for semesters 1,3,5,7,9
                    //    if (currentSemester != settingsForm.Semester)
                    //    {
                    //        responseData.IsSuccess = false;
                    //        responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemester} and you are trying to apply for Semester {settingsForm.Semester} which is not possible.Regular Application Forms cannot be less than your current semester.<br/>Please make sure the college has updated your admission data.</i>";
                    //        return responseData;
                    //    }
                    //}

                    //// For Regular Students checks semester validation.
                    //if (settingsForm.Semester < currentSemester)
                    //{
                    //    responseData.IsSuccess = false;
                    //    responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemester} and you are trying to apply for Semester {settingsForm.Semester} which is not possible.Regular Application Forms cannot be less than your current semester.</i>";
                    //    return responseData;
                    //}

                    #endregion

                    if (settingsForm.Semester < currentSemster)
                    {
                        responseData.IsSuccess = false;
                        responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemster} and you are trying to apply for Semester {settingsForm.Semester} which is not possbile because Regular Form Application must be greater than your current semester.</i>";
                        return responseData;
                    }

                    // Block Exam Form for missing Internals
                    responseData = new ResponseData() { IsSuccess = true };// CheckForMissingInternals(settingsForm.Semester);
                }
                else
                {
                    //if (currentSemster <= settingsForm.Semester)
                    //{
                    //    responseData.IsSuccess = false;
                    //    responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemster} and you are trying to apply for Semester {settingsForm.Semester} which is not possbile because Backlog Form Application must be less than your current semester.</i>";
                    //    return responseData;
                    //}

                    // For Backlog Students checks semester validation. 
                    if (studentBatch != DateTime.Now.Year && (settingsForm.Semester > currentSemster))
                    {
                        responseData.IsSuccess = false;
                        responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemster} and you are trying to apply for Semester {settingsForm.Semester} which is not possbile because Backlog Form Application must be less than your current semester.</i>";
                        return responseData;
                    }
                }

                // Check for End date of applications.
                if (DateTime.Now > settingsForm.EndDate)
                {
                    //Check for grace periode
                    if (DateTime.Now < settingsForm.EndDate.AddDays(settingsForm.GracePeriodDays1 + settingsForm.GracePeriodDays2))
                    {
                        responseData.IsSuccess = true;
                        responseData.ErrorMessage = "";
                    }
                    else
                    {
                        //close exam forms
                        responseData.IsSuccess = false;
                        responseData.ErrorMessage = "<i> The Last Date as well as the Late Fee Period for the Examination Form has ended.</i>";
                    }
                }
                else if (DateTime.Now < settingsForm.StartDate)
                {
                    responseData.IsSuccess = false;
                    responseData.ErrorMessage = "<i> The Date for the examination form has not started yet.</i>";
                }
            }
            return responseData;
        }

        public bool CheckStudentIsWhiteListed(short Semester)
        {
            ExaminationWhiteListCompact examinationWhiteListCompact =
                new ExaminationFormDB().CheckIfStudentIsWhiteListed(AppUserHelper.User_ID, Semester, short.Parse(DateTime.Now.Year.ToString()));

            //allow to one week for previous year after New Year
            if (examinationWhiteListCompact == null && DateTime.Now.Month == 01 && DateTime.Now.Day <= 13)
            {
                examinationWhiteListCompact =
                new ExaminationFormDB().CheckIfStudentIsWhiteListed(AppUserHelper.User_ID, Semester, short.Parse((DateTime.Now.Year - 1).ToString()));
            }
            return examinationWhiteListCompact != null;
        }

        public void GetAndUpdateFormNumber(ARGStudentExamFormSubjects studentExamForm, ARGExamFormDownloadable settingsForm)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                ARGExamFormDownloadable settingsFormsForFormCount = new ExaminationFormDB().GetExamFormDownloadable(settingsForm.ARGExamForm_ID);
                settingsForm.FormNumberCount = settingsFormsForFormCount.FormNumberCount + 1;

                new ExaminationFormDB().UpdateExamFormDownloadable(settingsForm.ARGExamForm_ID, settingsForm.FormNumberCount);

                studentExamForm.SetWorkFlow(RecordState.New);

                ExaminationCourseCategory studentCourse = new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.TableSuffix);
                if (settingsForm.CourseCategory == ExaminationCourseCategory.UG && AppUserHelper.CollegeCode.ToUpper().Equals("IASE"))
                {
                    studentCourse = settingsForm.CourseCategory;
                }


                // Form Number Format - EXM - Examination CourseCategory - Exam Year - Semester
                studentExamForm.FormNumber = GetExamFormNumber(studentCourse, settingsForm.Year, settingsForm.Semester, settingsForm.IsBacklogSetting) + settingsForm.FormNumberCount.ToString("D5");
                try
                {
                    new ExaminationFormDB().InsertStudentExamForm(studentExamForm, AppUserHelper.TableSuffix);
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    studentExamForm.FormNumber = GetExamFormNumber(studentCourse, settingsForm.Year, settingsForm.Semester, settingsForm.IsBacklogSetting) + (settingsForm.FormNumberCount + 99).ToString("D5");
                    new ExaminationFormDB().UpdateExamFormDownloadable(settingsForm.ARGExamForm_ID, (settingsForm.FormNumberCount + 99));
                    new ExaminationFormDB().InsertStudentExamForm(studentExamForm, AppUserHelper.TableSuffix);
                }
                transaction.Complete();
            }
        }

        public string GetExamFormNumber(ExaminationCourseCategory studentCourse, short settingsFormYear, short settingsFormSemester)
        {

            return "EXM" + studentCourse.ToString() + settingsFormYear.ToString().Substring(2, 2) + "-" + settingsFormSemester.ToString() + "-";
        }
        public string GetExamFormNumber(ExaminationCourseCategory studentCourse, short settingsFormYear, short settingsFormSemester, bool BacklogStudent = false)
        {

            return "EXM" + (BacklogStudent ? "BK" : "") + studentCourse.ToString() + settingsFormYear.ToString().Substring(2, 2) + "-" + settingsFormSemester.ToString() + "-";
        }

        public int SaveExaminationFormAndSubjectsForBacklogStudents(ARGStudentExamFormSubjects studentExamForm, ARGExamFormDownloadable settingsForm, Guid user_ID, short value)
        {
            int NoOfRecords = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                if (studentExamForm.ReAppearSubjects != null && studentExamForm.ReAppearSubjects.Count > 0 && studentExamForm.ReAppearSubjects.Where(i => i.IsApplied && i.RecordState == RecordState.New).Count() > 0)
                {
                    if (studentExamForm.RecordState == RecordState.New)
                        new ExaminationFormManager().GetAndUpdateFormNumber(studentExamForm, settingsForm);

                    foreach (var appliedSubject in studentExamForm.ReAppearSubjects)
                    {
                        if (appliedSubject.IsApplied && appliedSubject.RecordState == RecordState.New)
                        {
                            appliedSubject.StudentReExamForm_ID = Guid.NewGuid();
                            appliedSubject.StudentExamForm_ID = studentExamForm.StudentExamForm_ID;
                            appliedSubject.FeeStatus = FormStatus.InProcess;
                            appliedSubject.SetWorkFlow(RecordState.New);
                            NoOfRecords += new ExaminationFormDB().SaveExaminationSubjects(appliedSubject);
                        }
                    }
                }
                else
                {

                }
                transaction.Complete();
            }
            return NoOfRecords;
        }

        private int DeleteOldRecords(Guid studentExamForm_ID)
        {
            return new ExaminationFormDB().DeleteOldRecords(studentExamForm_ID);
        }

        /// <summary>
        /// action used for backlog examination form
        /// </summary>
        private List<ARGStudentReExamForm> GetReAppearSubjects(Guid student_ID, Guid StudentExamForm_ID, PrintProgramme printProgramme, short semester, int Year, List<ADMSubjectMaster> subjectsWithInternalPassed, bool isDivisionImprovement = false)
        {
            ResultCompact result = new ResultManager().GetResult(printProgramme, semester, AppUserHelper.User_ID, true);
            if (result == null)
                return new List<ARGStudentReExamForm>();

            result.SubjectResults.RemoveAll(s => s.HasResult == false);

            //remove passed subject
            var tuple_subjects = new ResultManager().GetPassed_FailedSubjects(result.SubjectResults, semester);

            result.SubjectResults = isDivisionImprovement ? tuple_subjects.Item1 : tuple_subjects.Item2;

            var appliedReAppearSubjects = new ResultDB().GetAppliedReAppearSubjects(AppUserHelper.User_ID, printProgramme, semester, Year);

            List<ARGStudentReExamForm> AppliedList = new List<ARGStudentReExamForm>();

            foreach (var SubjectResult in result.SubjectResults)
            {
                ARGStudentReExamForm reAppearSubject = new ARGStudentReExamForm();

                reAppearSubject.SubjectFullName = SubjectResult.SubjectFullName;

                int.TryParse(SubjectResult.ExternalMarks?.ToString(), out int TheoryMarks);
                reAppearSubject.TheoryMarks = TheoryMarks;

                int.TryParse(SubjectResult.InternalMarks?.ToString(), out int InternalMarks);
                reAppearSubject.PracticalMarks = InternalMarks;

                int.TryParse(SubjectResult.ExternalAttendance_AssessmentMarks?.ToString(), out int ExternalAttendance_AssessmentMarks);
                reAppearSubject.TheoryAttendance = ExternalAttendance_AssessmentMarks;

                int.TryParse(SubjectResult.InternalAttendance_AssessmentMarks?.ToString(), out int InternalAttendance_AssessmentMarks);
                reAppearSubject.PracticalAttendance = InternalAttendance_AssessmentMarks;

                reAppearSubject.HasExaminationFee = SubjectResult.HasExaminationFee;
                reAppearSubject.Subject_ID = SubjectResult.Subject_ID;
                reAppearSubject.SubjectCode = SubjectResult.SubjectCode;
                reAppearSubject.RecordState = RecordState.New;
                reAppearSubject.IsApplied = false;
                reAppearSubject.SubjectType = SubjectResult.SubjectType;
                reAppearSubject.Programme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(SubjectResult.Programme);

                var appliedForFailedSubject = appliedReAppearSubjects?.FirstOrDefault(i => i.Subject_ID == SubjectResult.Subject_ID);

                if (appliedForFailedSubject != null)
                {
                    if (appliedForFailedSubject.FeeStatus == FormStatus.FeePaid || appliedForFailedSubject.FeeStatus == FormStatus.UpdatedManually)
                        reAppearSubject.IsApplied = true;

                    reAppearSubject.StudentExamForm_ID = appliedForFailedSubject.StudentExamForm_ID;
                    reAppearSubject.StudentReExamForm_ID = appliedForFailedSubject.StudentReExamForm_ID;
                    reAppearSubject.RecordState = RecordState.Old;
                    reAppearSubject.Type = appliedForFailedSubject.Type;
                    reAppearSubject.FeeStatus = appliedForFailedSubject.FeeStatus;
                }

                if (subjectsWithInternalPassed.IsNotNullOrEmpty()
                    && subjectsWithInternalPassed.Any(x => x.Subject_ID == reAppearSubject.Subject_ID))
                {
                    reAppearSubject.HasInternalPassed = true;
                }
                else
                {
                    reAppearSubject.HasInternalPassed = false;
                }
                AppliedList.Add(reAppearSubject);
            }
            return AppliedList;
        }

        public List<ADMSubjectMaster> GetAppliedReAppearSubjects(Guid StudentExamForm_ID)
        {
            return new ExaminationFormDB().GetAppliedReAppearSubjectsNewTables(StudentExamForm_ID);
        }

        public List<ARGStudentReExamForm> GetSubjectsAppliedForBacklogForm(Guid StudentExamForm_ID)
        {
            return new ExaminationFormDB().GetSubjectsAppliedForBacklogFormNewTables(StudentExamForm_ID);
        }

        public decimal GetAmountForStudent(ARGStudentExamFormSubjects examForm, ARGExamFormDownloadable settingsForm = null)
        {
            decimal amount = 0;
            PrintProgramme printprogramme = AppUserHelper.TableSuffix;
            if (settingsForm != null)
            {
                var feeStructure = new ExaminationFormDB().GetFeeStructure(settingsForm.FeeStructure_ID) ?? new FeeStructure();

                if (settingsForm.IsHardCodeFeeSet)
                {
                    //Set Hard Code Fee if Regular Student.
                    if (settingsForm.Semester == examForm.StudentInfo.CurrentSemesterOrYear)
                    {
                        amount = settingsForm.HardCodeFeeAmount;
                    }
                    else if (!settingsForm.IsBacklogSetting && settingsForm.Semester == examForm.StudentInfo.CurrentSemesterOrYear + 1)
                    {
                        amount = settingsForm.HardCodeFeeAmount;
                    }
                    else
                    {
                        //calculate Fee if Backlog Student.
                        amount = GetFeeAmountForBacklogStudents(examForm, settingsForm, feeStructure);
                    }
                }
                else
                    amount = settingsForm.IsBacklogSetting ? GetFeeAmountForBacklogStudents(examForm, settingsForm, feeStructure) : GetFeeAmountForRegularStudents(examForm, settingsForm, feeStructure);

                //for division improve only
                if (amount > 0)
                {
                    decimal amountTemp = amount;
                    if (new ExaminationFormDB().HasAlreadyAppliedForDivisionImprovement(examForm.Student_ID, printprogramme))
                    {
                        //remove hot and cold charges
                        decimal hotandcoldCharges = feeStructure.ExaminationFund + feeStructure.OtherCharges + feeStructure.ITComponent;

                        if (hotandcoldCharges > 0)
                        {
                            amount -= feeStructure.ExaminationFund + feeStructure.OtherCharges + feeStructure.ITComponent;
                        }
                        if (amount < 0)
                        {
                            amount = amountTemp;
                        }
                    }
                }


                //check for min and max fee validation
                if (settingsForm.IsBacklogSetting == false && settingsForm.ValidateByMinMaxFee)
                {
                    // -1 means amount calculated is invalid
                    if (!(amount >= settingsForm.MinimumFee && amount <= settingsForm.MaximumFee))
                        return -1;
                }

                // Get Late Fee Amount
                if ((examForm.Status == FormStatus.Accepted || examForm.Status == FormStatus.BlockSemesterAdmitCard) && examForm.AmountPaid > 0)
                {
                    if (examForm.LateFeeAmount == GetLateFeeAmount(settingsForm, feeStructure))
                        examForm.LateFeeAmount = 0;
                }
                else
                    examForm.LateFeeAmount = GetLateFeeAmount(settingsForm, feeStructure);
            }

            // Get Paid Amount
            var paymentsList = new PaymentManager().GetPaymentDetails(examForm.StudentExamForm_ID, PaymentModuleType.Examination, printprogramme);
            examForm.AmountPaid = paymentsList.IsNotNullOrEmpty() ? paymentsList.Sum(i => i.TxnAmount) : 0;

            return amount;
        }


        private decimal GetFeeAmountForBacklogStudents(ARGStudentExamFormSubjects examForm, ARGExamFormDownloadable settingsForm, FeeStructure structure)
        {
            int noOfSubjectsApplied = 0;
            int AppliedNew = 0;
            decimal AmountToBePaid = 0;

            if (examForm.ReAppearSubjects.IsNotNullOrEmpty())
            {
                noOfSubjectsApplied = examForm.ReAppearSubjects.Where(subject => subject.IsApplied == true && subject.SubjectType != SubjectType.SEC && subject.RecordState == RecordState.Old
                                                                             && (subject.FeeStatus == FormStatus.FeePaid || subject.FeeStatus == FormStatus.UpdatedManually)).Count();

                //AppliedNew = examForm.ReAppearSubjects.Where(subject => subject.IsApplied == true && subject.SubjectType != SubjectType.SEC && subject.SubjectType != SubjectType.Practical && subject.SubjectType != SubjectType.Workshop && subject.SubjectType != SubjectType.Lab && (subject.FeeStatus != FormStatus.FeePaid && subject.FeeStatus != FormStatus.UpdatedManually)).Count();
                AppliedNew = examForm.ReAppearSubjects.Where(subject => subject.IsApplied == true && subject.HasExaminationFee && (subject.FeeStatus != FormStatus.FeePaid && subject.FeeStatus != FormStatus.UpdatedManually)).Count();
            }

            // Means has applied for only SEC subjects. We need not to calculate fee.
            if (noOfSubjectsApplied == 0 && AppliedNew == 0)
                return AmountToBePaid;
            else
            {
                // Add Minimum Fee
                if (noOfSubjectsApplied == 0)
                {
                    AmountToBePaid = structure.ExaminationFund + structure.OtherCharges + structure.ITComponent;
                }

                AmountToBePaid = ((AppliedNew) * structure.FeePerSubject) + AmountToBePaid;
                examForm.AmountToBePaid = AmountToBePaid;
            }
            return AmountToBePaid;
        }


        private static int GetFeeAmountForRegularStudents(ARGStudentExamFormSubjects examForm, ARGExamFormDownloadable settingsForm, FeeStructure structure)
        {
            int Amount = 0;
            int noOfSubjects = 0;

            Amount = structure.ExaminationFund + structure.OtherCharges + structure.ITComponent;

            if (examForm.StudentInfo.SelectedCombinations != null)
                noOfSubjects = examForm.StudentInfo.SelectedCombinations.FirstOrDefault(c => c.Semester == examForm.Semester).Subjects.Where(s => s.HasExaminationFee).Count();

            //Check for Additional Subjects and calculate Fee
            noOfSubjects += examForm.AdditionalSubjects?.Where(s => s.Subject.HasExaminationFee)?.Count() ?? 0;

            Amount += structure.FeePerSubject * noOfSubjects;

            return Amount;
        }

        private int GetLateFeeAmount(ARGExamFormDownloadable settingsForm, FeeStructure feeStructure)
        {
            var lateFee = 0;
            if (DateTime.Now > settingsForm.EndDate)
            {
                if (DateTime.Now <= settingsForm.EndDate.AddDays(settingsForm.GracePeriodDays1))
                {
                    lateFee = settingsForm.LateFee1;
                }
                else if (DateTime.Now <= settingsForm.EndDate.AddDays(settingsForm.GracePeriodDays1 + settingsForm.GracePeriodDays2))
                {
                    lateFee = settingsForm.LateFee2;
                }
            }
            return lateFee;
        }

        public ResponseData UpdateStudentExamForm(ARGStudentExamForm studentExamForm, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            ARGStudentExamForm _studentExamForm = new ExaminationFormDB().GetStudentExamForm(studentExamForm, printProgramme);
            bool printProgrammeChanged = false;
            if (_studentExamForm == null && printProgramme == PrintProgramme.IH)
            {
                _studentExamForm = new ExaminationFormDB().GetStudentExamForm(studentExamForm, PrintProgramme.UG);
                if (_studentExamForm != null)
                    printProgrammeChanged = true;
            }

            if (_studentExamForm == null)
                return new ResponseData() { ErrorMessage = "Student Exam Form Number not found...Check Program" };

            if (_studentExamForm.Status == FormStatus.Accepted)
                return new ResponseData() { SuccessMessage = "Student Exam Form already submitted.", IsSuccess = true };

            ARGPersonalInformation studentInfo = new StudentManager().GetStudent(_studentExamForm.Student_ID, printProgramme);
            if (studentInfo == null)
                return new ResponseData() { ErrorMessage = "Student not found ... Check Program" };

            if (studentInfo.AcceptCollege_ID.HasValue && studentInfo.AcceptCollege_ID.Value != Guid.Empty && studentInfo.AcceptCollege_ID != AppUserHelper.College_ID)
                return new ResponseData() { ErrorMessage = "Student belongs to " + studentInfo.AcceptCollegeID };

            var responseData = new ResponseData() { IsSuccess = true, SuccessMessage = "Record Saved Successfully" };
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    _studentExamForm.Status = studentExamForm.Status;
                    if (printProgrammeChanged)
                    {
                        responseData.NumberOfRecordsEffected = new ExaminationFormDB().UpdateStudentExamForm(_studentExamForm, PrintProgramme.UG);
                    }
                    else
                    {
                        responseData.NumberOfRecordsEffected = new ExaminationFormDB().UpdateStudentExamForm(_studentExamForm, printProgramme);
                    }
                    if (responseData.NumberOfRecordsEffected <= 0)
                        throw new TransactionAbortedException();

                    transactionScope.Complete();
                }
            }
            catch (TransactionAbortedException) { }
            if (responseData.NumberOfRecordsEffected != 1)
                return new ResponseData() { ErrorMessage = "Update operation failed...", IsSuccess = false };
            return responseData;
        }

        public List<StudentExamFormList> GetExaminationForms(Parameters parameter, PrintProgramme printProgramme, string Columns)
        {
            return new ExaminationFormDB().GetExaminationFormsList(parameter, printProgramme, Columns);
        }

        public List<StudentExamFormList> GetExaminationFormList(Parameters parameters, PrintProgramme printProgramme)
        {
            return new ExaminationFormDB().GetExaminationFormList(parameters, printProgramme);
        }
        public ExaminationFormSummary GetExaminationFormSummary(Parameters parameters, PrintProgramme printProgramme)
        {
            return new ExaminationFormDB().GetExaminationFormSummary(parameters, printProgramme);
        }


        public ARGStudentExamFormSubjects GetExaminationForm(Guid ExamForm_ID, PrintProgramme printProgramme)
        {
            ARGStudentExamFormSubjects examinationForm = new ExaminationFormDB().GetStudentExamForm(ExamForm_ID, printProgramme);

            if (examinationForm != null && !examinationForm.IsRegular)
                examinationForm.ReAppearSubjects = new ExaminationFormDB().GetExaminationFormSubjects(examinationForm.StudentExamForm_ID);

            return examinationForm;
        }
        public bool ExamFormSubmitted(Guid Student_ID, short Semester, short year, PrintProgramme printProgramme)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            return new ExaminationFormDB().ExamFormSubmitted(Student_ID, Semester, year, printProgramme);
        }

        public string GetRegistrationNoOrFormNumber(PrintProgramme printProgramme, string FormNumber)
        {
            return new ExaminationFormDB().GetRegistrationNoOrFormNumber(printProgramme, FormNumber);
        }
        #endregion

        #region Semester ExamAdmitCard Routines
        public List<SelectListItem> GetSemesterDDL(PrintProgramme programme) => new ExaminationFormDB().GetSemesterDDL(programme, AppUserHelper.User_ID);
        public List<PrintProgramme> GetAdmitPrintProgrammeDDL()
        {
            List<PrintProgramme> list = new List<PrintProgramme>();
            foreach (var item in new ExaminationFormDB().GetAdmitPrintProgrammeDDL() ?? new List<short>())
            {
                list.Add((PrintProgramme)Enum.Parse(typeof(PrintProgramme), item.ToString()));
            }
            return list.IsNullOrEmpty() ? null : list;
        }
        public List<short> GetAdmitSemesterDDL()
        {
            return new ExaminationFormDB().GetAdmitSemesterDDL();
        }

        public Guid GetStudentID(short Semester)
        {
            if (Semester == 0)
                return Guid.Empty;
            return new ExaminationFormDB().GetStudentID();
        }



        public SemesterExamCenterDetails GetSemesterCenterAllotmentDetails(Guid StudentExamForm_ID)
        {
            return new ExaminationFormDB().GetSemesterCenterAllotmentDetails(StudentExamForm_ID);
        }

        public Tuple<string, string, List<ADMSubjectMaster>, ARGStudentExamForm, SemesterExamCenterDetails> GetAdmitCardDetails(Guid Student_ID, short ExamSemester, PrintProgramme printProgramme, int Batch)
        {
            var examFormDownloadable = new ExaminationFormManager().GetExamFormDownlForSemAdmitCards(printProgramme, ExamSemester);
            if (examFormDownloadable == null)
                return Tuple.Create("Downloading of Admit Cards has either not started or has been stopped. ", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());
            if (!examFormDownloadable.AllowDownloadAdmitCards)
                return Tuple.Create("Downloading of Admit Cards has either not started or has been stopped. ", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());


            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            //get subject details
            ADMCombinationMaster aDMCombinationMaster = new ExaminationFormDB().GetStudentSemensterSubjectsAndCourse(Student_ID, ExamSemester, printProgramme);
            if (aDMCombinationMaster == null)
                return Tuple.Create($"You are not eligible. Reason : Semester-{ExamSemester} Exam form not submitted", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());

            string CourseName = new CourseDB().GetItem((Guid)aDMCombinationMaster.Course_ID)?.CourseFullName ?? string.Empty;

            List<ADMSubjectMaster> Subjects = null;
            ARGStudentExamForm _StudentExamForm = new ExaminationFormDB().GetExamFormDetails(Student_ID, ExamSemester, printProgramme);
            if (_StudentExamForm != null && _StudentExamForm.StudentExamForm_ID != Guid.Empty)
            {
                //block adimt card
                if (_StudentExamForm.Status == FormStatus.BlockSemesterAdmitCard)
                    return Tuple.Create("Your Admit Card has been blocked. Reasons could be<br/>Blocked by College. OR<br/>Pending fee not deposited yet.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());

                //Regular Student 
                if (_StudentExamForm.IsRegular)
                {
                    #region Get Subject
                    StudentManager studentManager = new StudentManager();
                    Subjects = new SubjectsManager().GetList((Guid)aDMCombinationMaster.Course_ID, aDMCombinationMaster.CombinationSubjects.ToGuidList());
                    List<StudentAdditionalSubject> _StudentAdditionalSubject = studentManager.GetStudentAdditionalSubjects(Student_ID, ExamSemester, printProgramme);
                    List<StudentAdditionalSubject> _StudentAdditionalSubjectTemp = null;
                    if (_StudentAdditionalSubject.IsNotNullOrEmpty())
                    {
                        _StudentAdditionalSubjectTemp = _StudentAdditionalSubject.Where(x => x.Subject != null)?.ToList();

                        if (_StudentAdditionalSubjectTemp.IsNotNullOrEmpty())
                            Subjects.AddRange(_StudentAdditionalSubjectTemp.Select(x => x.Subject));
                    }

                    SemesterExamCenterDetails CenterDetails = GetSemesterCenterAllotmentDetails(_StudentExamForm.StudentExamForm_ID);
                    #endregion

                    Tuple<bool, string, List<ADMSubjectMaster>> _BlockSemesterAdmitCardOrNot = BlockSemesterAdmitCardOrNot(Student_ID, ExamSemester, printProgramme, Batch, Subjects, CourseName, true);

                    if (_BlockSemesterAdmitCardOrNot.Item1)
                        return Tuple.Create(_BlockSemesterAdmitCardOrNot.Item2 + "<br/> Please contact your college.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());

                    if (CenterDetails != null && !string.IsNullOrWhiteSpace(_StudentExamForm.ExamRollNumber))
                        return Tuple.Create(CourseName, aDMCombinationMaster.CombinationCode, _BlockSemesterAdmitCardOrNot.Item3, _StudentExamForm, CenterDetails);
                    else
                        return Tuple.Create("Exam Roll Not Available or Center not alloted yet. Please visit University I.T Section.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());
                }
                else
                {
                    //Re-appear Student
                    Subjects = GetAppliedReAppearSubjects(_StudentExamForm.StudentExamForm_ID);
                    if (Subjects.IsNullOrEmpty())
                    {
                        return Tuple.Create("Your ReAppear Subjects could not be found. Please visit University I.T Section.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());
                    }

                    SemesterExamCenterDetails CenterDetails = GetSemesterCenterAllotmentDetails(_StudentExamForm.StudentExamForm_ID);
                    if (CenterDetails != null)
                    {
                        return Tuple.Create(CourseName, string.Empty, Subjects, _StudentExamForm, CenterDetails);

                        #region block for reappear   
                        //Tuple<bool, string, List<ADMSubjectMaster>> _BlockSemesterAdmitCardOrNot = BlockSemesterAdmitCardOrNot(Student_ID, ExamSemester, printProgramme, Batch, Subjects);
                        //if (_BlockSemesterAdmitCardOrNot.Item1)
                        //    return Tuple.Create(_BlockSemesterAdmitCardOrNot.Item2 + "<br/> Please contact your college.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());

                        //SemesterExamCenterDetails CenterDetails = GetSemesterCenterAllotmentDetails(_StudentExamForm.StudentExamForm_ID);
                        //if (CenterDetails != null)
                        //{
                        //    List<ADMSubjectMaster> finallist = new List<ADMSubjectMaster>();
                        //    foreach (var item in Subjects)
                        //    {
                        //        if (_BlockSemesterAdmitCardOrNot.Item3.Any(x => x.Subject_ID == item.Subject_ID))
                        //            finallist.Add(item);
                        //    }

                        //    return Tuple.Create(CourseName, string.Empty, finallist, _StudentExamForm, CenterDetails);
                        //} 
                        #endregion
                    }
                    else
                        return Tuple.Create("Exam Roll Not Available. or <br/>Center not alloted yet. or<br/>You are not eligible. Please visit University I.T Section.", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());
                }
            }
            return Tuple.Create("Discrepancies : <br/>Examination Form not submitted.<br/>Or Exam Fee not paid. Or<br/>You are not eligible", "", new List<ADMSubjectMaster>(), new ARGStudentExamForm(), new SemesterExamCenterDetails());
        }

        internal Tuple<bool, string, List<ADMSubjectMaster>> BlockSemesterAdmitCardOrNot(Guid Student_ID, int ExamSemester, PrintProgramme printProgramme, int Batch, List<ADMSubjectMaster> Subjects, string CourseName, bool isAdmitCard)
        {
            #region Avoid Internal Awards
            //return Tuple.Create(false, "", Subjects);
            #endregion

            #region Avoid Internal Awards for particular Course(s)
            //if (CourseName.ToLower().Contains("engineering"))
            //{
            //    return Tuple.Create(false, "", Subjects);
            //}
            #endregion

            #region Avoid PrintProgramme and Semester Wise Internal Awards
            //if (printProgramme == PrintProgramme.IH && ExamSemester == 7)
            //{
            //    return Tuple.Create(false, "", Subjects);
            //}
            //else if (printProgramme == PrintProgramme.PG && ExamSemester == 4)
            //{
            //    return Tuple.Create(false, "", Subjects);
            //}
            #endregion

            #region Avoid Internal Awards for UG Only
            //if (isAdmitCard == false && printProgramme == PrintProgramme.UG && DateTime.Now < new DateTime(2023, 01, 08, 23, 59, 00))
            //{
            //    return Tuple.Create(false, "", Subjects);
            //}
            #endregion

            #region Avoid Internal Awards for UG and particular batch Only
            if (isAdmitCard == false && printProgramme == PrintProgramme.UG && Batch == 2023 && ExamSemester == 1)
            {
                return Tuple.Create(false, "", Subjects);
            }
            if (isAdmitCard == false && (printProgramme == PrintProgramme.UG || printProgramme == PrintProgramme.IH) && Batch == 2021 && ExamSemester == 5)
            {
                return Tuple.Create(false, "", Subjects);
            }
            #endregion

            List<ADMSubjectMaster> tempSubjects = new List<ADMSubjectMaster>();
            tempSubjects.AddRange(Subjects);


            ResultCompact _Result = new ResultManager().GetResult(printProgramme, (short)ExamSemester, Student_ID, true);

            if (_Result == null || _Result.SubjectResults.IsNullOrEmpty())
                return Tuple.Create(true, "Your internal for all subjects have not been submitted. Please contact your College immediately.", new List<ADMSubjectMaster>());

            StringBuilder stringBuilder = new StringBuilder();

            List<SubjectResult> _Subjects = _Result.SubjectResults.Where(x =>
            x.HasResult && x.SubjectType != SubjectType.Lab
            && x.SubjectType != SubjectType.Practical
            && x.SubjectType != SubjectType.Workshop
            && x.IsInternalMarksApplicable)?.ToList() ?? new List<SubjectResult>();

            List<SubjectResult> OtherSubjects = _Result.SubjectResults.Where(x =>
                        x.SubjectType == SubjectType.Lab
                        || x.SubjectType == SubjectType.Practical
                        || x.SubjectType == SubjectType.Workshop
                        || x.IsInternalMarksApplicable == false)?.ToList() ?? new List<SubjectResult>();

            if (!isAdmitCard)
            {
                foreach (SubjectResult _Subject in _Subjects)
                {
                    if ((_Subject.IsInternalMarksApplicable && !_Subject.InternalMarks.HasValue && _Subject.InternalIsPartOf == MarksIsPartOf.Internal)
                        ||
                        (_Subject.IsExternalAttendance_AssessmentMarksApplicable && !_Subject.ExternalAttendance_AssessmentMarks.HasValue && _Subject.ExternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal)
                        ||
                        (_Subject.IsInternalAttendance_AssessmentMarksApplicable && !_Subject.InternalAttendance_AssessmentMarks.HasValue && _Subject.InternalAttendance_AssessmentIsPartOf == MarksIsPartOf.Internal))
                        stringBuilder.Append($"{_Subject.SubjectFullName} ({_Subject.SubjectType.GetEnumDescription()})<br/>");
                }
                if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()))
                    return Tuple.Create(true, "Your internal awards for below subjects has not been submitted by College : <br/><br/> " + stringBuilder.ToString(), new List<ADMSubjectMaster>());
            }
            _Result.SubjectResults = _Subjects;

            Subjects = _Result?.SubjectResults?.Where(x => x.InternalStatus == ResultStatus.P)?.Select(y => new ADMSubjectMaster()
            {
                SubjectFullName = y.SubjectFullName,
                SubjectType = y.SubjectType,
                Subject_ID = y.Subject_ID,
                HasResult = y.HasResult,
                HasExaminationFee = (tempSubjects.FirstOrDefault(x => x.Subject_ID == y.Subject_ID)?.HasExaminationFee ?? false)
            })?.ToList();


            foreach (SubjectResult item in OtherSubjects)
            {
                Subjects.Add(new ADMSubjectMaster
                {
                    SubjectFullName = item.SubjectFullName,
                    SubjectType = item.SubjectType,
                    Subject_ID = item.Subject_ID,
                    HasResult = item.HasResult,
                    HasExaminationFee = (tempSubjects.FirstOrDefault(x => x.Subject_ID == item.Subject_ID)?.HasExaminationFee ?? false)
                });
            }

            if (Subjects.IsNullOrEmpty())
            {
                return Tuple.Create(true, $"You have not cleared your internals for any Subject<br/>, therefore not eligible for Semester-{ExamSemester} examination", new List<ADMSubjectMaster>());
            }

            tempSubjects = null;//dispose

            return Tuple.Create(false, "", Subjects);
        }

        internal SubjectResult BlockExternalt(Guid Student_ID, Guid _Subject, short semesterbatch, short semester, PrintProgramme printProgramme)

        {

            SubjectResult _Result = new ResultDB().SubjectResult(printProgramme, Student_ID, semesterbatch, semester, true, false, _Subject)?.FirstOrDefault() ?? new SubjectResult();
            return _Result;
        }



        public ARGExamFormDownloadable GetExamFormDownlForSemAdmitCards(PrintProgramme printProgramme, short semester)
        {
            return new ExaminationFormDB().AllowSemesterAdmitCards(printProgramme, semester);
        }


        public ResponseData GetExaminationFormWidget(PrintProgramme programme, int Batch, short semester)
        {
            ResponseData responseData = new ResponseData();

            ExaminationFormWidget examinationFormWidget = new ExaminationFormWidget();

            var tableMapping = new GeneralFunctions().MappingTable(programme);

            examinationFormWidget.CourseWidgets = new ExaminationFormDB().GetCourseWidgetForProgramme(tableMapping, Batch, semester);

            if (examinationFormWidget.CourseWidgets != null)
            {
                examinationFormWidget.TotalNumberOfStudents = examinationFormWidget.CourseWidgets?.Sum(i => i.TotalNumberOfStudentsPerCourse) ?? 0;

                examinationFormWidget.Semester = semester;
                examinationFormWidget.Batch = Batch;

                examinationFormWidget.TotalBacklogStudents = examinationFormWidget.CourseWidgets?.Sum(i => i.NoOfBacklogStudents) ?? 0;
                examinationFormWidget.TotalRegularStudents = examinationFormWidget.CourseWidgets?.Sum(i => i.NoOfRegularStudents) ?? 0;

                examinationFormWidget.TotalForms = examinationFormWidget.CourseWidgets?.Sum(i => i.FormApplied) ?? 0;
                examinationFormWidget.TotalSuccessfullPayments = examinationFormWidget.CourseWidgets?.Sum(i => i.NoOfSuccessfulPayments) ?? 0;

                examinationFormWidget.TotalAmount = examinationFormWidget.CourseWidgets?.Sum(i => i.TotalAmountReceived) ?? 0;
                responseData.ResponseObject = examinationFormWidget;

                examinationFormWidget.LateFeeAmount = examinationFormWidget.CourseWidgets?.Sum(i => i.LateFeeAmountForRegularStudents) ?? 0 + examinationFormWidget.CourseWidgets?.Sum(i => i.LateFeeAmountForBackLogStudents) ?? 0;
            }
            else
            {
                responseData.IsSuccess = false;
                responseData.ErrorMessage = "No data found for the given filters. Please try again with different filter combinations.";
            }

            return responseData;
        }

        private int GetLateFeeAmountForWidget(PrintProgramme programme, int year, short semester)
        {
            var LateFeeAmount1 = new ExaminationFormDB().GetLateFee1AmountForWidget(programme, year, semester);

            var LateFeeAmount2 = new ExaminationFormDB().GetLateFee2AmountForWidget(programme, year, semester);

            return LateFeeAmount1 + LateFeeAmount2;
        }

        #endregion

        public ARGStudentExamForm GetRegularExamFormOnlyAccepted(Guid student_ID, int semester, PrintProgramme printProgramme)
        {
            return new ExaminationFormDB().GetRegularExamFormOnlyAccepted(student_ID, semester, printProgramme);
        }

        public bool Get(PrintProgramme printProgramme, Guid student_ID, short semester, short submittedyear)
        {
            return new ExaminationFormDB().Get(printProgramme, student_ID, semester, submittedyear);
        }


        public string GetStudentNameOnly(PrintProgramme printProgramme, Guid ExamForm_ID)
        {
            return new ExaminationFormDB().GetStudentNameOnly(printProgramme, ExamForm_ID);
        }
    }
}
