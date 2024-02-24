using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;

namespace CUSrinagar.Models
{
    public class AppUserProfessorSubjects : PorfessorSubjectBaseObject
    {
        public Guid ProfessorSubject_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        public short Semester { get; set; }
        public Guid User_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        public Guid Subject_ID { get; set; }
        public bool Status { get; set; }

        [IgnoreDBWriter]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        public string RollNoFrom { get; set; }
        public string RollNoTo { get; set; }
        [IgnoreDBWriter]
        public Programme Programme { get; set; }
    }
    public class PorfessorSubjectBaseObject : BaseWorkFlow
    {
        [IgnoreDBWriter]
        public string MinRollNo { get; set; }

        [IgnoreDBWriter]
        public string MaxRollNo { get; set; }
        [IgnoreDBWriter]
        public string NoofStudents { get; set; }
    }
}
