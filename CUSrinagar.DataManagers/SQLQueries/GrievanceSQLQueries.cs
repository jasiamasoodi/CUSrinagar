using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class GrievanceSQLQueries
    {
        internal SqlCommand GetGrievance(Guid grievance_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = " SELECT * FROM Grievance WHERE Grievance_ID = @Grievance_ID ";
            command.Parameters.AddWithValue("Grievance_ID", grievance_ID);
            return command;
        }

        internal string GetGrievanceLisQuerytCompact()
        {
            var query = $@"SELECT Grievance_ID,GrievanceID,FullName,Subject,Message,Category,Date,Status FROM dbo.Grievance ";
            return query;
        }

        internal SqlCommand GetGrievanceReplies(Guid grievance_ID)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = " SELECT GrievanceReply.*, 14 RoleID FROM GrievanceReply WHERE Grievance_ID = @Grievance_ID" +
                $" ORDER BY DATE";
            command.Parameters.AddWithValue("Grievance_ID", grievance_ID);
            return command;
        }

        internal string UpdateGrievanceCount()
        {
            return @" UPDATE Settings SET Count = Count + 1; ";
        }
    }
}
