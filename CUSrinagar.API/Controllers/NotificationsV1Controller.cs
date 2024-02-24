using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CUSrinagar.BusinessManagers;
using CUSrinagar.API.Models;
using CUSrinagar.Models;

namespace CUSrinagar.API.Controllers
{
    [RoutePrefix("api/v1/Notifications")]
    public class NotificationsV1Controller : ApiController
    {
        [HttpGet]
        [Route("All")]
        public IHttpActionResult All()
        {
            var parms = new NotificationManager().GetDefaultParameter(new GeneralModels.Parameters(), false,true, true, 20, true);

            APINotification _APINotification = new APINotification
            {
                AppNotifications = new NotificationManager().GetAllNotificationList(parms),
                AppButtonsLinks = ButtonsLinksManager.GetButtonsLinks()
            };

            return Ok(_APINotification);
        }
        [HttpGet]
        [Route("Notification/{id:guid}")]
        public IHttpActionResult Notification(Guid id)
        {
            Notification _Notification= new NotificationManager().GetNotificationById(id);
            return Ok(_Notification);
        }
    }
}
