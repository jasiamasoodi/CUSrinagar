using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class NEPPersonalInfo
    {
        public ARGPersonalInformation PersonalInformation { get; set; }
        public List<NEPCollegePreference> CollegePreference { get; set; }
        public List<Certificate> Documents { get; set; }
    }

    public class NEPCollegePreferences
    {
        public List<NEPCollegePreference> CollegePreference { get; set; }
    }

    public class NEPCollegePreference
    {
        public Guid Student_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("College")]
        public Guid College_ID { get; set; }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Preference No.")]
        public short PreferenceNo { get; set; }
    }

    public class NEPDocuments
    {
        [DisplayName("Photograph")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(20, 70, new string[] { ".jpg", ".bmp", ".png", ".jpeg" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase PhotographPath { get; set; }

        public string PhotographUrl { get; set; }

        public Guid Student_ID { get; set; }

        public List<Certificate> Documents { get; set; }
    }
}
