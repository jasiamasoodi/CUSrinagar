using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using GeneralModels;
using System.Web;
using CUSrinagar.Enums;
using System.Transactions;
using System.Text.RegularExpressions;
namespace CUSrinagar.BusinessManagers
{
    public class NotificationManager
    {
        public string GetMarqueeText() => new NotificationDB().GetMarqueeText();
        public int SaveUpdateMarqueeText(string marqueeText)
        {
            return new NotificationDB().SaveUpdateMarqueeText((marqueeText + "").Replace("<br/>","").Replace("<BR/>", ""));
        }

        public object GetAllNotification(string sidx, string sord, bool _search, string filters, int page, int rows)
        {
            object jsonData = new
            {
                total = 0,//total number of pages
                page,
                records = 0,//number of rows
                rows = new List<Notification>()
            };
            Parameters parameter = new Parameters();
            List<Notification> listNotification = new NotificationDB().GetAllNotifications();

            if (listNotification?.Any() ?? false)
            {
                decimal totalCount = listNotification.Count();
                int total = (int)Math.Ceiling(totalCount / rows);
                page = page - 1;
                listNotification = listNotification.OrderByDescending(x => x.GetType().GetProperty(sidx).GetValue(x)).Skip(page * rows).Take(rows).ToList();
                page = page + 1;
                jsonData = new
                {
                    total = total,//total number of pages
                    page,
                    records = totalCount,//number of rows
                    rows = listNotification
                };
            }
            return jsonData;
        }

        public List<SearchFilter> GetDefaultSearchFilter(bool showAll)
        {
            List<SearchFilter> listfilter = new List<SearchFilter>();
            SearchFilter defaultFilter = new SearchFilter() { Column = "Status", Operator = Enums.SQLOperator.EqualTo, Value = "true", GroupOperation = Enums.LogicalOperator.AND };
            listfilter.Add(defaultFilter);
            if (!showAll)
            {
                DateTime date = DateTime.MinValue;
                string currentDate = DateTime.Now.ToShortDateString();
                if (DateTime.TryParse(currentDate, out date))
                    currentDate = date.ToString("yyyy/MM/dd");
                defaultFilter = new SearchFilter() { Column = "EndDate", Operator = Enums.SQLOperator.GreaterThanEqualTo, Value = currentDate, GroupOperation = Enums.LogicalOperator.AND };
                listfilter.Add(defaultFilter);
            }
            return listfilter;
        }


        public Parameters GetDefaultParameter(Parameters parameter, bool setfilter, bool setpageinfo, bool setsortinfo, int pagesize = 15, bool showAll = false)
        {
            if (setpageinfo)
                parameter.PageInfo = new Paging() { DefaultOrderByColumn = "StartDate", PageNumber = 1, PageSize = pagesize };

            if (setsortinfo)
                parameter.SortInfo = new Sort() { ColumnName = "StartDate", OrderBy = SortOrder.Descending };

            if (setfilter)
            {
                if (parameter.Filters == null)
                    parameter.Filters = new List<SearchFilter>();
                parameter.Filters.AddRange(GetDefaultSearchFilter(showAll));
            }
            return parameter;
        }
        public List<Notification> GetAllNotificationList(Parameters parameter)
        {
            List<Notification> listNotification = new NotificationDB().GetAllNotificationRecords(parameter);
            return listNotification;
        }
        public List<NotificationCompactList> CompactList(Parameters parameter)
        {
            return new NotificationDB().CompactList(parameter);
            
        }

        public int AddNotification(Notification input)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                string filePath = null;
                input.Notification_ID = Guid.NewGuid();
                if (input.Files != null)
                { filePath = UploadFile(input); }
                input.Status = true;
                input.Link = (input.IsLink) ? input.Link : filePath;
                input.SetWorkFlow(RecordState.New);
                result = new NotificationDB().AddNotification(input);
                ts.Complete();
            }
            if (result > 0)
            {
                PushNotification pushNotification = new PushNotification
                {
                    notification = new BasePushNotification
                    {
                        title = $"Cluster Univ. Sgr-{input.NotificationType.ToString()}",
                        body = input.Description.Length > 100 ? input.Description.Substring(0, 100) : input.Description
                    }
                };
                if (input.IsLink)
                    pushNotification.notification.click_action = input.Link;

                new PushNotificationManager().SendAsync(pushNotification);
            }
            return result;
        }

        public int EditNotification(Notification input)
        {
            int result = 0;
            using (var ts = new TransactionScope())
            {
                //Edit Logic
                string filePath = null;
                if (input.Files != null)
                {
                    new NotificationManager().DeletePreviousAttachment(input.Link);
                    filePath = UploadFile(input);
                    input.Link = filePath;
                }
                input.UpdatedOn = DateTime.Now;
                List<string> ignoreQuery = new List<string>() {

                nameof(input.Notification_ID)
            };
                input.SetWorkFlow(RecordState.Old);
                result = new NotificationDB().EditNotification(input, null, ignoreQuery);
                ts.Complete();
            }
            return result;
        }

        public string UploadFile(Notification input)
        {
            string fileName = FormatFileName(Path.GetFileName(input.Files.FileName), input.Notification_ID);
            string filePath = GetFilePath() + "/" + fileName;
            input.Files.SaveAs(HttpRuntime.AppDomainAppPath + filePath);
            return filePath;

        }
        public bool checkFileExists(Notification input)
        {
            string fileName = FormatFileName(Path.GetFileName(input.Files.FileName), input.Notification_ID);
            string filePath = GetFilePath() + "/" + fileName;
            if (File.Exists(HttpRuntime.AppDomainAppPath + filePath))
            {
                return true;
            }
            return false;
        }
        public void DeletePreviousAttachment(string link)
        {
            if (link != null)
            {
                string path = link;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Format File Name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>

        public string FormatFileName(string fileName, Guid id)
        {


            string your_filename = fileName;
            string new_filename = Regex.Replace(your_filename, @"[^0-9a-zA-Z.]+", "");
            return new_filename.Trim();
        }


        /// <summary>
        /// Format File Path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetFilePath()
        {
            string path = HttpRuntime.AppDomainAppPath + "Notification";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = HttpRuntime.AppDomainAppPath + "FolderManager/Notification";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return "/FolderManager/Notification";
        }

        public Notification GetNotificationById(Guid Notification_Id)
        {
            return new NotificationDB().GetNotificationById(Notification_Id);
        }
        public List<Notification> GetNotificationByType(NotificationType NotificationType)
        {
            Parameters parameter = new Parameters();
            parameter = GetDefaultParameter(parameter, false, true, true, 11);
            return new NotificationDB().GetNotificationByType(NotificationType, parameter);
        }
        public List<SelectListItem> GetNotificationSLByType(NotificationType NotificationType)
        {
            return new NotificationDB().GetNotificationSLByType(NotificationType);
        }

        public dynamic GetNotificationSLByType(List<int> notifications)
        {
            return new NotificationDB().GetNotificationSLByType(notifications);
        }
        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            Notification notification = new NotificationManager().GetNotificationById(id);
            if (notification != null)
            {


                string deactivationMessage = "\"" + notification.Description + "\" Deactivated!";
                string activationMessage = "\"" + notification.Description + "\" Activated!";
                notification.Status = !notification.Status;
                msg = notification.Status ? activationMessage : deactivationMessage;
                notification.UpdatedOn = DateTime.Now;
                List<string> ignoreQuery = new List<string>() {

                nameof(notification.Notification_ID)
            };
                new NotificationDB().EditNotification(notification, null, ignoreQuery);
            }
            return msg;
        }

    }
}
