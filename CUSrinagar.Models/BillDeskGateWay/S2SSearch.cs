using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class S2SSearch
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Fee Type")]
        public PaymentModuleType FeeType { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Course")]
        public PrintProgramme Course { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Search Query")]
        [MaxLength(200, ErrorMessage = " Max 200 chars")]
        public string SearchQuery { get; set; }

        [DisplayName("Batch")]
        [Range(2016, 9999, ErrorMessage = "Invalid")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = " Invalid")]
        public string Batch { get; set; }

        [DisplayName("TxnDate")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        public string TxnDate { get; set; }
    }

    public class BillDeskStoredRequest
    {
        public string Request { get; set; }
        public DateTime CreatedOn { get; set; }

        [IgnoreDBRead][IgnoreDBWriter]
        public string[] RequestArray
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Request))
                    return Request.Split('|');
                else
                    return null;
            }
        }
    }
}
