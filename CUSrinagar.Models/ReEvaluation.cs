using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models.ValidationAttrs;

namespace CUSrinagar.Models
{

    public class ReEvaluation : BaseWorkFlow
    {
        public Guid ReEvaluation_ID { get; set; }

        public Guid Student_ID { get; set; }

        [IgnoreDBWriter]
        public string FullName { get; set; }

        public decimal FeeAmount { get; set; }

        public List<ReEvaluationStudentSubject> SubjectsForEvaluation { get; set; }

        public FormStatus FormStatus { get; set; }

        public string FormNumber { get; set; }

        public FormType FormType { get; set; }

        [IgnoreDBWriter]
        [DBColumnName("TxnDate")]
        public DateTime DateFrom { get; set; }

        [IgnoreDBWriter]
        [DBColumnName("TxnDate")]
        public DateTime DateTo { get; set; }

        [IgnoreDBWriter]
        public string ExamRollNumber { get; set; }

        public Guid? Notification_ID { get; set; }
        public short SubmittedYear { get; set; }
        public short Semester { get; set; }
        
        [IgnoreDBWriter]
        public string Code { get; set; }
        
        [IgnoreDBWriter]
        public string MobileNo { get; set; }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        // Only For Filter Purpose
        [IgnoreDBWriter]        
        public short SemesterBatch { get; set; }

        // Only For Filter Purpose
        [IgnoreDBWriter]        
        public Programme Programme { get; set; }

        // Only For display Purpose
        [IgnoreDBWriter]        
        public string CourseFullName { get; set; }

        // Only For display Purpose
        [IgnoreDBWriter]        
        public string CenterDetail { get; set; }
    }

    public class ReEvaluationSetting
    {
        public Guid ReEvaluationSetting_ID { get; set; }
        public ExaminationCourseCategory CourseCategory { get; set; }
        public short Semester { get; set; }
        public string FormNumberPrefix { get; set; }
        public int FormNumberCount { get; set; }
        public bool AllowDownloadForm { get; set; }
        public short SubmittedYear { get; set; }
        public decimal FeePerSubject { get; set; }
        public bool IsReEvaluation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public short DownloadDays { get; set; }
        public Guid? Notification_ID { get; set; }
        public string ResultNotification_IDs { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public List<ResultNotification> ResultNotifications { get; set; }
        public bool ValidateByExaminationForm { get; set; }
        public bool ValidateByResultNotificationIDs { get; set; }
    }

    public class ReEvaluationStudentSubject : BaseWorkFlow
    {
        public Guid ReEvaluationSubject_ID { get; set; }
        public Guid ReEvaluation_ID { get; set; }

        public Guid Subject_ID { get; set; }
        [IgnoreDBWriter]
        public string SubjectID { get; set; }
        public Guid Student_ID { get; set; }
        [IgnoreDBWriter]
        public bool OptForReEvaluation { get; set; }
        [IgnoreDBWriter]
        public bool OptForXerox { get; set; }
        [IgnoreDBWriter]
        public string FormNumber { get; set; }
        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }
    }

    public class ReEvaluationPayment : ReEvaluation
    {
        public PaymentDetails paymentDetails { get; set; }
    }

    public class ReEvaluationCompactList
    {
        public string FullName { get; set; }

        public int ReEvaluation_ID { get; set; }

        public int Student_ID { get; set; }

        public int FormStatus { get; set; }

        public int FormNumber { get; set; }

        public int Notification_ID { get; set; }

        public int Semester { get; set; }       

        public int CreatedOn { get; set; }

        public int ReEvaluationSubject_ID { get; set; }

        public int Subject_ID { get; set; }
        
    }


}
