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

    public class SubjectDetails
    {      
            public Guid Subject_ID { get; set; }
            public Guid Course_ID { get; set; }
            public string SubjectCode { get; set; }
            public string SubjectFullName { get; set; }
            public short TheoryMinPassMarks { get; set; }
            public short TheoryMaxMarks { get; set; }
            public int PracticalMinPassMarks { get; set; }
            public int PracticalMaxMarks { get; set; }
            public bool IsCompulsory { get; set; }
            public bool Status { get; set; }
            public DateTime CreatedOn { get; set; }
            public Guid? CreatedBy { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public Guid? UpdatedBy { get; set; }
            public int TheoryAttendance { get; set; }
            public int PracticalAttendence { get; set; }
            public short? IsCore { get; set; }
            public Guid? College_ID { get; set; }
            public int? Credit { get; set; }
            public SubjectType subjectType { get; set; }

            public string CourseCode { get; set; }
            public string CoursefullName { get; set; }
            public string SemesterName { get; set; }

            public String SubjectType { get; set; }
            public string Department { get; set; }
            public string Semester { get; set; }        

    }
}
