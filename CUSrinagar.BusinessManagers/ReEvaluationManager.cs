using CUSrinagar.Models;
using CUSrinagar.DataManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using GeneralModels;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System.Web.Mvc;
using System.Reflection;
using System.Web.Script.Serialization;

namespace CUSrinagar.BusinessManagers
{
    public class ReEvaluationManager
    {
        public ResponseData AddEvaluationForm(ReEvaluation reEvaluation, short SubmittedYear)
        {
            decimal totalReEvaluationFee = 0, totalXeroxFee = 0;
            string formNumbers = string.Empty;

            ResponseData response = ValidateReEvaluation(reEvaluation);
            //return new ResponseData() { ErrorMessage = "Process either stopped or has not been started yet." };

            using (TransactionScope transaction = new TransactionScope())
            {
                //Save Re-Evaluation Form
                if (response.IsSuccess && reEvaluation.SubjectsForEvaluation.IsNotNullOrEmpty() && reEvaluation.SubjectsForEvaluation.Where(x => x.OptForReEvaluation).IsNotNullOrEmpty())
                {
                    int NoOfSubjects = 1;

                    ExaminationCourseCategory examinationCourseCategory = new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme);
                    ReEvaluationSetting evaluationSettings = new ReEvaluationDB().GetReEvaluationSettings(true, examinationCourseCategory, reEvaluation.Semester, SubmittedYear);//todo
                    if (evaluationSettings == null && examinationCourseCategory == ExaminationCourseCategory.BED)
                    {
                        evaluationSettings = new ReEvaluationDB().GetReEvaluationSettings(true, ExaminationCourseCategory.UG, reEvaluation.Semester, SubmittedYear);//todo
                    }

                    if (evaluationSettings != null)
                    {
                        reEvaluation.FormType = FormType.ReEvaluation;
                        reEvaluation.ReEvaluation_ID = Guid.NewGuid();
                        reEvaluation.FormNumber = GetFormNumber(reEvaluation.Student_ID, evaluationSettings);
                        reEvaluation.FormStatus = FormStatus.Submitted;
                        reEvaluation.FeeAmount = GetFeeAmount(reEvaluation);
                        reEvaluation.Semester = evaluationSettings.Semester;
                        reEvaluation.SubmittedYear = evaluationSettings.SubmittedYear;
                        reEvaluation.Notification_ID = evaluationSettings.Notification_ID;
                        reEvaluation.SetWorkFlow(RecordState.New);

                        totalReEvaluationFee = reEvaluation.FeeAmount;
                        formNumbers = reEvaluation.FormNumber;

                        response.NumberOfRecordsEffected += new ReEvaluationDB().AddEvaluationForm(reEvaluation);

                        if (response.NumberOfRecordsEffected > 0)
                            foreach (var evaluationSubject in reEvaluation.SubjectsForEvaluation)
                            {
                                if (evaluationSubject.OptForReEvaluation)
                                {
                                    evaluationSubject.ReEvaluation_ID = reEvaluation.ReEvaluation_ID;
                                    evaluationSubject.SetWorkFlow(RecordState.New);
                                    NoOfSubjects += SaveSubject(evaluationSubject);
                                }
                            }
                        response.ResponseObject = reEvaluation;
                    }
                }
                ResponseData responsexerox = ValidateXerox(reEvaluation);
                //Save Digital Form
                if (responsexerox.IsSuccess && reEvaluation.SubjectsForEvaluation.IsNotNullOrEmpty() && reEvaluation.SubjectsForEvaluation.Where(i => i.OptForXerox).IsNotNullOrEmpty())
                {
                    response = responsexerox;

                    ExaminationCourseCategory examinationCourseCategory = new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme);
                    ReEvaluationSetting evaluationSettings = new ReEvaluationDB().GetReEvaluationSettings(false, examinationCourseCategory, reEvaluation.Semester, SubmittedYear);//todo

                    if (evaluationSettings == null && examinationCourseCategory == ExaminationCourseCategory.BED)
                    {
                        evaluationSettings = new ReEvaluationDB().GetReEvaluationSettings(false, ExaminationCourseCategory.UG, reEvaluation.Semester, SubmittedYear);//todo
                    }

                    if (evaluationSettings != null)
                    {
                        reEvaluation.FormType = FormType.Xerox;

                        reEvaluation.ReEvaluation_ID = Guid.NewGuid();
                        reEvaluation.FormStatus = FormStatus.Submitted;
                        reEvaluation.FormNumber = GetFormNumber(reEvaluation.Student_ID, evaluationSettings);
                        reEvaluation.FeeAmount = GetFeeAmount(reEvaluation);
                        reEvaluation.Semester = evaluationSettings.Semester;
                        reEvaluation.SubmittedYear = evaluationSettings.SubmittedYear;
                        reEvaluation.Notification_ID = evaluationSettings.Notification_ID;
                        reEvaluation.SetWorkFlow(RecordState.New);
                        totalXeroxFee = reEvaluation.FeeAmount;
                        response.NumberOfRecordsEffected += new ReEvaluationDB().AddEvaluationForm(reEvaluation);
                        formNumbers += "," + reEvaluation.FormNumber;

                        if (response.NumberOfRecordsEffected > 0)
                        {
                            var formCount = (ReEvaluationDB.GetFormNumberCount(evaluationSettings.IsReEvaluation) + 1).ToString("D4");
                            UpdateFormCount(formCount, evaluationSettings.IsReEvaluation);
                            foreach (var evaluationSubject in reEvaluation.SubjectsForEvaluation)
                            {
                                if (evaluationSubject.OptForXerox)
                                {
                                    evaluationSubject.ReEvaluation_ID = reEvaluation.ReEvaluation_ID;
                                    SaveSubject(evaluationSubject);
                                }
                            }
                        }
                        response.ResponseObject = reEvaluation;
                    }
                }
                transaction.Complete();
                reEvaluation.FeeAmount = totalReEvaluationFee + totalXeroxFee;
                reEvaluation.FormNumber = formNumbers;
            }
            response.IsSuccess = response.ResponseObject != null;
            return response;
        }

        public List<SelectListItem> GetAvailableSemestersForDropdown()
        {
            return new ReEvaluationDB().GetAvailableSemestersForDropdown();
        }

        public List<ReEvaluationCompactList> GetReEvaluationCompactLists(Parameters parameters)
        {
            return new ReEvaluationDB().GetReEvaluationCompactLists(parameters);
        }

        private decimal GetFeeAmount(ReEvaluation reEvaluation)
        {
            decimal totalFee = 0;
            var feePerSubject = new ReEvaluationManager().GetSubjectReEvaluationFee(reEvaluation.FormType);

            switch (reEvaluation.FormType)
            {
                case FormType.ReEvaluation:
                    totalFee = feePerSubject * reEvaluation.SubjectsForEvaluation.Where(x => x.OptForReEvaluation).ToList().Count;
                    break;
                case FormType.Xerox:
                    totalFee = feePerSubject * reEvaluation.SubjectsForEvaluation.Where(x => x.OptForXerox).ToList().Count;
                    break;
                default:
                    break;
            }
            return reEvaluation.FeeAmount = totalFee;
        }

        private static int SaveSubject(ReEvaluationStudentSubject reEvaluation)
        {
            reEvaluation.Student_ID = reEvaluation.Student_ID;
            reEvaluation.ReEvaluationSubject_ID = Guid.NewGuid();
            return new ReEvaluationManager().SaveEvaluationSubject(reEvaluation);
        }

        public int SaveEvaluationSubject(ReEvaluationStudentSubject evaluationSubject)
        {
            return new ReEvaluationDB().SaveEvaluationSubject(evaluationSubject);
        }

        public List<ReEvaluation> GetReEvaluationList(Parameters parameters, Programme programme)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            parameters.Filters.Add(new SearchFilter { Column = "Programme", TableAlias = "ADMCourseMaster", Operator = SQLOperator.EqualTo, GroupOperation = LogicalOperator.AND, Value = ((short)programme).ToString() });
            List<ReEvaluation> reEvaluations = new ReEvaluationDB().GetReEvaluationList(parameters, programme);

            if (reEvaluations.IsNotNullOrEmpty())
            {
                List<ReEvaluation> reEvaluationsWithCenters = reEvaluations.Where(x => !string.IsNullOrWhiteSpace(x.CenterDetail))?.ToList() ?? new List<ReEvaluation>();
                List<ReEvaluation> reEvaluationsWithoutCenters = reEvaluations.Where(p => !reEvaluationsWithCenters.Any(p2 => p2.ReEvaluation_ID == p.ReEvaluation_ID))?.ToList() ?? new List<ReEvaluation>();

                reEvaluations = new List<ReEvaluation>();
                reEvaluations.AddRange(reEvaluationsWithCenters);
                reEvaluations.AddRange(reEvaluationsWithoutCenters);
                ResultDB resultDB = new ResultDB();
                decimal? marksObt = null;

                foreach (var reEvaluation in reEvaluations)
                {
                    reEvaluation.SubjectsForEvaluation = new ReEvaluationDB().GetSubjectsForEvaluation(reEvaluation.ReEvaluation_ID, reEvaluation.Student_ID, printProgramme);

                    foreach (ReEvaluationStudentSubject subjectsForEvaluation in reEvaluation?.SubjectsForEvaluation ?? new List<ReEvaluationStudentSubject>())
                    {
                        //get external marks of reevaluation subj
                        marksObt = resultDB.SubjectResult(printProgramme, subjectsForEvaluation.Subject_ID, reEvaluation.Student_ID, reEvaluation.Semester)?.TotalExternalMarksObtained;
                        if (marksObt != null)
                        {
                            subjectsForEvaluation.SubjectID = subjectsForEvaluation.SubjectID + $" (ExtMarksObt ={marksObt})";
                        }
                        else
                        {
                            subjectsForEvaluation.SubjectID = subjectsForEvaluation.SubjectID + $" (ExtMarksObt not submitted by AP)";
                        }
                    }
                }
                reEvaluationsWithCenters = null;//dispose
                reEvaluationsWithoutCenters = null;//dispose
            }
            return reEvaluations;
        }

        public List<ReEvaluationPayment> GetReEvaluationsForStudent(Guid student_ID, short Semester, short SubmittedYear)
        {
            var reEvaluations = new ReEvaluationDB().GetReEvaluationsForStudent(student_ID, Semester, SubmittedYear);
            if (reEvaluations.IsNotNullOrEmpty())
                foreach (var reEvaluation in reEvaluations)
                    reEvaluation.SubjectsForEvaluation = new ReEvaluationDB().GetSubjectsForEvaluation(student_ID, reEvaluation.ReEvaluation_ID);
            return reEvaluations;
        }

        public List<ReEvaluationPayment> GetReEvaluationsForStudentWithPayment(Guid student_ID, short Semester, short SubmittedYear)
        {
            var reEvaluations = new ReEvaluationDB().GetReEvaluationsForStudent(student_ID, Semester, SubmittedYear);
            foreach (var reEvaluation in reEvaluations)
                reEvaluation.SubjectsForEvaluation = new ReEvaluationDB().GetSubjectsForEvaluation(student_ID, reEvaluation.ReEvaluation_ID);
            return reEvaluations;
        }

        public ReEvaluation GetReEvaluationsForStudentByReEvaluation_ID(Guid reEvaluation_ID)
        {
            var reEvaluation = new ReEvaluationDB().GetReEvaluationsForStudentByReEvaluation_ID(reEvaluation_ID);
            if (reEvaluation != null)
                reEvaluation.SubjectsForEvaluation = new ReEvaluationDB().GetSubjectsForEvaluation(reEvaluation_ID, reEvaluation.ReEvaluation_ID);
            return reEvaluation;
        }

        private void UpdateFormCount(string formNumber, bool IsReEvaluation)
        {
            int formCount = 0;
            if (int.TryParse(formNumber, out formCount))
                new GeneralFunctions().UpdateReEvaluationFormCount(formCount, IsReEvaluation);
        }

        #region PrivateMethods
        private decimal GetSubjectReEvaluationFee(FormType formtype)
        {
            return new ReEvaluationDB().GetSubjectReEvaluationFee(formtype);
        }

        private ResponseData ValidateReEvaluation(ReEvaluation reEvaluation)
        {

            ResponseData response = new ResponseData() { IsSuccess = false };
            if (reEvaluation.SubjectsForEvaluation != null)
                foreach (var subject in reEvaluation.SubjectsForEvaluation)
                {
                    if (new SubjectsManager().Get(subject.Subject_ID) == null)
                    {
                        response.ErrorMessage = " Subject does not Exist ";
                        return response;
                    }
                }

            if (!new ReEvaluationDB().IsReEvaluationOpen(reEvaluation.FormType))
            {
                response.IsSuccess = false;
                response.ErrorMessage = " The Process of applying for ReEvaluation/Xerox is either closed or will be allowed soon.";
                return response;
            }

            response.IsSuccess = true;

            return response;
        }
        private ResponseData ValidateXerox(ReEvaluation reEvaluation)
        {

            ResponseData response = new ResponseData() { IsSuccess = false };
            if (reEvaluation.SubjectsForEvaluation != null)
                foreach (var subject in reEvaluation.SubjectsForEvaluation)
                {
                    if (new SubjectsManager().Get(subject.Subject_ID) == null)
                    {
                        response.ErrorMessage = " Subject does not Exist ";
                        return response;
                    }
                }

            if (!new ReEvaluationDB().IsReEvaluationOpen(FormType.Xerox))
            {
                response.IsSuccess = false;
                response.ErrorMessage = " The Process of applying for ReEvaluation/Xerox is either closed or will be allowed soon.";
                return response;
            }

            response.IsSuccess = true;

            return response;
        }
        //private string GetFormNumber(Guid student_ID, FormType formType, ref ResponseData response, short SubmittedYear)
        //{
        //    string batch = string.Empty;
        //    var studentBatch = new StudentManager().StudentExists(student_ID);
        //    if (studentBatch == 0)
        //        response.ErrorMessage = " Student does not Exist ";
        //    else
        //        batch = studentBatch.ToString().Substring(2, 2);

        //    var formPrefix = batch + GetFormPrefix(formType);
        //    var courseCategory = new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme);
        //    var formCount = (ReEvaluationDB.GetFormNumberCount(formType) + 1).ToString("D4");

        //    UpdateFormCount(formCount, formType);

        //    // Form Number Format - XRX/REV - Examination CourseCategory - Exam Year - Semester
        //    return formPrefix + courseCategory + SubmittedYear.ToString().Substring(2, 2) + "-" + formCount;
        //}
        private string GetFormNumber(Guid student_ID, ReEvaluationSetting evaluationSetting)
        {
            string batch = string.Empty;
            var studentBatch = new StudentManager().StudentExists(student_ID);
            batch = studentBatch.ToString().Substring(2, 2);

            var formPrefix = batch + evaluationSetting.FormNumberPrefix;
            var courseCategory = evaluationSetting.CourseCategory;
            var formCount = (ReEvaluationDB.GetFormNumberCount(evaluationSetting.IsReEvaluation) + 1).ToString("D4");

            UpdateFormCount(formCount, evaluationSetting.IsReEvaluation);

            // Form Number Format - XRX/REV - Examination CourseCategory - Exam Year - Semester
            return formPrefix + courseCategory + evaluationSetting.SubmittedYear.ToString().Substring(2, 2) + "-" + formCount;
        }
        private string GetFormPrefix(FormType formType, short semester, short submittedYear)
        {
            return new ReEvaluationDB().GetFormPrefix(formType, semester, submittedYear);
        }

        #endregion
    }
}
