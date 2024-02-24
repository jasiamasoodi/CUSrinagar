using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CUSrinagar.Extensions;
using Terex;
using CUSrinagar.Enums;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class ReEvaluationSettingDB
    {
        public List<ReEvaluationSetting> List(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT * FROM ReEvaluationSetting";
            var helper = new GeneralFunctions().GetWhereClause<ReEvaluationSetting>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ReEvaluationSetting>(helper.Command);
        }

        public ReEvaluationSetting Get(Guid ReEvaluationSetting_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT * FROM ReEvaluationSetting WHERE  ReEvaluationSetting_ID='{ReEvaluationSetting_ID}'";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<ReEvaluationSetting>(TQuery);
        }

        public bool ReEvaluationSettingExists(short submittedYear, ExaminationCourseCategory ExaminationCourseCategory, short semester, bool IsRevaluation)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1 FROM ReEvaluationSetting WHERE  submittedyear={submittedYear} and courseCategory='{(short)ExaminationCourseCategory}' 
                            and Semester={semester} and IsReEvaluation={(IsRevaluation ? 1 : 0)}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public int Save(ReEvaluationSetting setting)
        {
            return new MSSQLFactory().InsertRecord<ReEvaluationSetting>(setting, "ReEvaluationSetting");
        }

        public int Update(ReEvaluationSetting setting)
        {
            List<string> ignoreList = new List<string>() { "ReEvaluationSetting_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "ReEvaluationSetting");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE ReEvaluationSetting_ID =@ReEvaluationSetting_ID";
            sqlCommand.Parameters.AddWithValue("@ReEvaluationSetting_ID", setting.ReEvaluationSetting_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Delete(Guid ReEvaluationSetting_ID)
        {
            var Query = $"Delete from ReEvaluationSetting WHERE ReEvaluationSetting_ID ='{ReEvaluationSetting_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int ChangeStatus(Guid ReEvaluationSetting_ID)
        {
            var Query = $"Update ReEvaluationSetting Set AllowDownloadForm=case when AllowDownloadForm=1 then 0 else 1 end WHERE ReEvaluationSetting_ID ='{ReEvaluationSetting_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public List<ReEvaluationSetting> List(PrintProgramme printProgramme, bool IsReEvaluation = true, short? SubmittedYear = null)
        {
            new MSSQLFactory().ExecuteNonQuery("UPDATE dbo.ReEvaluationSetting SET AllowDownloadForm=0,DownloadDays=0 WHERE EndDate<GETDATE()");

            string query = $@"SELECT * FROM ReEvaluationSetting WHERE AllowDownloadForm = 1 AND IsReEvaluation={(IsReEvaluation ? "1" : "0")}
                                    {(SubmittedYear.HasValue ? " AND SubmittedYear=" + SubmittedYear : "")}
                                    AND SYSDATETIME() >= StartDate AND SYSDATETIME() <= EndDate
                                    AND CourseCategory = { (short)new GeneralFunctions().GetExaminationCourseCategory(AppUserHelper.OrgPrintProgramme) } 
                                    ORDER BY CourseCategory,Semester";
            List<ReEvaluationSetting> list= new MSSQLFactory().GetObjectList<ReEvaluationSetting>(query);
            
            if(list.IsNullOrEmpty() && AppUserHelper.CollegeCode.ToUpper().Equals("IASE"))
            {
                query = $@"SELECT * FROM ReEvaluationSetting WHERE AllowDownloadForm = 1 AND IsReEvaluation={(IsReEvaluation ? "1" : "0")}
                                    {(SubmittedYear.HasValue ? " AND SubmittedYear=" + SubmittedYear : "")}
                                    AND SYSDATETIME() >= StartDate AND SYSDATETIME() <= EndDate
                                    AND CourseCategory = {(short)ExaminationCourseCategory.UG} 
                                    ORDER BY CourseCategory,Semester";
            }
            list = new MSSQLFactory().GetObjectList<ReEvaluationSetting>(query);
            return list;
        }

    }
}
