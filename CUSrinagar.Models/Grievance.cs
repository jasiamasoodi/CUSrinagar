using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{

    public class Grievance : BaseWorkFlow
    {
        public Guid Grievance_ID { get; set; }

        public string GrievanceID { get; set; }

        [Required]
        public string FullName { get; set; }

        public Guid? Student_ID { get; set; }

        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters.")]
        public string Subject { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters.")]
        [Required]
        public string Message { get; set; }
        public Guid? College_ID { get; set; }

        public Guid? UserAssigned_ID { get; set; }

        public GrievanceCategory Category { get; set; }

        public DateTime Date { get; set; }

        public string VerificationCode { get; set; }

        public bool IsNumberVerified { get; set; }

        public GrievanceStatus Status { get; set; }

        // dbo.GrievanceReplies.Grievance_ID -> dbo.Grievance.Grievance_ID (FK_GrievanceReplies_Grievance)
        public List<GrievanceReply> GrievanceReplies { get; set; }

        public bool AllowViewPublic { get; set; }
        public bool IsDiscarded { get; set; }
        //[IgnoreDBWriter]
        //public string CUSRegistrationNo { get; set; }
    }

    public class GrievanceReply : BaseWorkFlow
    {
        public Guid GrievanceReply_ID { get; set; }
        public Guid Grievance_ID { get; set; }
        public Guid? UserAssigned_ID { get; set; }
        [IgnoreDBWriter]
        public AppRoles RoleID { get; set; }
        public string FullName { get; set; }
        public bool IsAssignmentProcess { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool MarkedAsResolved { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public bool AllowViewPublic { get; set; }
        [IgnoreDBRead, IgnoreDBWriter]
        public bool IsDiscarded { get; set; }

    }

    public class GrievanceList
    {
        public Guid Grievance_ID { get; set; }
        public string GrievanceID { get; set; }
        public string FullName { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public GrievanceCategory Category { get; set; }
        public DateTime Date { get; set; }
        public GrievanceStatus Status { get; set; }
    }

    public class BatchUpdateGrievance
    {
        public List<Guid> Grievance_IDs { get; set; }
        public Guid UserAssigned_ID { get; set; }
        public bool UpdateUserAssigned { get; set; }
    }

    public class GrievanceWidgetSummary
    {
        public GrievanceCategory Category { get; set; }

        public int NoOfQueries { get; set; }
    }


    public class GrievanceResolvedWidgetSummary
    {
        public int Resolved { get; set; }

        public int UnResolved { get; set; }
    }

    public class GrievanceVerifiedWidgetSummary
    {
        public int Verified { get; set; }

        public int UnVerified { get; set; }
    }

    public class GrievanceAssignedWidgetSummary
    {
        public int Assigned { get; set; }

        public int UnAssigned { get; set; }
    }

    public class GrievanceGeneralWidgetSummary
    {
        public int AllowedForPublic { get; set; }

        public int NotAllowedForPublic { get; set; }
    }


    public class CourseCollegeWidget
    {
        public string CourseFullName { get; set; }

        public Guid Course_ID { get; set; }

        public PrintProgramme PrintProgramme { get; set; }
    }
    
}
