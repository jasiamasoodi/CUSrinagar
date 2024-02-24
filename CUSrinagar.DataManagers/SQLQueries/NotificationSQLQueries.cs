using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class NotificationSQLQueries
    {
        internal string GetAllNotifications = "SELECT * FROM [Notification] Order By StartDate Desc";
        internal SqlCommand GetAllNotificationRecords(Parameters parameter)
        {
            string query = "SELECT * from(SELECT *,StartDate As StartDateTo FROM Notification) tempSubject";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Notification>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }

        internal SqlCommand GetNotificationById(Guid Notification_Id)
        {
            SqlCommand command = new SqlCommand();
            string query = @"SELECT * FROM [Notification]
                                     WHERE Notification_ID = @Notification_Id";

            command.Parameters.AddWithValue("@Notification_ID", Notification_Id);
            command.CommandText = query;
            return command;
        }
        internal SqlCommand GetNotificationByType(NotificationType NotificationType, Parameters parameter)
        {
            string currentDate = DateTime.Now.ToShortDateString();
            DateTime date = DateTime.MinValue;

            if (DateTime.TryParse(currentDate, out date))
                currentDate = date.ToString("yyyy/MM/dd");
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT * FROM [Notification]
                                     WHERE NotificationType = @NotificationType
                                        AND Status=1
                                        AND ENDDATE >= '{currentDate}'";
            int type = (int)NotificationType;
            command.Parameters.AddWithValue("@NotificationType", type);
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            command.CommandText = query;
            return command;
        }
        internal SqlCommand GetNotificationSLByType(NotificationType NotificationType)
        {
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT  Distinct  CONVERT(nvarchar(50),[Notification_ID]) AS [Value],TRIM(Description) AS [Text] FROM [Notification]
                                     WHERE NotificationType = @NotificationType
                                        AND Status=1";
            int type = (int)NotificationType;
            command.Parameters.AddWithValue("@NotificationType", type);
            command.CommandText = query;
            return command;
        }

        internal SqlCommand GetNotificationSLByType(List<int> notifications)
        {
            string notificationstr = string.Join(",", notifications);
            SqlCommand command = new SqlCommand();
            string query = $@"SELECT  Distinct  CONVERT(nvarchar(50),[Notification_ID]) AS [Value],TRIM(Description) AS [Text] FROM [Notification]
                                     WHERE NotificationType IN ({notificationstr})
                                        AND Status=1";
            command.CommandText = query;
            return command;
        }
    }
}
