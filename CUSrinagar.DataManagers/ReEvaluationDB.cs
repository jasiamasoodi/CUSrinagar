using System;
using CUSrinagar.Models;
using Terex;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using System.Collections.Generic;
using GeneralModels;
using System.Web.Mvc;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;

namespace CUSrinagar.BusinessManagers
{
    public class ReEvaluationDB
    {
        public int AddEvaluationForm(ReEvaluation reEvaluation)
        {
            return new MSSQLFactory().InsertRecord<ReEvaluation>(reEvaluation);
        }

        public static int GetFormNumberCount(bool IsReEvaluation)
        {
            return new MSSQLFactory().ExecuteScalar<int>(new ReEvaluationSQLQueries().GetFormNumberCount(IsReEvaluation));
        }

        public decimal GetSubjectReEvaluationFee(FormType formType)
        {
            return new MSSQLFactory().ExecuteScalar<decimal>(new ReEvaluationSQLQueries().GetSubjectReEvaluationFee(formType));
        }

        public string GetFormPrefix(FormType formType,short semester,short submittedyear)
        {
            return new MSSQLFactory().ExecuteScalar<string>(new ReEvaluationSQLQueries().GetFormPrefix(formType)).ToString();
        }

        public List<ReEvaluationPayment> GetReEvaluationsForStudent(Guid student_ID, short Semester,short SubmittedYear)
        {
            var personalInformationTable = new GeneralFunctions().GetTableName(AppUserHelper.OrgPrintProgramme, Module.PersonalInformation);

            var query= $@" SELECT ReEvaluation.*,FullName AS StudentName 
                            FROM ReEvaluation 
                            INNER JOIN {personalInformationTable} ON {personalInformationTable}.Student_ID = ReEvaluation.Student_ID 
                            WHERE ReEvaluation.Student_ID = '{student_ID}' AND ReEvaluation.FormStatus = 11 AND ReEvaluation.Semester={Semester} AND SubmittedYear={SubmittedYear}";
            return new MSSQLFactory().GetObjectList<ReEvaluationPayment>(query);
        }

        public ReEvaluation GetReEvaluationsForStudentByReEvaluation_ID(Guid reEvaluation)
        {
            return new MSSQLFactory().GetObject<ReEvaluation>(new ReEvaluationSQLQueries().GetReEvaluationsForStudentByReEvaluation_ID(reEvaluation));
        }

        public int UpdateReEvaluationFormStatus(string formNumbers, FormStatus status)
        {
            string sql = new ReEvaluationSQLQueries().UpdateReEvaluationFormStatus(formNumbers, status);
            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteReEvaluationForm(Guid student_ID, Guid reEvaluation_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery(new ReEvaluationSQLQueries().DeleteReEvaluationForm(student_ID, reEvaluation_ID));
        }

        public int SaveEvaluationSubject(ReEvaluationStudentSubject evaluationSubject)
        {
            return new MSSQLFactory().InsertRecord(evaluationSubject, "ReEvaluationStudentSubjects");
        }

        public List<ReEvaluationStudentSubject> GetSubjectsForEvaluation(Guid student_ID, Guid reEvaluation_ID)
        {
            return new MSSQLFactory().GetObjectList<ReEvaluationStudentSubject>(new ReEvaluationSQLQueries().GetSubjectsForEvaluation(reEvaluation_ID, student_ID));
        }

        public List<ReEvaluationStudentSubject> GetSubjectsForEvaluation(Guid reEvaluation_ID, Guid student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObjectList<ReEvaluationStudentSubject>(new ReEvaluationSQLQueries().GetSubjectsForEvaluation(reEvaluation_ID, student_ID, programme));
        }

        public bool IsReEvaluationOpen(FormType formType)
        {
            string sql = new ReEvaluationSQLQueries().IsReEvaluationOpen(formType);
            bool result= new MSSQLFactory().ExecuteScalar<bool>(sql);
            if (result == false && AppUserHelper.CollegeCode.Equals("IASE"))
            {
                sql = sql.Replace("CourseCategory =2", "CourseCategory =1");
                result = new MSSQLFactory().ExecuteScalar<bool>(sql);
            }
            return result;
        }
        public short GetAppliedForSemester()
        {
            return new MSSQLFactory().ExecuteScalar<short>($@"SELECT TOP 1 Semester FROM ReEvaluation WHERE Student_ID='{AppUserHelper.User_ID}' ORDER BY CreatedOn DESC");
        }

        public List<ReEvaluation> GetReEvaluationList(Parameters parameters, Programme programme)
        {
            return new MSSQLFactory().GetObjectList<ReEvaluation>(new ReEvaluationSQLQueries().GetReEvaluationList(parameters, programme));
        }

        public ReEvaluationSetting GetReEvaluationSettings(bool IsRevaluation, ExaminationCourseCategory courseCategory, short semester, short SubmittedYear)
        {
            return new MSSQLFactory().GetObject<ReEvaluationSetting>($@"SELECT * FROM ReEvaluationSetting WHERE IsReEvaluation={(IsRevaluation ? 1 : 0)} 
                            AND AllowDownloadForm = 1 AND CourseCategory = {(short)courseCategory} and semester={semester} and submittedyear={SubmittedYear}");
        }
        public ReEvaluationSetting GetReEvaluationSettings(ExaminationCourseCategory courseCategory)
        {
            return new MSSQLFactory().GetObject<ReEvaluationSetting>($@"SELECT Top 1 * FROM ReEvaluationSettings WHERE AllowDownloadForm=1 AND CourseCategory = '{courseCategory.ToString()}' ");
        }

        public ReEvaluationSetting GetReEvaluationSettingsReprint(ExaminationCourseCategory courseCategory)
        {
            return new MSSQLFactory().GetObject<ReEvaluationSetting>($@"SELECT TOP 1 ReEvaluationSettings.* FROM ReEvaluationSettings
                                        JOIN ReEvaluation ON ReEvaluation.Notification_ID = ReEvaluationSettings.Notification_ID
                                        WHERE CourseCategory='{courseCategory.ToString()}' AND Student_ID='{AppUserHelper.User_ID}' AND ReEvaluationSettings.SubmittedYear>={DateTime.Now.Year}
                                        ORDER BY ReEvaluation.Semester DESC,ReEvaluationSettings.SubmittedYear DESC");
        }

        public List<SelectListItem> GetAvailableSemestersForDropdown()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new ReEvaluationSQLQueries().GetAvailableSemestersForDropdown);
        }

        public List<ReEvaluationCompactList> GetReEvaluationCompactLists(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<ReEvaluationCompactList>(new ReEvaluationSQLQueries().GetReEvaluationCompactLists(parameters));
        }

        

    }
}