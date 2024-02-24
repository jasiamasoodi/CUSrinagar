using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class Contact
    {
        public string Name { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Email")]
        [MaxLength(150, ErrorMessage = " Max 150 chars")]
        [RegularExpression(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+", ErrorMessage = " Invalid")]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required(ErrorMessage = " Required")]
        public string Message { get; set; }

    }
}
