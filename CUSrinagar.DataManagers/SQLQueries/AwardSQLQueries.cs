using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;
using CUSrinagar.Extensions;
namespace CUSrinagar.DataManagers.SQLQueries
{
    public class AwardSQLQueries
    {
        internal SqlCommand GetAllRecords(Parameters parameter, Programme programmme, MarksFor marksFor, int batch, SubjectType subjectType, bool hasExaminationFee, bool IsBacklog, string Courses, bool isPrint)
        {
            SearchFilter semfilter = parameter.Filters.Where(x => x.Column == "Semester").FirstOrDefault();
            string semester = semfilter.Value;
            SearchFilter subfilter = parameter.Filters.Where(x => x.Column == "CombinationSubjects").FirstOrDefault();
            string SubjectID = subfilter.Value;
            String Column = "";
            bool IsSkillORGE = (subjectType == SubjectType.SEC) || (subjectType == SubjectType.GE);

            int isBackklog = IsBacklog ? 1 : 0;
            Column = "ClassRollNo";
            parameter.Filters.Remove(semfilter);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmme);
            string tableName = new GeneralFunctions().GetTableName(semester, programmme);

            ///Main Subjects
            string query1 = GetStudents(semester, SubjectID, programmme, isBackklog);

            ///Additional Subjects
            string query2 = $@"SELECT Distinct {isBackklog} IsBacklog, ARGPersonalInformation{prgPostFix}.CUSRegistrationNo,FormStatus,ExamForm_Id,IsInternalPassed,
                                       ResultNotification_ID,
                                      (SELECT ExternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID)SubjectMinMarksTheory,
                                      (SELECT InternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID) SubjectMinMarksPractical,
                                      AcceptCollege_ID,
									  CurrentSemesterOrYear,
                                      '{SubjectID}' AS CombinationSubjects,
                                       ARGPersonalInformation{prgPostFix}.FullName,
                                      ARGPersonalInformation{prgPostFix}.Student_ID,
                                      ARGPersonalInformation{prgPostFix}.ClassRollNo,
                                      (SELECT SemesterBatch FROM ARGSelectedCombination{prgPostFix} ASComb WHERE ASComb.Semester={semester} AND ASComb.Student_id=ARGPersonalInformation{prgPostFix}.Student_ID)  SemesterBatch,Batch,RESULT.InternalUpdatedOn,
                                      RESULT._ID,RESULT.Subject_ID As Subject_ID,RESULT.ExternalMarks,RESULT.ExternalAttendance_AssessmentMarks,RESULT.InternalMarks,RESULT.InternalAttendance_AssessmentMarks,
                                        RESULT.ExternalSubmitted,RESULT.InternalSubmitted,Course_ID
                                      FROM dbo.ARGPersonalInformation{prgPostFix} 
									  INNER JOIN ARGStudentAdditionalSubjects{prgPostFix} ON ARGStudentAdditionalSubjects{prgPostFix}.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID
                                        AND ARGStudentAdditionalSubjects{prgPostFix}.Subject_ID='{SubjectID}' AND ARGStudentAdditionalSubjects{prgPostFix}.Semester={semester}
                                       LEFT JOIN {tableName} RESULT ON ARGPersonalInformation{prgPostFix}.Student_ID  =  RESULT.Student_ID AND RESULT.Subject_ID ='{SubjectID}'
                                    ";
            string query3 = "";
            if (IsSkillORGE && Convert.ToInt32(semester) < 7)
            {
                Programme getProg = programmme == Programme.IG || programmme == Programme.HS || programmme == Programme.Professional ? Programme.UG : Programme.IG;
                query3 = " Union " + GetStudents(semester, SubjectID, getProg, isBackklog);
            }
            string query = "select * from (" + query1 + " Union " + query2 + query3 + ") award ";
            SqlCommand command = new SqlCommand();
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AwardModel>(parameter.Filters);
            if (IsBacklog)
            {
                if (!hasExaminationFee)
                {
                    // query += $@"WHERE SemesterBatch<{batch} AND CombinationSubjects  LIKE  '%{SubjectID}%' 
                    //AND ( dbo.GetInternalResult((SELECT  TOP 1 SubjectMarksStructure_ID FROM VW_SubjectWithStructure WHERE subject_id='{SubjectID}'),Student_ID,{semester},'{SubjectID}') = 0
                    //OR dbo.GetExternalResult((SELECT  TOP 1 SubjectMarksStructure_ID FROM VW_SubjectWithStructure WHERE subject_id = '{SubjectID}'),Student_ID,{semester},'{SubjectID}') = 0
                    //OR (ResultNotification_ID IS NULL AND ExternalMarks<=0 AND YEAR(InternalUpdatedON)=Year(GetDate())))";
                    query += $@"WHERE SemesterBatch<{batch} AND CombinationSubjects  LIKE  '%{SubjectID}%'";
                }
                else
                {
                    //query += $@"WHERE SemesterBatch<{batch} AND CombinationSubjects  LIKE  '%{SubjectID}%' " +
                    //    $"AND ( dbo.GetInternalResult((SELECT  TOP 1 SubjectMarksStructure_ID FROM VW_SubjectWithStructure WHERE subject_id='{SubjectID}'),Student_ID,{semester},'{SubjectID}') = 0 " +
                    //    $"OR (  (ResultNotification_ID IS NULL AND ExternalMarks<=0 AND YEAR(InternalUpdatedON)=Year(GetDate()))) )";
                    query += $@"WHERE SemesterBatch<{batch} AND CombinationSubjects  LIKE  '%{SubjectID}%'";
                }

            }
            else
            { query += $"WHERE SemesterBatch= {batch} AND CombinationSubjects  LIKE  '%{SubjectID}%' "; }
            if (!(AppUserHelper.College_ID == Guid.Empty) && !(AppUserHelper.College_ID == null))
            { query += $" AND AcceptCollege_ID ='{AppUserHelper.College_ID}' "; }
            query += $" AND (IsInternalPassed=0 OR IsInternalPassed IS NULL)";
            if ((!string.IsNullOrEmpty(Courses) && !isPrint) && (!(AppUserHelper.College_ID == Guid.Empty) && !(AppUserHelper.College_ID == null)))
            {
                string coursesarr = Courses.Replace(",", "','");
                query += $" AND Course_ID IN('{coursesarr}') ";
            }
            string subquery = GetSubQueryForAward(marksFor, SubjectID, ref Column);
            query += " " + subquery;
            query += $" AND award.FormStatus={(int)FormStatus.Selected} ";
            parameter.SortInfo = parameter.SortInfo ?? new Sort();
            string sortColumn = parameter.SortInfo.ColumnName ?? Column;
            string orderby = (parameter.SortInfo.OrderBy.ToString() ?? SortOrder.Descending.ToString()).ToLower().Replace("ending", "");
            string sortQ = parameter.SortInfo.IsAlphaNumeric ? new GeneralFunctions().SortAplhaNumeric(sortColumn, parameter.SortInfo.OrderBy) : $" Order By  {sortColumn} {orderby}";

            helper.Command.CommandText = query + " " + sortQ;
            parameter.Filters.Add(semfilter);
            return helper.Command;

        }

        internal SqlCommand checkHasDate(Guid awardFilterSettingsID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT AwardFilterSettingsID FROM dbo.AwardFilterSettings
                                      WHERE AwardFilterSettingsID='{awardFilterSettingsID}'  AND (StartDate<=GETDATE() AND StartDate<=EndDate)  AND EndDate > GETDATE()";

            return command;
        }

        internal SqlCommand FetchAwardSettings(string id)
        {

            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT AwardSettings.*,AppUsers.College_ID,Course_ID FROM dbo.AwardSettings
                                        JOIN VW_SubjectWithStructure S
                                        ON AwardSettings.Subject_ID = S.Subject_ID
                                        JOIN AppUsers
                                        ON AppUsers.User_ID = AwardSettings.USER_ID  WHERE AwardSettings_ID='{id}' ";

            return command;

        }

        internal SqlCommand FetchAwardFilterSettings(string id)
        {

            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT * FROM dbo.AwardFilterSettings
                                          WHERE AwardFilterSettingsID='{id}' ";

            return command;


        }
        private string GetStudents(string semester, string SubjectID, Programme programme, int IsBacklog)
        {

            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string tableName = new GeneralFunctions().GetTableName(semester, programme);
            string Subject_ID = "Subject_ID";
            string Joincolumn1 = "Student_ID";
            string Joincolumn2 = "Student_ID";
            string query = $@" SELECT Distinct {IsBacklog} IsBacklog,ARGPersonalInformation{prgPostFix}.CUSRegistrationNo,FormStatus,
                                       ExamForm_ID,IsInternalPassed,ResultNotification_ID,
                                      (SELECT ExternalMinPassMarks FROM dbo.VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID)SubjectMinMarksTheory,
                                      (SELECT InternalMinPassMarks FROM dbo.VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID) SubjectMinMarksPractical,
                                      AcceptCollege_ID,
									  CurrentSemesterOrYear,
                                      CombinationSubjects,
                                      ARGPersonalInformation{prgPostFix}.FullName,
                                      ARGPersonalInformation{prgPostFix}.Student_ID,
                                      ARGPersonalInformation{prgPostFix}.ClassRollNo,
                                       ARGSelectedCombination{prgPostFix}.SemesterBatch,Batch,RESULT.InternalUpdatedOn,
                                      RESULT._ID,RESULT.{Subject_ID} As Subject_ID,RESULT.ExternalMarks,RESULT.ExternalAttendance_AssessmentMarks,RESULT.InternalMarks,RESULT.InternalAttendance_AssessmentMarks,
                                        RESULT.ExternalSubmitted,RESULT.InternalSubmitted,Course_ID
                                      FROM dbo.ARGPersonalInformation{prgPostFix} 
									  INNER JOIN dbo.ARGSelectedCombination{prgPostFix} ON ARGSelectedCombination{prgPostFix}.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID
                                       AND ARGSelectedCombination{prgPostFix}.Semester={semester} AND ARGSelectedCombination{prgPostFix}.Semester<= ARGPersonalInformation{prgPostFix}.CurrentSemesterOrYear
									  INNER JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{prgPostFix}.Combination_ID
                                      LEFT JOIN {tableName} RESULT ON ARGPersonalInformation{prgPostFix}.{Joincolumn1}  =  RESULT.{Joincolumn2} AND RESULT.{Subject_ID} = '{SubjectID}'
                                        ";
            return query;
        }

        internal SqlCommand GetAwardFilterSettings(Programme programme, MarksFor marksFor, int Semester)
        {

            SqlCommand command = new SqlCommand();
            string query = @"SELECT * FROM dbo.AwardFilterSettings 
                                     WHERE Semester=@Semester AND AwardType=@AwardType and Programme=@programme ";
            command.CommandText = query;
            command.Parameters.AddWithValue("@Semester", Semester);
            command.Parameters.AddWithValue("@AwardType", marksFor.ToString());
            command.Parameters.AddWithValue("@Programme", (int)programme);
            return command;
        }

        internal SqlCommand GetAllRecordsExt(Parameters parameter, Programme programmme, MarksFor marksFor, int Year, string Courses, bool isPrint, SubjectType subjectType, bool isResultDeclared)
        {
            SearchFilter semfilter = parameter.Filters.Where(x => x.Column == "Semester").FirstOrDefault();
            string semester = semfilter.Value;
            SearchFilter subfilter = parameter.Filters.Where(x => x.Column == "CombinationSubjects").FirstOrDefault();
            string SubjectID = subfilter.Value;
            String Column = "";
            bool IsSkillORGE = (subjectType == SubjectType.SEC);

            parameter.Filters.Remove(semfilter);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programmme);
            string tableName = new GeneralFunctions().GetTableName(semester, programmme);

            List<AppUserEvaluatorSubjects> AppUserEvaluatorSubjects = new AwardDB().GetEvaluator(new Guid(SubjectID));



            string subquery2 = GetSubQueryForAward(marksFor, SubjectID, ref Column);

            string query1 = GetExtAwardQuery(prgPostFix, tableName, semester, SubjectID, "Subject_ID", "Student_ID", "Student_ID", Year);
            string query;

            string query3 = "";
            if (IsSkillORGE && Convert.ToInt32(semester) < 7)
            {

                Programme getProg = programmme == Programme.IG || programmme == Programme.HS || programmme == Programme.Professional ? Programme.UG : Programme.IG;
                string NewtableName = new GeneralFunctions().GetTableName(semester, getProg);
                string prgPostFix2 = new GeneralFunctions().GetProgrammePostFix(getProg);
                query3 = " Union " + GetExtAwardQuery(prgPostFix2, NewtableName, semester, SubjectID, "Subject_ID", "Student_ID", "Student_ID", Year);
            }

            query1 = query1 + query3;
            query1 = "select * from (" + query1 + ") award2 ";

            query1 += $"WHERE  CombinationSubjects  LIKE  '%{SubjectID}%' ";
            query1 += " " + subquery2 + " ";
            //if (AppUserHelper.College_ID.HasValue)
            //{
            //    query1 += $" AND AcceptCollege_ID ='{AppUserHelper.College_ID}' ";
            //}
            if (isPrint && isResultDeclared)
            {
                query1 += $" AND  (ExternalUpdatedBy = '{AppUserHelper.User_ID}') ";
            }
            else
            { query1 += $" AND  (IsExternalPassed = 0  OR IsExternalPassed IS NULL)"; }


            if ((!string.IsNullOrEmpty(Courses) && !isPrint) && (subjectType != SubjectType.SEC))
            {
                string coursesarr = Courses.Replace(",", "','");
                query1 += $" AND Course_ID IN('{coursesarr}') ";
            }
            parameter.SortInfo = parameter.SortInfo ?? new Sort();
            string sortColumn = parameter.SortInfo.ColumnName ?? Column;
            string orderby = (parameter.SortInfo.OrderBy.ToString() ?? SortOrder.Descending.ToString()).ToLower().Replace("ending", "");
            string sortQ = parameter.SortInfo.IsAlphaNumeric ? new GeneralFunctions().SortAplhaNumeric(sortColumn, parameter.SortInfo.OrderBy) : $" Order By  {sortColumn} {orderby}";


            query = $"Select * FROM  ({query1} )tem  " + sortQ;
            SqlCommand command = new SqlCommand
            {
                CommandText = query
            };
            return command;
        }
        internal string GetExtAwardQuery(string prgPostFix, string tableName, string semester, string SubjectID, string SubjectIDCol, string jc1, string jc2, int Year)
        {
            string FilterColumn = "Year";
            int FilterValue = Year;
            string ExamFormWhere = GetExamFormWhere(SubjectID, semester, FilterColumn, Year, "EXMFRM");
            string query1 = $@" SELECT Distinct ARGPersonalInformation{prgPostFix}.CUSRegistrationNo,IsExternalPassed,
                                        ResultNotification_ID,EXMFRM.StudentExamForm_ID AS ExamForm_ID,
                                       (SELECT ExternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID)SubjectMinMarksTheory,
                                      (SELECT InternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID) SubjectMinMarksPractical,
                                      AcceptCollege_ID,
									  CurrentSemesterOrYear,
                                      CombinationSubjects,
                                      ARGPersonalInformation{prgPostFix}.FullName,
                                      ARGPersonalInformation{prgPostFix}.Student_ID,
                                      ARGPersonalInformation{prgPostFix}.ClassRollNo,EXMFRM.ExamRollNumber,
                                      EXMFRM.StudentCode,
                                     dbo.ARGSelectedCombination{prgPostFix}.SemesterBatch Batch,
                                      RESULT._ID,RESULT.{SubjectIDCol} As Subject_ID,ISNULL(RESULT.ExternalMarks,-3) ExternalMarks,RESULT.ExternalAttendance_AssessmentMarks,RESULT.InternalMarks,RESULT.InternalAttendance_AssessmentMarks,
                                        RESULT.ExternalSubmitted,RESULT.InternalSubmitted,ADMCombinationMaster.Course_ID,Result.ExternalUpdatedBy
                                   
                                      FROM dbo.ARGPersonalInformation{prgPostFix} 
									  INNER JOIN dbo.ARGSelectedCombination{prgPostFix} ON ARGSelectedCombination{prgPostFix}.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID
                                       AND ARGSelectedCombination{prgPostFix}.Semester={semester}
									  INNER JOIN dbo.ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{prgPostFix}.Combination_ID
                                      Inner JOIN ARGStudentExamForm{prgPostFix} EXMFRM ON EXMFRM.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID {ExamFormWhere}
                                      LEFT JOIN {tableName} RESULT ON ARGPersonalInformation{prgPostFix}.{jc1}  =  RESULT.{jc2} AND RESULT.{SubjectIDCol} = '{SubjectID}'
                                        ";
            ///Additional Subjects
            string query2 = $@"SELECT Distinct ARGPersonalInformation{prgPostFix}.CUSRegistrationNo,IsExternalPassed,
                                      ResultNotification_ID,EXMFRM.StudentExamForm_ID AS ExamForm_ID,
                                      (SELECT ExternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID) SubjectMinMarksTheory,
                                      (SELECT InternalMinPassMarks FROM VW_SubjectWithStructure WHERE Subject_ID=RESULT.Subject_ID) SubjectMinMarksPractical,
                                      AcceptCollege_ID,
									  CurrentSemesterOrYear,
                                      '{SubjectID}' AS CombinationSubjects,
                                       ARGPersonalInformation{prgPostFix}.FullName,
                                      ARGPersonalInformation{prgPostFix}.Student_ID,
                                      ARGPersonalInformation{prgPostFix}.ClassRollNo,EXMFRM.ExamRollNumber,
                                      EXMFRM.StudentCode,
                                       ARGPersonalInformation{prgPostFix}.Batch,
                                      RESULT._ID,RESULT.Subject_ID As Subject_ID,ISNULL(RESULT.ExternalMarks,-3) ExternalMarks,RESULT.ExternalAttendance_AssessmentMarks,RESULT.InternalMarks,RESULT.InternalAttendance_AssessmentMarks,
                                        RESULT.ExternalSubmitted,RESULT.InternalSubmitted,Course_ID,Result.ExternalUpdatedBy
                                      FROM dbo.ARGPersonalInformation{prgPostFix} 
									  INNER JOIN ARGStudentAdditionalSubjects{prgPostFix} ON ARGStudentAdditionalSubjects{prgPostFix}.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID
                                       AND ARGStudentAdditionalSubjects{prgPostFix}.Subject_ID='{SubjectID}' AND ARGStudentAdditionalSubjects{prgPostFix}.Semester={semester}
                                      Inner JOIN ARGStudentExamForm{prgPostFix} EXMFRM ON EXMFRM.Student_ID = ARGPersonalInformation{prgPostFix}.Student_ID {ExamFormWhere}
									   LEFT JOIN {tableName} RESULT ON ARGPersonalInformation{prgPostFix}.Student_ID  =  RESULT.Student_ID AND RESULT.Subject_ID ='{SubjectID}'
                                    ";
            string query = "select * from (" + query1 + " Union " + query2 + ") award ";
            return query;
        }
        internal SqlCommand GetEvaluator(Guid SubjectID, Guid user_ID)
        {
            SqlCommand command = new SqlCommand();
            string query = @"SELECT * FROM dbo.AppUserEvaluatorSubjects 
                                     WHERE User_ID=@User_ID AND Subject_ID=@Subject_ID ";
            command.CommandText = query;
            command.Parameters.AddWithValue("@User_ID", user_ID);
            command.Parameters.AddWithValue("@Subject_ID", SubjectID);

            return command;
        }
        public string GetExamFormWhere(string Subject_ID, string semester, string FilterColumn, int FilterValue, string ExamFormAlias)
        {
            string query;
            query = $@" AND {ExamFormAlias}.Semester={semester}
                      AND {ExamFormAlias}.{FilterColumn}={FilterValue} AND ({ExamFormAlias}.StudentCode IS NOT NULL OR {ExamFormAlias}.ExamRollNumber IS NOT NULL)
                      AND {ExamFormAlias}.Status = 4  AND({ExamFormAlias}.IsRegular = 1 OR ({ExamFormAlias}.IsRegular = 0 AND
                      EXISTS(SELECT Subject_ID FROM ARGStudentReExamForm RE WHERE Re.StudentExamForm_ID = {ExamFormAlias}.StudentExamForm_ID 
                      AND Re.Subject_ID = '{Subject_ID}' AND FeeStatus IN({(int)FormStatus.FeePaid},{(int)FormStatus.UpdatedManually})))) ";
            return query;

        }
        internal SqlCommand GetEvalvatorSubjects(Programme programme, Guid user_ID)
        {
            //PrintProgramme printprogramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            SqlCommand command = new SqlCommand();
            string query = @"SELECT AppUserEvaluatorSubjects.*,CourseCategory,S.Course_ID FROM dbo.AppUserEvaluatorSubjects 
                                    INNER JOIN VW_SubjectWithStructure S ON S.Subject_ID = AppUserEvaluatorSubjects.Subject_ID
                                    INNER JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID 
                                    WHERE User_ID=@User_ID  AND ADMCourseMaster.Programme=@Programme and AppUserEvaluatorSubjects.Status=1";

            //if (printprogramme == PrintProgramme.UG)
            //{
            //    query = query + " AND (PrintProgramme=@Programme OR PrintProgramme=@PProgramme )";
            //    command.Parameters.AddWithValue("@PProgramme", PrintProgramme.BED); ;
            //}
            //else
            //{
            //    query = query + " AND Programme=@Programme";
            //}
            command.CommandText = query;
            command.Parameters.AddWithValue("@User_ID", user_ID);
            command.Parameters.AddWithValue("@Programme", programme);
            return command;
        }

        internal SqlCommand GetProfessorSubjects(Programme programme, Guid user_ID)
        {
            //PrintProgramme printprogramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            SqlCommand command = new SqlCommand();
            string query = @"SELECT AppUserProfessorSubjects.*,CourseCategory,ADMCourseMaster.Course_ID FROM dbo.AppUserProfessorSubjects 
                                    INNER JOIN VW_SubjectWithStructure s ON S.Subject_ID = AppUserProfessorSubjects.Subject_ID
                                    INNER JOIN dbo.ADMCourseMaster ON ADMCourseMaster.Course_ID = S.Course_ID 
                                    WHERE User_ID=@User_ID AND ADMCourseMaster.Programme=@Programme";
            //if (printprogramme == PrintProgramme.UG)
            //{
            //    query = query + " AND (PrintProgramme=@Programme OR PrintProgramme=@PProgramme )";
            //    command.Parameters.AddWithValue("@PProgramme", PrintProgramme.BED); ;
            //}
            //else
            //{
            //    query = query + " AND PrintProgramme=@Programme";
            //}
            command.CommandText = query;
            command.Parameters.AddWithValue("@User_ID", user_ID);
            command.Parameters.AddWithValue("@Programme", programme);
            return command;
        }

        internal SqlCommand GetProfessor(Guid SubjectID, Guid user_ID)
        {

            SqlCommand command = new SqlCommand();
            string query = @"SELECT * FROM dbo.AppUserProfessorSubjects 
                                     WHERE User_ID=@User_ID AND Subject_ID=@Subject_ID ";
            command.CommandText = query;
            command.Parameters.AddWithValue("@User_ID", user_ID);
            command.Parameters.AddWithValue("@Subject_ID", SubjectID);

            return command;
        }
        internal SqlCommand UpdateSubmitAward(SubmitAward input, string Subject_IDCol)
        {
            string FilterColumn;
            int FilterValue;
            if (input.MarksFor == MarksFor.Practical)
            {
                FilterColumn = "Batch";
                FilterValue = input.Batch;
            }
            else
            {
                FilterColumn = "Year";
                FilterValue = input.Year;
            }
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(input.Programme);

            String Column = "";
            string tablename = new GeneralFunctions().GetTableName(input.Semester, input.Programme);
            SqlCommand command = new SqlCommand();
            string query = $@"Update {tablename}
                                SET {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}Submitted=1,
                                     {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}UpdatedON=GETDATE(),
                                     {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}UpdatedBy='{AppUserHelper.User_ID}'
                                WHERE {Subject_IDCol}='{input.CombinationSubjects}'
                                AND  Student_ID IN (";
            string squery;
            if (input.MarksFor == MarksFor.Practical)
            {
                squery = $"SELECT Student_ID FROM dbo.ARGPersonalInformation{prgPostFix} WHERE  ({FilterColumn}={FilterValue}) OR ( {FilterColumn}<{FilterValue})" +
                    $"  AND AcceptCollege_ID ='{AppUserHelper.College_ID}'";
            }
            else
            {
                string ExamFormWhere = GetExamFormWhere(input.CombinationSubjects, input.Semester, FilterColumn, FilterValue, "EXMFRM");
                ExamFormWhere = ExamFormWhere + $" AND AcceptCollege_ID = '{AppUserHelper.College_ID}'";
                squery = $@"SELECT STDINF.Student_ID FROM dbo.ARGPersonalInformation{prgPostFix} STDINF INNER JOIN ARGStudentExamForm{prgPostFix} EXMFRM
                               ON STDINF.Student_ID=EXMFRM.Student_ID {ExamFormWhere}";
            }
            query += squery;
            string subquery = GetSubQueryForAward(input.MarksFor, input.CombinationSubjects, ref Column);
            query += " " + subquery;

            command.CommandText = query + " )  AND ResultNotification_ID IS NULL";
            return command;
        }
        internal SqlCommand UpdateAward(SubmitAward input, List<string> Student_id, string Subject_IDCol)
        {
#pragma warning disable CS0219
            string FilterColumn;
#pragma warning restore CS0219

            int FilterValue;
            if (input.MarksFor == MarksFor.Practical)
            {
                FilterColumn = "Batch";
                FilterValue = input.Batch;
            }
            else
            {
                FilterColumn = "Year";
                FilterValue = input.Year;
            }
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(input.Programme);

            String Column = "";
            string tablename = new GeneralFunctions().GetTableName(input.Semester, input.Programme);
            SqlCommand command = new SqlCommand();
            string query = $@"Update {tablename}
                                SET {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}Submitted=1,
                                      {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}UpdatedON='{DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss")}',
                                      {(input.MarksFor == MarksFor.Practical ? "Internal" : "External")}UpdatedBy='{AppUserHelper.User_ID}'
                                WHERE {Subject_IDCol}='{input.CombinationSubjects}'
                                AND  Student_ID IN ('";

            query += String.Join("','", Student_id);
            string subquery = GetSubQueryForAward(input.MarksFor, input.CombinationSubjects, ref Column);
            query += " " + subquery;

            command.CommandText = query + "')  AND ResultNotification_ID IS NULL";
            return command;
        }

        internal SqlCommand FetchAwardSettings(SubmitAward input)
        {
            string FilterColumn;
            int FilterValue;
            if (input.MarksFor == MarksFor.Practical)
            {
                FilterColumn = "Batch";
                FilterValue = input.Batch;
            }
            else
            {
                FilterColumn = "Year";
                FilterValue = input.Year;
            }
            string semester = input.Semester;
            string SubjectID = input.CombinationSubjects;

            SqlCommand command = new SqlCommand();
            command.CommandText = $"SELECT * FROM dbo.AwardSettings  WHERE Subject_ID='{SubjectID}' AND {FilterColumn} = {FilterValue} AND USER_ID = '{AppUserHelper.User_ID}' AND Semester = {semester}";

            return command;
        }
        internal SqlCommand CheckAwardExists(SubmitAward input)
        {
            string FilterColumn;
            int FilterValue;

            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(input.Programme);
            string tablename = new GeneralFunctions().GetTableName(input.Semester, input.Programme);
            SqlCommand command = new SqlCommand();
            string query = $@"Select Count(Student_ID)
                                    from {tablename}
                                WHERE Subject_ID='{input.CombinationSubjects}' AND  
                                Student_ID IN (";
            string squery;
            if (input.MarksFor == MarksFor.Practical)
            {
                FilterColumn = "Batch";
                FilterValue = input.Batch;
                squery = $"SELECT Student_ID FROM dbo.ARGPersonalInformation{prgPostFix} WHERE   {FilterColumn}={FilterValue}";
            }
            else
            {
                FilterColumn = "Year";
                FilterValue = input.Year;
                string ExamFormWhere = GetExamFormWhere(input.CombinationSubjects, input.Semester, FilterColumn, FilterValue, "EXMFRMs");
                squery = $@"SELECT STDINF.Student_ID FROM dbo.ARGPersonalInformation{prgPostFix} STDINF INNER JOIN ARGStudentExamForm{prgPostFix} ExMFRMs
                               ON STDINF.Student_ID=EXMFRMs.Student_ID {ExamFormWhere}";
            }
            query += squery;
            command.CommandText = query + " )";
            return command;
        }
        internal SqlCommand CheckAwardSubmitted(int BatchOrYear, Parameters parameter, Programme programme, MarksFor marksFor)
        {
            string FilterColumn;
            SearchFilter semfilter = parameter?.Filters?.Where(x => x.Column == "Semester").FirstOrDefault() ?? new SearchFilter();
            string semester = semfilter?.Value ?? string.Empty;
            SearchFilter subfilter = parameter?.Filters?.Where(x => x.Column == "CombinationSubjects").FirstOrDefault() ?? new SearchFilter();
            string SubjectID = subfilter?.Value ?? string.Empty;
            string column = "ISFinalSubmit" + (marksFor == MarksFor.Theory ? marksFor.ToString() : "");
            if (marksFor == MarksFor.Practical)
            { FilterColumn = "Batch"; }
            else
            { FilterColumn = "Year"; }
            SqlCommand command = new SqlCommand();
            command.CommandText = $"SELECT {column} FROM dbo.AwardSettings  WHERE Subject_ID='{SubjectID}' AND {FilterColumn} = {BatchOrYear} AND USER_ID = '{AppUserHelper.User_ID}' AND Semester = {semester}";
            return command;
        }
        internal SqlCommand checkIsResultDeclared(int Semester, Programme programme)
        {
            PrintProgramme printProgramme = new GeneralFunctions().GetPrintProgramme(programme);
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" select Count(ResultNotification_ID) from ResultNotification where semester={Semester} and PrintProgramme = {(int)printProgramme} and batch =
                                    (select FilterValue from AwardFilterSettings where semester = {Semester} and programme ={(int)programme} and AwardType = 'Practical')";
            return command;
        }

        internal string GetSubQueryForAward(MarksFor marksFor, string SubjectId, ref string Column)
        {
            string MinRollNo;
            string MaxRollNo;
            string subquery = string.Empty;
            if (marksFor == MarksFor.Practical)
            {
                List<AppUserProfessorSubjects> appUserProfessorSubjects = new AwardDB().GetProfessor(new Guid(SubjectId));
                appUserProfessorSubjects = appUserProfessorSubjects.Where(x => !String.IsNullOrEmpty(x.RollNoFrom) && !String.IsNullOrEmpty(x.RollNoTo) && x.RollNoTo != "0" && x.RollNoFrom != "0").ToList();
                if (appUserProfessorSubjects?.Count() > 0)
                {
                    Column = "ClassRollNo";
                    subquery += " AND ((1=0) ";

                    foreach (AppUserProfessorSubjects aps in appUserProfessorSubjects ?? new List<AppUserProfessorSubjects>())
                    {
                        MinRollNo = aps.RollNoFrom;
                        MaxRollNo = aps.RollNoTo;
                        if (!string.IsNullOrEmpty(MinRollNo) && !string.IsNullOrEmpty(MaxRollNo))
                        {
                            subquery += "OR";
                            subquery += $"({Column}  IS NOT NULL AND  {Column} BETWEEN '{MinRollNo}' AND '{MaxRollNo}')";
                        }
                    }
                    subquery += "  ) ";
                }
            }
            else
            {
                List<AppUserEvaluatorSubjects> AppUserEvaluatorSubjects = new AwardDB().GetEvaluator(new Guid(SubjectId));

                int cnt = 1;
                int ttl = 0;
                foreach (AppUserEvaluatorSubjects aes in AppUserEvaluatorSubjects ?? new List<AppUserEvaluatorSubjects>())
                {
                    if (!string.IsNullOrEmpty(aes.StudentCodeFrom) && !string.IsNullOrEmpty(aes.StudentCodeTo))
                    {
                        ttl = AppUserEvaluatorSubjects != null ? AppUserEvaluatorSubjects.Where(obj => !string.IsNullOrEmpty(obj.StudentCodeFrom) && !string.IsNullOrEmpty(obj.StudentCodeTo)).Count() : 0;

                        Column = "StudentCode";
                        MinRollNo = aes.StudentCodeFrom;
                        MaxRollNo = aes.StudentCodeTo;
                    }
                    else
                    {
                        ttl = AppUserEvaluatorSubjects != null ? AppUserEvaluatorSubjects.Where(obj => !string.IsNullOrEmpty(obj.ExamRollNoFrom) && !string.IsNullOrEmpty(obj.ExamRollNoTo)).Count() : 0;


                        Column = "ExamRollNumber";
                        MinRollNo = aes.ExamRollNoFrom;
                        MaxRollNo = aes.ExamRollNoTo;
                    }
                    if (!string.IsNullOrEmpty(MinRollNo) && !string.IsNullOrEmpty(MaxRollNo))
                    {
                        subquery += (cnt != 1) ? "OR " : string.Empty;
                        subquery += (cnt == 1) ? " AND (" : string.Empty;
                        subquery += $"({Column}  IS NOT NULL AND ({Column}  BETWEEN '{MinRollNo}' AND '{MaxRollNo}' OR {Column}  IN('{MinRollNo}' , '{MaxRollNo}'))";
                        if (!string.IsNullOrEmpty(aes.Colleges))
                        {
                            List<Guid> collegelist = aes.Colleges.ToGuidList(',');
                            subquery += " AND AcceptCollege_ID IN (" + collegelist.ToIN() + ") ";
                        }
                        subquery += $")";
                        subquery += (cnt == ttl) ? " )" : string.Empty;
                        cnt++;
                    }

                }

            }
            return subquery;
        }
        internal SqlCommand GetAllAwardSetting(Parameters parameter)
        {

            string Query = $@"SELECT AwardSettings_ID,UserName,SubjectFullName,ISFinalSubmit,IsFinalSubmittheory from AwardSettings
                                JOIN VW_SubjectWithStructure S
                                ON S.Subject_ID = AwardSettings.SUBJECT_ID  AND S.Semester = AwardSettings.Semester
                                JOIN AppUsers
                                ON AppUsers.User_ID = AwardSettings.USER_ID";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AwardSettingsModel>(parameter.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return helper.Command;

        }
        internal SqlCommand GetAllAwardFilters(Parameters parameter)
        {
            parameter.Filters.Where(x => x.Column == "AwardType").FirstOrDefault().Value = ((AwardType)Enum.Parse(typeof(AwardType), parameter.Filters.Where(x => x.Column == "AwardType").FirstOrDefault().Value, true)).ToString();
            string Query = $@"SELECT * FROM dbo.AwardFilterSettings";
            //,(Stuff((Select ', ' + CourseFullName  FROM dbo.ADMCourseMaster WHERE Course_ID IN(''''+REPLACE(courses,',',''',''')+'''') FOR XML PATH('')),1,2,'')) CoursesName
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AwardFilterSettings>(parameter.Filters);
            Query += helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            return helper.Command;

        }
        internal SqlCommand GetAwardFilter(Guid id)
        {
            string Query = $@"SELECT * FROM dbo.AwardFilterSettings where AwardFilterSettingsID='{id}'";
            SqlCommand com = new SqlCommand();
            com.CommandText = Query;
            return com;

        }
    }
}
