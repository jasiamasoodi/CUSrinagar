using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{

    public class AppUsersDB
    {
        public List<AppUsers> GetAllAppUsers(Parameters parameter, IPrincipal obj)
        {
            return new MSSQLFactory().GetObjectList<AppUsers>(new AppUsersSQLQueries().GetAllAppUsers(parameter, obj));
        }
        public List<AppUsers> GetAllAppUsers(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AppUsers>(new AppUsersSQLQueries().GetAllAppUsers(parameter));
        }

        public List<AppUsers> GetAllAppUsersAP(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AppUsers>(new AppUsersSQLQueries().GetAllAppUsersAP(parameter));
        }

        public List<AppUserRoles> GetUserRoless(Guid user_ID)
        {
            return new MSSQLFactory().GetObjectList<AppUserRoles>(new AppUsersSQLQueries().GetUserRoles(user_ID));
        }

        public int AddUser(AppUsers input)
        {
            input.LastPasswordResetDate = DateTime.Now;
            return new MSSQLFactory().InsertRecord(input);

        }

        public int AddAPSubjects(AppUserProfessorSubjects input)
        {
            return new MSSQLFactory().InsertRecord(input);
        }


        public int AddUserRole(AppUserRoles input)
        {
            return new MSSQLFactory().InsertRecord<AppUserRoles>(input);

        }

        public int EditUser(AppUsers model, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            ignoreQuery.Add("LastPasswordResetDate");
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE User_ID=@User_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public AppUsers GetUserById(Guid id)
        {
            return new MSSQLFactory().GetObject<AppUsers>(new AppUsersSQLQueries().GetUser(id));
        }

        public AppUsers GetAPUserInfoById(Guid id)
        {
            return new MSSQLFactory().GetObject<AppUsers>(new AppUsersSQLQueries().GetAPUserInfoById(id));
        }

        public int DeleteUser(Guid id)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Delete from AppUsers";
            cmd.Parameters.AddWithValue("@User_ID", id);
            cmd.CommandText = Query + " Where User_ID = @User_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteUserDept(Guid id)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Delete from FTUserSectionMapping";
            cmd.Parameters.AddWithValue("@User_ID", id);
            cmd.CommandText = Query + " Where User_ID = @User_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteUserRoles(Guid id)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Delete from AppUserRoles";
            cmd.Parameters.AddWithValue("@User_ID", id);
            cmd.CommandText = Query + " Where User_ID = @User_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int DeleteUserRoles(Guid id,AppRoles appRoles)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"Delete from AppUserRoles";
            cmd.Parameters.AddWithValue("@User_ID", id);
            cmd.Parameters.AddWithValue("@RoleID", (int)appRoles);
            cmd.CommandText = Query + " Where User_ID = @User_ID AND RoleID = @RoleID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int DeleteAPUser(Guid User_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.AppUserProfessorSubjects";
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            cmd.CommandText = Query + " Where User_ID = @User_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int DeleteAPUserSubject(Guid ProfessorSubject_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.AppUserProfessorSubjects";
            cmd.Parameters.AddWithValue("@ProfessorSubject_ID", ProfessorSubject_ID);
            cmd.CommandText = Query + "  Where ProfessorSubject_ID = @ProfessorSubject_ID ";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int DeleteEvalUserSubject(Guid EvaluatorSubject_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.AppUserEvaluatorSubjects";
            cmd.Parameters.AddWithValue("@EvaluatorSubject_ID", EvaluatorSubject_ID);
            cmd.CommandText = Query + "  Where EvaluatorSubject_ID = @EvaluatorSubject_ID ";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int DeleteEvalUserSubject(string EvaluatorSubject_ID)
        {

            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.AppUserEvaluatorSubjects";
            cmd.CommandText = Query + $"  Where EvaluatorSubject_ID IN ({EvaluatorSubject_ID}) ";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }


        public string CheckAPSubjects(AppUserProfessorSubjects input)
        {
            string query = $@"SELECT SubjectFullName+' (Sem-'+CONVERT(varchar(10),AppUserProfessorSubjects.Semester) +') Already Assigned To '+UserName AS Result  FROM AppUserProfessorSubjects inner JOIN AppUsers ON AppUsers.User_ID = AppUserProfessorSubjects.User_ID JOIN dbo.ADMSubjectMaster ON ADMSubjectMaster.Subject_ID = AppUserProfessorSubjects.Subject_ID  WHERE AppUsers.College_ID=@College_ID AND AppUserProfessorSubjects.Semester=@Semester AND AppUserProfessorSubjects.Subject_ID=@Subject_ID AND  AppUsers.User_ID !=@User_ID";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            sqlCommand.Parameters.AddWithValue("@Semester", input.Semester);
            sqlCommand.Parameters.AddWithValue("@Subject_ID", input.Subject_ID);
            sqlCommand.Parameters.AddWithValue("@User_ID", input.User_ID);
            return new MSSQLFactory().ExecuteScalar<string>(sqlCommand);
        }

        public FTUserSectionMapping GetEmployeeDepartment(Guid User_Id)
        {
            return new MSSQLFactory().GetObject<FTUserSectionMapping>(new AppUsersSQLQueries().GetEmployeeDepartments(User_Id));
        }
        public int DeleteEmployeeDepartment(Guid User_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.FTUserSectionMapping";
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            cmd.CommandText = Query + "  Where User_ID = @User_ID ";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int AddEmployeeDepartment(FTUserSectionMapping input)
        {
            return new MSSQLFactory().InsertRecord<FTUserSectionMapping>(input);

        }
        public List<AppUsers> GetUserByColumn(string columnName, string data, Guid? User_Id)
        {
            return new MSSQLFactory().GetObjectList<AppUsers>(new AppUsersSQLQueries().GetUserByColumn(columnName, data, User_Id));
        }


        public List<AppUserProfessorSubjects> GetAllAPSubjectsByUserID(Guid user_ID)
        {
            return new MSSQLFactory().GetObjectList<AppUserProfessorSubjects>(new AppUsersSQLQueries().GetAllAPSubjectsByUserID(user_ID));
        }


        public PorfessorSubjectBaseObject GetMinMaxRollNo(Guid CourseId, Guid SubjectId, int Semester, PrintProgramme programme)
        {
            int programmeis = (int)new GeneralFunctions().GetProgramme(programme);
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string Query = $@"SELECT ISNULL(MIN(ClassRollNo),0) AS MinRollNo,ISNULL(Max(ClassRollNo),0) AS MaxRollNo,COUNT(Pinfo.Student_ID) AS NoofStudents  FROM ARGPersonalInformation{tablePostFix} Pinfo
                                JOIN ARGSelectedCombination{tablePostFix} SComb ON SComb.Student_ID = Pinfo.Student_ID AND Semester = @Semester AND Pinfo.CurrentSemesterOrYear<={Semester} 
                                JOIN ADMCombinationMaster Comb ON Comb.Combination_ID = SComb.Combination_ID
                                JOIN dbo.AwardFilterSettings FilterS ON Programme={programmeis} AND Batch=FilterValue AND AwardType='Practical' AND FilterS.Semester={Semester}
                                WHERE AcceptCollege_ID = @College_ID 
                                AND CombinationSubjects  LIKE  '%{SubjectId}%' AND ClassRollNo IS NOT NULL";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            cmd.Parameters.AddWithValue("@CourseId", CourseId);
            cmd.Parameters.AddWithValue("@SubjectId", SubjectId);
            cmd.Parameters.AddWithValue("@Semester", Semester);
            cmd.CommandText = Query;
            return new MSSQLFactory().GetObject<PorfessorSubjectBaseObject>(cmd);//return Json object

        }
        public List<SelectListItem> SelectListAppUser(Parameters Parameter)
        {
            var Query = "Select CAST( User_ID AS NVARCHAR(36)) [Value], FullName AS [Text] From AppUsers ";
            FilterHelper Helper = Helper = new GeneralFunctions().GetWhereClause<AppUsers>(Parameter.Filters);
            Query += Helper.WhereClause;
            Query += new GeneralFunctions().GetPagedQuery(Query, Parameter);
            Helper.Command.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(Helper.Command);
        }

        #region Evalvator
        public int AddEvalvator(AppUsers input)
        {
            return new MSSQLFactory().InsertRecord(input);
        }

        public bool CheckEvalvatorSubjects(AppUserEvaluatorSubjects input)
        {
            string query = @"SELECT COUNT(AppUserEvaluatorSubjects.Semester) AS Result FROM AppUserEvaluatorSubjects inner JOIN AppUsers ON AppUsers.User_ID = AppUserEvaluatorSubjects.User_ID WHERE ExamRollNoFrom BETWEEN @ExamRollNoFrom AND @ExamRollNoTo AND Semester=@Semester AND Subject_ID=@Subject_ID AND  AppUsers.User_ID!=@User_ID";
            SqlCommand sqlCommand = new SqlCommand(query);
            sqlCommand.Parameters.AddWithValue("@Semester", input.Semester);
            sqlCommand.Parameters.AddWithValue("@Subject_ID", input.Subject_ID);
            sqlCommand.Parameters.AddWithValue("@User_ID", input.User_ID);
            return new MSSQLFactory().ExecuteScalar<int>(sqlCommand) > 0;
        }

        public int AddEvalvatorSubjects(AppUserEvaluatorSubjects input)
        {
            return new MSSQLFactory().InsertRecord(input);
        }

        public void DeleteUserSubj(Guid id)
        {
            new AppUsersDB().DeleteEvalvatorUser(id);
            new AppUsersDB().DeleteAPUser(id);
        }

        public List<AppUsers> GetAllAppEvalvators(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<AppUsers>(new AppUsersSQLQueries().GetAllAppEvalvators(parameter, AppUserHelper.College_ID));
        }
        public AppUsers GetEvalvatorInfoById(Guid id)
        {
            return new MSSQLFactory().GetObject<AppUsers>(new AppUsersSQLQueries().GetEvalvatorInfoById(id));
        }

        public List<AppUserEvaluatorSubjects> GetEvalvatorSubjectsByUserID(Guid user_ID)
        {
            return new MSSQLFactory().GetObjectList<AppUserEvaluatorSubjects>(new AppUsersSQLQueries().GetEvalvatorSubjectsByUserID(user_ID));
        }

        public int EditEvalvator(AppUsers model, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            ignoreQuery.Add("LastPasswordResetDate");
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(model, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE User_ID=@User_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int DeleteEvalvatorUser(Guid User_ID)
        {
            SqlCommand cmd = new SqlCommand();
            string Query = $"DELETE FROM dbo.AppUserEvaluatorSubjects";
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            cmd.CommandText = Query + " Where User_ID = @User_ID";
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public EvalvatorSubjectsBaseObject GetMinMaxRollNoEvalvator(Guid CourseId, Guid SubjectId, int Semester, Programme programme)
        {
            int programmeis = (int)programme;
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string Query = $@"select MIN(MinRollNo) AS MinRollNo, MAX(MaxRollNo) AS MaxRollNo,SUM(NoofStudents) AS NoofStudents FROM
                                (SELECT ISNULL(MIN(cast(ExamRollNumber as bigint)),0) AS MinRollNo,ISNULL(Max(cast(ExamRollNumber as bigint)),0) AS MaxRollNo,COUNT(ARGPersonalInformation{tablePostFix}.Student_ID) AS NoofStudents FROM ARGPersonalInformation{tablePostFix}
                                JOIN ARGSelectedCombination{tablePostFix} ON ARGSelectedCombination{tablePostFix}.Student_ID = ARGPersonalInformation{tablePostFix}.Student_ID AND Semester = @Semester
                                JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
                               inner join ARGStudentExamForm{tablePostFix} on ARGStudentExamForm{tablePostFix}.Student_ID=ARGPersonalInformation{tablePostFix}.Student_ID AND ARGStudentExamForm{tablePostFix}.Status=4 AND ARGStudentExamForm{tablePostFix}.Semester={Semester}
								JOIN AwardFilterSettings Filters ON Filters.Semester={Semester} AND AwardType='Theory' AND ARGStudentExamForm{tablePostFix}.Year=Filters.FilterValue and Programme={programmeis}
                                 WHERE  CombinationSubjects  LIKE  '%{SubjectId}%' AND ExamRollNumber IS NOT NULL
                                            UNION
                                     SELECT ISNULL(MIN(ExamRollNumber), 0) AS MinRollNo,
           ISNULL(MAX(ExamRollNumber), 0) AS MaxRollNo,
           COUNT(ARGPersonalInformation{tablePostFix}.Student_ID) AS NoofStudents
    FROM ARGPersonalInformation{tablePostFix}
        JOIN ARGSelectedCombination{tablePostFix}
            ON ARGSelectedCombination{tablePostFix}.Student_ID = ARGPersonalInformation{tablePostFix}.Student_ID
               AND Semester = @Semester
        JOIN ADMCombinationMaster
            ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
        INNER JOIN ARGStudentExamForm{tablePostFix}
            ON ARGStudentExamForm{tablePostFix}.Student_ID = ARGPersonalInformation{tablePostFix}.Student_ID
               AND ARGStudentExamForm{tablePostFix}.Status = 4
               AND ARGStudentExamForm{tablePostFix}.Semester = @Semester
        JOIN dbo.ARGStudentAdditionalSubjects{tablePostFix}
            ON ARGStudentAdditionalSubjects{tablePostFix}.Student_ID = ARGSelectedCombination{tablePostFix}.Student_ID
        JOIN AwardFilterSettings Filters
            ON Filters.Semester =@Semester
               AND AwardType = 'Theory'
               AND ARGStudentExamForm{tablePostFix}.Year = Filters.FilterValue
               AND Programme ={programmeis}
    WHERE Subject_ID = '{SubjectId}'
          AND ExamRollNumber IS NOT NULL
) AS temp
WHERE MinRollNo > 0";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@CourseId", CourseId);
            cmd.Parameters.AddWithValue("@SubjectId", SubjectId);
            cmd.Parameters.AddWithValue("@Semester", Semester);
            cmd.CommandText = Query;
            return new MSSQLFactory().GetObject<EvalvatorSubjectsBaseObject>(cmd);//return Json object

        }

        public EvalvatorSubjectsBaseObject GetMinMaxStudentCode(Guid CourseId, Guid SubjectId, int Semester, PrintProgramme programme)
        {
            int programmeis = (int)new GeneralFunctions().GetProgramme(programme);
            string tablePostFix = new GeneralFunctions().GetProgrammePostFix(programme);
            string Query = $@"SELECT ISNULL(MIN(StudentCode),0) AS MinStudentCode,ISNULL(Max(StudentCode),0) AS MaxStudentCode ,COUNT(ARGPersonalInformation{tablePostFix}.Student_ID) AS NoofStudents FROM ARGPersonalInformation{tablePostFix}
                                JOIN ARGSelectedCombination{tablePostFix} ON ARGSelectedCombination{tablePostFix}.Student_ID = ARGPersonalInformation{tablePostFix}.Student_ID AND Semester = @Semester
                                JOIN ADMCombinationMaster ON ADMCombinationMaster.Combination_ID = ARGSelectedCombination{tablePostFix}.Combination_ID
 inner join ARGStudentExamForm{tablePostFix} on ARGStudentExamForm{tablePostFix}.Student_ID=ARGPersonalInformation{tablePostFix}.Student_ID AND ARGStudentExamForm{tablePostFix}.Status=4 AND ARGStudentExamForm{tablePostFix}.Semester={Semester}
								JOIN AwardFilterSettings Filters ON Filters.Semester={Semester} AND AwardType='Theory' AND ARGStudentExamForm{tablePostFix}.Year=Filters.FilterValue and Programme={programmeis}
                                 WHERE  CombinationSubjects  LIKE  '%{SubjectId}%' AND StudentCode IS NOT NULL";
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@CourseId", CourseId);
            cmd.Parameters.AddWithValue("@SubjectId", SubjectId);
            cmd.Parameters.AddWithValue("@Semester", Semester);
            cmd.CommandText = Query;
            return new MSSQLFactory().GetObject<EvalvatorSubjectsBaseObject>(cmd);//return Json object
        }

        public List<SelectListItem> GetUserList(AppRoles role, Parameters parameter)
        {
            string Query = @"SELECT  convert(nvarchar(120),AppUsers.User_ID) AS Value,FullName AS Text  FROM AppUsers
                                JOIN AppUserRoles
                                ON AppUserRoles.User_ID = AppUsers.User_ID";

            var helper = new GeneralFunctions().GetWhereClause<ADMSubjectMaster>(parameter.Filters);
            Query += helper.WhereClause + " AND RoleName = @RoleName " + new GeneralFunctions().GetPagedQuery(Query, parameter);
            helper.Command.CommandText = Query;
            helper.Command.Parameters.AddWithValue("@RoleName", role.ToString());
            return new MSSQLFactory().GetObjectList<SelectListItem>(helper.Command);
        }

        public int AssignProfessorCRClasses(ProfessorCRClasses professorCRClasses)
        {
            return new MSSQLFactory().InsertRecord<ProfessorCRClasses>(professorCRClasses, "ProfessorCRClasses");
        }

        public ProfessorCRClasses GetProfessorCRClasses(ProfessorCRClasses professorCRClasses)
        {
            string Query = $@"SELECT  *  FROM ProfessorCRClasses JOIN AppUsers ON User_ID=Professor_ID
                        Where Batch={professorCRClasses.Batch} AND College_ID='{professorCRClasses.College_ID}'
                              AND Subject_ID='{professorCRClasses.Subject_ID}' AND Semester={(int)professorCRClasses.Semester}";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            return new MSSQLFactory().GetObject<ProfessorCRClasses>(cmd);
        }

        public int EditProfessorCRClasses(ProfessorCRClasses professorCRClasses)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(professorCRClasses, null, null);
            sqlCommand.CommandText += " WHERE ProfessorCRClasses_ID=@ProfessorCRClasses_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
        #endregion




    }
}
