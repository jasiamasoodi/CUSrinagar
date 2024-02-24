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
    public class CombinationSettingDB
    {
        public int GetTotalStudentsAlloted(Guid course_ID, Guid AcceptCollege_ID, short batch, PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT COUNT(ADMCombinationMaster.Course_ID)
                                FROM ARGPersonalInformation_{printProgramme.ToString()} p
                                    JOIN ARGSelectedCombination_{printProgramme.ToString()} sc
                                        ON sc.Student_ID = p.Student_ID
                                    JOIN ADMCombinationMaster
                                        ON ADMCombinationMaster.Combination_ID = sc.Combination_ID
                                WHERE p.Batch = @Batch
                                      AND sc.Semester = 1
                                      AND ADMCombinationMaster.Course_ID = @Course_ID
                                      AND AcceptCollege_ID = @AcceptCollege_ID
                                GROUP BY ADMCombinationMaster.Course_ID;";
            cmd.Parameters.AddWithValue("@Course_ID", course_ID);
            cmd.Parameters.AddWithValue("@AcceptCollege_ID", AcceptCollege_ID);
            cmd.Parameters.AddWithValue("@Batch", batch);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd);
        }

        public int ToggleCollegeChangeCombination(List<Guid> combinationSetting_IDs)
        {
            var Query = $@"update dbo.CombinationSetting SET AllowCollegeChangeCombination=CASE WHEN AllowCollegeChangeCombination=1 THEN 0 ELSE 1 END,
                        ClosingDate=CASE WHEN AllowCollegeChangeCombination=1 THEN ClosingDate ELSE dateadd(hour,1,getdate()) END
                        WHERE CombinationSetting_ID IN ({combinationSetting_IDs.ToIN()})" ;
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #region CombinationSetting
        public CombinationSetting GetCombinationSetting(Guid course_ID, short semester, short batch)
        {
            string TQuery = $@"SELECT CombinationSetting.*,CourseFullName 
                                FROM CombinationSetting
                                JOIN ADMCourseMaster ON CombinationSetting.Course_ID = ADMCourseMaster.Course_ID  
                                WHERE  CombinationSetting.Course_ID='{course_ID}'  AND CombinationSetting.Semester = {semester} AND CombinationSetting.Batch = {batch}";
            return new MSSQLFactory().GetObject<CombinationSetting>(TQuery);
        }

        public CombinationSetting GetCombinationSetting(Guid CombinationSetting_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT CombinationSetting.*,CourseFullName 
                                FROM CombinationSetting
                                JOIN ADMCourseMaster ON CombinationSetting.Course_ID = ADMCourseMaster.Course_ID  
                                WHERE  CombinationSetting_ID='{CombinationSetting_ID}'";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<CombinationSetting>(TQuery);
        }

        public List<CombinationSetting> GetCombinationSettings(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT CombinationSetting.*,CourseFullName 
                                FROM CombinationSetting
                                JOIN ADMCourseMaster ON CombinationSetting.Course_ID = ADMCourseMaster.Course_ID";
            var helper = new GeneralFunctions().GetWhereClause<CombinationSetting>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<CombinationSetting>(helper.Command);
        }

        public CombinationSetting GetCombinationSetting(short batch, Guid course_ID, int semester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT CombinationSetting.*,CourseFullName 
                                FROM CombinationSetting
                                JOIN ADMCourseMaster ON CombinationSetting.Course_ID = ADMCourseMaster.Course_ID  
                                WHERE  batch={batch} and course_ID='{course_ID}' and Semester={semester}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<CombinationSetting>(cmd);
        }
        public bool CombinationSettingExists(short batch, Guid course_ID, int semester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1 FROM CombinationSetting WHERE  batch={batch} and course_ID='{course_ID}' and Semester={semester}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public int Save(CombinationSetting setting)
        {
            return new MSSQLFactory().InsertRecord<CombinationSetting>(setting, "CombinationSetting");
        }

        public int Update(CombinationSetting setting)
        {
            List<string> ignoreList = new List<string>() { "CombinationSetting_ID", "CreatedOn", "CreatedBy" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "CombinationSetting");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE CombinationSetting_ID =@CombinationSetting_ID";
            sqlCommand.Parameters.AddWithValue("@CombinationSetting_ID", setting.CombinationSetting_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Delete(Guid combinationSetting_ID)
        {
            var Query = $"Delete from CombinationSetting WHERE WHERE CombinationSetting_ID ='{combinationSetting_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        #endregion

        #region SubjectCombinationSetting
        public SubjectCombinationSetting GetSubjectCombinationSetting(Guid course_ID, short semester, short batch)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT Setting._ID,Setting.Batch,Setting.Course_ID
,Setting.BaseSubject_ID,BaseSubjectMaster.SubjectFullName AS BaseSubjectFullName,BaseSubjectMaster.Semester AS BaseSemester
,Setting.DependentSubject_ID,DependentSubjectMaster.SubjectFullName AS DependedSubjectFullName,DependentSubjectMaster.Semester AS DependentSemester
,Setting.IsActive,Setting.Dated
FROM 
SubjectCombinationSetting Setting
JOIN ADMSubjectMaster BaseSubjectMaster ON BaseSubjectMaster.Subject_ID = Setting.BaseSubject_ID
LEFT JOIN ADMSubjectMaster DependentSubjectMaster ON DependentSubjectMaster.Subject_ID = Setting.DependentSubject_ID";
            cmd.Parameters.AddWithValue("@Course_ID", course_ID);
            cmd.Parameters.AddWithValue("@Semester", semester);
            cmd.Parameters.AddWithValue("@batch", batch);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<SubjectCombinationSetting>(TQuery);
        }

        public SubjectCombinationSetting SubjectCombinationSetting(Guid _ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT Setting.*
                                ,BaseSubjectMaster.SubjectFullName AS BaseSubjectFullName,BaseSubjectMaster.Semester AS BaseSemester,BaseSubjectMaster.Course_ID,BaseSubjectMaster.Programme
                                ,DependentSubjectMaster.SubjectFullName AS DependedSubjectFullName,DependentSubjectMaster.Semester AS DependentSemester
                                ,ADMCourseMaster.CourseFullName
                                FROM 
                                SubjectCombinationSetting Setting
                                JOIN ADMSubjectMaster BaseSubjectMaster ON BaseSubjectMaster.Subject_ID = Setting.BaseSubject_ID
                                Join ADMCourseMaster ON ADMCourseMaster.Course_ID=BaseSubjectMaster.Course_ID
                                LEFT JOIN ADMSubjectMaster DependentSubjectMaster ON DependentSubjectMaster.Subject_ID = Setting.DependentSubject_ID
                                Where Setting._ID='{_ID}'";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<SubjectCombinationSetting>(cmd);
        }

        public List<SubjectCombinationSetting> SubjectCombinationSettingList(Guid BaseSubject_ID,short ForSemester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT Setting.*
                                ,BaseSubjectMaster.SubjectFullName AS BaseSubjectFullName,BaseSubjectMaster.Semester AS BaseSemester,BaseSubjectMaster.Course_ID
                                ,DependentSubjectMaster.SubjectFullName AS DependentSubjectFullName,DependentSubjectMaster.Semester AS DependentSemester
                                ,ADMCourseMaster.CourseFullName
                                FROM 
                                SubjectCombinationSetting Setting
                                JOIN ADMSubjectMaster BaseSubjectMaster ON BaseSubjectMaster.Subject_ID = Setting.BaseSubject_ID
                                Join ADMCourseMaster ON ADMCourseMaster.Course_ID=BaseSubjectMaster.Course_ID
                                LEFT JOIN ADMSubjectMaster DependentSubjectMaster ON DependentSubjectMaster.Subject_ID = Setting.DependentSubject_ID
                                Where Setting.DependentSubject_ID IS NOT NULL AND Setting.BaseSubject_ID='{BaseSubject_ID}' AND ForSemester={ForSemester}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObjectList<SubjectCombinationSetting>(cmd);
        }

        public List<SubjectCombinationSetting> GetSubjectCombinationSettings(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"Select * FROM(Select Setting.*,BaseSubjectMaster.Programme,BaseSubjectMaster.Course_ID
                                ,BaseSubjectMaster.SubjectFullName AS BaseSubjectFullName,BaseSubjectMaster.Semester AS BaseSemester
                                ,DependentSubjectMaster.SubjectFullName AS DependentSubjectFullName,DependentSubjectMaster.Semester AS DependentSemester
                                ,ADMCourseMaster.CourseFullName
                                FROM 
                                SubjectCombinationSetting Setting
                                JOIN ADMSubjectMaster BaseSubjectMaster ON BaseSubjectMaster.Subject_ID = Setting.BaseSubject_ID
                                LEFT Join ADMCourseMaster ON ADMCourseMaster.Course_ID=BaseSubjectMaster.Course_ID
                                LEFT JOIN ADMSubjectMaster DependentSubjectMaster ON DependentSubjectMaster.Subject_ID = Setting.DependentSubject_ID)Setting";
            //string Query1 = $@"SELECT * FROM(Select Setting.*,BaseSubjectMaster.Programme
            //                    ,BaseSubjectMaster.SubjectFullName AS BaseSubjectFullName,BaseSubjectMaster.Semester AS BaseSemester
            //                    ,DependentSubjectMaster.SubjectFullName AS DependentSubjectFullName,DependentSubjectMaster.Semester AS DependentSemester
            //                    ,ADMCourseMaster.CourseFullName
            //                    FROM 
            //                    SubjectCombinationSetting Setting
            //                    JOIN ADMSubjectMaster BaseSubjectMaster ON BaseSubjectMaster.Subject_ID = Setting.BaseSubject_ID
            //                    LEFT Join ADMCourseMaster ON ADMCourseMaster.Course_ID=Setting.Course_ID
            //                    LEFT JOIN ADMSubjectMaster DependentSubjectMaster ON DependentSubjectMaster.Subject_ID = Setting.DependentSubject_ID 
            //                    {(CSV? "WHERE DependentSubject_ID IS NOT NULL" : "WHERE DependentSubject_ID IS NULL)Setting")}";
            var helper = new GeneralFunctions().GetWhereClause<SubjectCombinationSetting>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SubjectCombinationSetting>(helper.Command);
        }

        public bool SubjectCombinationSettingExists(Guid BaseSubect_ID, short ForSemester)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1
                            FROM 
                            SubjectCombinationSetting Setting
                            WHERE Setting.BaseSubject_ID='{BaseSubect_ID}' AND ForSemester={ForSemester}";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(TQuery) > 0;
        }
        public bool SubjectCombinationSettingExists(Guid BaseSubect_ID, short BaseSemester, Guid _ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT 1
                            FROM 
                            SubjectCombinationSetting Setting
                            WHERE Setting.BaseSubject_ID='{BaseSubect_ID}' AND ForSemester={BaseSemester} AND _ID<>'{_ID}' AND DependentSubject_ID IS NULL";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }
        public int Save(SubjectCombinationSetting setting)
        {
            return new MSSQLFactory().InsertRecord<SubjectCombinationSetting>(setting, "SubjectCombinationSetting");
        }

        public int Update(SubjectCombinationSetting setting)
        {
            List<string> ignoreList = new List<string>() { "_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "SubjectCombinationSetting");
            sqlCommand.CommandText = sqlCommand.CommandText + " WHERE _ID =@_ID";
            sqlCommand.Parameters.AddWithValue("@_ID", setting._ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Update(SubjectCombinationSetting newsetting, SubjectCombinationSetting oldsetting)
        {
            var Query = $@"UPDATE SubjectCombinationSetting SET BaseSubject_ID='{newsetting.BaseSubject_ID}',ForSemester={newsetting.ForSemester},Dated='{DateTime.Now}' WHERE BaseSubject_ID='{oldsetting.BaseSubject_ID}' AND ForSemester={oldsetting.ForSemester} AND DependentSubject_ID IS NOT NULL";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int DeleteSubjectCombinationSetting(Guid _ID)
        {
            var Query = $"Delete from SubjectCombinationSetting WHERE _ID ='{_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        public int DeleteBaseSubjectCombinationSetting(Guid BaseSubject_ID,short ForSemester)
        {
            var Query = $"Delete from SubjectCombinationSetting WHERE BaseSubject_ID ='{BaseSubject_ID}' AND ForSemester={ForSemester}";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }
        #endregion

        #region CombinationSettingStructure

        public List<CombinationSettingStructure> CombinationSettingStructureList(Parameters parameters)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $@"SELECT CombinationSettingStructure.* FROM CombinationSettingStructure";
            var helper = new GeneralFunctions().GetWhereClause<CombinationSettingStructure>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<CombinationSettingStructure>(helper.Command);
        }
        public CombinationSettingStructure GetCombinationSettingStructure(Guid CombinationSettingStructure_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string TQuery = $@"SELECT CombinationSettingStructure.* FROM CombinationSettingStructure WHERE  CombinationSettingStructure_ID='{CombinationSettingStructure_ID}'";
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetObject<CombinationSettingStructure>(TQuery);
        }
        public int Save(CombinationSettingStructure setting)
        {
            return new MSSQLFactory().InsertRecord<CombinationSettingStructure>(setting, "CombinationSettingStructure");
        }
        public int Update(CombinationSettingStructure setting)
        {
            List<string> ignoreList = new List<string>() { "CombinationSettingStructure_ID" };
            var sqlCommand = new MSSQLFactory().UpdateRecord(setting, ignoreList, ignoreList, "CombinationSettingStructure");
            sqlCommand.CommandText = sqlCommand.CommandText + $" WHERE CombinationSettingStructure_ID ='{setting.CombinationSettingStructure_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        #endregion
    }
}
