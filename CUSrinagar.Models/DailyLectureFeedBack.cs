using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CUSrinagar.Models
{
    public class DailyLectureFeedBack
    {
        public Guid DailyLectureFeedBack_ID { get; set; }
        public Guid Student_ID { get; set; }

        [DisplayName("Teacher Name (First Name Middle Name Last Name)")]
        [Required(ErrorMessage = " Required")]
        [MinLength(6, ErrorMessage = " Invalid")]
        [MaxLength(70, ErrorMessage = " Max 70 chars")]
        [RegularExpression(@"^([\s]*?[a-zA-Z]{1,}[ A-Za-z&-.]*)$", ErrorMessage = " Invalid")]
        public string FacultyName { get; set; }

        [DisplayName("Faculty Type")]
        [Required(ErrorMessage = " Required")]
        public FacultyTypes FacultyType { get; set; }

        [DisplayName("Mode of Teaching")]
        [Required(ErrorMessage = " Required")]
        public ModeOfTeachings ModeOfTeaching { get; set; }

        [DisplayName("Lecture Date & Time")]
        [Required(ErrorMessage = " Required")]
        public DateTime LectureDateTime { get; set; }

        [Required(ErrorMessage = " Required")]
        public Guid Subject_ID { get; set; }

        [DisplayName("Lecture Topic")]
        [Required(ErrorMessage = " Required")]
        [MinLength(6, ErrorMessage = " Invalid")]
        [MaxLength(140, ErrorMessage = "Max 140 chars")]
        public string LectureTopic { get; set; }

        [DisplayName("Faculty Arrives on Time")]
        [Required(ErrorMessage = " Required")]
        public bool FacultyArrivesOnTime { get; set; }

        [DisplayName("Faculty Leaves on Time")]
        [Required(ErrorMessage = " Required")]
        public bool FacultyLeavessOnTime { get; set; }

        [DisplayName("Faculty Behaviour")]
        [Required(ErrorMessage = " Required")]
        public Rating FacultyBehaviour { get; set; }

        [DisplayName("Rate the Lecture")]
        [Required(ErrorMessage = " Required")]
        public Rating RateTheLecture { get; set; }

        [DisplayName("Material Provided")]
        [Required(ErrorMessage = " Required")]
        public MaterialType MaterialProvided { get; set; }

        [DisplayName("Any Remarks (200 chars max)")]
        [MaxLength(200, ErrorMessage = "Max 200 chars")]
        public string AnyRemarks { get; set; }

        public DateTime CreatedOn { get; set; }



        [IgnoreDBWriter]
        public short Semester { get; set; }

        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        [IgnoreDBWriter]
        public string FullName { get; set; }

        [IgnoreDBWriter]
        public string CollegeFullName { get; set; }

        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }

        [IgnoreDBWriter]
        public string CUSRegistrationNo { get; set; }

        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }

        [IgnoreDBWriter]
        public Guid Course_ID { get; set; }

        [IgnoreDBWriter]
        public Guid College_ID { get; set; }

        [IgnoreDBWriter]
        [IgnoreDBRead]
        [Required(ErrorMessage = " Required")]
        [RegularExpression("^([0-9]*){12}$",ErrorMessage ="ABC ID should be 12 digit number")]
        [MinLength(12,ErrorMessage ="ABC ID should be 12 digit number")]
        [MaxLength(12,ErrorMessage ="ABC ID should be 12 digit number")]
        [Remote("ValidateABCID", "Feedback",areaReference:AreaReference.UseCurrent, ErrorMessage = "ABC ID already used by another candidate")]
        public string ABCID { get; set; }
    }
}
