using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{

    //public class ADMSubjectMaster
    //{
    //    public Guid _ID { get; set; }

    //    public Guid Subject_ID { get; set; }

    //    public string Name { get; set; }

    //    public Guid Course_ID { get; set; }

    //    public int MinMarks { get; set; }

    //    public int MaxMarks { get; set; }

    //    public decimal TheoryMarks { get; set; }

    //    public decimal PracticalAttendance { get; set; }

    //    public decimal PracticalMarks { get; set; }

    //    public decimal TheoryAttendance { get; set; }
    //    [IgnoreDBWriter]
    //    public string UpdatedByUserName { get; set; }

    //    public ResultStatus ResultStatus { get; set; }

    //    public string Subjects { get; set; }
    //    public short Semerster { get; set; }

    //    [IgnoreDBRead]
    //    [IgnoreDBWriter]
    //    public string SemesterName { get; set; }

    //    [IgnoreDBRead]
    //    [IgnoreDBWriter]
    //    public RecordState RecordStatus { get; set; }


    //}

    //public class SubjectCombinations
    //{

    //    public string ColgCode { get; set; }

    //    public string CourseCode { get; set; }

    //    public string CombinationCode { get; set; }

    //    public string FullCode { get; set; }

    //    public Programme Category { get; set; }

    //    public string Subject1Name { get; set; }

    //    public Guid Subject1_ID { get; set; }

    //    public Guid Subject2_ID { get; set; }

    //    public string Subject2Name { get; set; }

    //    public Guid Subject3_ID { get; set; }

    //    public string Subject3Name { get; set; }

    //    public Guid Subject4_ID { get; set; }

    //    public string Subject4Name { get; set; }

    //    public Guid Subject5_ID { get; set; }

    //    public string Subject5Name { get; set; }

    //    public Guid Subject6_ID { get; set; }

    //    public string Subject6Name { get; set; }

    //    public Guid Subject7_ID { get; set; }

    //    public string Subject7Name { get; set; }

    //    public string ExamRollNumber { get; set; }
    //}


    public class StudentAdditionalSubject : BaseWorkFlow
    {
        public Guid AdditionalSubject_ID { get; set; }
        public Guid Student_ID { get; set; }
        public Guid Subject_ID { get; set; }
        public bool IsVerified { get; set; }
        public short? Semester { get; set; }
        public Guid Course_ID { get; set; }
        public int SemesterBatch { get; set; }
        [IgnoreDBWriter]
        public ADMSubjectMaster Subject { get; set; }
    }
}
