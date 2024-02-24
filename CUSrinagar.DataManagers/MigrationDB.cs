using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;
namespace CUSrinagar.DataManagers
{
    public class MigrationDB
    {
        public MigrationPI GetData(string StudentName, string RegistrationId)
        {

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@StudentName", StudentName);
            command.Parameters.AddWithValue("@RegistrationId", RegistrationId);
            command.CommandText = $@"	SELECT * FROM  MigrationDetail INNER JOIN ARGPersonalInformation_UG
											ON ARGPersonalInformation_UG.Student_ID = FormsToDownload.Student_ID
											WHERE ARGPersonalInformation_UG.FullName=@StudentName";
            return new MSSQLFactory().GetObject<MigrationPI>(command);
        }
        //public static List<FormsToDownload> GetMigrationFormList()
        //{
        //    return new MSSQLFactory().GetObjectList<FormsToDownload>(new MigrationSQLQueries().GetMigrationFormList);
        //}


        public static int SaveDownloadForm(MigrationPI model)
        {
            return new MSSQLFactory().InsertRecord<MigrationPI>(model,"MigrationDetail");
        }
        public bool UpdateFormStatus(Guid Form_ID)
        {
            string query = $"UPDATE MigrationDetail SET FormStatus={(int)FormStatus.FeePaid} WHERE Form_ID=@Form_Id";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@Form_ID", Form_ID);
            return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        }

        public List<MigrationPI> GetMigrationFormList(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<MigrationPI>(new MigrationSQLQueries().GetMigrationFormList(parameter));
        }

        public List<MigrationPI> GetALLMigrationFormList(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<MigrationPI>(new MigrationSQLQueries().GetALLMigrationFormList(parameter));
        }

        //public bool UpdateForm(Guid Form_ID, int AcceptReject,string REMARKSBYCOLLEGE)
        //{
        //    string query = "UPDATE MigrationDetail SET FormStatus=@AcceptReject,REMARKSBYCOLLEGE=@REMARKSBYCOLLEGE WHERE Form_ID=@Form_Id";
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandText = query;
        //    cmd.Parameters.AddWithValue("@Form_ID", Form_ID);
        //    cmd.Parameters.AddWithValue("@AcceptReject", AcceptReject);
        //    cmd.Parameters.AddWithValue("@REMARKSBYCOLLEGE", REMARKSBYCOLLEGE);
        //    return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        //}
        //public bool UpdateForm(Guid Form_ID)
        //{
        //    string query = "UPDATE MigrationDetail SET ForwardedToCollege=1 WHERE Form_ID=@Form_Id";
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandText = query;
        //    cmd.Parameters.AddWithValue("@Form_ID", Form_ID);
        //     return !new MSSQLFactory().ExecuteScalar<bool>(cmd);
        //}
        public MigrationPI GetData(Guid Form_ID)
        {

            SqlCommand command = new SqlCommand();
            command.Parameters.AddWithValue("@Form_ID", Form_ID);
            command.CommandText = $@"	SELECT * FROM  MigrationDetail WHERE Form_ID=@Form_ID";
            return new MSSQLFactory().GetObject<MigrationPI>(command);
        }
        public int GetLastSerialNo()
        {
            var query = $@"SELECT Top 1 SerialNo FROM  MigrationDetail Order By SerialNo Desc";
            return new MSSQLFactory().ExecuteScalar<int>(query);
        }
    }
}
