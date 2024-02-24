using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CUSrinagar.Enums;
using System.Security.Principal;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class AppUsersSQLQueries
    {
        internal string GetAppUsers = @"SELECT DISTINCT AppUsers.User_ID from AppUsers 
                                        JOIN AppUserRoles ON AppUserRoles.User_ID = AppUsers.User_ID ";
        internal SqlCommand GetAllAppUsers(Parameters parameter, IPrincipal obj)
        {
            string query = GetAppUsers;
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AppUsers>(parameter.Filters);
            if (obj.IsInRole(CUSrinagar.Enums.AppRoles.University_DeputyController.ToString()))
            { query += helper.WhereClause + " AND RoleID = @RoleID2 "; }
            else
            {
                query += helper.WhereClause + " AND RoleID <>@RoleID AND RoleID <>@RoleID1 AND RoleID <>@RoleID2";
            }
            query = $"SELECT AppUsers.* FROM AppUsers WHERE User_ID IN({query}) " + new GeneralFunctions().GetPagedQuery(query, parameter); ;
            helper.Command.CommandText = query;
            helper.Command.Parameters.AddWithValue("@RoleID", AppRoles.College_AssistantProfessor);
            helper.Command.Parameters.AddWithValue("@RoleID1", AppRoles.University_Evaluator);
            helper.Command.Parameters.AddWithValue("@RoleId2", AppRoles.University_Coordinator);
            return helper.Command;
        }
        internal SqlCommand GetAllAppUsers(Parameters parameter)
        {
            string query = GetAppUsers;
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AppUsers>(parameter.Filters);
                query += helper.WhereClause ;

            query = $"SELECT AppUsers.* FROM AppUsers WHERE User_ID IN({query}) " + new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }


        internal SqlCommand GetAllAppUsersAP(Parameters parameter)
        {
            string query = GetAppUsers;
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AppUsers>(parameter.Filters);
            query += helper.WhereClause + " AND RoleID=@RoleID";
            query += " AND College_ID=@College_ID";
            query += " AND Status=@Status";
            query = $"SELECT AppUsers.* FROM AppUsers WHERE User_ID IN({query}) " + new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.Parameters.AddWithValue("@RoleID", AppRoles.College_AssistantProfessor);
            helper.Command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            helper.Command.Parameters.AddWithValue("@Status", true);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetUserRoles(Guid user_ID)
        {
            string query = "SELECT * from AppUserRoles WHERE User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", user_ID);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetUser(Guid id)
        {
            string query = "SELECT * from AppUsers WHERE User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", id);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetAPUserInfoById(Guid id)
        {
            string query = "SELECT * FROM AppUsers WHERE dbo.AppUsers.User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", id);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetUserByColumn(string columnName, string data, Guid? User_Id)
        {
            SqlCommand command = new SqlCommand();
            string query = $"SELECT * from AppUsers WHERE {columnName}=@Data";
            if (User_Id.HasValue)
            {
                query = query + " AND User_ID != @UserID";
                command.Parameters.AddWithValue("@UserID", User_Id);
            }
            command.Parameters.AddWithValue("@Data", data);
            command.CommandText = query;
            return command;
        }


        internal SqlCommand GetAllAPSubjectsByUserID(Guid user_ID)
        {
            string query = "SELECT  psub.*,sub.Course_id,Programme  from AppUserProfessorSubjects psub join ADMSubjectMaster sub on psub.subject_id=sub.subject_id WHERE User_ID = @UserID " +
                "ORDER BY psub.Semester,sub.Course_ID,sub.Subject_ID,RollNoFrom,psub.CreatedOn asc";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", user_ID);
            command.CommandText = query;
            return command;
        }
        #region Evalvator
        internal SqlCommand GetAllAppEvalvators(Parameters parameter,Guid? College_ID)
        {
            string query = GetAppUsers;
            FilterHelper helper = new GeneralFunctions().GetWhereClause<AppUsers>(parameter.Filters);
            query += helper.WhereClause + " AND RoleID=@RoleID AND College_Id=@College_ID";
            query = $"SELECT AppUsers.* FROM AppUsers WHERE User_ID IN({query}) " + new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.Parameters.AddWithValue("@RoleID", AppRoles.University_Evaluator);
            helper.Command.Parameters.AddWithValue("@College_ID", College_ID);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetEmployeeDepartments(Guid user_Id)
        {
            string query = "SELECT * FROM FTUserSectionMapping WHERE User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", user_Id);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetEvalvatorInfoById(Guid id)
        {
            string query = "SELECT * FROM AppUsers WHERE dbo.AppUsers.User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", id);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetEvalvatorSubjectsByUserID(Guid user_ID)
        {
            string query = "SELECT ESub.*,Sub.Course_Id from AppUserEvaluatorSubjects ESub join ADMSubjectMaster Sub on ESub.Subject_id=Sub.Subject_id WHERE User_ID = @UserID";
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@UserID", user_ID);
            command.CommandText = query+ " Order By ESub.CreatedOn";
            return command;
        }
        #endregion

    }
}
