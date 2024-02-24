using CUSrinagar.Enums;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using CUSrinagar.BusinessManagers;
using System.Collections.Generic;
using System.Web.Mvc;
using GeneralModels;
using Newtonsoft.Json;
using System.Linq;

namespace CUSrinagar.WebApp.CUStudentZone.Controllers
{
    [OAuthorize(AppRoles.Student)]
    public class MessageController : StudentBaseController
    {
        public ActionResult Message()
        {
            GetResponseViewBags();
            return View();
        }
        public ActionResult MessageList(Parameters parameter)
        {
            parameter.SortInfo = new Sort() { ColumnName = "Date", OrderBy = System.Data.SqlClient.SortOrder.Descending };
            List<Message> list = new MessageManager().GetStudentMessageList(parameter);
            return PartialView(list);
        }

        public string GetLatestMessages()
        {
            List<Message> list = new MessageManager().GetStudentQuickMessageList();
            if (list == null) return JsonConvert.SerializeObject(new List<Message>());
            var newlist = list.Select(x => new { x.MessageTitle, Date = x.Date.ToString("dd-MMMM-yyyy") }).ToList();
            return JsonConvert.SerializeObject(newlist);
        }

    }
}