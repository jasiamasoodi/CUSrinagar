using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AppRegisteredEmail
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(250, ErrorMessage = " Max 250 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string RegisteredEmail { get; set; }
    }
    public class AppChangePassword
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Current Password")]
        [MinLength(3, ErrorMessage = " Min 3 chars")]
        [MaxLength(35, ErrorMessage = " Max 35 chars")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("New Password")]
        [MinLength(8, ErrorMessage = " Min 8 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [RegularExpression(@"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W]).{8,30})", ErrorMessage = " Invalid")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Confirm Password")]
        [MinLength(8, ErrorMessage = " Min 8 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [Compare("NewPassword",ErrorMessage ="New password and comfirm password did not match")]
        public string ConfrimPassword { get; set; }
    }    
}
