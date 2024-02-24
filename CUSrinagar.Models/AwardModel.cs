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
    public class AwardModelCompact : BaseWorkFlow
    {
        public Guid _ID { get; set; }
        public Guid Student_ID { get; set; }
        public Guid Subject_ID { get; set; }

        public string ExternalMarks { get; set; }
        public string ExternalAttendance_AssessmentMarks { get; set; }
        public bool ExternalSubmitted { get; set; }

        public string InternalMarks { get; set; }
        public string InternalAttendance_AssessmentMarks { get; set; }
        public bool InternalSubmitted { get; set; }

        public Guid? ResultNotification_ID { get; set; }
    }
    public class AwardModel
    {
        public bool IsBacklog { get; set; }
        public Guid Subject_ID { get; set; }
        public Guid? ExamForm_ID { get; set; }
        //  public int Semester { get; set; }
        //public MarksFor MarksFor { get; set; }
        public string CUSRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string ExamRollNumber { get; set; }
        public string StudentCode { get; set; }
        public Decimal ExternalMarks { get; set; }
        public Decimal ExternalAttendance_AssessmentMarks { get; set; }
        public Decimal InternalMarks { get; set; }
        public Decimal InternalAttendance_AssessmentMarks { get; set; }
        public bool ExternalSubmitted { get; set; }
        public bool InternalSubmitted { get; set; }
        public int Session { get; set; }
        public RecordState RecordStatus { get; set; }
        public Guid _ID { get; set; }
        public string ClassRollNo { get; set; }
        public Guid Student_ID { get; set; }
        public Guid? ResultNotification_ID { get; set; }
        public short Batch { get; set; }
        [IgnoreDBWriter]
        public short SemesterBatch { get; set; }
        [IgnoreDBWriter]
        public string CombinationSubjects { get; set; }
        [IgnoreDBWriter]
        public int SubjectMinMarksTheory { get; set; }
        [IgnoreDBWriter]
        public int SubjectMinMarksPractical { get; set; }
        [IgnoreDBWriter]
        public Guid Subject_IDCpy { get; set; }
        [IgnoreDBWriter]
        public bool InternalResult { get; set; }
        [IgnoreDBWriter]
        public bool OverallResult { get; set; }
        [IgnoreDBWriter]
        public Guid UpdatedBy { get; set; }
    }

}
