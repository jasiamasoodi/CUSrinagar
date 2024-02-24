using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using System.Data.SqlClient;
using System;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class SubjectSQLQueries
    {
        internal SqlCommand GetSubjectById(Guid subject_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * FROM [ADMSubjectMaster]
                                     WHERE Subject_ID = @subject_Id";

            command.Parameters.AddWithValue("@Subject_ID", subject_Id);
            return command;
        }
        internal SqlCommand GetAllSubjects(Guid Course_Id)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT convert(nvarchar(50),[Subject_ID]) as [Value],[SubjectFullName] as [Text] FROM [ADMSubjectMaster]
 
                                     WHERE Course_ID = @Course_Id Order By SubjectFullName";

            command.Parameters.AddWithValue("@Course_Id", Course_Id);
            return command;
        }
        internal SqlCommand GetAllSubjects(Guid Course_Id, int Semester,bool checkcollege)
        {
            SqlCommand command = new SqlCommand();
            string query = @"SELECT convert(nvarchar(50),[Subject_ID]) as [Value],[SubjectFullName]+'-'+dbo.FNSubjectTypeDescription(SubjectType)+ISNULL((SELECT '-'+CM.CollegeCode FROM ADMCollegeMaster CM WHERE CM.College_ID = ADMSubjectMaster.College_ID),'') as [Text] 
                                                 FROM [ADMSubjectMaster] 
                                      WHERE Course_ID = @Course_Id  AND Semester=@Semester";
            if (AppUserHelper.College_ID != null && checkcollege)
            {
                query = query + $"   AND (College_ID = '{AppUserHelper.College_ID}' OR College_ID IS NULL)";
            }
            query = query + "  Order By SubjectFullName";
            command.CommandText = query;
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@Course_Id", Course_Id);
            return command;
        }
        internal SqlCommand GetAllSubjects(string CourseFullName, Guid College_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT convert(nvarchar(50),[Subject_ID]) as [Value],[SubjectFullName] as [Text] FROM [ADMSubjectMaster]
                                    INNER JOIN dbo.ADMCourseMaster 
                                    ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID
                                    WHERE REPLACE(Trim(CourseFullName),' ','') = @CourseFullName AND College_ID= @College_ID Order By SubjectFullName";

            command.Parameters.AddWithValue("@CourseFullName", CourseFullName);
            command.Parameters.AddWithValue("@College_Id", College_ID);
            return command;
        }

        #region SubjectInfo

        internal SqlCommand GetAllSubjectList(Parameters parameter)
        {
            string query = $@"select coursecode,Coursefullname,SemesterName,subjectfullname,subjecttype,theoryAttendance,PracticalAttendence,TheoryMaxMarks,TheoryMinPassMarks,PracticalMaxMarks,PracticalMinPassMarks  from admsubjectmaster inner join admcoursemaster on 
							admcoursemaster.course_ID=admsubjectmaster.course_ID inner join syllabus on syllabus.Subject_ID=admsubjectmaster.Subject_ID
							order by coursefullname,semesterName,subjectfullname";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
        // internal SqlCommand GetAllSubjectDetails(Parameters parameter)
        // {           

        //     SqlCommand command = new SqlCommand();
        //     command.CommandText = @"select distinct coursecode,Coursefullname,SemesterName,subjectfullname,subjecttype,theoryAttendance,PracticalAttendence,TheoryMaxMarks,TheoryMinPassMarks,PracticalMaxMarks,PracticalMinPassMarks  from admsubjectmaster inner join admcoursemaster on 
        //admcoursemaster.course_ID=admsubjectmaster.course_ID inner join syllabus on syllabus.Subject_ID=admsubjectmaster.Subject_ID
        //                     inner join ADMCollegeCourseMapping on ADMCollegeCourseMapping.Course_ID= ADMCourseMaster.Course_ID 							
        //where ADMCollegeCourseMapping.College_ID=@College_ID  and
        //admcoursemaster.Course_ID= @Course_ID 
        // order by coursefullname,SemesterName,subjectfullname";


        //     return command;
        // }


        internal SqlCommand GetAllSubjectDetails(Parameters parameter)
        {
            string query = $@"SELECT *
                                FROM
                                (
                                    SELECT DISTINCT
                                        CASE
                                            WHEN (ADMSubjectMaster.TheoryMaxMarks + ADMSubjectMaster.TheoryAttendance) <> 0 THEN
                                        (0.4 * (ADMSubjectMaster.TheoryMaxMarks + ADMSubjectMaster.TheoryAttendance))
                                            ELSE
                                                0
                                        END AS TheoryMinPassMarks,
                                        (SELECT DISTINCT
                                            CASE
                                                WHEN (ADMSubjectMaster.PracticalMaxMarks + ADMSubjectMaster.PracticalAttendence) <> 0 THEN
                                            (0.4 * (ADMSubjectMaster.PracticalMaxMarks + ADMSubjectMaster.PracticalAttendence))
                                                ELSE
                                                    0
                                            END )AS PracticalMinPassMarks,
                                        ADMCollegeCourseMapping.College_ID,
                                        ADMCourseMaster.Course_ID,
                                        ADMCourseMaster.CourseCode,
                                        ADMCourseMaster.CourseFullName,
                                        Syllabus.SemesterName,
                                        ADMSubjectMaster.SubjectFullName,
                                        ADMSubjectMaster.subjectType,
                                        ADMSubjectMaster.TheoryAttendance,
                                        ADMSubjectMaster.PracticalAttendence,
                                        (ADMSubjectMaster.TheoryMaxMarks + ADMSubjectMaster.TheoryAttendance) AS TheoryMaxMarks,
                                        (ADMSubjectMaster.PracticalMaxMarks + ADMSubjectMaster.PracticalAttendence) AS PracticalMaxMarks
                                      
                                    FROM ADMSubjectMaster
                                        INNER JOIN ADMCourseMaster
                                            ON ADMCourseMaster.Course_ID = ADMSubjectMaster.Course_ID AND ADMCourseMaster.Status=1
                                        INNER JOIN Syllabus
                                            ON Syllabus.Subject_ID = ADMSubjectMaster.Subject_ID AND ADMSubjectMaster.Status=1
                                        INNER JOIN ADMCollegeCourseMapping
                                            ON ADMCollegeCourseMapping.Course_ID = ADMCourseMaster.Course_ID AND ADMCollegeCourseMapping.Status = 1
                                ) AS tblSubject ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<SubjectDetails>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
        #endregion
        internal SqlCommand GetSkillSubjectsByCollege(Guid Id, int semester)
        {
            //CourseId For Skill Course '4A583C48-313C-4FEC-80B6-2BEC220E71E6'
            string query = @"SELECT * FROM ADMSubjectMaster
                                    WHERE ADMSubjectMaster.Course_ID='4A583C48-313C-4FEC-80B6-2BEC220E71E6' AND College_ID=@College_ID";

            SqlCommand command = new SqlCommand();
            if (semester > 0)
            {
                query = query + $"   AND Semester=@Semester";
                command.Parameters.AddWithValue("@Semester", semester);
            }
            command.CommandText = query + "  ORDER BY SubjectFullName";
            command.Parameters.AddWithValue("@College_ID", Id);
            return command;
        }

        internal SqlCommand GetSubjectByType(Guid Id, SubjectType subjectType,short? semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = @"SELECT * FROM ADMSubjectMaster
                                    WHERE ADMSubjectMaster.SubjectType=@SubjectType AND College_ID=@College_ID 
                                    AND Semester=@Semester
            ORDER BY SubjectFullName";
             command.Parameters.AddWithValue("@Semester", semester);
            command.Parameters.AddWithValue("@College_ID", Id);
            command.Parameters.AddWithValue("@SubjectType", subjectType);
            return command;
        }
    }
}
