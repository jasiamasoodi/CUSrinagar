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
    public class ResultHistory : BaseWorkFlow
    {
        public Guid ResultHistory_ID { get; set; }
        public Guid Semester_ID { get; set; }
        public Guid? ExamForm_ID { get; set; }
        public Programme Programme { get; set; }
        public Semester Semester { get; set; }
        public Guid? ResultNotification_ID { get; set; }
        public decimal? ExternalMarks { get; set; }
        public decimal? ExternalAttendance_AssessmentMarks { get; set; }
        public decimal? InternalMarks { get; set; }
        public decimal? InternalAttendance_AssessmentMarks { get; set; }


    }
    public class oldResultHistory : SemesterModel
    {
        public Guid ResultHistory_ID { get; set; }

        public Guid Entity_ID { get; set; }

        public string PrintProgramme { get; set; }

        public string Semester { get; set; }

        public short PreviousMarks { get; set; }

        public MarksType MarksType { get; set; }
    }

    public class ResultInternalsHistory : oldResultHistory
    {
        public Guid ResultHistoryInternal_ID { get; set; }

        public decimal PreviousTheoryAttendance { get; set; }

        public decimal PreviousPracticalMarks { get; set; }

        public decimal PreviousPracticalAttendance { get; set; }

    }
}
