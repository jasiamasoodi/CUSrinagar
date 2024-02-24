using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ADMCollegeMaster : BaseWorkFlow
    {
        public Guid College_ID { get; set; }
        public string CollegeCode { get; set; }

        [DisplayName("College Name")]
        [Required(ErrorMessage = " Required")]
        public string CollegeFullName { get; set; }
        public string SchoolFullName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string Country { get; set; }
        public string ContactNo1 { get; set; }
        public string ContactNo2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public bool Status { get; set; }
        public bool IsConstituentCollege { get; set; }
        public List<ADMCombinationMaster> Combinations { get; set; } = new List<ADMCombinationMaster>();
        public List<ADMCourseMaster> Courses { get; set; } = new List<ADMCourseMaster>();
    }

    public class CUSCollegeInfoDashboard
    {
        [IgnoreDBWriter]
        public string ClgCode { get; set; }
        [IgnoreDBWriter]
        public string CourseCategory { get; set; }
        [IgnoreDBWriter]
        public int Roll { get; set; }
    }
}
