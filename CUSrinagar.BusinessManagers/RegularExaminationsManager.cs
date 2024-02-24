using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class RegularExaminationsManager
    {
        public List<SelectListItem> GetAllowedSemesterExaminations()
        {
            var list = new RegularExaminationsDB().GetAllowedSemesterExaminations() ?? new List<SelectListItem>();

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

            var whiteListedEntries = new ExaminationWhiteListManager().GetAllApplications(parameters);
            if (whiteListedEntries.IsNotNullOrEmpty())
            {
                foreach (var whiteListedEntry in whiteListedEntries)
                {
                    allowedSemestersList = new List<SelectListItem>()
                    {
                        new SelectListItem()
                        {
                            Text =  (whiteListedEntry.Semester).ToString(),
                            Value = (whiteListedEntry.Semester).ToString()
                        }
                    };

                    if (!whiteListedEntry.IsBacklog)
                    {
                        list.AddRange(allowedSemestersList);
                    }
                }
            }
            return list;
        }

        public RegularExaminationForm GetExaminationForm(RegularExaminationViewModel examinationViewModel)
        {
            RegularExaminationForm examinationForm = CheckExaminationFormExists(AppUserHelper.User_ID, examinationViewModel.ApplyingForSemester);

            if (examinationForm == null)
                examinationForm = GetNewExaminationForm(examinationViewModel.ApplyingForSemester,examinationViewModel.FeeAmount);

            return examinationForm;
        }

        private RegularExaminationForm CheckExaminationFormExists(Guid user_ID, int applyingForSemester)
        {
            return new RegularExaminationsDB().GetExaminationForm(user_ID, applyingForSemester);
        }

        private RegularExaminationForm GetNewExaminationForm(int ApplyingForSemester,decimal FeeAmount)
        {
            RegularExaminationForm examinationForm = new RegularExaminationForm()
            {
                Semester = ApplyingForSemester,
                AmountPaid = FeeAmount,
                ExaminationYear = DateTime.Now.Year,
                ExamRollNumber = null,
                Status = FormStatus.InProcess,
                Student_ID = AppUserHelper.User_ID,
                StudentExamForm_ID = Guid.NewGuid()
            };

            examinationForm.ExaminationSettings = GetExaminationSettings(AppUserHelper.OrgPrintProgramme, ApplyingForSemester);

            ResponseData response = AllowExaminationForStudent(examinationForm.ExaminationSettings);

            if (response.IsSuccess)
            {
                examinationForm.Notification_ID = examinationForm.ExaminationSettings.Notification_ID;

                examinationForm.AmountToBePaid = GetAmountForRegularExamination(examinationForm.ExaminationSettings, examinationForm.SelectedCombination, examinationForm.AdditionalSubjects);
                examinationForm.LateFeeAmount = GetLateFeeAmountForRegularExaminationForm(examinationForm.ExaminationSettings);                
                
                examinationForm.SetWorkFlow(RecordState.New);

                SaveExaminationForm(examinationForm, examinationForm.ExaminationSettings);
            }

            examinationForm.Response = response;
            return examinationForm;
        }

        private void SaveExaminationForm(RegularExaminationForm studentExamForm, ARGExamFormDownloadable settingsForm)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                ExaminationCourseCategory studentCourse = new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.TableSuffix);

                //Form Number Format - EXM - Examination CourseCategory - Exam Year - Semester
                studentExamForm.FormNumber = GetExamFormNumber(studentCourse, settingsForm.Year, settingsForm.Semester, settingsForm.IsBacklogSetting) + settingsForm.FormNumberCount.ToString("D5");

                if (new RegularExaminationsDB().InsertStudentExamForm(studentExamForm, AppUserHelper.TableSuffix) > 0)
                {
                    settingsForm.FormNumberCount += 1;
                    new RegularExaminationsDB().UpdateExamFormDownloadable(settingsForm.ARGExamForm_ID, settingsForm.FormNumberCount);
                }
                transaction.Complete();
            }
        }

        private string GetExamFormNumber(ExaminationCourseCategory studentCourse, short year, short semester, bool isBacklogSetting)
        {
            return "EXM" + (isBacklogSetting ? "BK" : "") + studentCourse.ToString() + year.ToString().Substring(2, 2) + "-" + semester.ToString() + "-";
        }

        private decimal GetLateFeeAmountForRegularExaminationForm(ARGExamFormDownloadable examinationSettings)
        {
            var lateFee = 0;
            double daysAfterEndDate = (DateTime.Now.Date - examinationSettings.EndDate).TotalDays;

            if (daysAfterEndDate > 0)
                lateFee = examinationSettings.LateFee1;

            return lateFee;
        }

        private decimal GetAmountForRegularExamination(ARGExamFormDownloadable settingsForm, ARGSelectedCombination selectedCombination, List<StudentAdditionalSubject> additionalSubjects)
        {
            decimal amount = 0;
            if (settingsForm != null)
            {
                var feeStructure = new ExaminationFormDB().GetFeeStructure(settingsForm.FeeStructure_ID);

                if (settingsForm.IsHardCodeFeeSet)
                    amount = settingsForm.HardCodeFeeAmount;
                else
                    amount = GetFeeAmountForRegularStudents(settingsForm, feeStructure, selectedCombination, additionalSubjects);
            }
            return amount;
        }

        private decimal GetFeeAmountForRegularStudents(ARGExamFormDownloadable settingsForm, FeeStructure structure, ARGSelectedCombination selectedCombination, List<StudentAdditionalSubject> additionalSubjects)
        {
            int Amount = 0;
            int noOfSubjects = 0;

            Amount = structure.ExaminationFund + structure.OtherCharges + structure.ITComponent;

            if (selectedCombination != null)
                noOfSubjects = selectedCombination.Subjects.Where(s => s.HasExaminationFee).Count();

            if (additionalSubjects != null)
                noOfSubjects += additionalSubjects.Count();

            // Minimum Amount covers two subjects. Remove them.
            Amount += structure.FeePerSubject * (noOfSubjects - 2);

            return Amount;
        }

        private ResponseData AllowExaminationForStudent(ARGExamFormDownloadable settingsForm)
        {
            ResponseData responseData = new ResponseData() { IsSuccess = true };

            responseData = ExaminationFormDatesAllowed(settingsForm);

            if (!responseData.IsSuccess)
                return responseData;

            //Check if Examination Form is allowed to download.
            if (!settingsForm.AllowDownloadForm)
            {
                responseData.IsSuccess = false;
                responseData.ErrorMessage = " The process of applying for the Examination Forms has been disabled.";
            }
            else
            {
                responseData = CheckForSemesterMismatch(settingsForm);
                if (!responseData.IsSuccess)
                    return responseData;

                // Block Exam Form for missing Internals
                responseData = CheckForMissingInternals(settingsForm.PrintProgramme, settingsForm.Semester);
            }
            return responseData;
        }

        private ResponseData CheckForSemesterMismatch(ARGExamFormDownloadable settingsForm)
        {
            ResponseData responseData = new ResponseData() { IsSuccess = true };

            // Check If the student is applying for semester greater than his Current Semester.
            int currentSemester = new StudentManager().GetCurrentSemester(AppUserHelper.User_ID, settingsForm.PrintProgramme);

            if (currentSemester % 2 == 0)
            {
                // Check applying for semester is same or 1 semester greater as current semester for semesters 2,4,6,8,10
                if (currentSemester != settingsForm.Semester || (currentSemester + 1) != settingsForm.Semester)
                {
                    responseData.IsSuccess = false;
                    responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemester} and you are trying to apply for Semester {settingsForm.Semester} which is not possible.Regular Examination Forms must be applied for your your current semester only.<br/>Please make sure the college has updated your admission data.</i>";
                    return responseData;
                }
            }
            else
            {
                // Check applying for semester is same as current semester for semesters 1,3,5,7,9
                if (currentSemester != settingsForm.Semester)
                {
                    responseData.IsSuccess = false;
                    responseData.ErrorMessage = $"<i>Your Current Semester is {currentSemester} and you are trying to apply for Semester {settingsForm.Semester} which is not possible.Regular Examination Forms must be applied for your your current semester only.<br/>Please make sure the college has updated your admission data.</i>";
                    return responseData;
                }
            }

            return responseData;
        }

        private ResponseData ExaminationFormDatesAllowed(ARGExamFormDownloadable settingsForm)
        {
            ResponseData responseData = new ResponseData { IsSuccess = true };

            // Check for End date of applications.
            if (DateTime.Now > settingsForm.EndDate)
            {
                // If Grace Period i.e form has been allowed second time officialy allow the form.
                if (settingsForm.IsGracePeriod)
                {
                    responseData.IsSuccess = true;
                    responseData.ErrorMessage = "";
                }
                else
                {
                    // Late Fee Grace Period.
                    // Example - 10 Jan 2018 - 15 Jan 2018 = Normal Fee .
                    // 15 Jan 2018 - 18 Jan 2018 = Late Fee i.e Grace Period.
                    if (DateTime.Now > settingsForm.EndDate.AddDays(settingsForm.GracePeriodDays1))
                    {
                        responseData.IsSuccess = false;
                        responseData.ErrorMessage = "<i> The Grace Period for the Examination Form Application has ended.</i>";
                    }
                }
            }
            else if (DateTime.Now < settingsForm.StartDate)
            {
                responseData.IsSuccess = false;
                responseData.ErrorMessage = "<i> The process for applying for the examination forms has not started yet.</i>";
            }

            return responseData;
        }

        private ResponseData CheckForMissingInternals(PrintProgramme printProgramme, short Semester)
        {
            ResultCompact result = new ResultManager().GetResult(printProgramme, Semester, AppUserHelper.User_ID);

            if (result == null || (result?.SubjectResults.IsNullOrEmpty() ?? true))
                return new ResponseData() { IsSuccess = false, ErrorMessage = "Your Internal Marks for any subjects has not been submitted to the University. Please contact your college for submission of internals to the university." };

            var blockedSubjects = result.SubjectResults.Where(s => s.SubjectType != SubjectType.AE && s.SubjectType != SubjectType.SEC && !s.InternalMarks.HasValue).Select(x => x.SubjectFullName);
            if (blockedSubjects.IsNotNullOrEmpty())
                return new ResponseData() { IsSuccess = false, ErrorMessage = "Your Internal Marks for " + string.Join(",", blockedSubjects) + " have not been submitted to the University.<br/> Please contact your college for submission of internals to the university." };
            else
                return new ResponseData() { IsSuccess = true };
        }

        private ARGExamFormDownloadable GetExaminationSettings(PrintProgramme orgPrintProgramme, int applyingForSemester)
        {
            ARGExamFormDownloadable settingsForm = null;
            ExaminationCourseCategory courseCategory = new GeneralFunctions().GetExaminationCourseCategory(orgPrintProgramme);

            if (ExaminationAllowedForStudentCourse(AppUserHelper.User_ID, orgPrintProgramme, applyingForSemester))
                settingsForm = new RegularExaminationsDB().GetRegularExamFormDownloadableWithoutDateValidation(courseCategory, applyingForSemester);

            return settingsForm;
        }

        private bool ExaminationAllowedForStudentCourse(Guid user_ID, PrintProgramme orgPrintProgramme, int applyingForSemester)
        {
            orgPrintProgramme = new GeneralFunctions().MappingTable(orgPrintProgramme);
            return new RegularExaminationsDB().ExaminationAllowedForStudentCourse(user_ID, orgPrintProgramme, applyingForSemester);
        }

        public RegularExaminationViewModel GetExaminationFormViewModel(short semester)
        {
            RegularExaminationViewModel examinationViewModel = new RegularExaminationsDB().GetExaminationFormViewModel(AppUserHelper.User_ID);
            examinationViewModel.ApplyingForSemester = semester;
            examinationViewModel.SelectedCombination = new CombinationManager().GetSelectedCombination(AppUserHelper.User_ID, AppUserHelper.OrgPrintProgramme, semester);
            examinationViewModel.AdditionalSubjects = new StudentManager().GetStudentAdditionalSubjects(AppUserHelper.User_ID, semester, AppUserHelper.OrgPrintProgramme);
            examinationViewModel.ExaminationSettings = GetExaminationSettings(AppUserHelper.OrgPrintProgramme, examinationViewModel.ApplyingForSemester);
            examinationViewModel.FeeAmount = GetAmountForRegularExamination(examinationViewModel.ExaminationSettings, examinationViewModel.SelectedCombination, examinationViewModel.AdditionalSubjects);
            return examinationViewModel;
        }
    }
}
