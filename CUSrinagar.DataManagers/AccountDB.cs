using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class AccountDB
    {
        #region AppUsers
        public AppUsers GetItem(Guid User_ID)
        {
            string query = "SELECT * FROM AppUsers WHERE [User_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return new MSSQLFactory().GetObject<AppUsers>(cmd);
        }
        public List<AppUserRoles> GetUserRoles(Guid User_ID)
        {
            string query = "SELECT * FROM AppUserRoles WHERE [User_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return new MSSQLFactory().GetObjectList<AppUserRoles>(cmd);
        }
        public Guid? SignIn(AppLogin appLogin)
        {
            string query = "SELECT [User_ID] FROM AppUsers WHERE [Password]=@Password AND (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Password", appLogin.Password.Encrypt());
            cmd.Parameters.AddWithValue("@UserName", appLogin.UserName);
            return new MSSQLFactory().ExecuteScalar<Guid?>(cmd);
        }

        public Guid LogMeOutOfOtherDevices(Guid User_ID)
        {
            string query = "SELECT SecurityStamp FROM AppUsers WHERE [USER_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return new MSSQLFactory().ExecuteScalar<Guid>(cmd);
        }

        public Guid LogMeOutOfOtherDevices(Guid User_ID, PrintProgramme printProgramme)
        {
            string query = $"SELECT CreatedBy FROM dbo.ARGPersonalInformation_{printProgramme.ToString()} WHERE Student_ID=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return new MSSQLFactory().ExecuteScalar<Guid>(cmd);
        }


        public AppUsers GetUserByToken(string token)
        {
            string query = $@"SELECT * FROM AppUsers WHERE [PasswordResetToken]=@PasswordResetToken
                              AND CAST(@CurrentUtcDate AS DATETIME)<= DATEADD(HOUR, 5, TokenIssuedOn)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            SqlParameter sqlParameter = new SqlParameter("@CurrentUtcDate", System.Data.SqlDbType.DateTime);
            sqlParameter.Value = DateTime.UtcNow;
            cmd.Parameters.Add(sqlParameter);

            cmd.Parameters.AddWithValue("@PasswordResetToken", token);
            return new MSSQLFactory().GetObject<AppUsers>(cmd);
        }
        public int SetToken(Guid User_ID, string token)
        {
            string query = $@"UPDATE AppUsers SET PasswordResetToken=@token,
                                                  TokenIssuedOn =@TokenIssuedOn WHERE [USER_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@TokenIssuedOn", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int ReSetToken(string UserName)
        {
            string query = $@"UPDATE AppUsers SET PasswordResetToken=NULL,
                                                  TokenIssuedOn =NULL WHERE [UserName]=@UserName";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public bool IsAccountDisabled(Guid User_ID)
        {
            string query = "SELECT [Status] FROM AppUsers WHERE [USER_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public DateTime IsAccountLocked(Guid User_ID)
        {
            string query = "SELECT LockoutEndDateUtc FROM AppUsers WHERE [USER_ID]=@User_ID AND LockoutEnabled=@LockoutEnabled";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            cmd.Parameters.AddWithValue("@LockoutEnabled", true);
            return new MSSQLFactory().ExecuteScalar<DateTime>(cmd);
        }

        public int RemoveAccountLockout(Guid User_ID)
        {
            string query = "UPDATE AppUsers SET LockoutEndDateUtc=@LockoutEndDateUtc,AccessFailedCount=0 WHERE [User_ID]=@User_ID";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@User_ID", User_ID);
            cmd.Parameters.AddWithValue("@LockoutEndDateUtc", DBNull.Value);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }




        public int UpdatePassword(string UserName, string NewPassword)
        {
            string query = $"UPDATE AppUsers SET Password='{NewPassword}',LastPasswordResetDate=@LastPasswordResetDate WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.Parameters.AddWithValue("@LastPasswordResetDate", DateTime.Now);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public bool ComparePassword(string UserName, string NewPassword)
        {
            string query = $"SELECT COUNT(AU.User_ID) FROM dbo.AppUsers AU WHERE AU.UserName=@UserName AND AU.Password=@NewPassword";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.Parameters.AddWithValue("@NewPassword", NewPassword);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }


        public int UpdateAccessFailedCount(string UserName)
        {
            string query = "UPDATE AppUsers SET AccessFailedCount=(AccessFailedCount+1) WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }
        public int GetAccessFailedCount(string UserName)
        {
            string query = "SELECT AccessFailedCount FROM AppUsers WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            return new MSSQLFactory().ExecuteScalar<int>(cmd);
        }

        public int UpdateLockoutEndDateUtc(string UserName, DateTime dateTime)
        {
            string query = "UPDATE AppUsers SET LockoutEndDateUtc=@LockoutEndDateUtc WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            cmd.Parameters.AddWithValue("@LockoutEndDateUtc", dateTime);
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public bool UserNameExists(string UserName)
        {
            string query = "SELECT Count(UserName) FROM AppUsers WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }

        public bool IsAccountLockedEnabled(string UserName)
        {
            return true;
            //string query = "SELECT LockoutEnabled FROM AppUsers WHERE (UserName=@UserName OR Email=@UserName)";
            //SqlCommand cmd = new SqlCommand();
            //cmd.CommandText = query;
            //cmd.Parameters.AddWithValue("@UserName", UserName);
            //return new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public Guid GetUser_ID(string UserName)
        {
            string query = "SELECT [User_ID] FROM AppUsers WHERE (UserName=@UserName OR Email=@UserName)";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@UserName", UserName);
            return new MSSQLFactory().ExecuteScalar<Guid>(cmd);
        }
        #endregion

        #region StudentZone
        public Guid SignIn(ARGReprint aRGReprint)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CUSRegistrationNo", aRGReprint.FormNo);

            if (aRGReprint.DOB == new DateTime(8888, 07, 07))
            {
                command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                   AND FormStatus NOT IN('{(short)FormStatus.AutoAdmissionCancel}','{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') ORDER BY Batch DESC";
            }
            else
            {
                command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));
                command.CommandText = $@"SELECT TOP 1 Student_ID FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (DOB=@DOB) AND (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                   AND FormStatus NOT IN('{(short)FormStatus.AutoAdmissionCancel}','{(short)FormStatus.Rejected}','{(short)FormStatus.Disqualified}','{(short)FormStatus.Cancelled}','{(short)FormStatus.CancelRegistration}') ORDER BY Batch DESC";
            }
            return new MSSQLFactory().ExecuteScalar<Guid>(command);
        }

        public FormStatus? SignInStatus(ARGReprint aRGReprint)
        {
            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@CUSRegistrationNo", aRGReprint.FormNo);

            if (aRGReprint.DOB == new DateTime(8888, 07, 07))
            {
                command.CommandText = $@"SELECT TOP 1 FormStatus FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                        ORDER BY Batch DESC";
            }
            else
            {
                command.Parameters.AddWithValue("@DOB", aRGReprint.DOB.ToString("yyyy-MM-dd"));
                command.CommandText = $@"SELECT TOP 1 FormStatus FROM ARGPersonalInformation_{aRGReprint.PrintProgrammeOption.ToString()} WHERE (DOB=@DOB) AND (CUSRegistrationNo = @CUSRegistrationNo OR StudentFormNo=@CUSRegistrationNo OR BoardRegistrationNo=@CUSRegistrationNo)
                                         ORDER BY Batch DESC";
            }
            short statusResponse = new MSSQLFactory().ExecuteScalar<short>(command);

            if (statusResponse <= 0)
                return null;
            else
                return (FormStatus)Enum.Parse(typeof(FormStatus), statusResponse.ToString());
        }

        public bool IsStudentPassout(Guid Student_ID, PrintProgramme PrintProgrammeOption)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@"SELECT TOP 1 IsPassout FROM ARGPersonalInformation_{PrintProgrammeOption.ToString()} WHERE Student_ID='{Student_ID}'
                                        ORDER BY Batch DESC";

            return new MSSQLFactory().ExecuteScalar<bool>(command);
        }
        #endregion
    }
}
