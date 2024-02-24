using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CUSrinagar.Models
{
    public class Alomini
    {
        public Guid Student_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Current Employment Status")]
        [MaxLength(20,ErrorMessage ="Max 20 chars")]
        public string EmploymentStatus { get; set; }

        [DisplayName("Organisation / Description / Other Details(500 chars max)")]
        [MaxLength(500,ErrorMessage ="Max 500 chars")]
        public string OtherDetails { get; set; }
        public DateTime LastSavedOn { get; set; }
    }
}
