using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ARGCollegesCoursesPreferences : BaseWorkFlow
    {
        public ARGCollegesCoursesPreferences()
        { }
        public ARGCollegesCoursesPreferences(int prefNo)
        {
            PreferenceNo = prefNo;
        }

        public Guid Student_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Select College Name")]

        public Guid? College_ID { get; set; }

        public List<SelectedCombinations> combination_IDs { get; set; } = new List<SelectedCombinations>();

        [Required(ErrorMessage = " Required")]
        [DisplayName("Select Course Name")]
        public Guid? Course_ID { get; set; }

        public int PreferenceNo { get; }
    }
    public class ARGCollegesCoursesPreferencesNonRequired : BaseWorkFlow
    {
        public ARGCollegesCoursesPreferencesNonRequired()
        {
        }
        public ARGCollegesCoursesPreferencesNonRequired(int prefNo)
        {
            PreferenceNo = prefNo;
        }
        public List<SelectedCombinationsNonRequired> combination_IDs { get; set; } = new List<SelectedCombinationsNonRequired>();
        public Guid Student_ID { get; set; }

        [DisplayName("Select College Name")]
        public Guid? College_ID { get; set; }
        public Guid? CombinationSelected_ID { get; set; }

        [DisplayName("Select Course Name")]
        public Guid? Course_ID { get; set; }
        public int PreferenceNo { get; }
    }


    public class SelectedCombinations
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("Combination Name")]
        public Guid CombinationSelected_ID { get; set; }
    }
    public class SelectedCombinationsNonRequired
    {
        [DisplayName("Combination Name")]
        public Guid? CombinationSelected_ID { get; set; }
    }

}
