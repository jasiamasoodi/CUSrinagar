using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.Extensions;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;
using System.Web.Mvc;

namespace CUSrinagar.DataManagers
{
    public class AwardDB
    {
        public List<AwardModel> GetAllRecords(Parameters parameter, Programme programme, MarksFor marksFor, int batch, SubjectType subjectType, bool hasExaminationFee, bool IsBacklog, string Courses, bool isPrint)
        {
            return new MSSQLFactory().GetObjectList<AwardModel>(new AwardSQLQueries().GetAllRecords(parameter, programme, marksFor, batch, subjectType, hasExaminationFee, IsBacklog, Courses, isPrint));
        }
        public List<AwardModel> GetAllRecordsExt(Parameters parameter, Programme programme, MarksFor marksFor, int Year, string Courses, bool isPrint, SubjectType subjectType, bool isResultDeclared)
        {
            return new MSSQLFactory().GetObjectList<AwardModel>(new AwardSQLQueries().GetAllRecordsExt(parameter, programme, marksFor, Year, Courses, isPrint, subjectType, isResultDeclared));
        }
        public List<AppUserProfessorSubjects> GetProfessorSubjects(Programme programme, Guid user_ID)
        {
            return new MSSQLFactory().GetObjectList<AppUserProfessorSubjects>(new AwardSQLQueries().GetProfessorSubjects(programme, user_ID));
        }
        public List<AppUserProfessorSubjects> GetProfessor(Guid SubjectID)
        {
            return new MSSQLFactory().GetObjectList<AppUserProfessorSubjects>(new AwardSQLQueries().GetProfessor(SubjectID, AppUserHelper.User_ID));
        }
        public List<AppUserEvaluatorSubjects> GetEvaluator(Guid SubjectID)
        {
            return new MSSQLFactory().GetObjectList<AppUserEvaluatorSubjects>(new AwardSQLQueries().GetEvaluator(SubjectID, AppUserHelper.User_ID));
        }

        public List<AppUserEvaluatorSubjects> GetEvalvatorSubjects(Programme programme, Guid user_ID)
        {
            return new MSSQLFactory().GetObjectList<AppUserEvaluatorSubjects>(new AwardSQLQueries().GetEvalvatorSubjects(programme, user_ID));
        }


        internal int EditAwardSettings(AwardSettingsModel input, List<string> ignoreQuery, List<string> ignoreParameter)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<AwardSettingsModel>(input, ignoreQuery, ignoreParameter, "AwardSettings");
            sqlCommand.CommandText += " WHERE AwardSettings_ID=@AwardSettings_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public IEnumerable<AwardSettingsModel> GetAllAwardSetting(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AwardSettingsModel>(new AwardSQLQueries().GetAllAwardSetting(parameter));
        }

        public bool ReleaseAward(string id, bool Status)
        {
            string query = "UPDATE AwardSettings SET ISFinalSubmit=@Status WHERE AwardSettings_ID=@AwardSettings_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@AwardSettings_ID", id);
            cmd.Parameters.AddWithValue("@Status", Status);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        public bool ReleaseAwardExt(string id, bool Status)
        {
            string query = "UPDATE AwardSettings SET ISFinalSubmitTheory=@Status WHERE AwardSettings_ID=@AwardSettings_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@AwardSettings_ID", id);
            cmd.Parameters.AddWithValue("@Status", Status);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }
        public bool ReleaseUserSubjects(AwardSettingsModel model, Programme programme)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string tableName = new GeneralFunctions().GetTableName(model.Semester.ToString(), programme);
            string query = $@"UPDATE Sem
                             SET Sem.InternalSubmitted = @Status
                             FROM {tableName} Sem
                                JOIN ARGPersonalInformation{prgPostFix} PI
                            ON PI.Student_ID = Sem.Student_ID
                            JOIN ARGSelectedCombination{prgPostFix} SC
                            ON SC.Student_ID = Sem.Student_ID AND SC.Semester={ model.Semester}
                            WHERE Sem.InternalUpdatedBy = @User_Id AND
                               Subject_ID = @Subject_ID AND SemesterBatch = @Batch
                             AND AcceptCollege_ID = @College_ID";
            query += $@"   UPDATE Sem
                             SET Sem.InternalSubmitted = @Status
                             FROM {tableName}  Sem
                            JOIN ARGPersonalInformation{prgPostFix} PI
                            ON PI.Student_ID = Sem.Student_ID
                                JOIN ARGSelectedCombination{ prgPostFix}  SC
                                ON SC.Student_ID = Sem.Student_ID AND SC.Semester={ model.Semester}
                                                            WHERE Sem.InternalUpdatedBy = @User_Id AND
                                                               Subject_ID = @Subject_ID   AND AcceptCollege_ID = @College_ID AND SemesterBatch<@Batch
                                         AND ( dbo.GetInternalResult((SELECT  TOP 1 SubjectMarksStructure_ID FROM admsubjectmaster WHERE subject_id=@Subject_ID),Sem.Student_ID,{model.Semester},@Subject_ID)=0 OR ResultNotification_ID IS NULL)";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", model.USER_ID);
            cmd.Parameters.AddWithValue("@Status", !model.ISFinalSubmit);
            cmd.Parameters.AddWithValue("@Subject_ID", model.SUBJECT_ID);
            cmd.Parameters.AddWithValue("@Batch", model.Batch);
            cmd.Parameters.AddWithValue("@College_ID", model.College_Id);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public bool UpdateAwardStatus(AwardFilterSettings model, string type)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(model.Programme);
            string tableName = new GeneralFunctions().GetTableName(((int)model.Semester).ToString(), model.Programme);
            //PrintProgramme printProgramme = new GeneralFunctions().GetPrintProgramme(model.Programme);
            string query = string.Empty;
            string subquery = model.CourseList?.Count() > 0 ? $" and ADMSubjectMaster.Course_ID IN('{  string.Join("','", model.CourseList)}') " : $"  AND Programme IN({(int)model.Programme})  ";
            subquery = subquery + (model.CollegeList?.Count() > 0 ? $" and AcceptCollege_ID IN ('{  string.Join("','", model.CollegeList)}') " : "");
            if (type == "internal")
            { query = $@"UPDATE {tableName} SET Is{type}Passed=1  WHERE _ID IN 
                                (SELECT _ID FROM VWStudentResultAllSemesters JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWStudentResultAllSemesters.Subject_ID 
                                JOIN VWStudentCourse ON VWStudentCourse.Student_ID = VWStudentResultAllSemesters.Student_ID AND ADMSubjectMaster.Semester={(int)model.Semester}
                                AND ( IsClearAndLocked=0  OR IsClearAndLocked IS NULL) 
                                --AND InternalMarks>=dbo.GetInternalMinMarks(SubjectMarksStructure_ID) 
                                 AND dbo.GetInternalResult(SubjectMarksStructure_ID,VWStudentResultAllSemesters.Student_ID,ADMSubjectMaster.Semester,ADMSubjectMaster.Subject_ID)=1
                                AND is{type}passed=0  " + subquery + ")"; }

            else
            {
                query = $@"UPDATE {tableName} SET Is{type}Passed=1  WHERE _ID IN 
                                (SELECT _ID FROM VWStudentResultAllSemesters JOIN ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = VWStudentResultAllSemesters.Subject_ID 
                                JOIN VWStudentCourse ON VWStudentCourse.Student_ID = VWStudentResultAllSemesters.Student_ID AND ADMSubjectMaster.Semester={(int)model.Semester} 
                                AND ( IsClearAndLocked=0  OR IsClearAndLocked IS NULL) 
                                AND TotalInternalMarks>=dbo.GetInternalMinMarks(SubjectMarksStructure_ID)

                                AND TotalExternalMarks>=dbo.GetExternalMinMarks(SubjectMarksStructure_ID) AND is{type}passed=0   " + subquery + ")";
            }
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public bool ReleaseUserSubjectsExt(AwardSettingsModel model, Programme programme)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string tableName = new GeneralFunctions().GetTableName(model.Semester.ToString(), programme);
            string query = $@"UPDATE Sem
                             SET Sem.ExternalSubmitted = @Status
                             FROM {tableName} Sem  JOIN ARGPersonalInformation{prgPostFix} PI
                            ON PI.Student_ID = Sem.Student_ID
                                  JOIN ARGStudentExamForm{prgPostFix} EF
                            ON EF.Student_ID = Sem.Student_ID AND EF.Semester={ model.Semester} AND EF.Year=@Year
                            WHERE Sem.ExternalUpdatedBy = @User_Id AND IsRegular=1
                             AND  Subject_ID = @Subject_ID 
                             AND AcceptCollege_ID = @College_ID";
            query += $@"  UPDATE Sem
                             SET Sem.ExternalSubmitted = @Status
                             FROM {tableName} Sem  JOIN ARGPersonalInformation{prgPostFix} PI
                            ON PI.Student_ID = Sem.Student_ID
                                  JOIN ARGStudentExamForm{prgPostFix} EF
                            ON EF.Student_ID = Sem.Student_ID AND EF.Semester={ model.Semester} AND EF.Year=@Year
                            	JOIN ARGStudentReExamForm RE ON RE.StudentExamForm_ID = EF.StudentExamForm_ID AND RE.Subject_ID = '{model.SUBJECT_ID}'
                            WHERE Sem.ExternalUpdatedBy = @User_Id AND IsRegular=0
                              AND Sem.Subject_ID = @Subject_ID 
                             AND AcceptCollege_ID = @College_ID";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", model.USER_ID);
            cmd.Parameters.AddWithValue("@Status", !model.ISFinalSubmitTheory);
            cmd.Parameters.AddWithValue("@Subject_ID", model.SUBJECT_ID);
            cmd.Parameters.AddWithValue("@Year", model.Year);
            cmd.Parameters.AddWithValue("@College_ID", model.College_Id);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }


        public int EditFilter(AwardFilterSettings model, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE AwardFilterSettingsID=@AwardFilterSettingsID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        public AwardFilterSettings checkHasDate(AwardFilterSettings awardFilterSettings)
        {
            return new MSSQLFactory().GetObject<AwardFilterSettings>(new AwardSQLQueries().checkHasDate(awardFilterSettings.AwardFilterSettingsID));
        }

        public bool OpenAwardLink(string id, bool Status, DateTime EndDate)
        {
            string query = "UPDATE AwardFilterSettings SET ISActive=@Status,EndDate=@EndDate,Courses=NULL,Colleges=NULL WHERE AwardFilterSettingsID=@AwardFilterSettingsID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@AwardFilterSettingsID", id);
            cmd.Parameters.AddWithValue("@Status", Status);
            cmd.Parameters.AddWithValue("@EndDate", EndDate);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public AwardFilterSettings FetchAwardFilterSettings(string id)
        {
            return new MSSQLFactory().GetObject<AwardFilterSettings>(new AwardSQLQueries().FetchAwardFilterSettings(id));
        }

        public AwardSettingsModel FetchAwardSettings(string id)
        {
            return new MSSQLFactory().GetObject<AwardSettingsModel>(new AwardSQLQueries().FetchAwardSettings(id));
        }

        public IEnumerable<AwardFilterSettings> GetAllAwardFilters(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AwardFilterSettings>(new AwardSQLQueries().GetAllAwardFilters(parameter));
        }
        public AwardFilterSettings GetAwardFilter(Guid id)
        {
            return new MSSQLFactory().GetObject<AwardFilterSettings>(new AwardSQLQueries().GetAwardFilter(id));
        }
    }
}
