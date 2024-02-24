using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class MSStudentMarks
    {
        public Guid Marks_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectTitle { get; set; }
        public SubjectType SubjectType { get; set; }
        public short Semester { get; set; }
        public SGPAType SGPAType { get; set; }
         public decimal Credit { get; set; }
        public decimal CreditWeightage { get; set; }
        public Guid Student_ID { get; set; }
        public short InternalMinMarks { get; set; }
        public short InternalMaxMarks { get; set; }
        public short InternalMarksObt { get; set; }
        public bool InternalResultStatus { get; set; }
        public short ExternalMinMarks { get; set; }
        public short ExternalMaxMarks { get; set; }
        public short ExternalMarksObt { get; set; }
        public bool ExternalResultStatus { get; set; }
        public string GradeLetter { get; set; }
        public decimal GradePoints { get; set; }
        [IgnoreDBWriter]
        public string FullName{ get; set; }
        [IgnoreDBWriter]
        public int Batch{ get; set; }
        [IgnoreDBWriter]
        public string PrintProgramme{ get; set; }
        [IgnoreDBWriter]
        public Guid Course_Id { get; set; }
        public string CUSRegistrationNo { get; set; }
    }

    public class SubjectMarks
    {
        public Guid Subject_ID { get; set; }
        public string SubjectTitle { get; set; }
        public SGPAType SGPAType { get; set; }
        public SubjectType SubjectType { get; set; }
        public short Semester { get; set; }
        public decimal Credit { get; set; }
        public string GradeLetter { get; set; }
        public short GradePoints { get; set; }

        public short InternalMinMarks { get; set; }
        public short InternalMaxMarks { get; set; }
        public short InternalMarksObt { get; set; }
        public bool InternalResultStatus { get; set; }
        public short ExternalMinMarks { get; set; }
        public short ExternalMaxMarks { get; set; }
        public short ExternalMarksObt { get; set; }
        public bool ExternalResultStatus { get; set; }

        public short TotalMaxMarks { get; set; } //For Engineering Module

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public short SortOrder
        {
            get
            {
                switch (SubjectType)
                {
                    case SubjectType.Core:
                    case SubjectType.Major:
                        return 1;
                    case SubjectType.DSE:
                    case SubjectType.MID:
                        return 2;
                    case SubjectType.DCE:
                    case SubjectType.MDisc:
                        return 3;
                    case SubjectType.GE:
                    case SubjectType.VAC:
                        return 4;
                    case SubjectType.OE:
                    case SubjectType.AE:
                    case SubjectType.BSC:
                        return 5;
                    case SubjectType.MIL:
                    case SubjectType.ESC:
                    case SubjectType.MVoc:
                        return 6;
                    case SubjectType.HSMC:
                    case SubjectType.CourseType_01:
                        return 7;
                    case SubjectType.CourseType_02:
                        return 8;
                    case SubjectType.CourseType_03:
                        return 9;
                    case SubjectType.OC:
                        return 10;
                    case SubjectType.Lab:
                        return 11;
                    case SubjectType.Practical:
                        return 12;
                    case SubjectType.Workshop:
                        return 13;
                    case SubjectType.Internship:
                        return 14;
                    case SubjectType.Research:
                        return 15;
                    case SubjectType.Seminar:
                        return 16;
                    case SubjectType.FirstSemesterExclusion:
                        return 17;
                    case SubjectType.SEC:
                        return 18;
                    default:
                        return 100;
                }
            }
        }

    }    
}
