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
    public class CombinationDB
    {
        public List<ADMCombinationMaster> GetADMCombinationMastersForDateSheetClash(Programme programme, short Semester, short examYear)
        {
            string programmeIn = $" IN({(short)programme})";
            if (programme == Programme.IG)
            {
                programmeIn = $" IN({(short)Programme.IG},{(short)Programme.HS},{(short)Programme.Professional})";
            }
            PrintProgramme printProgramme = GeneralFunctions.ProgrammeToPrintProgrammeMapping(programme);
            string sqlQuery = $@"SELECT DISTINCT cm.Combination_ID,
                                   col.CollegeFullName AS CombinationCode,
                                   c.CourseFullName,
                                   c.Course_ID,
                                   cm.Semester,
                                           cm.CombinationSubjects
                            FROM dbo.ADMCombinationMaster cm
                            JOIN dbo.ARGSelectedCombination_{printProgramme} sc ON sc.Combination_ID = cm.Combination_ID AND sc.Semester={Semester}
                            JOIN dbo.ARGStudentExamForm_{printProgramme} e ON e.Student_ID=sc.Student_ID AND e.Semester={Semester} AND e.Status={((short)(FormStatus.Accepted))} AND e.Year={examYear} 
                                JOIN dbo.ADMCourseMaster c
                                    ON c.Course_ID = cm.Course_ID
                                       AND cm.Status = 1
                                       AND cm.Semester = {Semester}
                                JOIN dbo.ADMCollegeMaster col
                                    ON col.College_ID = cm.College_ID
                                       AND c.Programme {programmeIn}
		                               WHERE c.Programme {programmeIn} AND cm.Semester={Semester} AND e.Year={examYear} AND e.Semester={Semester}
                                       AND e.Status={((short)(FormStatus.Accepted))}
                            ORDER BY col.CollegeFullName,
                                     c.CourseFullName;";

            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(sqlQuery);
            #region s
            //return new MSSQLFactory().GetObjectList<ADMCombinationMaster>($@"SELECT cm.Combination_ID,
            //                                                    col.CollegeFullName AS CombinationCode,
            //                                                    c.CourseFullName,
            //                                                    c.Course_ID,
            //                                                    cm.Semester,
            //                                                    CASE 
            //                                                    WHEN (SELECT COUNT(S.Combination_ID) FROM dbo.ARGSelectedCombination_{printProgramme.ToString()} S WHERE S.Semester=1 AND S.Combination_ID=Combination_ID)<1
            //                                                      THEN
            //                                                      NULL
            //                                                      ELSE
            //                                                      cm.CombinationSubjects
            //                                                      END CombinationSubjects	
            //                                            FROM dbo.ADMCombinationMaster cm
            //                                            JOIN dbo.ADMCourseMaster c ON c.Course_ID = cm.Course_ID AND cm.Status=1 AND cm.Semester={Semester}
            //                                            JOIN dbo.ADMCollegeMaster col ON col.College_ID = cm.College_ID
            //                                            AND c.Programme {programmeIn} ORDER BY col.CollegeFullName,c.CourseFullName"); 
            #endregion
        }

        public int Save(ADMCombinationMaster aDMCombinationMaster)
        {
            return new MSSQLFactory().InsertRecord(aDMCombinationMaster);
        }
        public int Edit(ADMCombinationMaster aDMCombinationMaster)
        {
            List<string> ignoreQuery = new List<string>() {
                nameof(aDMCombinationMaster.Combination_ID)
            , nameof(aDMCombinationMaster.Course_ID)
             , nameof(aDMCombinationMaster.Semester)
             , nameof(aDMCombinationMaster.Status)
            , nameof(aDMCombinationMaster.CreatedBy)
            , nameof(aDMCombinationMaster.CreatedOn)
            , nameof(aDMCombinationMaster.College_ID)};
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<ADMCombinationMaster>(aDMCombinationMaster, ignoreQuery, null);
            sqlCommand.CommandText += " WHERE Combination_ID=@Combination_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int UpdateByIDs(ADMCombinationMaster aDMCombinationMaster)
        {
            string Query = $"Update ADMCombinationMaster Set CombinationSubjects=dbo.ParseCombinationMasterSubjects(@CombinationSubjects),UpdatedOn=GetDate(),UpdatedBy='{AppUserHelper.User_ID}' Where Combination_ID='{aDMCombinationMaster.Combination_ID}'";
            SqlCommand sqlCommand = new SqlCommand(Query);
            sqlCommand.Parameters.AddWithValue("@CombinationSubjects", aDMCombinationMaster.CombinationSubjects);
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public ADMCombinationMaster CombinationAlreadyExists(Guid College_ID, string Combination, Guid? Combination_Id = null)
        {
            string query = $"SELECT * FROM ADMCombinationMaster WHERE College_ID='{College_ID}' AND CombinationSubjects=@CombinationSubjects";
            SqlCommand cmd = new SqlCommand();
            if (!Combination_Id.IsNullOrEmpty())
            {
                query = query + $"  AND Combination_Id<>'{Combination_Id}'";
                //cmd.Parameters.AddWithValue("@Combination_Id", Combination_Id);
            }
            cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.Parameters.AddWithValue("@CombinationSubjects", Combination);
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(cmd);
        }
        public List<ADMCombinationMaster> CombinationDDLDB(Parameters Parameter)
        {
            var Query = $@"SELECT DISTINCT VWSCMasterCompact.Combination_ID,CombinationCode,(RTRIM(LTRIM(VWSCMasterCompact.CombinationCode)) +'$'+RTRIM(LTRIM(VWSCMasterCompact.SubjectFullName))) AS CombinationSubjects,CourseFullName,Semester FROM VWSCMasterCompact ";
            FilterHelper Helper = new GeneralFunctions().GetWhereClause<ADMCombinationMaster>(Parameter.Filters);
            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(Helper.Command);
        }

        public List<ADMCombinationMaster> CombinationDDLDB(Guid College_ID, Guid Course_ID, int CombSemester)
        {
            string query = $@"SELECT DISTINCT VWSCMasterCompact.Combination_ID,CombinationCode, (RTRIM(LTRIM(VWSCMasterCompact.CombinationCode)) +'$'+RTRIM(LTRIM(VWSCMasterCompact.SubjectFullName))) AS CombinationSubjects 
                            FROM VWSCMasterCompact 
                            WHERE Course_ID = '{Course_ID}' AND College_ID = '{College_ID}' AND Semester = {CombSemester} AND Status = 1 ";

            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@College_ID", College_ID);
            // cmd.Parameters.AddWithValue("@Course_ID", Course_ID);
            //cmd.Parameters.AddWithValue("@Semester", CombSemester);
            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(query);
        }

        public ADMCombinationMaster GetCombination(Guid College_ID, string CombinationCode)
        {
            string query = "SELECT ADMCombinationMaster.* FROM ADMCombinationMaster WHERE College_ID=@College_ID AND CombinationCode=@CombinationCode";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.Parameters.AddWithValue("@CombinationCode", CombinationCode);
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(cmd);
        }
        public ADMCombinationMaster GetCombinationBySubjects(Guid College_ID, string subjects, Guid Course_ID)
        {
            string query = "SELECT ADMCombinationMaster.* FROM ADMCombinationMaster WHERE College_ID=@College_ID AND CombinationSubjects=@CombinationSubjects AND Course_ID=@Course_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.Parameters.AddWithValue("@Course_ID", Course_ID);
            cmd.Parameters.AddWithValue("@CombinationSubjects", subjects);
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(cmd);
        }

        public bool CombinationCodeAlreadyExists(Guid College_ID, string CombinationCode, Guid? Combination_Id = null)
        {
            string query = $"SELECT COUNT(CombinationCode) FROM ADMCombinationMaster WHERE College_ID='{College_ID}' AND CombinationCode=@CombinationCode";
            SqlCommand cmd = new SqlCommand();
            if (!Combination_Id.IsNullOrEmpty())
            {
                query = query + $"  AND Combination_Id<>'{Combination_Id}'";
                //cmd.Parameters.AddWithValue("@Combination_Id", Combination_Id);
            }
            cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.Parameters.AddWithValue("@CombinationCode", CombinationCode);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public bool IsCombinationInUser(Guid combination_ID)
        {
            string query = $@"SELECT 1 NumberOfStudents FROM(
                                SELECT Combination_ID FROM ARGSelectedCombination_UG
                                UNION ALL
                                SELECT Combination_ID FROM ARGSelectedCombination_IH
                                UNION ALL
                                SELECT Combination_ID FROM ARGSelectedCombination_PG
                                )COMB WHERE COMB.Combination_ID = '{combination_ID}'";
            return new MSSQLFactory().ExecuteScalar<int>(query) > 0;
        }

        public int Delete(Guid combination_ID)
        {
            string query = $@"DELETE FROM ADMCombinationMaster WHERE Combination_ID = '{combination_ID}'";
            return new MSSQLFactory().ExecuteNonQuery(query);
        }

        public bool CombinationExistsInCollege(Guid College_ID, Guid Combination_ID, Guid Course_ID, short Semester)
        {
            string query = $@"SELECT COUNT(1) FROM ADMCombinationMaster WHERE College_ID='{College_ID}' AND Combination_ID='{Combination_ID}' AND Course_ID='{Course_ID}' AND Semester='{Semester}'";
            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@College_ID", College_ID);
            //cmd.Parameters.AddWithValue("@Combination_ID", Combination_ID);
            return new MSSQLFactory().ExecuteScalar<int>(query) > 0;
        }

        public ADMCombinationMaster Combination(Guid College_ID, string CombinationCode)
        {
            string query = "SELECT * FROM ADMCombinationMaster WHERE College_ID=@College_ID AND CombinationCode=@CombinationCode";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.Parameters.AddWithValue("@CombinationCode", CombinationCode);
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(cmd);
        }

        public List<ADMCombinationMaster> GetList(Guid College_ID, ADMCourseMaster aDMCourseMaster, string CombinationSemester = null)
        {
            string TQuery = "SELECT * FROM ADMCombinationMaster WHERE College_ID=@College_ID";
            SqlCommand cmd = new SqlCommand();
            if (aDMCourseMaster.Status != null)
            {
                TQuery += " AND [Status]=@Status";
                cmd.Parameters.AddWithValue("@Status", aDMCourseMaster.Status);
            }
            if (int.TryParse(CombinationSemester + "", out int semester))
            {
                TQuery += " AND [Semester]=@Semester";
                cmd.Parameters.AddWithValue("@Semester", semester);
            }
            if (aDMCourseMaster.Course_ID != null)
            {
                TQuery += " AND Course_ID=@Course_ID";
                cmd.Parameters.AddWithValue("@Course_ID", aDMCourseMaster.Course_ID);
            }
            if (!string.IsNullOrWhiteSpace(aDMCourseMaster.SearchCombinationCode))
            {
                TQuery += "  AND CombinationCode like @CombinationCode";
                cmd.Parameters.AddWithValue("@CombinationCode", aDMCourseMaster.SearchCombinationCode.ToLike());
            }

            cmd.Parameters.AddWithValue("@College_ID", College_ID);
            cmd.CommandText = TQuery + " ORDER BY CombinationCode,[Status],Semester";
            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(cmd);
        }

        public int ChangeStatus(List<Guid> Combination_IDs, Status status, Guid AppUser_ID)
        {
            string TQuery = $"UPDATE ADMCombinationMaster SET [Status]=@Status,UpdatedOn=@UpdatedOn,UpdatedBy=@UpdatedBy WHERE Combination_ID IN({Combination_IDs.ToIN()})";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);
            cmd.Parameters.AddWithValue("@UpdatedBy", AppUser_ID);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public ARGSelectedCombinationSubject GetSelectedCombinationCompact(Guid Student_ID, PrintProgramme printProgramme, short semester)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);
            var comb = new MSSQLFactory().GetObject<ARGSelectedCombinationSubject>(new CombinationSQLQueries().GetSelectedCombinationCompact(Student_ID, printProgramme, semester));
            if (comb != null && comb.CombinationSubjects.IsNotNullOrEmpty())
            {
                comb.Subjects = new SubjectDB().GetList(comb.CombinationSubjects.ToGuidList());
            }
            return comb;
        }

        public List<ADMCombinationMaster> List(Parameters parameter)
        {
            var Query = $@"SELECT Combination_ID,College_ID,ADMCombinationMaster.Course_ID,Semester,CombinationCode,CombinationSubjects,ADMCombinationMaster.Status,CourseFullName
                            FROM ADMCombinationMaster
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID";
            var Helper = new GeneralFunctions().GetWhereClause<ADMCombinationMaster>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(Helper.Command);
        }

        public List<ADMCombinationMaster> UsedCombinationList(Parameters parameter)
        {
            var Query = $@"SELECT ADMCombinationMaster.Combination_ID,College_ID,ADMCombinationMaster.Course_ID,Semester,CombinationCode,CombinationSubjects,ADMCombinationMaster.Status,CourseFullName
                            FROM ADMCombinationMaster
                            JOIN ADMCourseMaster ON ADMCourseMaster.Course_ID = ADMCombinationMaster.Course_ID
                            JOIN(
                            SELECT Combination_ID FROM ARGSelectedCombination_UG
                            UNION  
                            SELECT Combination_ID FROM ARGSelectedCombination_IH
                            UNION  
                            SELECT Combination_ID FROM ARGSelectedCombination_PG
                            )SC ON SC.Combination_ID = ADMCombinationMaster.Combination_ID";
            var Helper = new GeneralFunctions().GetWhereClause<ADMCombinationMaster>(parameter.Filters);
            Helper.Command.CommandText = Query + Helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            return new MSSQLFactory().GetObjectList<ADMCombinationMaster>(Helper.Command);
        }

        public ADMCombinationMaster GetCombinationByID(Guid combination_ID)
        {
            string query = $@"SELECT * FROM ADMCombinationMaster  WHERE ADMCombinationMaster.Combination_ID=@Combination_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Combination_ID", combination_ID);
            return new MSSQLFactory().GetObject<ADMCombinationMaster>(cmd);
        }

        public List<ADMSubjectMaster> GetCombinationSubjects(Guid combination_ID)
        {
            string query = $@"SELECT * FROM VWSCMaster WHERE Combination_ID='{combination_ID}'";
            return new MSSQLFactory().GetObjectList<ADMSubjectMaster>(query);
        }

        public UserActivity GetUserActivity(Guid combination_ID)
        {
            UserActivity userActivityCreated = new MSSQLFactory().GetObject<UserActivity>($@"SELECT AU.FullName AS CreatedByUser,AU.CreatedOn AS CreationDate FROM dbo.ADMCombinationMaster CM
                                                                LEFT JOIN dbo.AppUsers AU ON AU.User_ID = CM.CreatedBy
                                                                WHERE CM.Combination_ID = '{combination_ID}'");

            UserActivity userActivityUpdated = new MSSQLFactory().GetObject<UserActivity>($@"SELECT AU.FullName AS UpdatedByUser,AU.CreatedOn AS UpdatedDate FROM dbo.ADMCombinationMaster CM
                                                                LEFT JOIN dbo.AppUsers AU ON AU.User_ID = CM.UpdatedBy 
                                                                WHERE CM.Combination_ID = '{combination_ID}'");

            userActivityCreated.UpdatedByUser = userActivityUpdated.UpdatedByUser;
            userActivityCreated.UpdatedDate = userActivityUpdated.UpdatedDate;

            return userActivityCreated;
        }

        public List<Guid> GetCombinationsBySubject(Programme programme, Guid Subject_ID, List<short> batch, List<short> otherbatch, bool checkcombination)
        {
            string Batchs = string.Join(",", batch);
            string otherbatchs = string.Join(",", otherbatch);
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string query = "SELECT Combination_ID FROM ADMCombinationMaster WHERE  CombinationSubjects like %@CombinationSubjects%  ";
            if (checkcombination)
            {
                query = $@"SELECT DISTINCT c.combination_ID FROM admcombinationmaster c
                            JOIN argselectedcombination{prgPostFix} s ON s.Combination_ID = c.Combination_ID
                            JOIN argpersonalinformation{prgPostFix}  p ON p.student_ID = s.student_ID
                            WHERE combinationsubjects LIKE '%{Subject_ID}%' AND Batch IN({Batchs}) AND c.Combination_Id
                                NOT IN
                                (SELECT DISTINCT c.combination_ID FROM admcombinationmaster c
   
                                 JOIN argselectedcombination{prgPostFix} s ON s.Combination_ID = c.Combination_ID
   
                                 JOIN argpersonalinformation{prgPostFix} p ON p.student_ID = s.student_ID
   
                                 WHERE combinationsubjects LIKE '%{Subject_ID}%' AND Batch  IN({otherbatchs}))";
            }
            else { query = $@"SELECT DISTINCT c.combination_ID FROM admcombinationmaster c
                                JOIN argselectedcombination{prgPostFix}  s ON s.Combination_ID=c.Combination_ID
                                JOIN argpersonalinformation{prgPostFix}  p ON p.student_ID=s.student_ID
                                WHERE combinationsubjects LIKE '%{Subject_ID}%' AND Batch IN({Batchs})"; }
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            return new MSSQLFactory().GetSingleValues<Guid>(cmd);
        }

        public List<Guid> CreateCopyOfCombinations(List<Guid> allCombinationsHavingSubjectthesBatchsinUnion, DateTime createdon)
        {

            string ids = string.Join("','", allCombinationsHavingSubjectthesBatchsinUnion);
            string TQuery = $@"INSERT INTO dbo.ADMCombinationMaster
                            (
                                Combination_ID,
                                College_ID,
                                Course_ID,
                                Semester,
                                CombinationCode,
                                CombinationSubjects,
                                Status,
                                CreatedOn,
                                CreatedBy,
                                  ParentCombination_ID
                            )
                            SELECT
                                NewID(),
                                College_ID,
                                Course_ID,
                                Semester,
                                CombinationCode+'{"CPY"}',
                                CombinationSubjects,
                                Status,
                                @CreatedOn,
                                CreatedBy,
                                Combination_ID
                            FROM admcombinationmaster c WHERE c.combination_ID IN
                            ('{ids}')


               SELECT Combination_ID FROM admcombinationmaster  WHERE CreatedOn=@CreatedOn AND CombinationCode like '%CPY'";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@CreatedOn", createdon);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().GetSingleValues<Guid>(cmd);
        }

        public int AssignCopiedCombinationsToStudents(Programme programme, List<short> batch, short semester, List<Guid> allCombinationsHavingSubjectthesBatchsinUnion, DateTime createdon)
        {
            string prgPostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string Batchs = string.Join(",", batch);
            string ids = string.Join("','", allCombinationsHavingSubjectthesBatchsinUnion);
            string TQuery = $@"UPDATE SC
                                SET sc.Combination_ID=(SELECT Combination_ID FROM dbo.ADMCombinationMaster WHERE ParentCombination_ID=sc.Combination_ID and CreatedOn=@CreatedOn)
                                from dbo.ARGSelectedCombination{prgPostFix}  SC
                                JOIN dbo.ARGPersonalInformation{prgPostFix} ON  ARGPersonalInformation{prgPostFix} .Student_ID = SC.Student_ID AND Batch IN({Batchs})
                                WHERE combination_Id IN ('{ids}')";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@CreatedOn", createdon);
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public string ReplaceCombinationsSubject(Guid FindSubject, Guid ReplaceSubject, string CombinationSubjects)
        {
            string TQuery = $@"Select dbo.FNReplaceCombinationSubject ('{CombinationSubjects}','{FindSubject}','{ReplaceSubject}')  ";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = TQuery;
            return new MSSQLFactory().ExecuteScalar<string>(cmd);
        }

        public bool SetCombinationEndDate(int? batch, DateTime? endDate, PrintProgramme? printProgramme, int? semester)
        {
            if (printProgramme == null || endDate == null)
            {
                return false;
            }
            string filter = $" Where c.AllowCollegeChangeCombination=1 and CM.PrintProgramme={(short)printProgramme.Value} ";

            if (semester != null)
            {
                filter += $" And c.Semester={semester.Value}";
            }

            if (batch != null)
            {
                filter += $" And c.Batch={batch.Value}";
            }
            string TQuery = $@"UPDATE c SET Closingdate=@date FROM dbo.CombinationSetting c
								JOIN dbo.ADMCourseMaster CM ON CM.Course_ID = c.Course_ID								
								{filter}";

            SqlCommand cmd1 = new SqlCommand();
            cmd1.CommandText = TQuery;
            cmd1.Parameters.AddWithValue("@date", endDate);
            return new MSSQLFactory().ExecuteNonQuery(cmd1) > 0;
        }

        public void CloseCombinationSettingsLink()
        {
            string TQuery = $@" UPDATE dbo.CombinationSetting SET AllowCollegeChangeCombination=0,AllowStudentChangeCombination=0
	                            WHERE ClosingDate<=GETDATE() AND (AllowCollegeChangeCombination=1 OR AllowStudentChangeCombination=1)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = TQuery;
            new MSSQLFactory().ExecuteNonQuery(cmd);
        }
    }
}
