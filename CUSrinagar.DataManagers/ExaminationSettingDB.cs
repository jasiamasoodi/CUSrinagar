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
    public class ExaminationSettingDB
    {
        public ExaminationSetting GetExaminationSetting(string FormNumberPrefix, ExaminationCourseCategory courseCat, Semester semester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT ARGExamFormDownloadable.*
                                FROM ARGExamFormDownloadable
                               WHERE  ARGExamFormDownloadable.FormNumberPrefix=@FormNumberPrefix  AND ARGExamFormDownloadable.Semester = @Semester AND ARGExamFormDownloadable.CourseCategory = @CourseCategory";
            cmd.Parameters.AddWithValue("@FormNumberPrefix", FormNumberPrefix);
            cmd.Parameters.AddWithValue("@Semester", semester);
            cmd.Parameters.AddWithValue("@CourseCategory", courseCat);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<ExaminationSetting>(cmd);
        }

        public ExaminationSetting GetExaminationSetting(Guid ARGExamForm_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT ARGExamFormDownloadable.*
                                FROM ARGExamFormDownloadable
                                WHERE  ARGExamForm_ID=@ARGExamForm_ID";
            cmd.Parameters.AddWithValue("@ARGExamForm_ID", ARGExamForm_ID);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<ExaminationSetting>(cmd);
        }

        public List<ExaminationSetting> GetExaminationSettings(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT ARGExamFormDownloadable.* FROM ARGExamFormDownloadable ";
            var helper = new GeneralFunctions().GetWhereClause<ExaminationSetting>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ExaminationSetting>(helper.Command);
        }

        public ExaminationSetting GetExaminationSettings(string FormNumberPrefix, ExaminationCourseCategory courseCat, Semester semester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT ARGExamFormDownloadable.*
                                FROM ARGExamFormDownloadable
                                WHERE  FormNumberPrefix={FormNumberPrefix} and CourseCategory='{courseCat}' and Semester={semester}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<ExaminationSetting>(cmd);
        }
        public bool ExaminationSettingExists(ExaminationCourseCategory courseCat, short semester, int year, bool isBacklogSetting)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1 FROM ARGExamFormDownloadable WHERE CourseCategory='{courseCat.ToString()}' and Semester={(int)semester} and Year={year} and IsBacklogSetting={(isBacklogSetting ? '1' : '0')}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public int AllowExamFormApplications(DateTime startDate, DateTime endDate, Guid notification_ID, Guid ARGExamForm_ID)
        {
            SqlCommand cmd = new SqlCommand
            {
                CommandText = $@"UPDATE ARGExamFormDownloadable SET ALLOWDOWNLOADFORM=1,STARTDATE={startDate},EndDate={endDate},Notification_ID={notification_ID} WHERE ARGExamForm_ID={ARGExamForm_ID}"
            };
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int Save(ExaminationSetting setting)
        {
            return new MSSQLFactory().InsertRecord<ExaminationSetting>(setting, "ARGExamFormDownloadable");
        }

        public int Update(ExaminationSetting setting)
        {
            List<string> ignoreList = new List<string>() { "ARGExamForm_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "ARGExamFormDownloadable");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE ARGExamForm_ID =@ARGExamForm_ID";
            sqlCommand.Parameters.AddWithValue("@ARGExamForm_ID", setting.ARGExamForm_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Delete(Guid ARGExamForm_ID)
        {
            var Query = $"Delete from ARGExamFormDownloadable WHERE WHERE ARGExamForm_ID ='{ARGExamForm_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
    }
}
