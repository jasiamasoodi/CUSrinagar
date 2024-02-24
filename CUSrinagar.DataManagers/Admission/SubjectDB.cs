using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System.Data.SqlClient;
using Terex;
using System.Web.Mvc;
using GeneralModels;
using CUSrinagar.DataManagers.SQLQueries;
using System.Collections;
using System.Data;

namespace CUSrinagar.DataManagers
{
    public class SubjectDB
    {
        public List<ADMSubjectMaster> GetList(Guid Course_ID, int combinationSemester)
        {
            string Qurery = "SELECT S.Subject_ID ,	S.Course_ID ,	S.SubjectCode,	S.Semester ,S.SubjectFullName,	S.SubjectType,	S.Status,	S.College_ID,	S.IsCompulsory,	S.SubjectNumber,S.SubjectCapacitySet,S.SubjectCapacity,	S.Programme,S.Department_ID,S.FromBatch,S.ToBatch,S.HasResult,S.HasExaminationfee,ST.* FROM ADMSubjectMaster S" +
                "  JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID WHERE Course_ID=@Course_ID AND [Status]=1";

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            if (combinationSemester != 0)
            {
                sqlCommand.Parameters.AddWithValue("@Semester", combinationSemester);
                Qurery += " AND Semester=@Semester";
            }
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(sqlCommand);
        }


        public List<ADMSubjectMaster> GetList(Guid Course_ID, List<Guid> subject_IDs)
        {
            string Qurery = $@"SELECT S.*,DepartmentFullName FROM dbo.ADMSubjectMaster S
                            LEFT JOIN dbo.Department D ON D.Department_ID = S.Department_ID  
                            WHERE S.Subject_ID IN({subject_IDs.ToIN()}) ORDER BY SubjectFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            //sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(sqlCommand);
        }

        public List<ADMSubjectMasterCompact> List(Parameters parameter)
        {
            string Query = $@"SELECT ADMCollegeMaster.CollegeCode,S.Course_ID, CourseFullName ,Subject_ID , SubjectFullName ,SubjectCode ,DepartmentFullName,SubjectType,ST.TotalCredit , Semester,S.Status ,S.Programme
                                FROM ADMSubjectMaster S
                                LEFT JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID
                                LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                                LEFT JOIN Department ON Department.Department_ID = S.Department_ID
                                LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.College_ID";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMSubjectMasterCompact>(helper.Command);
        }

        public ADMSubjectMaster GetSubjectsCourse(Guid subject_ID)
        {
            string Query = $"SELECT Course_Id FROM ADMSubjectMaster WHERE Subject_ID IN('{subject_ID}') ORDER BY SubjectFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(sqlCommand);
        }

        public List<ADMSubjectMaster> GetAllSubjects(Parameters parameter)
        {
            string Query = $@"SELECT S.*,DepartmentFullName FROM VW_SubjectWithStructure S
                            LEFT JOIN Department ON Department.Department_ID = S.Department_ID ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(helper.Command);
        }


        //GetAllSubjects For Result         
        public List<ADMSubjectMaster> GetAllSubjectsSemester(string SearchText, int semester, PrintProgramme programme)
        {

            string Query = $@" SELECT DISTINCT CM.Subject_ID,SubjectFullName FROM dbo.ARGPersonalInformation_{programme} P
										   JOIN dbo.ARGSelectedCombination_{programme} Comb ON Comb.Student_ID = P.Student_ID
										   JOIN VWCombinationMaster  CM ON CM.Combination_ID = Comb.Combination_ID AND CM.semester = Comb.Semester
										   JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = CM.Subject_ID 
										   JOIN dbo.ARGStudentExamForm_{programme} Exm ON Exm.Student_ID = P.Student_ID										   
										   WHERE (CUSRegistrationNo = @SearchText OR  Exm.ExamRollNumber=@SearchText)  AND CM.semester = @semester  AND HasResult=1 ";

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@SearchText", SearchText);
            sqlCommand.Parameters.AddWithValue("@semester", semester);
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(sqlCommand);
        }

        public int AddSubject(ADMSubjectMaster input)
        {
            return new MSSQLFactory().InsertRecord<ADMSubjectMaster>(input);
        }

        public bool HasStudentsAssociated(Guid subject_ID)
        {
            var Query = $@"Select 1 From VWStudentWithDetail Where Subject_ID='{subject_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;
        }

        public bool HasSyllabus(Guid subject_ID)
        {
            var Query = $@"Select 1 From Syllabus Where Subject_ID='{subject_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;
        }

        public bool HasResult(Programme programme, short semester, Guid subject_ID)
        {
            var printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            var Query = $@"Select 1 From {printProgramme.ToString()}_Semester{semester} Where Subject_ID='{subject_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(Query) > 0;
        }

        public int Delete(Guid subject_ID)
        {
            var Query = $@"Delete From ADMSubjectMaster Where Subject_ID='{subject_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        public int UpdateSubjectStatus(bool Status, Guid Subject_ID)
        {
            int stst = Status ? 1 : 0;
            var Query = $@"Update ADMSubjectMaster Set Status={stst},UpdatedOn='{DateTime.Now}'  Where Subject_ID='{Subject_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(Query);
        }

        //public List<SubjectCombinations> GetCombinationList(Parameters parameter, string courseCode, string collegeCode)
        //{
        //    string Query = $@"SELECT [ColgCode]
        //                      ,[CourseCode]
        //                      ,[CombinationCode]
        //                      ,[FullCode]
        //                      ,[CourseCategory]
        //                      ,[Subject1] AS [Subject1Name]
        //                      ,[Subject2] AS [Subject2Name]
        //                      ,[Subject3] AS [Subject3Name]
        //                      ,[Subject4] AS [Subject4Name]
        //                      ,[Subject5] AS [Subject5Name]
        //                      ,[Subject6] AS [Subject6Name]
        //                      ,[Subject7] AS [Subject7Name]
        //                   FROM [SubjectCombinations] where CourseCode = @Course_Code and ColgCode = @College_code";
        //    SqlCommand sqlCommand = new SqlCommand();
        //    Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
        //    sqlCommand.CommandText = Query;
        //    sqlCommand.Parameters.AddWithValue("@Course_Code", courseCode);
        //    sqlCommand.Parameters.AddWithValue("@College_code", collegeCode);
        //    return new MSSQLFactory().GetObjectList<SubjectCombinations>(sqlCommand);
        //}

        //public ADMSubjectMaster GetSubjectById(Guid subject_ID)
        //{
        //    string Query = $@"SELECT S.Subject_ID,S.Course_ID,S.SubjectCode,S.Semester,s.SubjectFullName,S.SubjectType,ST.* FROM 
        //                       ADMSubjectMaster S JOIN MSSubjectMarksStructure ST ON ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID where Subject_ID = @Subject_ID";
        //    SqlCommand sqlCommand = new SqlCommand();
        //    sqlCommand.CommandText = Query;
        //    sqlCommand.Parameters.AddWithValue("@Subject_ID", subject_ID);
        //    return new MSSQLFactory().GetObject<ADMSubjectMaster>(sqlCommand);
        //}


        public ADMSubjectMaster GetOldSubjectById(Guid subject_ID)
        {
            string Query = $@"SELECT * FROM [Subjects] where Subject_ID = @Subject_ID";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            sqlCommand.Parameters.AddWithValue("@Subject_ID", subject_ID);
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(sqlCommand);
        }


        public ADMSubjectMaster ExistSubject(string columnName, string columnValue, ADMSubjectMaster adMSubjectMaster, bool IsEdit)
        {
            string Query = $@"SELECT Subject_ID  
                           FROM [ADMSubjectMaster] where  " + columnName + " = @" + columnName;
            if (columnName != "SubjectCode")
            {
                Query += $@" and Course_ID = @Course_ID and  Semester= @Semester ";
            }
            if (IsEdit)
            {
                Query += $@" and Subject_ID !='{adMSubjectMaster.Subject_ID}'";
            }
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            sqlCommand.Parameters.AddWithValue("@Semester", adMSubjectMaster.Semester);
            sqlCommand.Parameters.AddWithValue("@Course_ID", adMSubjectMaster.Course_ID);
            sqlCommand.Parameters.AddWithValue("@" + columnName, columnValue);
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(sqlCommand);
        }

        public int EditSubject(ADMSubjectMaster input, List<string> ignoreQuery, List<string> ignoreParameter)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<ADMSubjectMaster>(input, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE Subject_ID=@Subject_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);


        }

        public List<ADMSubjectMaster> GetList(List<Guid> subject_IDs)
        {
            string Qurery = $@"SELECT S.*,D.*,C.CourseFullName FROM VW_SubjectWithStructure S
                            LEFT JOIN dbo.Department D ON D.Department_ID = S.Department_ID
                            inner join ADMCourseMaster C on S.Course_ID=C.Course_ID
                            WHERE Subject_ID IN({subject_IDs.ToIN()}) ORDER BY SubjectFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(sqlCommand);
        }

        public List<ADMSubjectMaster> GetSubjectNamesTypeOnly(List<Guid> subject_IDs, List<SubjectType> subType)
        {
            string subjectTypeToCheck = subType.Count <= 0 ? "" : $"AND SubjectType IN({subType.Select(x => (short)x).EnumToIN()})";
            string Qurery = $"SELECT SubjectFullName,SubjectType FROM ADMSubjectMaster WHERE Subject_ID IN({subject_IDs.ToIN()}) " +
                $"{subjectTypeToCheck}" +
                $" ORDER BY SubjectType, SubjectFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(sqlCommand);
        }

        public List<DropDown> GetListDDL(Guid Course_ID, bool _withCompulsary)
        {
            // todo (SubjectCode+' - '+SubjectFullName)
            string Qurery = "SELECT Subject_ID AS [Value],SubjectFullName AS [Text] FROM ADMSubjectMaster WHERE Course_ID=@Course_ID AND [Status]=1";

            SqlCommand sqlCommand = new SqlCommand();
            if (!_withCompulsary)
            {
                Qurery += " AND IsCompulsory=@IsCompulsory";
                sqlCommand.Parameters.AddWithValue("@IsCompulsory", _withCompulsary);
            }

            sqlCommand.CommandText = Qurery + " ORDER BY SubjectFullName";
            sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            return new MSSQLFactory().GetObjectList<DropDown>(sqlCommand);
        }

        public List<Guid> GetSubjectIdentifiersFromSubjectCodes(string inClause)
        {
            return new MSSQLFactory().GetSingleValues<Guid>($"SELECT Subject_ID FROM ADMSubjectMaster WHERE SubjectCode IN ({inClause}) ");
        }

        public List<SelectListItem> DepartmentDataList(string SchoolFullName)
        {
            string Query = $@"SELECT CAST(Department_ID AS VARCHAR(100)) AS [Value],DepartmentFullName AS [Text] FROM Department " + (string.IsNullOrEmpty(SchoolFullName) ? "" : "Where SchoolFullName=@SchoolFullName");
            Query += " Order By DepartmentFullName";
            SqlCommand sqlCommand = new SqlCommand(Query);
            if (!string.IsNullOrEmpty(SchoolFullName))
                sqlCommand.Parameters.AddWithValue("@SchoolFullName", SchoolFullName ?? "");
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }
        public List<SelectListItem> SchoolSelectList(Guid? College_ID)
        {
            string Qurery = $@"SELECT DISTINCT Trim(SchoolFullName) AS Value,Trim(SchoolFullName) AS Text 
                                FROM ADMCourseMaster 
                                JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                                WHERE SchoolFullName IS NOT NULL {(College_ID.HasValue ? " AND College_ID='" + College_ID + "'" : "")}";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Qurery);
        }
        public List<DropDown> GetListDDL(Guid Course_ID, int Semester, bool _withCompulsary)
        {
            // todo (SubjectCode+' - '+SubjectFullName)
            string Qurery = "SELECT Subject_ID AS [Value],SubjectFullName AS [Text] FROM ADMSubjectMaster WHERE Course_ID=@Course_ID AND Semester=@Semester AND [Status]=1";

            SqlCommand sqlCommand = new SqlCommand();
            if (!_withCompulsary)
            {
                Qurery += " AND IsCompulsory=@IsCompulsory";
                sqlCommand.Parameters.AddWithValue("@IsCompulsory", _withCompulsary);
            }

            sqlCommand.CommandText = Qurery + " ORDER BY SubjectFullName";
            sqlCommand.Parameters.AddWithValue("@Course_ID", Course_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", Semester);
            return new MSSQLFactory().GetObjectList<DropDown>(sqlCommand);
        }

        public List<SelectListItem> SubjectDDL(Parameters parameters)
        {
            var helper = new FilterHelper();
            string Query = @"SELECT DISTINCT  CONVERT(NVARCHAR(50),[Subject_ID]) as [Value],[SubjectFullName] as [Text]
                                FROM ADMSubjectMaster
                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID
                                LEFT JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMSubjectMaster.Course_ID";
            if (parameters != null)
            {
                helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameters.Filters);
                Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            }
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(helper.Command);
        }

        public List<ADMSubjectMaster> GetSubjectByType(Guid Id, SubjectType SubjectType, short? semester)
        {
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new SubjectSQLQueries().GetSubjectByType(Id, SubjectType, semester));
        }

        public List<SubjectCompact> SubjectListComact(Parameters parameters)
        {
            string Query = $@"SELECT DISTINCT SubjectFullName,Subject_ID,SubjectType,Semester,DepartmentFullName,TotalCredit,FromBatch,ToBatch,CollegeCode,CourseFullName
                                FROM VW_SubjectWithStructure S
                                JOIN ADMCourseMaster C ON C.Course_ID=S.Course_ID
                                LEFT JOIN Department ON Department.Department_ID = S.Department_ID 
                                LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.College_ID";
            var helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SubjectCompact>(helper.Command);
        }
        public List<SubjectCompact> VWSCSubjectListComact(Parameters parameters)
        {
            string Query = $@"SELECT DISTINCT SubjectFullName,Subject_ID,SubjectType,Semester,DepartmentFullName,TotalCredit,CourseFullName,collegecode FROM VWSCMasterCompact";
            var helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameters.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameters);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SubjectCompact>(helper.Command);
        }
        public List<SelectListItem> GetAllSubjects(Guid Course_Id)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new SubjectSQLQueries().GetAllSubjects(Course_Id));
        }
        public List<SelectListItem> GetAllSubjects(Guid Course_Id, int Semester, bool checkcollege)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new SubjectSQLQueries().GetAllSubjects(Course_Id, Semester, checkcollege));
        }

        public bool SubjectNameAlreadyExists(Guid course_ID, Semester semester, string SubjectFullName, Guid? Subject_ID = null)
        {
            string Query = $@"SELECT 1  FROM ADMSubjectMaster Where Course_ID='{course_ID}' AND Semester={(short)semester} AND SubjectFullName=@SubjectFullName " + (Subject_ID.IsNullOrEmpty() ? "" : $" AND Subject_ID<>'{Subject_ID}'");
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@SubjectFullName", SubjectFullName);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }
        public bool SubjectNameAlreadyExists(Programme programme, Guid College_ID, Semester semester, string SubjectFullName, Guid? Subject_ID = null)
        {
            string Query = $@"SELECT 1  FROM ADMSubjectMaster Where Programme={(short)programme} AND College_ID='{College_ID}' AND Semester={(short)semester} AND SubjectFullName=@SubjectFullName " + (Subject_ID.IsNullOrEmpty() ? "" : $" AND Subject_ID<>'{Subject_ID}'");
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@SubjectFullName", SubjectFullName);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }
        public bool SubjectCodeAlreadyExists(string SubjectCode, Guid? Subject_ID = null)
        {
            string Query = $@"SELECT 1  FROM ADMSubjectMaster Where SubjectCode=@SubjectCode" + (Subject_ID.IsNullOrEmpty() ? "" : $" AND Subject_ID<>'{Subject_ID}'");
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@SubjectCode", SubjectCode);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }

        public string SubjectInUse(Programme programme, short Semester, Guid Subject_ID)
        {
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            string Query = $@"SELECT TOP 1 'Subject In Use: For Batch: '+ CAST(STDINFO.Batch AS VARCHAR(10)) +' AND Course: '+STDINFO.CourseFullName FROM 
                                VWStudentCourse STDINFO 
                                JOIN {printProgramme}_Semester{Semester.ToString()} Result ON  Result.Student_ID = STDINFO.Student_ID
                                WHERE Result.Subject_ID='{Subject_ID}' AND  Result.ResultNotification_ID IS NOT NULL";
            return new MSSQLFactory().ExecuteScalar<string>(Query);
        }

        public ADMSubjectMaster Get(Guid subject_ID)
        {
            string Query = $@"SELECT Department.SchoolFullName,S.Subject_ID ,	S.Course_ID ,	S.SubjectCode,	S.Semester ,S.SubjectFullName,	S.SubjectType,	S.Status,	S.College_ID,	S.IsCompulsory,	S.SubjectNumber,S.SubjectCapacitySet,S.SubjectCapacity,	S.Programme,S.Department_ID,S.FromBatch,S.ToBatch,S.HasResult,S.HasExaminationfee,ST.*,
                            CollegeFullName,CourseFullName,ADMCourseMaster.Programme,CourseCode,DepartmentFullName ,ADMCourseMaster.PrintProgramme,(SELECT TOP 1 MS.Marks_ID FROM MSStudentMarks MS WHERE   S.Subject_Id=MS.Subject_Id) Marks_ID
                            FROM ADMSubjectMaster S
                            LEFT JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID
                            LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                            LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.College_ID 
                            LEFT JOIN Department ON S.Department_ID=Department.Department_ID WHERE S.Subject_ID = '{subject_ID}'";
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(Query);
        }
        public ADMSubjectMaster Get(Programme programme, Guid? Course_ID, short Semester, SubjectType subjectType)
        {
            string Query = $@"SELECT top 1 COALESCE(ADMCourseMaster.SchoolFullName, Department.SchoolFullName)SchoolFullName,
                            ST.*,S.Subject_ID ,	S.Course_ID ,	S.SubjectCode,	S.Semester ,S.SubjectFullName,	S.SubjectType,	S.Status,	S.College_ID,	S.IsCompulsory,	S.SubjectNumber,S.SubjectCapacitySet,S.SubjectCapacity,	S.Programme,S.Department_ID,S.FromBatch,S.ToBatch,S.HasResult,S.HasExaminationfee,
                            CollegeFullName,CourseFullName,S.Programme,CourseCode,DepartmentFullName ,ADMCourseMaster.PrintProgramme
                            FROM ADMSubjectMaster S
                            JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID
                            LEFT JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                            LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.College_ID 
                            LEFT JOIN Department ON S.Department_ID=Department.Department_ID 
                            WHERE S.Programme ='{(short)programme}' {(Course_ID.HasValue ? "AND S.Course_ID='" + Course_ID + "'" : "")} AND Semester={Semester} AND SubjectType={(short)subjectType} ORDER BY S.UpdatedOn DESC,S.CreatedOn Desc";
            return new MSSQLFactory().GetObject<ADMSubjectMaster>(Query);
        }

        public int Save(ADMSubjectMaster model)
        {
            try
            {
                var ignoreList = typeof(MSSubjectMarksStructure).GetProperties().Where(x => x.DeclaringType == typeof(MSSubjectMarksStructure)).Select(x => x.Name).ToList();
                ignoreList.Remove("SubjectMarksStructure_ID");
                return new MSSQLFactory().InsertRecord<ADMSubjectMaster>(model, "ADMSubjectMaster", ignoreList);
            }
            catch (SqlException ex) when (ex.Number == 2601)
            { return -2601; }
        }

        public int Update(ADMSubjectMaster model)
        {
            try
            {
                List<string> ignoreQuery = new List<string>() { nameof(model.Subject_ID), nameof(model.CreatedOn), nameof(model.CreatedBy) };
                var ignoreList = typeof(MSSubjectMarksStructure).GetProperties().Where(x => x.DeclaringType == typeof(MSSubjectMarksStructure)).Select(x => x.Name).ToList();
                ignoreList.Remove("SubjectMarksStructure_ID");
                ignoreQuery.AddRange(ignoreList);
                SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<ADMSubjectMaster>(model, ignoreQuery, ignoreQuery, "ADMSubjectMaster");
                sqlCommand.CommandText += " WHERE Subject_ID=@Subject_ID";
                sqlCommand.Parameters.AddWithValue("@Subject_ID", model.Subject_ID);
                return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
            }
            catch (SqlException ex) when (ex.Number == 2601)
            { return -2601; }
        }

        public List<SelectListItem> GetAllSubjects(string CourseFullName, Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new SubjectSQLQueries().GetAllSubjects(CourseFullName, College_ID));
        }

        public List<SelectListItem> SubjectSelectList(Parameters parameter)
        {
            var Query = "SELECT convert(nvarchar(50),[Subject_ID]) as [Value],[SubjectFullName] as [Text] FROM [ADMSubjectMaster] ";
            FilterHelper filterHelper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            filterHelper.Command.CommandText = Query + filterHelper.WhereClause;
            return new MSSQLFactory().GetObjectList<SelectListItem>(filterHelper.Command);
        }

        public string GetSubjectCode(ADMSubjectMaster subject)
        {
            string Query = $@"SELECT TOP 1
            REPLACE(temp.SubjectCode, temp.Postfix,'')+''+CONVERT(NVARCHAR(250),(temp.Postfix+1)) FROM
            (SELECT CONVERT(INT, (REPLACE(SubjectCode, LEFT(SubjectCode, COALESCE(NULLIF(PATINDEX('%[0123456789]%', SubjectCode) - 1, -1), LEN(SubjectCode))),''))) AS Postfix, 
            SubjectCode FROM dbo.ADMSubjectMaster WHERE Course_ID = @Course_ID) AS temp ORDER BY Postfix DESC";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            sqlCommand.Parameters.AddWithValue("@Course_ID", subject.Course_ID);
            return new MSSQLFactory().ExecuteScalar<string>(sqlCommand);
        }

        public string GetSubjectNumber(Guid course_Id, int semester)
        {
            string Query = $@"SELECT TOP 1
             CONVERT(NVARCHAR(250),(SubjectNumber+1)) 
             FROM dbo.ADMSubjectMaster WHERE Course_ID = @Course_ID AND Semester=@Semester ORDER BY SubjectNumber DESC";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_Id);
            sqlCommand.Parameters.AddWithValue("@Semester", semester);
            return new MSSQLFactory().ExecuteScalar<string>(sqlCommand);
        }

        public int GetSubjectNumber(Guid? course_Id, int semester, SubjectType subjectType, Programme programme, Guid? College_ID = null)
        {
            string Query = $@"SELECT TOP 1 COALESCE( TRY_CONVERT(INT,SubjectNumber),0)
             FROM dbo.ADMSubjectMaster WHERE programme='{(short)programme}' AND SubjectType='{(short)subjectType}' {(course_Id.HasValue ? " AND Course_ID='" + course_Id + "'" : "")} {(College_ID.HasValue ? "AND College_ID='" + College_ID + "'" : "")} AND Semester={semester} ORDER BY SubjectNumber DESC";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) + 1;
        }


        #region SubjectInfo

        public List<SubjectDetails> SubjectInfo()
        {
            string query = $@"
							select coursecode,Coursefullname,SemesterName,subjectfullname,subjecttype,theoryAttendance,PracticalAttendence,TheoryMaxMarks,TheoryMinPassMarks,PracticalMaxMarks,PracticalMinPassMarks  from VW_SubjectWithStructure S inner join admcoursemaster on 
							admcoursemaster.course_ID=S.course_ID inner join syllabus on syllabus.Subject_ID=S.Subject_ID
							order by coursefullname,semesterName,subjectfullname";

            return new MSSQLFactory().GetObjectList<SubjectDetails>(query);
        }

        public List<ADMSubjectMaster> GetAllSubjectList(Parameters parameter)
        {
            string Query = $@"SELECT  S.Subject_ID ,	S.Course_ID ,	S.SubjectCode,	S.Semester ,S.SubjectFullName,	S.SubjectType,	S.Status,	S.College_ID,	S.IsCompulsory,	S.SubjectNumber,S.SubjectCapacitySet,S.SubjectCapacity,	S.Programme,S.Department_ID,S.FromBatch,S.ToBatch,S.HasResult,S.HasExaminationfee,
                        ST.*,DepartmentFullName,CollegeCode,CollegeFullName,CourseFullName
                            FROM ADMSubjectMaster S  
                            LEFT JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID
                            LEFT JOIN Department ON Department.Department_ID = S.Department_ID 
                            LEFT JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = S.College_ID";
            var helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            Query += helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(helper.Command);
        }

        public List<SubjectDetails> GetAllSubjectDetails(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<SubjectDetails>(new SubjectSQLQueries().GetAllSubjectDetails(parameter));
        }
        #endregion

        #region old
        public List<SelectListItem> GetOldCollegeSubjectList()
        {
            string Qurery = "SELECT CONVERT(nvarchar(50),[Subject_ID]) as [Value],[Name] as [Text] FROM Subjects Order By Name";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);

        }
        #endregion


        public List<ADMSubjectMaster> GetSkillSubjectsByCollege(Guid Id, int semester)
        {

            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(new SubjectSQLQueries().GetSkillSubjectsByCollege(Id, semester));
        }

        public List<SubjectCompact> GetSubjectsOf(Guid Course_ID, Guid College_ID, short semester, string programmes, string[] SubjectType)
        {
            string Query = $@"SELECT S.Subject_ID ,	S.Course_ID ,	S.SubjectCode,	S.Semester ,S.SubjectFullName,	S.SubjectType,	S.Status,	S.College_ID,	S.IsCompulsory,	S.SubjectNumber,S.SubjectCapacitySet,S.SubjectCapacity,	S.Programme,S.Department_ID,S.FromBatch,S.ToBatch,S.HasResult,S.HasExaminationfee,
                   ST.* FROM ADMSubjectMaster S JOIN MSSubjectMarksStructure ST ON  ST.SubjectMarksStructure_ID = S.SubjectMarksStructure_ID
           WHERE (College_ID='{College_ID}'  OR Course_ID='{Course_ID}') AND Semester={semester} AND Programme IN ({programmes})";
            Query += SubjectType != null && SubjectType.Length > 0 ? $"SubjectType IN ({string.Join(",", SubjectType)})" : "";
            return new MSSQLFactory().GetObjectList<SubjectCompact>(Query);
        }

        #region DeparmentSection
        public Department GetDepartment(Guid Department_ID)
        {
            string Query = $@"SELECT * FROM Department WHERE Department_ID = '{Department_ID}'";
            return new MSSQLFactory().GetObject<Department>(Query);
        }
        public Department GetDepartment(string SchoolFullName, string DepartmentFullName)
        {
            string Query = $@"SELECT * FROM Department WHERE SchoolFullName=@SchoolFullName AND DepartmentFullName=@DepartmentFullName";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@SchoolFullName", SchoolFullName);
            sqlCommand.Parameters.AddWithValue("@DepartmentFullName", DepartmentFullName);
            return new MSSQLFactory().GetObject<Department>(sqlCommand);
        }

        public int Save(Department model)
        {
            return new MSSQLFactory().InsertRecord<Department>(model, "Department");
        }

        public int Update(Department model)
        {
            List<string> ignoreQuery = new List<string>() { nameof(model.Department_ID) };
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<Department>(model, ignoreQuery, ignoreQuery, "Department_ID");
            sqlCommand.CommandText += " WHERE Department_ID=@Department_ID";
            sqlCommand.Parameters.AddWithValue("@Department_ID", model.Department_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        #endregion


        #region ADMSubjectMasterMarksStructure
        public List<MSSubjectMarksStructure> Structures(Parameters parameter)
        {
            string Query = $@"SELECT top 30 * FROM ADMSubjectMasterMarksStructure";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<MSSubjectMarksStructure>(helper.Command);
        }

        public List<ADMSubjectMaster> GetAdditionalSubjectNamesTypeOnly(Programme programme, short semester, List<SubjectType> subjectTypes, Guid Course_ID)
        {
            PrintProgramme _printprogramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);

            string programmeSQL = (programme == Programme.HS || programme == Programme.IG || programme == Programme.Professional)
                ? $" AND ADMCourseMaster.Programme IN({(short)Programme.HS},{(short)Programme.IG},{(short)Programme.Professional}) "
                : $" AND ADMCourseMaster.Programme IN({(short)programme}) ";

            string subjectTypeToCheck = subjectTypes.Count <= 0 ? "" : $" AND SubjectType IN({subjectTypes.Select(x => (short)x).EnumToIN()}) ";

            string Query = $@"SELECT DISTINCT
                                       SubjectFullName,
                                       SubjectType
                                FROM dbo.ARGStudentAdditionalSubjects_{_printprogramme} A
                                    LEFT JOIN dbo.ADMSubjectMaster
                                        ON ADMSubjectMaster.Subject_ID = A.Subject_ID
                                           AND A.Semester = {semester}
                                    LEFT JOIN dbo.ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID AND A.Course_ID='{Course_ID}'
                                    LEFT JOIN dbo.ARGPersonalInformation_{_printprogramme} P
                                        ON P.Student_ID = A.Student_ID
                                WHERE A.Semester = {semester} {subjectTypeToCheck} 
                                      AND A.IsVerified = 1
                                      AND P.IsPassout <> 1 {programmeSQL}
                                      AND P.FormStatus NOT IN ({(short)FormStatus.CancelRegistration}, {(short)FormStatus.AutoAdmissionCancel})
                                      AND P.AcceptCollege_ID IS NOT NULL AND A.Course_ID='{Course_ID}';";

            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(Query);
        }
        #endregion
    }
}
