using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{

    public class AppUserEvaluatorSubjects : EvalvatorSubjectsBaseObject
    {
        public Guid EvaluatorSubject_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        public short Semester { get; set; }
        public Guid User_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        public Guid Subject_ID { get; set; }
        public bool Status { get; set; }
        public string ExamRollNoFrom { get; set; }
        public string ExamRollNoTo { get; set; }
        public string StudentCodeFrom { get; set; }
        public string StudentCodeTo { get; set; }
        public string Colleges { get; set; }
        [IgnoreDBWriter]
        //[Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }
        [IgnoreDBWriter]
        public List<string> CollegeList { get; set; }
    }
    public class EvalvatorSubjectsBaseObject : BaseWorkFlow
    {
        //public Int64? MinRollNo { get; set; }
        //public Int64? MaxRollNo { get; set; }

        [IgnoreDBWriter]
        public string MinRollNo { get; set; }

        [IgnoreDBWriter]
        public string MaxRollNo { get; set; }
        [IgnoreDBWriter]
        public string MinStudentCode { get; set; }

        [IgnoreDBWriter]
        public string MaxStudentCode { get; set; }

        [IgnoreDBWriter]
        public string NoofStudents { get; set; }
    }

    
}
