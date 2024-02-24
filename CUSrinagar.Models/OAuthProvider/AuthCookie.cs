using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AuthCookie
    {
        public static string Name { get { return "CUSAuth"; } }
        public static int MaxLoginAttempts { get { return 5; } }
        public static int MaxLockHours { get { return 3; } }
        public static int MaxResetPasswordMonths { get { return 3; } }
    }
}
