using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Enums
{
    public enum SignInStatus
    {
        Failed = 0,
        Success = 1,
        LockedOut = 2,
        Disabled = 3,
        PasswordExpired = 4
    }
}
