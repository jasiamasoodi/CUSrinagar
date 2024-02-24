using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Web;
using GeneralModels;
using CUSrinagar.Enums;
using System.Transactions;
using System.Threading.Tasks;

namespace CUSrinagar.BusinessManagers
{
    public class ResultNotificationManager
    {
        public List<ResultNotificationList> ListCompact(Parameters parameters)
        {
            if (parameters == null) parameters = new Parameters();
            if (parameters.SortInfo == null || string.IsNullOrEmpty(parameters.SortInfo.ColumnName))
                parameters.SortInfo = new Sort() { ColumnName = "Dated", OrderBy = SortOrder.Descending };

            return new ResultNotificationDB().ListCompact(parameters);
        }
        public Task<ResultNotification> GetAsync(Guid ResultNotification_ID)
        {
            return new TaskFactory().StartNew(() => new ResultNotificationDB().Get(ResultNotification_ID));
        }
        
        public  ResultNotification  CheckAnyBacklog(Guid id,String Searchtext,PrintProgramme printProgramme,short semester)
        {
            return new ResultNotificationDB().CheckAnyBacklog(id,Searchtext, printProgramme, semester);
        }
        public ResultNotification Get(Guid ResultNotification_ID)
        {
            return new ResultNotificationDB().Get(ResultNotification_ID);
        }

        public List<SelectListItem> DDL(Parameters parameters)
        {
            var collection = new ResultNotificationDB().List(parameters);
            List<SelectListItem> list = new List<SelectListItem>();
            if (collection != null)
                foreach (var item in collection)
                    list.Add(new SelectListItem() { Text = $"{item.PrintProgramme.ToString()}:Sem-{item.Semester}:{item.Title}:{item.Dated.ToString("dd-MMM-yyyy")}:{(item.ParentNotification_ID.HasValue ? "HasParent" : "")}", Value = item.ResultNotification_ID.ToString() });
            return list;
        }

        public List<AwardCount> ValidateResult(ResultNotification resultNotification)
        {
            return new ResultNotificationDB().GetCount(resultNotification);
        }
    }
}
