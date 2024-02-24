using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AppUserRoles : BaseWorkFlow
    {
        public Guid User_ID { get; set; }
        public AppRoles RoleID { get; set; }
        public string RoleName { get; set; }
    }
}
