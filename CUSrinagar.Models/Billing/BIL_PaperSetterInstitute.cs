using System;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class BIL_PaperSetterInstitute
    {
        public Guid User_ID { get; set; }

        [MaxLength(200)]
        [Required(ErrorMessage = "Institute is required")]
        public string Institute { get; set; }
    }

}
