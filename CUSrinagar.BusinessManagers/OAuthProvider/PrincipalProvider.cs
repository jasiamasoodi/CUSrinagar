using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using CUSrinagar.Models;
using CUSrinagar.Extensions;

namespace CUSrinagar.OAuth
{
    public class PrincipalProvider : IPrincipalProvider
    {
        public AppUserCompact AppUser { get; set; }
        public IIdentity Identity { get; private set; }
        public PrincipalProvider(AppUserCompact _AppUser, object _GenericIdentity)
        {
            this.Identity = new GenericIdentity(_GenericIdentity.ToString());
            this.AppUser = _AppUser;
        }
        public bool IsInRole(string role)
        {
            //check for roles here
            if (!string.IsNullOrWhiteSpace(role))
            {
                bool result = false;
                foreach (var item in role.Split(',').ToList())
                {
                    result = AppUser.UserRoles.Any(x => item.ToLower().Trim() == x.ToString().ToLower().Trim());
                    if (result)
                        return true;
                }
                return result;
            }
            return true;
        }
    }
}