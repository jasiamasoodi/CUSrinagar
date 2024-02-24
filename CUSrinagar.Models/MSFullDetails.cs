using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class MSFullDetails
    {
        public Guid Marks_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public string  SubjectCode  { get; set; }
        public string  SubjectTitle { get; set; }
        public short SubjectType { get; set; }
        public short  Semester { get; set; }
        public short Credit { get; set; }
        public short CreditWeightage { get; set; }
        public Guid Student_Id { get; set; }
        public short InternalMinMarks { get; set; }
        public short InternalMaxMarks { get; set; }
        public short InternalMarksObt { get; set; }
        public bool InternalResultStatus { get; set; }
        public short ExternalMinMarks { get; set; }
        public short ExternalMaxMarks { get; set; }
        public short ExternalMarksObt { get; set; }
        public bool ExternalResultStatus { get; set; }
        public bool IsLocked { get; set; }

    }
}
