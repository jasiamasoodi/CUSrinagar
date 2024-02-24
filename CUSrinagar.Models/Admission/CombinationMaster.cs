using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class ADMCombinationMaster : BaseWorkFlow
    {
        public Guid Combination_ID { get; set; }
        public Guid College_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Choose Course ")]
        public Guid? Course_ID { get; set; }
        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Combination Code ")]
        [RegularExpression(@"^([a-zA-Z0-9]-*){1,10}$", ErrorMessage = " Invalid (Remove Space and Special chars)")]
        [Remote("CombinationCodeExists","Combination",ErrorMessage ="Combination Code already exists", AdditionalFields="College_ID,Combination_ID" )]
        public string CombinationCode { get; set; }

        public string CombinationSubjects { get; set; }
        public bool Status { get; set; }
        public short? Semester { get; set; }

        [IgnoreDBRead][IgnoreDBWriter]
        public CombinationHelper CombinationHelper { get; set; }

        [IgnoreDBRead][IgnoreDBWriter]
        public List<ADMSubjectMaster> SubjectsDetails { get; set; } = new List<ADMSubjectMaster>();
    }
    public class CombinationHelper
    {
        public short MinSubjectsAllowed { get; set; }
        public short MaxSubjectsAllowed { get; set; }
        public SelectList SubjectSelectListItems { get; set; }
        public List<BaseCombinationHelper> FinalCombinations { get; set; }
    }

    public class BaseCombinationHelper
    {
        public bool IsCompulsary { get; set; }

        [Required(ErrorMessage =" Required")]
        [DisplayName("Subject ")]
        public Guid? Subject_ID { get; set; }
        public string SubjectsName { get; set; }
    }    
}
