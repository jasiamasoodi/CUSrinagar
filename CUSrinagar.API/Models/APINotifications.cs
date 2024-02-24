using System;
using System.Collections.Generic;
using System.Linq;
using CUSrinagar.Models;

namespace CUSrinagar.API.Models
{
    public class APINotification
    {
        public List<Notification> AppNotifications { get; set; }
        public List<ButtonsLinks> AppButtonsLinks { get; set; }
    }
}