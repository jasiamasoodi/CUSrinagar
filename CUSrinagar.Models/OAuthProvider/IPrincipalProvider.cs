using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;

namespace CUSrinagar.Models
{
    public interface IPrincipalProvider : IPrincipal
    {
        AppUserCompact AppUser { get; set; }
    }
}
