using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace CUSrinagar.Models
{
    public class ResultNotificationList
    {
        public Guid ResultNotification_ID { get; set; }
        public short Semester { get; set; }
        public short? Batch { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public string Title { get; set; }
        public DateTime Dated { get; set; }
        public bool ImportedToMasterTable { get; set; }
    }


    public class ResultNotification : BaseWorkFlow
    {
        public Guid ResultNotification_ID { get; set; }
        [Required(ErrorMessage = "Semester is required")]
        public short Semester { get; set; }
        [Required(ErrorMessage = "Batch is required")]
        public short? Batch { get; set; }
        [Required(ErrorMessage = "Print Programme is required")]
        public PrintProgramme PrintProgramme { get; set; }
        [IgnoreDBWriter]
        [Required(ErrorMessage = "Programme is required")]
        public Programme Programme { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Site Notification is required")]
        [Display(Name = "Site Notification")]
        public string ResultNotificationID { get; set; }
        [Required(ErrorMessage = "Notification No is required")]
        [Display(Name ="Notification No")]
        public string NotificationNo { get; set; }
        [Required(ErrorMessage = "Date is required")]
        public DateTime Dated { get; set; }
        [Required(ErrorMessage = "Remark is required")]
        public string Remark { get; set; }
        [Display(Name = "Parent Notification")]
        public Guid? ParentNotification_ID { get; set; }
        public bool ImportedToMasterTable { get; set; }
        public bool IsActive { get; set; }
        [IgnoreDBWriter]
        [Display(Name = "Exam Form Submission Year")]
        public int ExamFormSubmissionYear { get; set; }
        public bool ShowPrintMarksCard { get; set; }
        public bool ShowPrintTranscript { get; set; }
        public bool ShowPrintDegreeCertificate { get; set; }
        [IgnoreDBWriter]
        public Guid College_ID{ get; set; }
        [IgnoreDBWriter]
        public bool IsBacklog { get; set; }
        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public Guid Subject_ID { get; set; }
        public List<AwardCount> AwardCounts { get; set; }
        public List<Guid> CourseIds { get; set; }

    }
    public class AwardCount
    {
    public int TotalCount { get; set; }
    public int InternalCount { get; set; }
    public int ExternalCount { get; set; }
    }
}
