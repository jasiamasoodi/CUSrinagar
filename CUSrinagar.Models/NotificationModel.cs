using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class NotificationCompactList
    {
        public string Description { get; set; }
        public string Link { get; set; }
        public bool IsLink { get; set; }
    }
    public class Notification : BaseWorkFlow
    {
        [KeyAttribute]
        public Guid Notification_ID { get; set; }


        [Required(ErrorMessage = "Description required")]
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase Files { get; set; }

        [StringLength(250)]

        public string Link { get; set; }

        [Required(ErrorMessage = "Posting Date required")]
        [Display(Name = "Posting Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:mm/dd/yyyy}")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Closing Date required")]
        [Display(Name = "Closing Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:mm/dd/yyyy}")]
        public DateTime EndDate { get; set; }

        public bool Status { get; set; }
        public bool IsLink { get; set; }

        [Required(ErrorMessage = "Notification Type required")]
        public NotificationType NotificationType { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public Semester Semester { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public DateTime StartDateTo { get { return StartDate; } }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }
    }
}
