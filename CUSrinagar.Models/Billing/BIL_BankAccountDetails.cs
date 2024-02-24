using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class BIL_BankAccountDetails
    {
        public Guid Account_ID { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Bank Name is required")]
        [DisplayName("Bank Name")]
        public string BankName { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Branch is required")]
        public string Branch { get; set; }

        [MaxLength(16)]
        [Required(ErrorMessage = "Account No is required")]
        [DisplayName("Account Number")]
        [StringLength(16,MinimumLength =16,ErrorMessage ="Account Number must be 16 digits")]
        public string AccountNo { get; set; }

        [MaxLength(11)]
        [Required(ErrorMessage = "IFS Code is required")]
        [DisplayName("IFS Code")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "IFSC Code must be 11 characters")]
        public string IFSCode { get; set; }

        public Guid User_ID { get; set; }

        [IgnoreDBWriter]
        public bool IsEditable { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
