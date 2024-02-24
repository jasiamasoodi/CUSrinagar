using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
   public class AppLogin
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Username Or Email")]
        [MinLength(3, ErrorMessage = " Min 3 chars")]
        [MaxLength(35, ErrorMessage = " Max 35 chars")]
        public string UserName { get; set; }


        [Required(ErrorMessage = " Required")]
        [DisplayName("Password")]
         public string Password { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        [Required(ErrorMessage = " Required")]
        [DisplayName("Confirm Password")]
        [MinLength(3, ErrorMessage = " Min 3 chars")]
        [MaxLength(35, ErrorMessage = " Max 35 chars")]
        public string ConfirmPassword { get; set; }
        [DisplayName("Keep me Signed-In")]
        public bool RememberMe { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string ReturnUrl { get; set; }
    }
    public class ForgotPassword
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Username / Email")]
        [MinLength(3, ErrorMessage = " Min 3 chars")]
        [MaxLength(35, ErrorMessage = " Max 35 chars")]
        public string UserName { get; set; }


        [Required(ErrorMessage = " Required")]
        [DisplayName("Password")]
        [MinLength(8, ErrorMessage = " Min 8 chars")]
        [MaxLength(30, ErrorMessage = " Max 30 chars")]
        [RegularExpression(@"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W]).{8,30})", ErrorMessage = " Invalid")]
        public string Password { get; set; }
        [IgnoreDBRead]
        [IgnoreDBWriter]
        [Required(ErrorMessage = " Required")]
        [DisplayName("Confirm Password")]
        [MinLength(3, ErrorMessage = " Min 3 chars")]
        [MaxLength(35, ErrorMessage = " Max 35 chars")]
        public string ConfirmPassword { get; set; }
    }
}
