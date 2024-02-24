using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CUSrinagar.Models
{
    public class ADMCourseMaster : BaseWorkFlow
    {
        [DisplayName("Course ")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [DisplayName("Status ")]
        public bool? Status { get; set; }

        [DisplayName("Combination Code ")]
        [RegularExpression(@"^([a-zA-Z0-9]*){1,5}$", ErrorMessage = " Invalid (Remove Space and Special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string SearchCombinationCode { get; set; }


        public Programme Programme { get; set; }
        public PrintProgramme PrintProgramme { get; set; }

        public string CourseCode { get; set; }

        public string CourseFullName { get; set; }

        public short Duration { get; set; }
        public Scheme Scheme { get; set; }



        public CourseCategory CourseCategory { get; set; }
        public short MinCombinationSubjects { get; set; }
        public short MaxCombinationSubjects { get; set; }
        public bool RegistrationOpen { get; set; }
        public bool HasCombination { get; set; }
        public int StartingYear { get; set; }
        public int? EndingYear { get; set; }

        public string SchoolCode { get; set; }
        public string SchoolFullName { get; set; }
        public short CourseNumber { get; set; }
        public bool AllowExaminationForm { get; set; }
        public List<ADMCombinationMaster> CombinationDetails { get; set; } = new List<ADMCombinationMaster>();
        public List<ADMSubjectMaster> Subjects { get; set; } = new List<ADMSubjectMaster>();
        public List<ADMCollegeCourseMapping> CourseMappingList { get; set; } = new List<ADMCollegeCourseMapping>();
        public bool AllowNewSelectionList { get; set; }
        public int CurrentSelectionListNo { get; set; }
        public string GroupName { get; set; }

        [DisplayName("College")]
        [IgnoreDBWriter]
        public Guid College_ID { get; set; }

        [IgnoreDBWriter]
        public int MeritBasislInTackCapacity { get; set; }

        public int TotalCredits { get; set; }
        public bool HasLateralEntry { get; set; }
        public short TotalCreditsForLateralEntry { get; set; }
        public short LateralEntryStartingSemester { get; set; }
    }
    public class ADMCollegeCourseMapping
    {
        public Guid College_ID { get; set; }
        public Guid Course_ID { get; set; }
        public bool Status { get; set; }
        public bool AllowBOPEERegistration { get; set; }
        public int MeritBasislInTackCapacity { get; set; }
        public int SelfFinancedInTackCapacity { get; set; }
        public string AllowedGender { get; set; }
    }

    public class CourseList
    {
        public Guid Course_ID { get; set; }
        public string CollegeFullName { get; set; }
        public string CourseFullName { get; set; }
        public int Duration { get; set; }
        public int MeritBasislInTackCapacity { get; set; }
        public Programme Programme { get; set; }
    }
}
