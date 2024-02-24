using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using Terex;
using System.Data.SqlClient;
using CUSrinagar.DataManagers.SQLQueries;
using System.Web.Mvc;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{

    public class NotificationDB
    {
        public string GetMarqueeText()
       => new MSSQLFactory().ExecuteScalar<string>($"SELECT TOP 1 MarqueeText FROM NotiMarquee;");

        public int SaveUpdateMarqueeText(string marqueeText)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $@"DELETE FROM dbo.NotiMarquee;
                                INSERT INTO dbo.NotiMarquee(MarqueeText) VALUES(@mText);";
            cmd.Parameters.AddWithValue("@mText", marqueeText.Trim());
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }


        public List<Notification> GetAllNotificationRecords(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<Notification>(new NotificationSQLQueries().GetAllNotificationRecords(parameter));
        }
        public List<NotificationCompactList> CompactList(Parameters parameter)
        {
            string Query = "SELECT Description,Link,IsLink from Notification ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<Notification>(parameter.Filters);
            helper.Command.CommandText = Query + helper.WhereClause + new GeneralFunctions().GetPagedQuery(Query, parameter);
            return new MSSQLFactory().GetObjectList<NotificationCompactList>(helper.Command);
        }

        public int AddNotification(Notification input)
        {
            return new MSSQLFactory().InsertRecord<Notification>(input, "Notification");
        }

        public int EditNotification(Notification input, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord<Notification>(input, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE Notification_ID=@Notification_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);

        }

        public List<Notification> GetAllNotifications()
        {
            return new MSSQLFactory().GetObjectList<Notification>(new NotificationSQLQueries().GetAllNotifications);
        }

        public Notification GetNotificationById(Guid Notification_Id)
        {
            return new MSSQLFactory().GetObject<Notification>(new NotificationSQLQueries().GetNotificationById(Notification_Id));
        }
        public List<Notification> GetNotificationByType(NotificationType NotificationType, Parameters parameter)
        {
            try
            {
                return new MSSQLFactory().GetObjectList<Notification>(new NotificationSQLQueries().GetNotificationByType(NotificationType, parameter));
            }
            catch (TypeInitializationException)
            {
                return new List<Notification>();
            }
        }
        public List<SelectListItem> GetNotificationSLByType(NotificationType NotificationType)
        {

            return new MSSQLFactory().GetObjectList<SelectListItem>(new NotificationSQLQueries().GetNotificationSLByType(NotificationType));

        }

        public dynamic GetNotificationSLByType(List<int> notifications)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>(new NotificationSQLQueries().GetNotificationSLByType(notifications));
        }
    }

}
