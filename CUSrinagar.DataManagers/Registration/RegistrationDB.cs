using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Extensions;
using System.Web.Mvc;
using Terex;
using GeneralModels;

namespace CUSrinagar.DataManagers
{
    public class RegistrationDB
    {
        #region New Registration
        public int Save(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            try
            {
                if (printProgramme == PrintProgramme.IH)
                {
                    return new MSSQLFactory().InsertRecord(aRGPersonalInformation, $"ARGPersonalInformation_{printProgramme.ToString()}");
                }
                else
                {
                    return new MSSQLFactory().InsertRecord(aRGPersonalInformation, $"ARGPersonalInformation_{printProgramme.ToString()}", new List<string> { nameof(aRGPersonalInformation.Preference) });
                }
            }
            catch (SqlException)
            {
                return 0;
            }
        }
        public int Save(ARGStudentAddress aRGStudentAddress, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().InsertRecord(aRGStudentAddress, $"ARGStudentAddress_{printProgramme.ToString()}");
        }
        public int Save(ARGCoursesApplied aRGCoursesApplied, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().InsertRecord(aRGCoursesApplied, $"ARGCoursesApplied_{printProgramme.ToString()}");
        }
        public int Save(ARGStudentPreviousQualifications aRGStudentPreviousQualifications, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().InsertRecord(aRGStudentPreviousQualifications, $"ARGStudentPreviousQualifications_{printProgramme.ToString()}");
        }
        public ARGFormNoMaster GetFromNoMaster(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Programme", printProgramme);
            cmd.CommandText = "SELECT Top 1 * FROM ARGFormNoMaster WHERE PrintProgramme=@Programme ORDER BY BatchToSet DESC";
            return new MSSQLFactory().GetObject<ARGFormNoMaster>(cmd);
        }
        public int UpdateFromNoMaster(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Programme", printProgramme);
            cmd.CommandText = "UPDATE ARGFormNoMaster SET FormNoCount=(FormNoCount+1) WHERE PrintProgramme=@Programme";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int AssignRegistrationNumbersToStudent(string sqlQuery)
        {
            SqlCommand command = new SqlCommand
            {
                CommandTimeout = 5000,
                CommandText = sqlQuery
            };
            return new MSSQLFactory().ExecuteNonQuery(command);
        }
        public string GetPreviousRegnNoOfCUS(int Batch, string boarRegn)
        {
            string SQL = $@"SELECT  TOP 1  CUSRegistrationNo
                            FROM
                            (
                                SELECT TOP 1  CUSRegistrationNo
                                FROM dbo.ARGPersonalInformation_IH
                                WHERE Batch < {Batch}
                                      AND BoardRegistrationNo = '{boarRegn}' AND AcceptCollege_ID IS NOT NULL  ORDER BY Batch DESC
                                UNION
                                SELECT  TOP 1 CUSRegistrationNo
                                FROM dbo.ARGPersonalInformation_UG
                                WHERE Batch < {Batch}
                                      AND BoardRegistrationNo = '{boarRegn}' AND AcceptCollege_ID IS NOT NULL  ORDER BY Batch DESC
                                UNION
                                SELECT  TOP 1 CUSRegistrationNo
                                FROM dbo.ARGPersonalInformation_PG
                                WHERE Batch < {Batch}
                                      AND BoardRegistrationNo = '{boarRegn}' AND AcceptCollege_ID IS NOT NULL  ORDER BY Batch DESC
                            ) temp";
            return new MSSQLFactory().ExecuteScalar<string>(SQL);
        }

        public List<ARGCoursesApplied> GetCoursesApplied(Guid Student_ID, List<Guid> Course_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>($@"SELECT ARGCoursesApplied_{programme.ToString()}.*,ADMCourseMaster.CourseFullName AS CourseName,ADMCourseMaster.CourseCode
                                                                FROM ARGCoursesApplied_{programme.ToString()} JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCoursesApplied_{programme.ToString()}.Course_ID
                                                                WHERE ARGCoursesApplied_{programme.ToString()}.Course_ID IN ({Course_ID.ToIN()}) AND Student_ID='{Student_ID}'");
        }

        public ARGBoardRegistrationData GetBoardRegistrationData(string BoardRegistrationNo)
        {
            string query = $@"SELECT TOP 1 
                            ISNULL(BoardRegistrationNo, '') AS BoardRegistrationNo,
	                        ISNULL(FullName, '') AS FullName,
	                        ISNULL(FathersName, '') AS FathersName,
	                        ISNULL(MothersName, '') AS MothersName,
	                        ISNULL(DOB,'1/1/0001 12:00:00 AM') AS DOB,
                            ISNULL(Gender, '') AS Gender,
                            ISNULL(Session, '') AS Session,
                            ISNULL(MarksObt, '0') AS MarksObt,
                            MaxMarks,
                            ISNULL(YearOfPassing, '0') AS YearOfPassing,
                            ISNULL(RollNo, '') AS RollNo
                        FROM ARGBoardRegistrationData  WHERE BoardRegistrationNo=@BoardRegistrationNo ORDER BY YearOfPassing DESC";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@BoardRegistrationNo", BoardRegistrationNo);
            return new MSSQLFactory().GetObject<ARGBoardRegistrationData>(sqlCommand);
        }

        public int GetPreviousSchoolRegistrationCount(string registrationNumber)
        {
            string query = $@"SELECT CAST(REPLACE(MAX(AllStudents.CUSRegistrationNo),'{registrationNumber}','')AS INT) FROM (
                                    SELECT CUSRegistrationNo FROM ARGPersonalInformation_UG
                                    UNION
                                    SELECT CUSRegistrationNo FROM ARGPersonalInformation_IH
                                    UNION
                                    SELECT CUSRegistrationNo FROM ARGPersonalInformation_PG
                                    ) AllStudents WHERE AllStudents.CUSRegistrationNo LIKE '{registrationNumber}%'
                                    ";

            return new MSSQLFactory().ExecuteScalar<int>(query);
        }

        public bool CheckBoardRegNoExists(string BoardRegistrationNo, int Batch, PrintProgramme printProgramme)
        {
            string query = $@"SELECT SUM(t.CT)
                            FROM
                            (
                                SELECT COUNT(BoardRegistrationNo) AS CT
                                FROM ARGPersonalInformation_{printProgramme.ToString()}
                                WHERE (BoardRegistrationNo = @BoardRegistrationNo OR StudentFormNo=@BoardRegistrationNo OR CUSRegistrationNo=@BoardRegistrationNo)
                                      AND Batch = @Batch 
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_PG
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_UG
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_IH
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                            ) t;";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@BoardRegistrationNo", BoardRegistrationNo);
            sqlCommand.Parameters.AddWithValue("@Batch", Batch);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }

        public bool CheckBoardRegNoExists(string BoardRegistrationNo, Guid Student_ID, int Batch, PrintProgramme printProgramme)
        {
            string query = $@"SELECT SUM(t.CT)
                            FROM
                            (
                                SELECT COUNT(BoardRegistrationNo) AS CT
                                FROM ARGPersonalInformation_{printProgramme.ToString()}
                                WHERE (BoardRegistrationNo = @BoardRegistrationNo OR StudentFormNo=@BoardRegistrationNo OR CUSRegistrationNo=@BoardRegistrationNo)
                                      AND Batch = @Batch AND Student_ID<>@Student_ID
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_PG
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_UG
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                                UNION
                                    SELECT COUNT(BoardRegistrationNo) AS CT
                                    FROM dbo.ARGPersonalInformation_IH
                                    WHERE CUSRegistrationNo = @BoardRegistrationNo
                            ) t;";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@BoardRegistrationNo", BoardRegistrationNo);
            sqlCommand.Parameters.AddWithValue("@Batch", Batch);
            sqlCommand.Parameters.AddWithValue("@Student_ID", Student_ID);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }

        public int GetBatchForAssigningRegistrationNumbers()
        {
            return new MSSQLFactory().ExecuteScalar<short>("SELECT MAX(Batch) Batch FROM ARGPersonalInformation_UG ");
        }


        public List<NewAdmissionsWidget> GetNewAdmissionsStatistics(Parameters parameters, PrintProgramme printProgramme)
        {
            var tableNameSuffix = new GeneralFunctions().MappingTable(printProgramme);
            string sqlQuery = $@"SELECT Batch,CourseFullName,COUNT(ARGCoursesApplied_{tableNameSuffix.ToString()}.Student_ID) NoOfStudents 
                                FROM ARGPersonalInformation_{tableNameSuffix.ToString()} 
                                INNER JOIN ARGCoursesApplied_{tableNameSuffix.ToString()} ON ARGCoursesApplied_{tableNameSuffix.ToString()}.Student_ID = ARGPersonalInformation_{tableNameSuffix.ToString()}.Student_ID
                                INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ARGCoursesApplied_{tableNameSuffix.ToString()}.Course_ID
                                WHERE Batch = (SELECT MAX(Batch) FROM ARGPersonalInformation_{tableNameSuffix.ToString()}) AND FormStatus IN ({(short)FormStatus.FeePaid},{(short)FormStatus.Submitted}) 
                                GROUP BY Batch,CourseFullName 
                                ORDER BY CourseFullName ";

            sqlQuery += new GeneralFunctions().GetPagedQuery(sqlQuery, parameters);

            return new MSSQLFactory().GetObjectList<NewAdmissionsWidget>(sqlQuery);
        }

        public int GetNewAdmissionsStudentCountWithFees()
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT Count(Student_ID) FROM ARGPersonalInformation_IH WHERE Batch= YEAR(SYSDATETIME()) AND FormStatus IN ({(short)FormStatus.FeePaid},{(short)FormStatus.Submitted}) ");
        }

        public int GetNewAdmissionsStudentCountWithoutFees()
        {
            return new MSSQLFactory().ExecuteScalar<int>($"SELECT Count(Student_ID) FROM ARGPersonalInformation_IH WHERE Batch= YEAR(SYSDATETIME()) AND FormStatus IN ({(short)FormStatus.InProcess}) ");
        }
        public int GetGraduationOnlyNewAdmissionsStudentCountWithFees()
        {
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT SUM(a) FROM (
                                                                SELECT COUNT(c.Student_ID) a FROM dbo.ARGPersonalInformation_IH p
                                                                JOIN dbo.ARGCoursesApplied_IH c
                                                                ON c.Student_ID = p.Student_ID
                                                                WHERE p.FormStatus = 10 AND c.Course_ID='A3EE7F98-7B82-4D95-A2C0-FABA7A18240E'--exclude Int.b.ed - med
                                                                AND p.Batch = {DateTime.Now.Year}
                                                                GROUP BY c.Student_ID HAVING COUNT(c.Student_ID) = 1
                                                                )Students");
        }
        public int GetRegistrationNumberAssignedFor(Guid college_ID, PrintProgramme programme, short batchToAssign)
        {
            programme = new GeneralFunctions().MappingTable(programme);
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(Student_ID) TotalStudents FROM ARGPersonalInformation_{programme} WHERE AcceptCollege_ID ='{college_ID}'  
                                                    AND Batch = {batchToAssign} AND CUSRegistrationNo IS NOT NULL");
        }

        public int GetTotalNewStudentsForCollege(Guid college_ID, PrintProgramme programme, short batchToAssign)
        {
            programme = new GeneralFunctions().MappingTable(programme);
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(Student_ID) TotalStudents FROM ARGPersonalInformation_{programme} WHERE AcceptCollege_ID ='{college_ID}' 
                                                                AND Batch = {batchToAssign} AND CUSRegistrationNo IS NULL ");
        }


        public List<SelectListItem> GetStudentsSelectListItem()
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>("SELECT CAST(Student_ID AS VARCHAR(40)) [Value], CUSRegistrationNo [Text]  FROM VWAllStudents WHERE CUSRegistrationNo IS NOT NULL");
        }

        public NewAdmissionsWidget GetCountForAptitude(int Batch)
        {
            string sqlQuery = $@" SELECT {Batch} Batch, ('Common Aptitude Test') AS CourseFullName, COUNT(P.Student_ID) AS NoOfStudents
                                    FROM ARGPersonalInformation_IH P
                                    WHERE Batch = {Batch}
                                          AND FormStatus = 10
                                          AND P.Student_ID NOT IN (
                                                                      SELECT A.Student_ID
                                                                      FROM ARGPersonalInformation_IH P
                                                                          JOIN ARGCoursesApplied_IH A
                                                                              ON A.Student_ID = P.Student_ID
                                                                      WHERE Batch = {Batch}
                                                                            AND FormStatus = 10
                                                                            AND A.Course_ID = 'FC32E138-4EE2-4DA2-9453-5C8368180BC3'
                                                                  );";


            return new MSSQLFactory().GetObject<NewAdmissionsWidget>(sqlQuery);
        }

        public List<CourseWiseGenderCountWidget> GetCourseWiseGenderCount(int? batch, Guid? college_ID, PrintProgramme? programme, string courseFullName)
        {
            string tableName = string.Empty;
            string selectedCombinationTable = string.Empty;

            if (programme != null)
            {
                switch (programme)
                {
                    case PrintProgramme.UG:
                        tableName = " ARGPersonalInformation_UG ";
                        selectedCombinationTable = " ARGSelectedCombination_UG";
                        break;

                    case PrintProgramme.IH:
                        tableName = " ARGPersonalInformation_IH ";
                        selectedCombinationTable = " ARGSelectedCombination_IH";
                        break;

                    case PrintProgramme.PG:
                        tableName = " ARGPersonalInformation_PG ";
                        selectedCombinationTable = " ARGSelectedCombination_PG";
                        break;

                    default:
                        break;
                }
            }

            string whereClause = "WHERE 1 = 1 ";
            SqlCommand command = new SqlCommand();
            if (batch != null)
            {
                command.Parameters.AddWithValue("@Batch", batch);
                whereClause += $" AND Batch = @Batch ";
            }

            if (college_ID != null)
            {
                command.Parameters.AddWithValue("@AcceptCollege_ID", college_ID);
                whereClause += $" AND AcceptCollege_ID = @AcceptCollege_ID ";
            }

            if (programme != null)
            {
                command.Parameters.AddWithValue("@PrintProgramme", programme);
                whereClause += $" AND PrintProgramme = @PrintProgramme ";
            }

            string query = $@"SELECT Batch,Gender,COUNT(Gender) NoOfStudents FROM {tableName} 
                                INNER JOIN {selectedCombinationTable} ON {selectedCombinationTable}.Student_ID = {tableName}.Student_ID AND Semester = CurrentSemesterOrYear
                                INNER JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = {selectedCombinationTable}.Combination_ID
                                INNER JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                                {whereClause}
                                GROUP BY Gender,Batch ORDER BY Batch DESC ";

            command.CommandText = query;
            return new MSSQLFactory().GetObjectList<CourseWiseGenderCountWidget>(command);
        }

        public List<AdmissionFormWidgetCourseWise> GetCourseWiseCountForProgramme(int? batch, Guid? college_ID, PrintProgramme? programme)
        {
            string query = string.Empty;
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };

            parameters.Filters.Add(new SearchFilter() { TableAlias = "AllStudents", Column = "FormStatus", Operator = SQLOperator.EqualTo, Value = ((int)FormStatus.Selected).ToString() });

            if (batch != null)
            {
                parameters.Filters.Add(new SearchFilter() { TableAlias = "SelectedCombinations", Column = "Semester", Operator = SQLOperator.EqualTo, Value = "1" });
                parameters.Filters.Add(new SearchFilter() { TableAlias = "AllStudents", Column = "Batch", Operator = SQLOperator.EqualTo, Value = batch.ToString() });
            }
            else
                parameters.Filters.Add(new SearchFilter() { TableAlias = "SelectedCombinations", Column = "Semester", Operator = SQLOperator.EqualTo, Value = "1" });

            if (college_ID != null)
                parameters.Filters.Add(new SearchFilter() { TableAlias = "AllStudents", Column = "AcceptCollege_ID", Operator = SQLOperator.EqualTo, Value = college_ID.ToString() });

            if (programme != null)
                parameters.Filters.Add(new SearchFilter() { Column = "programme", Operator = SQLOperator.EqualTo, Value = programme.ToString() });

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(parameters.Filters);

            query = $@"SELECT PrintProgramme,CourseFullName ,COUNT(SelectedCombinations.Student_ID) NoOfStudents
                                FROM
                                (
                                    SELECT Student_ID, Batch, FormStatus,AcceptCollege_ID
                                    FROM dbo.ARGPersonalInformation_UG
                                    UNION
                                    SELECT Student_ID, Batch, FormStatus,AcceptCollege_ID
                                    FROM dbo.ARGPersonalInformation_IH
                                    UNION
                                    SELECT Student_ID, Batch, FormStatus,AcceptCollege_ID
                                    FROM dbo.ARGPersonalInformation_PG
                                ) AllStudents
                                    INNER JOIN
                                    (
                                        SELECT Student_ID, Combination_ID, Semester
                                        FROM dbo.ARGSelectedCombination_UG
                                        UNION
                                        SELECT Student_ID, Combination_ID, Semester
                                        FROM dbo.ARGSelectedCombination_IH
                                        UNION
                                        SELECT Student_ID, Combination_ID, Semester
                                        FROM dbo.ARGSelectedCombination_PG
                                    ) SelectedCombinations
                                        ON SelectedCombinations.Student_ID = AllStudents.Student_ID
                                    INNER JOIN dbo.ADMCombinationMaster
                                        ON ADMCombinationMaster.Combination_ID = SelectedCombinations.Combination_ID
                                    INNER JOIN dbo.ADMCourseMaster
                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            {helper.WhereClause}
                            GROUP BY PrintProgramme,CourseFullName 
                            ORDER BY CourseFullName ";
            helper.Command.CommandText = query;
            return new MSSQLFactory().GetObjectList<AdmissionFormWidgetCourseWise>(helper.Command);
        }


        public List<AdmissionFormWidgetCourseWise> GetProgrammeWiseCount(int? batch, PrintProgramme programme)
        {
            Parameters parameters = new Parameters() { Filters = new List<SearchFilter>() };

            parameters.Filters.Add(new SearchFilter() { Column = "PrintProgramme", Operator = SQLOperator.EqualTo, Value = ((int)programme).ToString() });

            if (batch != null)
                parameters.Filters.Add(new SearchFilter() { Column = "Batch", Operator = SQLOperator.EqualTo, Value = batch.ToString() });

            FilterHelper helper = new GeneralFunctions().GetWhereClause<ARGPersonalInformation>(parameters.Filters);

            programme = new GeneralFunctions().MappingTable(programme);

            helper.Command.CommandText = $@"SELECT CourseFullName, COUNT(ARGPersonalInformation_{programme}.Student_ID) NoOfStudents, PrintProgramme
                                                FROM ARGPersonalInformation_{programme}
                                                    INNER JOIN ARGSelectedCombination_{programme}
                                                        ON ARGSelectedCombination_{programme}.Student_ID = ARGPersonalInformation_{programme}.Student_ID
                                                           AND ARGSelectedCombination_{programme}.Semester = 1
                                                    INNER JOIN ADMCombinationMaster
                                                        ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination_{programme}.Combination_ID
                                                           AND ADMCombinationMaster.Semester = ARGSelectedCombination_{programme}.Semester
                                                    INNER JOIN ADMCourseMaster
                                                        ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID                            
                                                                            {helper.WhereClause}
                                                                            GROUP BY PrintProgramme,CourseFullName 
                                                                            ORDER BY CourseFullName ";

            return new MSSQLFactory().GetObjectList<AdmissionFormWidgetCourseWise>(helper.Command);
        }



        public List<AdmissionFormWidget> GetAdmissionFormMainWidget(int? batch, Guid? college_ID, PrintProgramme? programme = null)
        {
            string query = string.Empty;
            string whereClause = string.Empty;
            SqlCommand sqlCommand = new SqlCommand();

            if (batch != null)
            {
                whereClause += " WHERE Batch = @Batch ";
                sqlCommand.Parameters.AddWithValue("@Batch", batch);

                if (college_ID != null)
                {
                    sqlCommand.Parameters.AddWithValue("@AcceptCollege_ID", college_ID);
                    whereClause += " AND AcceptCollege_ID = @AcceptCollege_ID ";
                }

            }
            else
            {
                if (college_ID != null)
                {
                    whereClause += " WHERE AcceptCollege_ID = @AcceptCollege_ID ";
                    sqlCommand.Parameters.AddWithValue("@AcceptCollege_ID", college_ID);
                }
            }

            if (programme != null)
            {
                switch (programme)
                {
                    case PrintProgramme.UG:
                        query = " SELECT  Batch, Gender,COUNT(Student_ID)NoOfStudents FROM ARGPersonalInformation_UG ";
                        break;
                    case PrintProgramme.PG:
                        query = " SELECT Batch,COUNT(Student_ID)NoOfStudents, Gender FROM ARGPersonalInformation_PG ";
                        break;
                    case PrintProgramme.IH:
                        query = " SELECT Batch,COUNT(Student_ID)NoOfStudents, Gender FROM ARGPersonalInformation_IH ";
                        break;
                    case PrintProgramme.BED:
                        whereClause = whereClause.Replace("WHERE", "AND");
                        query = $@"  SELECT VWStudentCourse.Batch,Gender,COUNT(ARGPersonalInformation_UG.Student_ID) NoOfStudents FROM VWStudentCourse 
                                     INNER JOIN ARGPersonalInformation_UG ON ARGPersonalInformation_UG.Student_ID = VWStudentCourse.Student_ID
                                     WHERE CourseFullName = 'B.Ed' {whereClause}
                                     GROUP BY VWStudentCourse.Batch,Gender ";
                        break;
                    default:
                        break;
                }

                query += whereClause + " GROUP BY Batch, Gender ";
            }
            else
            {
                query = $@"SELECT 
                                    AllStudents.Batch,AllStudents.Gender,COUNT(AllStudents.Student_ID)NoOfStudents
                                    FROM
                                    (
                                        SELECT Student_ID, Batch, Gender,AcceptCollege_ID
                                        FROM ARGPersonalInformation_IH
                                        UNION ALL
                                        SELECT Student_ID, Batch, Gender,AcceptCollege_ID
                                        FROM ARGPersonalInformation_UG
                                        UNION ALL
                                        SELECT Student_ID, Batch, Gender,AcceptCollege_ID
                                        FROM ARGPersonalInformation_PG
                                    ) AllStudents
                                    {whereClause}
                                    GROUP BY AllStudents.Batch,AllStudents.Gender ";
            }
            sqlCommand.CommandText = query;
            return new MSSQLFactory().GetObjectList<AdmissionFormWidget>(sqlCommand);
        }

        public int UpdateFormStatus(FormStatus status, Guid student_ID, PrintProgramme printprogramme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(printprogramme);
            string query = $"UPDATE ARGPersonalInformation{postfix} SET FormStatus = @FormStatus WHERE Student_ID = @Student_ID";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@FormStatus", (short)status);
            sqlCommand.Parameters.AddWithValue("@Student_ID", student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateFormStatus(Guid student_ID, Programme Programme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(Programme);
            string query = $@"UPDATE ARGPersonalInformation{postfix} SET FormStatus={(int)FormStatus.CancelRegistration} , AcceptCollege_ID = null WHERE Student_ID = @Student_ID;";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Student_ID", student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateAppUserInfo(Guid acceptCollege_ID, Guid acceptedBy_ID, Guid student_ID, Programme Programme)
        {
            string postfix = new GeneralFunctions().GetProgrammePostFix(Programme);
            string query = $@"UPDATE ARGPersonalInformation{postfix} SET AcceptedBy_ID = @AcceptedBy_ID , AcceptCollege_ID = @AcceptCollege_ID WHERE Student_ID = @Student_ID;";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@AcceptedBy_ID", acceptedBy_ID);
            sqlCommand.Parameters.AddWithValue("@AcceptCollege_ID", acceptCollege_ID);
            sqlCommand.Parameters.AddWithValue("@Student_ID", student_ID);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<ARGSelectedCombination> GetSelectedCombinations(Guid student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().GetObjectList<ARGSelectedCombination>(new RegistrationSQLQueries().GetSelectedCombinations(student_ID, programme));
        }

        public Guid GetStudentIDByFormNoAndDOB(ARGReprint aRGReprint)
        {
            return new MSSQLFactory().ExecuteScalar<Guid>(new RegistrationSQLQueries().GetStudentIDByFormNoAndDOB(aRGReprint));
        }

        public Guid GetStudentIDByFormNoAndDOBCUS(ARGReprint aRGReprint)
        {
            return new MSSQLFactory().ExecuteScalar<Guid>(new RegistrationSQLQueries().GetStudentIDByFormNoAndDOBCUS(aRGReprint));
        }

        public Guid GetStudentIDByFormNoEntranceRollNoAndDOBCUS(ARGReprint aRGReprint)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                command.Parameters.AddWithValue("@StudentFormNo", aRGReprint.FormNo.Trim());
                command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));
                command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (StudentFormNo = @StudentFormNo OR CAST(EntranceRollNo AS nvarchar(200))=@StudentFormNo OR BoardRegistrationNo=@StudentFormNo) AND 
                                   DOB=@DOB ORDER BY CreatedOn DESC";
                return new MSSQLFactory().ExecuteScalar<Guid>(command);
            }
            catch (SqlException)
            {
                return Guid.Empty;
            }
        }





        public ARGPersonalInformation GetStudentByID(Guid ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<ARGPersonalInformation>(new RegistrationSQLQueries().GetStudentByID(ID, printProgramme));

        }
        public ARGStudentAddress GetStudentAddress(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<ARGStudentAddress>(new RegistrationSQLQueries().GetStudentAddress(student_ID, printProgramme));
        }

        public List<ARGStudentPreviousQualifications> GetStudentAcademicDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGStudentPreviousQualifications>(new RegistrationSQLQueries().GetStudentAcademicDetails(student_ID, printProgramme));
        }

        public List<ARGCoursesApplied> GetStudentCoursesApplied(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new RegistrationSQLQueries().GetStudentCoursesApplied(student_ID, printProgramme));
        }
        public List<ARGCoursesApplied> GetStudentCoursesApplied(List<Guid> course_Ids)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>(new RegistrationSQLQueries().GetStudentCoursesApplied(course_Ids));
        }

        public List<short> GetAdmitCardCategory()
        {
            return new MSSQLFactory().GetSingleValues<short>("SELECT PrintProgramme FROM ARGDownloadAdmitCards WHERE AllowDownloadAdmitCards=1");
        }

        public int GetAdmitCardCategoryYear(PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand("SELECT ForAcademicSession FROM ARGDownloadAdmitCards WHERE PrintProgramme = @PrintProgramme");
            command.Parameters.AddWithValue("@PrintProgramme", printProgramme);
            return new MSSQLFactory().ExecuteScalar<int>(command);
        }

        public bool IsEngLateralEntry(PrintProgramme printProgramme)
        {
            SqlCommand command = new SqlCommand("SELECT TOP 1 IsEngLateralEntry FROM ARGDownloadAdmitCards WHERE PrintProgramme = @PrintProgramme and AllowDownloadAdmitCards=1");
            command.Parameters.AddWithValue("@PrintProgramme", printProgramme);
            return new MSSQLFactory().ExecuteScalar<bool>(command);
        }

        public List<string> SendBulkSMS()
        {
            return new MSSQLFactory().GetSingleValues<string>
                (@"");
        }
        #endregion

        #region Get New Colleges
        public List<SelectListItem> GetCollegeCourses()
        {
            string query = $"SELECT DISTINCT CAST(Course_ID AS NVARCHAR(150)) AS [Value],CourseFullName AS [Text] FROM ADMCourseMaster WHERE PrintProgramme={(short)PrintProgramme.UG} AND Status=1";
            return new MSSQLFactory().GetObjectList<SelectListItem>(query);
        }
        public List<string> GetColleges(Guid Course_ID)
        {
            string query = @"SELECT DISTINCT cl.CollegeFullName+' ('+ cl.Address+')' FROM ADMCombinationMaster c 
                            JOIN ADMCollegeMaster cl ON c.College_ID=cl.College_ID WHERE c.Course_ID=@Course_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Course_ID", Course_ID);
            cmd.CommandText = query;
            return new MSSQLFactory().GetSingleValues<string>(cmd);
        }
        #endregion

        public bool HasAlreadyAppliedInIG(string BoardRegnNo, short Batch)
        {
            string TSQL = $@"SELECT COUNT(StudentFormNo) FROM ARGPersonalInformation_IH
                            WHERE BoardRegistrationNo=@BoardRegistrationNo AND EntranceRollNo IS NOT NULL AND Batch=@Batch";
            SqlCommand cmd = new SqlCommand(TSQL);
            cmd.Parameters.AddWithValue("@BoardRegistrationNo", BoardRegnNo);
            cmd.Parameters.AddWithValue("@Batch", Batch);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public List<short> GetBatchs(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetSingleValues<short>($@"SELECT DISTINCT Batch FROM ARGPersonalInformation_{printProgramme.ToString()} WHERE Batch NOT IN(
                                                                SELECT DISTINCT Batch FROM ARGPersonalInformation_{printProgramme.ToString()} WHERE CUSRegistrationNo IS NOT NULL) 
                                                                ORDER BY Batch ASC");
        }
        public List<short> GetAllBatchs(Programme programme)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            return new MSSQLFactory().GetSingleValues<short>($@"SELECT DISTINCT Batch FROM ARGPersonalInformation{prgPostFix} ORDER BY Batch ASC");
        }
        public int CloseNewAdmission(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE ADMCourseMaster SET RegistrationOpen=0 WHERE PrintProgramme={(short)printProgramme} AND Status=1;
                                                         UPDATE ARGFormNoMaster SET AllowOnlinePayment=0 WHERE PrintProgramme={(short)printProgramme};");
        }

        public int UpdateIsProvisional(Guid student_ID, PrintProgramme programme, bool IsProvisional = false)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.ARGPersonalInformation_{programme.ToString()} SET IsProvisional={(IsProvisional ? "1" : "0")} WHERE Student_ID='{student_ID}'");
        }

        public int DeleteAcademicDetailsAll(Guid student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.ARGStudentPreviousQualifications_{programme.ToString()} WHERE Student_ID='{student_ID}'");
        }

        public bool IsLateralEntry(Guid Student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().ExecuteScalar<bool>($"SELECT IsLateralEntry FROM dbo.ARGPersonalInformation_{programme.ToString()} WHERE Student_ID='{Student_ID}'");
        }

        public bool CheckAlreadyExistsInAffiliated(string BoardRegnNo, int Batch, PrintProgramme PrintProgrammeOption)
        {
            string sql = $@"SELECT Count(Student_ID) FROM ARGPersonalInformation_{PrintProgrammeOption.ToString()} WHERE BoardRegistrationNo='{BoardRegnNo}' AND Batch={Batch}
                                   AND FormStatus NOT IN('{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}')";
            return new MSSQLFactory().ExecuteScalar<int>(sql) > 0;
        }

        public bool CheckMobileNoExists(string MobileNo, int Batch, Guid Student_ID, PrintProgramme printProgramme)
        {
            string isInEdit = (Student_ID == Guid.Empty) ? "" : $" AND a.Student_ID<>'{Student_ID}'";
            string _batchToSearchFrom = (Student_ID == Guid.Empty) ? $"{Batch}" : $"(SELECT TOP 1 Batch FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}')";
            string query = $@"SELECT COUNT(a.Mobile) FROM dbo.ARGStudentAddress_{printProgramme.ToString()} a
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = a.Student_ID
                                WHERE p.Batch={_batchToSearchFrom} AND a.Mobile=@Mobile {isInEdit}";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Mobile", MobileNo);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }
        public bool CheckEmailExists(string Email, int Batch, Guid Student_ID, PrintProgramme printProgramme)
        {
            string isInEdit = (Student_ID == Guid.Empty) ? "" : $" AND a.Student_ID<>'{Student_ID}'";
            string _batchToSearchFrom = (Student_ID == Guid.Empty) ? $"{Batch}" : $"(SELECT TOP 1 Batch FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID='{Student_ID}')";
            string query = $@"SELECT COUNT(a.Email) FROM dbo.ARGStudentAddress_{printProgramme.ToString()} a
                                JOIN dbo.ARGPersonalInformation_{printProgramme.ToString()} p ON p.Student_ID = a.Student_ID
                                WHERE p.Batch={_batchToSearchFrom} AND a.Email=@Email {isInEdit}";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Email", Email);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }

        public short GetStudentNormalBatch(Guid student_ID, PrintProgramme programme)
        {
            return new MSSQLFactory().ExecuteScalar<short>($@"SELECT TOP 1 Batch FROM dbo.ARGPersonalInformation_{programme} WHERE Student_ID='{student_ID}'");
        }

        public bool CheckBoardRegNoOrCUETApplicationNoExists
            (PrintProgramme selectedPorgramme, string boardRegistrationNo, string applicationNo, short batchToSet)
        {
            SqlCommand cmd =
                new SqlCommand($@"SELECT COUNT(Student_ID) FROM dbo.ARGPersonalInformation_{selectedPorgramme} 
                                    WHERE (BoardRegistrationNo=@BoardRegistrationNo OR CUETApplicationNo=@CUETApplicationNo) AND Batch={batchToSet};");

            cmd.Parameters.AddWithValue("@CUETApplicationNo", applicationNo);
            cmd.Parameters.AddWithValue("@BoardRegistrationNo", boardRegistrationNo);

            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public string CheckAlreadyHasLiveAdmissionInCollege
            (PrintProgramme selectedPorgramme, string boardRegistrationNo)
        {
            SqlCommand cmd =
                new SqlCommand($@"SELECT TOP 1 CollegeFullName FROM dbo.ARGPersonalInformation_{selectedPorgramme}
                        JOIN dbo.ADMCollegeMaster ON AcceptCollege_ID=College_ID
                         WHERE BoardRegistrationNo=@BoardRegistrationNo
                        AND IsPassout=0 AND FormStatus=9 AND AcceptCollege_ID IS NOT NULL");

            cmd.Parameters.AddWithValue("@BoardRegistrationNo", boardRegistrationNo);

            return new MSSQLFactory().ExecuteScalar<string>(cmd);
        }

        public ARGPersonalInformation GetCUETDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            SqlCommand cmd =
                new SqlCommand($@"SELECT CUETApplicationNo,CUETEntranceRollNo FROM dbo.ARGPersonalInformation_{printProgramme}
                        WHERE Student_ID=@Student_ID");

            cmd.Parameters.AddWithValue("@Student_ID", student_ID);

            return new MSSQLFactory().GetObject<ARGPersonalInformation>(cmd);
        }
    }
}
