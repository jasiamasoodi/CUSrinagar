using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUSrinagar.Models;
using Terex;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using GeneralModels;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace CUSrinagar.DataManagers
{
    public class ResultNotificationDB
    {

        public List<ResultNotificationList> ListCompact(Parameters Parameter)
        {
            var Query = $@"SELECT ResultNotification_ID,Semester,Batch,PrintProgramme,Title,Dated,ImportedToMasterTable FROM ResultNotification";
            FilterHelper Helper = new GeneralFunctions().GetWhereClause<ResultNotification>(Parameter.Filters);
            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ResultNotificationList>(Helper.Command);
        }
        public List<ResultNotification> List(Parameters Parameter)
        {
            var Query = $@"SELECT * FROM ResultNotification";
            FilterHelper Helper = new GeneralFunctions().GetWhereClause<ResultNotification>(Parameter.Filters);
            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ResultNotification>(Helper.Command);
        }
        public ResultNotification Get(Guid resultNotification_ID)
        {
            var Query = new ResultNotificationSQLQueries().GetItemQuery(resultNotification_ID);
            return new MSSQLFactory().GetObject<ResultNotification>(Query);
        }
       
        public ResultNotification CheckAnyBacklog(Guid id, String Searchtext,PrintProgramme printProgramme,short semester)
        {
            var Query = new ResultNotificationSQLQueries().CheckAnyBacklog(id,Searchtext, printProgramme,semester);
            return new MSSQLFactory().GetObject<ResultNotification>(Query);
        }


        public List<ResultNotification> GetList(List<Guid> list)
        {
            string Qurery = $"SELECT * FROM ResultNotification WHERE ResultNotification_ID IN({list.ToIN()}) ORDER BY Dated";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<ResultNotification>(sqlCommand);
        }

        public List<AwardCount> GetCount(ResultNotification resultNotification)
        {
            string tblname = new GeneralFunctions().GetSemesterTableName(resultNotification.Semester.ToString(), resultNotification.Programme);
            string query = $@"SELECT COUNT(Detail.Student_ID) TotalCount,Sum(CAST(dbo.HaveInternals(Detail.Subject_ID,SubjectMarksStructure_ID,Detail.Student_ID,{resultNotification.Semester}) AS int)) InternalCount
                                                                       ,Sum(CAST(dbo.HaveExternals(Detail.Subject_ID,SubjectMarksStructure_ID,Detail.Student_ID,{resultNotification.Semester}) AS int)) ExternalCount  FROM dbo.VWStudentWithDetail Detail
                                                JOIN {tblname} Result
                                                ON Result.Student_ID = Detail.Student_ID
                                                AND Result.Subject_ID = Detail.Subject_ID
                                                JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = Result.Subject_ID
                                                WHERE Detail.Batch = {resultNotification.Batch} AND Detail.Semester = {resultNotification.Semester}";
            if (resultNotification.Course_ID != null && resultNotification.Course_ID != Guid.Empty)
            { query = query + $" AND Detail.Course_ID = '{resultNotification.Course_ID}'"; }
            if (resultNotification.College_ID != null && resultNotification.College_ID != Guid.Empty)
            { query = query + $" AND Detail.AcceptCollege_ID = '{resultNotification.College_ID}'"; }
            if (resultNotification.Subject_ID != null && resultNotification.Subject_ID != Guid.Empty)
            { query = query + $" AND Detail.Subject_ID = '{resultNotification.Subject_ID}'"; }
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = query;
            return new MSSQLFactory().GetObjectList<AwardCount>(sqlCommand);
        }


        public ResultNotification GetNotification(ResultNotification resultNotification, int Batch)
        {
            string query = string.Empty;
            PrintProgramme Programme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(resultNotification.Programme);
            query = $@"Select *  ResultNotification where semester={resultNotification.Semester} and Batch={Batch} and Programme={(int)Programme} ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().GetObject<ResultNotification>(cmd);
        }
        public int DeclareResult(ResultNotification resultNotification)
        {
            string query = string.Empty;
            string pFix = new GeneralFunctions().GetPrintProgrammeFix(resultNotification.PrintProgramme);
            if (resultNotification.IsBacklog)
            {
                query = $@"UPDATE semResult
                              SET InternalSubmitted = 1,
                              ExternalSubmitted = 1,
                              ResultNotification_ID = '{resultNotification.ResultNotification_ID}',
                              ExamForm_ID = eform.StudentExamForm_ID
                              FROM dbo.{pFix}_Semester{resultNotification.Semester} semResult JOIN
                              VWStudentCourse pinfo ON pinfo.Student_ID = semResult.Student_ID AND batch = {resultNotification.Batch}  AND ISNULL(semresult.IsClearAndLocked,0)=0
                              JOIN dbo.ARGStudentExamForm_{pFix} eform ON eform.student_ID = semResult.student_ID and semester={resultNotification.Semester} and IsRegular=0 and Status={(int)FormStatus.Accepted}
                              AND eform.Year={resultNotification.ExamFormSubmissionYear}
                              JOIN dbo.ARGStudentReExamForm ON eform.StudentExamForm_ID=ARGStudentReExamForm.StudentExamForm_ID
							  AND ARGStudentReExamForm.Subject_ID = semResult.Subject_ID AND eform.Student_ID=semResult.Student_ID
	                          Left JOIN msstudentmarks MS ON pinfo.student_Id= MS.Student_id AND MS.Marks_ID IS NULL
                              ";
                if (resultNotification.CourseIds != null && resultNotification.CourseIds.Count > 0)
                    query = query + $"  Where Course_Id IN ('{string.Join("','", resultNotification.CourseIds)}')";
            }
            else
            {
                query = $@"UPDATE semResult
                              SET InternalSubmitted = 1,
                              ExternalSubmitted = 1,
                              ResultNotification_ID = '{resultNotification.ResultNotification_ID}',
                              ExamForm_ID = eform.StudentExamForm_ID
                              FROM {pFix}_Semester{resultNotification.Semester}  semResult JOIN
                              VWStudentCourse pinfo ON pinfo.Student_ID = semResult.Student_ID 
							  JOIN dbo.ARGSelectedCombination_{pFix} SC ON SC.Student_ID = pinfo.Student_ID AND Sc.Semester={resultNotification.Semester} AND SemesterBatch ={resultNotification.Batch}
                              JOIN dbo.ARGStudentExamForm_{pFix} eform ON eform.student_ID = semResult.student_ID and eform.Semester={resultNotification.Semester} and IsRegular=1 and Status={(int)FormStatus.Accepted}
                              where semResult.ResultNotification_id is NULL";
                if (resultNotification.CourseIds != null && resultNotification.CourseIds.Count > 0)
                    query = query + $" And Course_Id IN ('{string.Join("','", resultNotification.CourseIds)}')";

            }
          
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public List<SelectListItem> GetResultNotifications(int semester)
        {
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT convert(nvarchar(50),[ResultNotification_ID]) as [Value],[Title] as [Text] from [ResultNotification]";

            if (semester > 0)
            { query = query + $@" where semester ={semester}"; }
            command.CommandText = query + "  order by [Semester]";
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }
        public int UpdateImportStatus(Guid _ID)
        {
            string Query = $@"UPDATE ResultNotification SET ImportedToMasterTable=1 WHERE ResultNotification_ID='{_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

    }
}
