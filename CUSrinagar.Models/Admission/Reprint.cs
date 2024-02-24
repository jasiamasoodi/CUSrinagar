using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CUSrinagar.Models
{
   public class ARGReprint : BaseWorkFlow
    {
        [DisplayName("Form Numer")]
        [Required(ErrorMessage = " Required")]       
        [MaxLength(150, ErrorMessage = "Max 150 chars")]
        [RegularExpression(@"^([a-zA-Z0-9]{1,}[A-Z0-9a-z._-]*)$", ErrorMessage = " Invalid")]
        public string FormNo { get; set; }

        [DisplayName("Date of birth (dd-mm-yyyy)")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:(?:0?[1-9]|1\d|2[0-8])\-(?:0?[1-9]|1[0-2]))\-(?:(?:1[6-9]|[2-9]\d)\d{2}))$|^(?:(?:(?:31\-0?[13578]|1[02])|(?:(?:29|30|31)\-(?:0?[1,3-9]|1[0-2])))\-(?:(?:1[6-9]|[2-9]\d)\d{2}))$|^(?:29\-0?2\-(?:(?:(?:1[6-9]|[2-9]\d)(?:0[48]|[2468][048]|[13579][26]))))$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead][IgnoreDBWriter]
        public string EnteredDOB { get; set; }


        public DateTime DOB
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EnteredDOB))
                {
                    string[] splitDate = EnteredDOB.Split('-');
                    if (splitDate != null && splitDate.Count() == 3)
                    {
                        try
                        {
                            return new DateTime(Convert.ToInt32(splitDate[2]), Convert.ToInt32(splitDate[1]), Convert.ToInt32(splitDate[0]));
                        }
                        catch (ArgumentOutOfRangeException) { }
                    }
                }
                return DateTime.MinValue;
            }
            set
            {
                DOB = value;
            }
        }
    }
}
