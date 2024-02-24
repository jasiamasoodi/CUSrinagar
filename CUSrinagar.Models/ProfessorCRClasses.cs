using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ProfessorCRClasses : BaseWorkFlow
    {
        public Guid ProfessorCRClasses_ID { get; set; }
        [Required(ErrorMessage = " Required")]
        public int Batch { get; set; }
        [Required(ErrorMessage = " Required")]
        public Semester Semester { get; set; }
        [Required(ErrorMessage = " Required")]
        public Guid Subject_ID { get; set; }
        [Required(ErrorMessage = " Required")]
        public Guid Professor_ID { get; set; }
        [Required(ErrorMessage = " Required")]
        public Guid CR_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [IgnoreDBWriter]
        public Programme Programme { get; set; }
        [Required(ErrorMessage = " Required")]
        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }
        [Required(ErrorMessage = " Required")]
        [IgnoreDBWriter]
        public Guid College_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }
        [IgnoreDBWriter]
        public string Professor { get; set; }
        [IgnoreDBWriter]
        public string CR { get; set; }
        [IgnoreDBWriter]
        public int ClassCount { get; set; }
        [IgnoreDBWriter]
        public int ObjectionCount { get; set; }
        [IgnoreDBWriter]
        public int ClassesEngaged { get; set; }
        public List<ProfessorCRClassDetails> ProfessorCRClassDetails { get; set; }
    }
    public class ProfessorCRClassDetails : BaseWorkFlow
    {
        public Guid ProfessorCRClassDetails_ID { get; set; }
        public Guid ProfessorCRClasses_ID { get; set; }
        public ProfessorStatus professorStatus { get; set; }
        public string ProfessorRemarks { get; set; }
        public DateTime? ClassDatenTimeByProfessor { get; set; }
        public int DurationByProfessor { get; set; }
        public DateTime? ProfessorResponseOn { get; set; }

        public CRStatus crStatus { get; set; }
        public string CRRemarks { get; set; }
        public DateTime? ClassDatenTimeByCR { get; set; }
        public int DurationByCR { get; set; }
        public DateTime? CRResponseOn { get; set; }


    }
}
