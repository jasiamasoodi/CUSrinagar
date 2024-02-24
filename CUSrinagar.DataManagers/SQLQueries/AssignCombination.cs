using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class AssignCombinationSQLQueries
    {
        internal static SqlCommand GetStudentsList(Parameters parameters)
        {
            var Subject = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("SUBJECT_ID"));
            var semester = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("SEMESTER"));
            var batch = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("BATCH")).Value;
            var programme = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));
            var combination = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("COMBINATION"));
            var Subject_Id = (Subject != null) ? Subject.Value : "";
            var combinationId = (combination != null) ? combination.Value : "";
            var course = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("COURSE"));
            var courseId = (course != null) ? course.Value : "";
            var semesterId = (semester != null) ? semester.Value : "";
            var collegeId = AppUserHelper.College_ID;
            if (programme == null)
                programme.Value = "IG";
            SqlCommand command = new SqlCommand();
            string tableName = "ARGPersonalInformation";
            PrintProgramme ProgrammeEnum = (PrintProgramme)Convert.ToInt16(programme.Value);
            string ProgrammeName = new GeneralFunctions().GetProgrammePostFix(ProgrammeEnum);
            tableName = tableName + ProgrammeName;
            string ExamName = GetExamName(ProgrammeEnum);
            string query = $@" SELECT DISTINCT
                                {tableName}.CUSRegistrationNo 
                                ,{tableName}.StudentFormNo 
                                ,BoardRegistrationNo
                                ,{tableName}.Student_ID
                                ,{tableName}.FullName
                                ,{tableName}.FathersName
                                ,((MarksObt / MaxMarks) * 100) AS Percentage
                                , Mobile
                                ,Category,{tableName}.ClassRollNo
                                , ADMCombinationMaster.CombinationCode
                                ,PermanentAddress,District,PinCode
                                FROM {tableName} LEFT JOIN ARGStudentPreviousQualifications{ProgrammeName}
                                ON {tableName}.Student_ID = ARGStudentPreviousQualifications{ProgrammeName}.Student_ID
                                and ExamName ='{ExamName}'
                                INNER JOIN ARGStudentAddress{ProgrammeName}
                                ON {tableName}.Student_ID = ARGStudentAddress{ProgrammeName}.Student_ID
                                INNER JOIN dbo.ARGSelectedCombination{ProgrammeName} SelComb   ON ARGPersonalInformation{ProgrammeName}.Student_ID = SelComb.Student_ID 
                                INNER join ADMCombinationMaster on ADMCombinationMaster.Combination_ID=SelComb.Combination_ID
                                AND ADMCombinationMaster.CombinationSubjects LIKE '%{Subject_Id}%'
                                WHERE ARGPersonalInformation{ProgrammeName}.AcceptCollege_ID = '{collegeId}'";
            query = (batch != null) ? (query + $@" AND ARGPersonalInformation{ProgrammeName}.Batch= '{batch}'") : query;
            query = (course != null) ? (query + $@" AND ADMCombinationMaster.Course_ID = '{courseId}'") : query;
            query = (combination != null) ? (query + $@" AND SelComb.Combination_ID = '{combinationId}'") : query;
            query = (semester != null) ? (query + $@" AND SelComb.Semester = {semesterId}") : query;
            query += $" AND FormStatus IN ({(int)FormStatus.Accepted},{(int)FormStatus.Selected},{(int)FormStatus.Submitted})";
            query += $" AND ADMCombinationMaster.Semester = {semesterId}";
            if (parameters.PageInfo == null)
            {
                command.CommandText = query + " ORDER BY Percentage ASC;";
            }
            else
            {
                query += new GeneralFunctions().GetPagedQuery(query, parameters);
                command.CommandText = query;
            }
            return command;
        }

        private static string GetExamName(PrintProgramme programme)
        {
            switch (programme)
            {
                case PrintProgramme.PG:
                    return "GRADUATION";
                default:
                    return "12TH";
            }
        }

        internal static SqlCommand GetCourseName(Guid course_ID)
        {
            SqlCommand command = new SqlCommand();
            string sqlQuery = $@"SELECT CourseFullName FROM [ADMCourseMaster]
                                where Course_ID=@course_ID";
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("@course_ID", course_ID);
            return command;
        }

        internal static SqlCommand GetCombination(Guid combinationId)
        {
            SqlCommand command = new SqlCommand();
            string sqlQuery = $@"SELECT CombinationCode,CombinationSubjects FROM ADMCombinationMaster
                                where Combination_ID=@combination_ID AND College_ID=@College_ID";
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("@combination_ID", combinationId);
            command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            return command;
        }

        internal static SqlCommand GetCombinationsCount(Parameters parameters)
        {
            var semester = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("SEMESTER"));
            var batch = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("SEMESTERBATCH")).Value;
            var programme = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));
            var course = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("COURSE"));
            var courseId = (course != null) ? course.Value : "";
            var semesterId = (semester != null) ? semester.Value : "";
            var collegeId = AppUserHelper.College_ID;
            if (programme == null)
                programme.Value = "IG";
            string tableName = "ARGSelectedCombination";
            PrintProgramme ProgrammeEnum = GeneralFunctions.ProgrammeToPrintProgrammeMapping((Programme)Convert.ToInt16(programme.Value));
            string ProgrammeName = new GeneralFunctions().GetProgrammePostFix(ProgrammeEnum);
            tableName = tableName + ProgrammeName;
            SqlCommand command = new SqlCommand();

            string sqlQuery = $@"SELECT SUM(AssignedStudentCount) OVER() TotalRow_Count,*FROM(SELECT CombinationCode
                                  , CombinationSubjects
                                  , (SELECT COUNT(Student_ID) FROM (SELECT Distinct
                                     ARGPersonalInformation{ProgrammeName}.Student_ID
                                     FROM  dbo.ARGSelectedCombination{ProgrammeName}
                                     SC INNER JOIN dbo.ARGPersonalInformation{ProgrammeName} ON ARGPersonalInformation{ProgrammeName}.Student_ID = SC.Student_ID
                                     WHERE SC.Combination_ID =  CM.Combination_ID AND SC.SemesterBatch={batch}
                                     AND SC.Semester={semesterId}  AND FormStatus IN({(int)FormStatus.Accepted},{(int)FormStatus.Selected},{(int)FormStatus.Submitted})
                                     ) temp) AS AssignedStudentCount



                                  FROM ADMCombinationMaster CM
                                 WHERE CM.College_ID = @College_ID AND CM.Course_ID = @Course_ID  and CM.Semester={semesterId}
								  )tempC";
            sqlQuery += new GeneralFunctions().GetPagedQuery(sqlQuery, parameters);
            command.CommandText = sqlQuery;
            command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            command.Parameters.AddWithValue("@Course_ID", courseId);
            return command;
        }
        internal static SqlCommand GetSubjectsCount(Parameters parameters)
        {
            var programme = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("PROGRAMME"));

            string categoryWise = (parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("CATEGORYWISE"))?.Value ?? "False").ToLower().Trim();

            string categoryWiseSql = " COUNT(student_ID) AssignedStudentCount ";
            string categoryGroupBy = "";
            string categoryColumnName = "";

            var categoryFilter = parameters.Filters.FirstOrDefault(filter => filter.Column.ToUpper().Equals("CATEGORYWISE"));
            parameters.Filters.Remove(categoryFilter);

            if (categoryWise == "true")
            {
                categoryWiseSql = " COUNT(temp.Category) AssignedStudentCount,temp.Category ";
                categoryGroupBy = " ,temp.Category ";
                categoryColumnName = " Category, ";
            }

            if (programme == null)
                programme.Value = "IG";
            string tableName = "ARGSelectedCombination";

            Programme ProgrammeEnum = (Programme)Convert.ToInt16(programme.Value);
            string ProgrammeName = "_" + GeneralFunctions.ProgrammeToPrintProgrammeMapping(ProgrammeEnum).ToString();

            tableName = tableName + ProgrammeName;
            string sqlQuery = $@"SELECT MAX(temp.SubjectFullName) SubjectFullName,Max(temp.Subject_ID) Subject_ID,Max(temp.SubjectType) SubjectType,
                                        MAX(temp.CourseFullName) CourseFullName,
                                        MAX(temp.Semester) Semester,{categoryWiseSql} FROM 
                               (SELECT DISTINCT ADMCourseMaster.Course_ID Course,ARGSelectedCombination{ProgrammeName}.SemesterBatch,
                                     ARGPersonalInformation{ProgrammeName}.Student_ID,
                                    ADMSubjectMaster.Subject_ID,ADMSubjectMaster.SubjectType,
                                    ADMSubjectMaster.SubjectFullName ,
                                    ADMSubjectMaster.Semester,
									CourseFullName,
									ADMCourseMaster.Programme,
                                    Gender,{categoryColumnName}
									ARGPersonalInformation{ProgrammeName}.AcceptCollege_ID
                                    FROM ARGPersonalInformation{ProgrammeName}
                                    JOIN dbo.ARGSelectedCombination{ProgrammeName}
                                     ON ARGSelectedCombination{ProgrammeName}.Student_ID = ARGPersonalInformation{ProgrammeName}.Student_ID
                                     JOIN dbo.VWSCMaster 
                                            ON VWSCMaster.Combination_ID = ARGSelectedCombination{ProgrammeName}.Combination_ID
                                       JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWSCMaster.Subject_ID
                                       JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = VWSCMaster.Course_ID
	                                    UNION
		                            SELECT DISTINCT
                                        ADMCourseMaster.Course_ID Course,
                                        ARGSelectedCombination{ProgrammeName}.SemesterBatch,
                                        ARGPersonalInformation{ProgrammeName}.Student_ID,
                                        ADMSubjectMaster.Subject_ID,
                                        ADMSubjectMaster.SubjectType,
                                        ADMSubjectMaster.SubjectFullName,
                                        ADMSubjectMaster.Semester,
                                        CourseFullName,
										ADMCourseMaster.Programme,
                                        Gender,{categoryColumnName}
                                        ARGPersonalInformation{ProgrammeName}.AcceptCollege_ID
                                FROM ARGPersonalInformation{ProgrammeName}
                                    JOIN dbo.ARGSelectedCombination{ProgrammeName}
                                        ON ARGSelectedCombination{ProgrammeName}.Student_ID = ARGPersonalInformation{ProgrammeName}.Student_ID
                                    JOIN dbo.ARGStudentAdditionalSubjects{ProgrammeName} a ON a.Student_ID = ARGPersonalInformation{ProgrammeName}.Student_ID			
                                    JOIN ADMSubjectMaster
                                        ON ADMSubjectMaster.Subject_ID = a.Subject_ID
                                    JOIN ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = a.Course_ID
                                    ) temp  ";
            FilterHelper filterHelper = new GeneralFunctions().GetWhereClause<SubjectsCount>(parameters.Filters);
            sqlQuery += filterHelper.WhereClause;
            sqlQuery += $@"  AND Programme={(short)ProgrammeEnum}
                        {((parameters.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "acceptcollege_id") == null && AppUserHelper.College_ID != null)
                        ? " AND AcceptCollege_ID=@AcceptCollege_ID" : "")}  GROUP BY SubjectFullName{categoryGroupBy},
                                             Subject_ID,Course,Semester";


            sqlQuery += new GeneralFunctions().GetPagedQuery(sqlQuery, parameters);


            filterHelper.Command.CommandText = sqlQuery;

            if (parameters.Filters.FirstOrDefault(x => x.Column.ToLower().Trim() == "acceptcollege_id") == null && AppUserHelper.College_ID != null)
                filterHelper.Command.Parameters.AddWithValue("@AcceptCollege_ID", AppUserHelper.College_ID);

            return filterHelper.Command;
        }

        //internal static SqlCommand GetCombinationCount(Guid Combination_ID, short Semester, short Batch, PrintProgramme printProgramme)
        //{
        //    SqlCommand command = new SqlCommand();
        //    string sqlQuery = $@"SELECT COUNT(STDCOMB.Student_ID) 
        //                        FROM ARGSelectedCombination_{printProgramme.ToString()} STDCOMB 
        //                        JOIN ARGPersonalInformation_{printProgramme.ToString()} STD ON	STDCOMB.Student_ID = STD.Student_ID
        //                        WHERE STDCOMB.Combination_ID='{Combination_ID}' AND STD.Batch={Batch} AND STDCOMB.Semester={Semester}";
        //    command.CommandText = sqlQuery;
        //    return command;
        //}

        //internal static SqlCommand GetActualCombinationCount(Guid SelectedCombination_ID, Guid College_ID)
        //{
        //    SqlCommand command = new SqlCommand();
        //    string sqlQuery = $@" SELECT COUNT(*) FROM dbo.ARGSelectedCombination_UG
        //                            INNER JOIN ARGPersonalInformation_UG ON ARGPersonalInformation_UG.Student_ID = ARGSelectedCombination_UG.Student_ID
        //                            WHERE AcceptedBy_ID IS NOT NULL AND Combination_ID = @SelectedCombination_ID AND ARGPersonalInformation_UG.AcceptCollege_ID=@College_ID";
        //    command.CommandText = sqlQuery;
        //    command.Parameters.AddWithValue("@SelectedCombination_ID", SelectedCombination_ID);
        //    command.Parameters.AddWithValue("@College_ID", College_ID);
        //    return command;
        //}

        internal static SqlCommand GetStudentDetails(string formNumber, PrintProgramme programme)
        {
            SqlCommand command = new SqlCommand();
            string tableName = "ARGPersonalInformation";

            switch (programme)
            {
                case PrintProgramme.UG:
                    tableName += "_UG";
                    break;
                case PrintProgramme.PG:
                    tableName += "_PG";
                    break;
                case PrintProgramme.IH:
                    tableName += "_IH";
                    break;
                default:
                    break;
            }
            //order by and batch is specified because if boardregn is present in more than 1 batch - donot remove
            command.CommandText = $" SELECT TOP 1 * FROM {tableName} WHERE StudentFormNo = @formNumber OR BoardRegistrationNo  = @formNumber ORDER BY Batch DESC";
            command.Parameters.AddWithValue("@formNumber", formNumber);
            return command;
        }

        internal static string Delete(Guid student_ID, Guid college_ID, string tableName)
        {
            return $" DELETE FROM {tableName} WHERE Student_ID ='" + student_ID + "' AND College_ID='" + college_ID + "'";
        }
    }
}
