using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;

namespace CUSrinagar.OAuth
{
    public interface IPrincipalProvider : IPrincipal
    {
        AppUsers AppUser { get; set; }
    }
}
