using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;
using System.Web.Mvc;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class CourseDB
    {
        public List<ADMCourseMaster> GetCollegeCourseMapping(Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT ADMCourseMaster.* FROM ADMCollegeCourseMapping
	                                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
	                                                WHERE ADMCollegeCourseMapping.Status=1 AND College_ID='{College_ID}' ORDER BY CourseFullName ASC;");
        }
        public List<Guid> GetCourseGuidList(string CourseNameLike)
        {
            return new MSSQLFactory().GetSingleValues<Guid>($@"SELECT * FROM ADMCourseMaster JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                                                     WHERE CourseFullName LIKE '%{CourseNameLike}%' ORDER BY CourseFullName ASC;");
        }
        #region test

        public List<ARGCoursesApplied> GetEnglishStudentLists()
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>($@"SELECT * FROM ARGCoursesApplied_UG WHERE Course_ID='389A6FFB-DE48-40E7-8A46-FB8693216A3C' ");
        }

        #endregion
        public List<ADMCourseMaster> GetCourseLists(string CourseNameLike)
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT * FROM ADMCourseMaster JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                                                     WHERE CourseFullName LIKE '%{CourseNameLike}%' AND MeritBasislInTackCapacity>0  ORDER BY CourseFullName ASC;");
        }
        public List<ADMCourseMaster> GetCourseLists(Programme programme)
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT * FROM ADMCourseMaster JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                                                     WHERE Programme= {(short)programme} AND MeritBasislInTackCapacity>0 and ADMCourseMaster.HasCombination=1 and ADMCourseMaster.status=1  ORDER BY CourseFullName ASC;");
        }
        public List<ADMCourseMaster> GetUGCourseListsCollegeWise()
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT  ADMCollegeCourseMapping.Course_ID,College_ID,CourseFullName,CurrentSelectionListNo,MeritBasislInTackCapacity,AllowNewSelectionList  FROM ADMCourseMaster JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID WHERE PrintProgramme=1 AND StartingYear>=2022 and MeritBasislInTackCapacity>0 ORDER BY CourseFullName ASC");
        }
        public List<ADMCourseMaster> GetUGCourseLists()
        {
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>($@"SELECT  ADMCollegeCourseMapping.Course_ID,CourseFullName,CurrentSelectionListNo,SUM(MeritBasislInTackCapacity) MeritBasislInTackCapacity,AllowNewSelectionList  FROM ADMCourseMaster JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID WHERE PrintProgramme=1 AND StartingYear>=2022  and MeritBasislInTackCapacity>0  GROUP BY ADMCollegeCourseMapping.Course_ID,CourseFullName,CurrentSelectionListNo,AllowNewSelectionList  ORDER BY CourseFullName ASC");
        }
        public List<ARGCoursesApplied> GetCoursesAppliedMapping(Guid College_ID)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>($@"SELECT ADMCollegeCourseMapping.Course_ID,CourseFullName AS CourseName,CourseCode,Programme,PrintProgramme FROM ADMCollegeCourseMapping
	                                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
	                                                WHERE ADMCollegeCourseMapping.Status=1 AND College_ID='{College_ID}' AND AllowBOPEERegistration=1 ORDER BY CourseFullName ASC;");
        }
        public List<ARGCoursesApplied> GetCoursesAppliedMapping(Guid College_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>($@"SELECT ADMCollegeCourseMapping.Course_ID,CourseFullName AS CourseName,CourseCode,Programme,PrintProgramme FROM ADMCollegeCourseMapping
	                                                JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
	                                                WHERE ADMCollegeCourseMapping.Status=1 AND College_ID='{College_ID}' AND ADMCourseMaster.PrintProgramme={(short)printProgramme} ORDER BY CourseFullName ASC;");
        }
        public Guid GetStudentCourse_ID(string CUSRegistrationNoORStudentFormNo, PrintProgramme programme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT TOP 1 cm.Course_ID FROM ARGPersonalInformation_{programme.ToString()} p
                                    JOIN ARGSelectedCombination_{programme.ToString()} sc ON sc.Student_ID = p.Student_ID AND sc.Semester=1
                                    JOIN ADMCombinationMaster cm ON cm.Combination_ID = sc.Combination_ID
                                    WHERE (p.CUSRegistrationNo=@CUSRegistrationNoORStudentFormNo OR p.StudentFormNo=@CUSRegistrationNoORStudentFormNo)";

            command.Parameters.AddWithValue("@CUSRegistrationNoORStudentFormNo", CUSRegistrationNoORStudentFormNo);
            return new MSSQLFactory().ExecuteScalar<Guid>(command);
        }

        public ADMCourseMaster GetItem(Guid Course_ID)
        {
            string Qurery = $"SELECT * FROM ADMCourseMaster WHERE Course_ID='{Course_ID}'";
            return new MSSQLFactory().GetObject<ADMCourseMaster>(Qurery);
        }

        public ADMCourseMaster GetCourseProgramme(Guid course_Id)
        {
            string Query = $"SELECT Programme FROM ADMCourseMaster WHERE Course_ID IN('{course_Id}') ORDER BY CourseFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObject<ADMCourseMaster>(sqlCommand);
        }

        public List<SelectListItem> GetCourseList(Guid College_ID)
        {
            //string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 AND Course_ID IN(
            //                SELECT distinct Course_ID FROM ADMCombinationMaster WHERE College_ID = @College_ID and[Status] = 1)order by CourseFullName";
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 AND Course_ID IN(
                            SELECT distinct Course_ID FROM ADMCombinationMaster WHERE  [Status] = 1)order by CourseFullName";

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@College_ID", College_ID);
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }

        public List<SelectListItem> GetCourseListNEP(Guid College_ID)
        {
            //string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 AND Course_ID IN(
            //                SELECT distinct Course_ID FROM ADMCombinationMaster WHERE College_ID = @College_ID and[Status] = 1)order by CourseFullName";
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 
						     order by CourseFullName";

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@College_ID", College_ID);
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }
        public SelectListItem GetSkillCourse()
        {
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where  Course_ID='4A583C48-313C-4FEC-80B6-2BEC220E71E6'";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObject<SelectListItem>(sqlCommand);
        }

        public List<SelectListItem> GetOtherSubjectCourse()
        {
            string Query = $@"SELECT  convert(nvarchar(120),Course_ID) AS Value,
                                (CourseFullName)            AS Text  
                     FROM ADMCourseMaster  
                     WHERE  CourseFullName like '%generic elective%' or CourseFullName like '%skill enhancement%'";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }
        public List<SelectListItem> GetCourseList()
        {
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 AND Course_ID IN(
                            SELECT distinct Course_ID FROM ADMCombinationMaster WHERE [Status] = 1)order by CourseFullName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public List<SelectListItem> GetAllCourseList()
        {
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 order by CourseFullName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public List<SelectListItem> GetCourseList(Guid College_ID, PrintProgramme printProgramme)
        {
            string prog = "";
            if (printProgramme == PrintProgramme.BED)
            {
                prog = $"={(short)PrintProgramme.BED}";
            }
            else
            {
                printProgramme = new GeneralFunctions().MappingTable(printProgramme);
                prog = $"={(short)printProgramme}";
            }

            string Query = $@"SELECT DISTINCT CONVERT(nvarchar(50),ADMCourseMaster.Course_ID) AS Value,CourseFullName AS Text,Programme FROM ADMCourseMaster
                            LEFT JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                            WHERE 1=1 AND ADMCollegeCourseMapping.College_ID='{College_ID}'
                            AND ADMCourseMaster.PrintProgramme{prog}
                            ORDER BY Programme,CourseFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }

        public List<SelectListItem> GetCourseList(Guid? College_ID, PrintProgramme? printProgramme, Programme? programme)
        {

            Guid ClgId = Guid.Parse("9d03a374-4398-4a48-be2a-fd9911ec6f82");  //For GCET College           
            if (College_ID == ClgId)
            {
                programme = Programme.Engineering;
            }

            string Query = @"SELECT DISTINCT CONVERT(nvarchar(50),ADMCourseMaster.Course_ID) AS Value,CourseFullName AS Text,Programme FROM ADMCourseMaster
                            LEFT JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                            WHERE 1=1" + ((College_ID != null && College_ID.Value != Guid.Empty) ? $" AND ADMCollegeCourseMapping.College_ID='{College_ID.Value}'" : "")
                            + (printProgramme.HasValue ? $" AND ADMCourseMaster.PrintProgramme={(short)printProgramme.Value}" : "")
                            + (programme.HasValue ? $" AND ADMCourseMaster.Programme={(short)programme.Value}" : "") + " ORDER BY Programme,CourseFullName";

            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }

        public string GetCourses(string courses)
        {
            courses = courses.Replace(",", "','");
            string query = $@"SELECT Stuff(
                                  (SELECT N', ' + CourseFullName FROM dbo.ADMCourseMaster  WHERE course_ID IN ('{courses}') FOR XML PATH(''), TYPE)
                                  .value('text()[1]', 'nvarchar(max)'),1,2,N'') ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<string>(cmd);
        }


        public List<SelectListItem> GetCollegeList(Guid? Course_ID, PrintProgramme? printProgramme, Programme? programme)
        {
            string Query = @"SELECT DISTINCT CONVERT(nvarchar(50),ADMCollegeMaster.College_ID) AS Value,ADMCollegeMaster.CollegeFullName AS Text
                            FROM ADMCollegeMaster
                            JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.College_ID = ADMCollegeMaster.College_ID
                            JOIN ADMCourseMaster ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                            WHERE ADMCollegeCourseMapping.Status=1 " + (Course_ID.HasValue ? $"AND  ADMCollegeCourseMapping.Course_ID='{Course_ID.Value}'" : "") + (printProgramme.HasValue ? $" AND ADMCourseMaster.PrintProgramme={(short)printProgramme.Value}" : "") + (programme.HasValue ? $" AND ADMCourseMaster.Programme={(short)programme.Value}" : "") + " ORDER BY CollegeFullName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(Query);
        }
        public List<ARGCoursesApplied> GetCourseListForRegistration(PrintProgramme programme)
        {
            string Query = @"SELECT Course_ID,CourseFullName AS CourseName,Programme,CourseCode,GroupName FROM ADMCourseMaster WHERE [Status]=1 AND RegistrationOpen=1 ";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@PrintProgramme", programme);
            Query += $" AND PrintProgramme=@PrintProgramme ORDER BY GroupName,Programme,CourseFullName";
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(sqlCommand);
        }

        public List<ARGCoursesApplied> GetCourseListForAdminRegistration(PrintProgramme programme)
        {
            string Query = @"SELECT Course_ID,CourseFullName AS CourseName,Programme FROM ADMCourseMaster WHERE [Status]=1 AND HasCombination>0";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@PrintProgramme", programme);
            Query += $" AND PrintProgramme=@PrintProgramme ORDER BY CourseFullName";
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(sqlCommand);
        }

        public List<ARGCoursesApplied> GetCourseListForAdminRegistrationCollegeWise(PrintProgramme programme, Guid College_ID)
        {
            string Query = $@"SELECT DISTINCT ADMCollegeCourseMapping.Course_ID,
                                   CourseFullName AS CourseName,
                                   Programme
                            FROM ADMCourseMaster
                                JOIN dbo.ADMCollegeCourseMapping
                                    ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID
                            WHERE ADMCourseMaster.Status = 1
	                              AND AllowBOPEERegistration=1
                                  AND ADMCollegeCourseMapping.Status = 1 AND College_ID='{College_ID}' ";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@PrintProgramme", programme);
            Query += $" AND PrintProgramme=@PrintProgramme ORDER BY CourseFullName";
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(sqlCommand);
        }

        public List<PrintProgramme> GetClosedResistrationProgrammeCategories()
        {
            List<PrintProgramme> programs = null;
            string Query = $@"SELECT DISTINCT abc.PrintProgramme AS pp FROM  ADMCourseMaster abc
                            WHERE  abc.RegistrationOpen = 1 AND abc.Course_ID<>'FC32E138-4EE2-4DA2-9453-5C8368180BC3'";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            List<short> list = new MSSQLFactory().GetSingleValues<short>(sqlCommand);
            if (list.IsNotNullOrEmpty())
            {
                programs = new List<PrintProgramme>();
                foreach (var item in list)
                    programs.Add((PrintProgramme)Enum.Parse(typeof(PrintProgramme), item.ToString()));
            }
            return programs;
        }

        public PrintProgramme? GetClosedResistrationProgrammeCategoriesIBEdMEd()
        {
            PrintProgramme? programs = null;
            string Query = $@"SELECT DISTINCT abc.PrintProgramme AS pp FROM  ADMCourseMaster abc
                            WHERE  abc.RegistrationOpen = 1 AND abc.Course_ID='FC32E138-4EE2-4DA2-9453-5C8368180BC3'";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            short list = new MSSQLFactory().ExecuteScalar<short>(sqlCommand);
            if (list > 0)
                programs = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), list.ToString());
            return programs;
        }




        public List<ADMCollegeCourseMapping> GetCourseCollege(Guid course_Id)
        {
            string query = string.Empty;
            SqlCommand sqlCommand = new SqlCommand();

            query = @"Select * FROM ADMCollegeCourseMapping  WHERE Course_ID = @Course_ID";
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_Id);

            sqlCommand.CommandText = query;
            return new MSSQLFactory().GetObjectList<ADMCollegeCourseMapping>(sqlCommand);
        }



        public List<ADMCourseMaster> GetOfferedCoursesForDisplay()
        {
            string query = $@"SELECT ADMCollegeCourseMapping.Course_ID,CollegeFullName AS CourseCode,CourseFullName,Duration,
                            ADMCollegeMaster.SchoolFullName,Scheme,MeritBasislInTackCapacity AS MinCombinationSubjects,SelfFinancedInTackCapacity AS MaxCombinationSubjects,Programme,PrintProgramme  
                            FROM ADMCollegeCourseMapping
                            JOIN ADMCollegeMaster ON ADMCollegeMaster.College_ID = ADMCollegeCourseMapping.College_ID
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID
                            WHERE (PrintProgramme IN({(short)PrintProgramme.IH},{(short)PrintProgramme.BED}) AND ADMCollegeCourseMapping.Status=1) OR ADMCollegeCourseMapping.Course_ID='c23dd7f4-a933-4deb-ba8e-f933830bccf8' ORDER BY CollegeFullName";

            return new MSSQLFactory().GetObjectList<ADMCourseMaster>(query);
        }

        public List<short> GetProgrammes(Guid? College_ID)
        {
            var Query = $@"SELECT DISTINCT Programme FROM ADMCourseMaster
                                    JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID 
                                    WHERE ADMCourseMaster.Status=1 AND Programme IS NOT NULL  {(College_ID.HasValue ? $" AND College_ID='{College_ID.Value}'" : "")}";
            return new MSSQLFactory().GetSingleValues<short>(Query);
        }

        public int AddCollegeCourse(ADMCourseMaster input)
        {
            int r = 0;
            foreach (ADMCollegeCourseMapping mapping in input.CourseMappingList)
            {
                mapping.Course_ID = input.Course_ID;
                mapping.Status = true;
                r += new MSSQLFactory().InsertRecord<ADMCollegeCourseMapping>(mapping);
            }
            return r;
        }



        public int DeleteAlreadyAssignedColleges(Guid course_ID)
        {
            string query = string.Empty;
            SqlCommand sqlCommand = new SqlCommand();

            query = @"Delete FROM ADMCollegeCourseMapping  WHERE Course_ID = @Course_ID";
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);

            sqlCommand.CommandText = query;
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<AwardForSubjects> GetSubjectsForCourseAndCollege(Guid college_ID, Guid course_ID, PrintProgramme printProgramme, short semester, short batch)
        {
            string InformationTableName = new GeneralFunctions().GetTableName(printProgramme, Module.PersonalInformation);
            string combinationTableName = new GeneralFunctions().GetTableName(printProgramme, Module.SelectedCombination);
            string resultTableName = new GeneralFunctions().GetTableName(printProgramme, Module.Result);
            resultTableName += semester;


            string query = $@" SELECT SubjectFullName,COUNT(Awards.Student_ID) AS TotalNumberOfStudents, SUM(Awards.InternalSubmitted) InternalSubmitted, SUM(Awards.ExternalSubmitted) ExternalSubmitted, SUM(Awards.InternalEntered) InternalRecieved, SUM(Awards.ExternalEntered) ExternalRecieved,
                                COUNT(Awards.Student_ID) - SUM(Awards.InternalSubmitted) InternalNotSubmitted,COUNT(Awards.Student_ID) - SUM(Awards.ExternalSubmitted) ExternalNotSubmitted
                                    FROM
                                    (
                                        SELECT S.SubjectFullName, Combination.Student_ID,
                                               CAST(ISNULL(Result.InternalSubmitted, 0) AS SMALLINT) InternalSubmitted,
                                               CAST(ISNULL(Result.ExternalSubmitted, 0) AS SMALLINT) ExternalSubmitted
											   ,
                                              CASE WHEN (Result.TotalInternalMarks>0) THEN 1 ELSE 0 end InternalEntered,
                                              CASE WHEN (Result.TotalExternalMarks>0) THEN 1 ELSE 0 end ExternalEntered
                                        FROM  {InformationTableName} StudentInfo
                                            INNER JOIN {combinationTableName} Combination
                                                ON Combination.Student_ID = StudentInfo.Student_ID
                                                   AND Combination.Semester =  {semester}
                                            INNER JOIN VWSCMaster
                                                ON VWSCMaster.Combination_ID = Combination.Combination_ID
                                                   AND VWSCMaster.semester = Combination.Semester
                                            LEFT JOIN dbo.VWStudentResultAllSemesters Result
                                                ON Result.Student_ID = StudentInfo.Student_ID
                                                   AND VWSCMaster.Subject_ID = Result.Subject_ID
                                            INNER JOIN VW_SubjectWithStructure S
                                                ON S.Subject_ID = VWSCMaster.Subject_ID
                                        WHERE VWSCMaster.Course_ID = @Course_ID
                                              AND VWSCMaster.College_ID = @College_ID
                                              AND Batch = @Batch
                                    ) Awards
                                     GROUP BY Awards.SubjectFullName
                                     ORDER BY InternalSubmitted,ExternalSubmitted ";

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@Course_ID", course_ID);
            sqlCommand.Parameters.AddWithValue("@College_ID", college_ID);
            sqlCommand.Parameters.AddWithValue("@Batch", batch);
            sqlCommand.CommandText = query;

            return new MSSQLFactory().GetObjectList<AwardForSubjects>(sqlCommand);
        }

        //public static List<SelectListItem> OldCourseList(string CollegeCode, string Category)
        //{
        //    string query = string.Empty;
        //    SqlCommand sqlCommand = new SqlCommand(); query = @"SELECT CourseCode AS VALUE,DegreeName AS TEXT FROM COURSES  WHERE Clgcode = @CollegeCode AND Category=@Category";
        //    sqlCommand.Parameters.AddWithValue("@CollegeCode", CollegeCode);
        //    sqlCommand.Parameters.AddWithValue("@Category", Category);
        //    sqlCommand.CommandText = query;
        //    return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        //}
        //public static List<SelectListItem> OldCollegeCourseSubjectList(string CollegeCode, string Category, string CourseCode)
        //{
        //    string query = string.Empty;
        //    SqlCommand sqlCommand = new SqlCommand(); query = @"SELECT convert(nvarchar(120),Subject_ID) AS Value,SubjectID AS Text  FROM VWSC WHERE ColgCode=@ColgCode AND CourseCategory=@CourseCategory AND CourseCode=@CourseCode";
        //    sqlCommand.Parameters.AddWithValue("@ColgCode", CollegeCode);
        //    sqlCommand.Parameters.AddWithValue("@CourseCategory", Category);
        //    sqlCommand.Parameters.AddWithValue("@CourseCode", CourseCode);
        //    sqlCommand.CommandText = query;
        //    return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        //}
        //public static List<SelectListItem> OldCourseList()
        //{
        //    return new MSSQLFactory().GetObjectList<SelectListItem>(new CourseSQLQueries().GetOldCourseList);
        //}

        public int UpdateSelectionList(ADMCourseMaster model)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = $@"Update ADMCOURSEMaster SET
                AllowNewSelectionList=1,
                CurrentSelectionListNo =(CurrentSelectionListNo+1),
                UpdatedBy='{AppUserHelper.User_ID}',
                UpdatedON='{DateTime.Now}'
                WHERE Course_ID=@Course_ID";
            sqlCommand.Parameters.AddWithValue("@Course_ID", model.Course_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<ADMCourseMaster> GetAllCourseList(Guid College_ID)
        {
            string Query = @"SELECT  *  FROM ADMCourseMaster  WHERE  [Status]=1 AND Course_ID IN(
                            SELECT distinct Course_ID FROM ADMCombinationMaster WHERE College_ID = @College_ID and[Status] = 1)";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Parameters.AddWithValue("@College_ID", College_ID);
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>(sqlCommand);
        }

        public List<SelectListItem> CoursesSelectListItem()
        {
            //SqlCommand command = new SqlCommand();
            string query = $@"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster] ORDER BY Programme,CourseFUllName";
            return new MSSQLFactory().GetObjectList<SelectListItem>(query);
        }

        public List<SelectListItem> SelectList(Parameters parameter)
        {
            string query = $@"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMCourseMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(helper.Command);
        }

        public List<SelectListItem> SelectListDept(Parameters parameter)
        {
            string query = $@"SELECT convert(nvarchar(50),[Department_ID]) as [Value],[DepartmentFullName] as [Text] from [Department]";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMCourseMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(helper.Command);
        }


        public List<SelectListItem> GetAllCoursesByProgramme(int programmeId, short? Status = null, bool checkHasCom = false, bool checkSelectionlistAllowed = false)
        {
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]
                                     WHERE {(Status.HasValue ? "Status=" + Status.ToString() + " AND " : "")}  Programme = @ProgrammeId";

            if (checkHasCom)
            { query = query + " AND (HasCombination !=NULL OR HasCombination>0)"; }

            command.CommandText = query + "  order by [CourseFullName]";
            command.Parameters.AddWithValue("@ProgrammeId", programmeId);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }

        public List<SelectListItem> GetAllCoursesByPrintProgramme(PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]
                                     WHERE PrintProgramme = @printProgramme order by [CourseFullName]";

            command.Parameters.AddWithValue("@printProgramme", (short)printProgramme);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }
        public List<SelectListItem> GetAllCoursesByProgramme(Programme Programme)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT convert(nvarchar(50),[Course_ID]) as [Value],[CourseFullName] as [Text] from [ADMCourseMaster]
                                     WHERE Programme = @Programme order by [CourseFullName]";

            command.Parameters.AddWithValue("@Programme", (short)Programme);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);
        }

        public ADMCourseMaster GetCourseById(Guid course_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * from [ADMCourseMaster]
                                     WHERE Course_ID = @course_Id";

            command.Parameters.AddWithValue("@course_Id", course_Id);
            return new MSSQLFactory().GetObject<ADMCourseMaster>(command);
        }

        public List<ADMCourseMaster> GetAllCourseListOfAllColleges(Parameters parameter)
        {
            string query = @"SELECT  *  FROM ADMCourseMaster ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMCourseMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<ADMCourseMaster>(helper.Command);
        }

        public List<CourseList> List(Parameters parameter)
        {
            string query = $@"SELECT ADMCourseMaster.Course_ID,CollegeFullName,CourseFullName,Duration,MeritBasislInTackCapacity
                            FROM ADMCollegeMaster
                            JOIN ADMCollegeCourseMapping ON ADMCollegeCourseMapping.College_ID = ADMCollegeMaster.College_ID and ADMCollegeCourseMapping.status=1
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCollegeCourseMapping.Course_ID";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMCourseMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<CourseList>(helper.Command);
        }
        public int AddCourse(ADMCourseMaster input)
        {
            return new MSSQLFactory().InsertRecord<ADMCourseMaster>(input);
        }
        public int EditCourse(ADMCourseMaster input, List<string> ignoreQuery, List<string> ignoreParameter)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<ADMCourseMaster>(input, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE Course_ID=@Course_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);


        }
        public Guid GetCollegefromCourseMapping(Guid Student_ID, Guid Course_Id, PrintProgramme printProgramme)
        {
            string query = $@"SELECT TOP 1 CC.College_ID FROM NEPCollegePreferences  SCP join ADMCollegeCourseMapping CC ON SCP.College_ID = CC.College_ID  WHERE 
                                 CC.Course_ID='{Course_Id}'  AND Student_ID='{Student_ID}' AND (SELECT COUNT(Student_Id) FROM SeatAllocationMatrix_{printProgramme.GetTablePFix()}
                               WHERE Course_IdAssigned='{Course_Id}' AND College_IdAssigned=cc.College_ID)<cc.MeritBasislInTackCapacity ORDER BY PreferenceNo ASC";
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<Guid>(command);
        }
        public Guid GetCourseForIG(PrintProgramme chPrintProgramme, Guid course_ID)
        {
            string query = $@"Select * from AdmCourseMaster where Course_ID <>'{course_ID}' AND Coursefullname like'%'+(
                              Select Trim(Replace(Coursefullname,'Integrated','')) from AdmCourseMaster where Course_ID='{course_ID}') +'%'  AND Coursefullname Not like'%-'+(
                              Select Trim(Replace(Coursefullname,'Integrated','')) from AdmCourseMaster where Course_ID='{course_ID}') +'%' and PrintProgramme={(int)chPrintProgramme}";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().ExecuteScalar<Guid>(cmd);
        }
        #region Evalvator
        public List<SelectListItem> GetAllCourseListOfAllColleges()
        {
            string Query = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  where [Status]=1 ORDER BY CourseFullName";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }
        #endregion


        #region Subject Info Page
        public List<SelectListItem> GetAllCourseListByCollegeID(Guid College_ID)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @"SELECT  convert(nvarchar(120),Course_ID) AS Value,CourseFullName AS Text  FROM ADMCourseMaster  WHERE  [Status]=1 AND Course_ID IN(
                            SELECT distinct Course_ID FROM ADMCombinationMaster WHERE College_ID = @College_ID and[Status] = 1) ORDER BY CourseFullName"
            };

            command.Parameters.AddWithValue("@College_ID", College_ID);
            return new MSSQLFactory().GetObjectList<SelectListItem>(command);

        }

        public List<CourseCollegeWidget> GetAllCourseListByCollegeID(Guid College_ID, PrintProgramme? printProgramme)
        {
            string printProgrammeQuery = string.Empty;

            if (printProgramme != null)
                printProgrammeQuery = $"AND PrintProgramme = {(short)printProgramme}";
            SqlCommand command = new SqlCommand
            {
                CommandText = $@"SELECT Course_ID,CourseFullName,PrintProgramme  FROM ADMCourseMaster  WHERE  [Status]=1 AND Course_ID IN(
                                        SELECT distinct Course_ID FROM ADMCombinationMaster WHERE College_ID = @College_ID and[Status] = 1 )
                                    {printProgrammeQuery}
                                    ORDER BY Programme,CourseFullName "
            };
            command.Parameters.AddWithValue("@College_ID", College_ID);
            return new MSSQLFactory().GetObjectList<CourseCollegeWidget>(command);

        }

        public List<ADMCollegeCourseMapping> GetCollegeCourseMappingList(Guid College_ID)
        {
            SqlCommand command = new SqlCommand
            {
                CommandText = @"SELECT DISTINCT  * FROM dbo.ADMCollegeCourseMapping WHERE College_ID=@College_ID AND Status=1"
            };

            command.Parameters.AddWithValue("@College_ID", College_ID);
            return new MSSQLFactory().GetObjectList<ADMCollegeCourseMapping>(command);
        }

        public IEnumerable<SelectListItem> GetCoursesForSyllabus(List<Programme> programmes)
        {
            string SqlQuery = $@"SELECT CAST(ADMCourseMaster.Course_ID AS VARCHAR(50)) AS [Value],CourseFullName AS [Text] FROM dbo.ADMCourseMaster
                                    LEFT JOIN dbo.Syllabus ON Syllabus.Course_ID = ADMCourseMaster.Course_ID
                                    WHERE ADMCourseMaster.Status=1 AND HasCombination=1 AND StartingYear<2022 and Programme IN({programmes.Select(x => (short)x).ToIN()})
                                    ORDER BY CourseFullName,Programme ASC";

            if (programmes.Any(x => x != Programme.UG))
            {
                SqlQuery = $@"SELECT CAST(ADMCourseMaster.Course_ID AS VARCHAR(50)) AS [Value],CourseFullName AS [Text] FROM dbo.ADMCourseMaster
                                    LEFT JOIN dbo.Syllabus ON Syllabus.Course_ID = ADMCourseMaster.Course_ID
                                    WHERE ADMCourseMaster.Status=1 AND HasCombination=1 and Programme IN({programmes.Select(x => (short)x).ToIN()})
                                    ORDER BY CourseFullName,Programme ASC";

            }

            return new MSSQLFactory().GetObjectList<SelectListItem>(SqlQuery);
        }

        public List<Guid> GetSkillGECourseIDsByProgramme(Programme programme)
        {
            string sqlquery = $@"SELECT DISTINCT Course_ID  FROM dbo.ADMCourseMaster 
                                WHERE (CourseFullName LIKE 'skill enhance%' OR CourseFullName LIKE 'Generic Elective%')
                                AND Programme={(short)programme}";
            return new MSSQLFactory().GetSingleValues<Guid>(sqlquery);
        }

        public List<Guid> GetCUETCourse(string courseNamePart, Programme programme)
        {
            string sqlquery = $@"SELECT DISTINCT Course_ID,CourseFullName FROM dbo.ADMCourseMaster WHERE CourseFullName LIKE '%{courseNamePart}%'
                                 AND Programme={(short)programme} AND HasCombination=1 AND Status=1 AND StartingYear>=2022";
            return new MSSQLFactory().GetSingleValues<Guid>(sqlquery);
        }

        #endregion
    }
}
