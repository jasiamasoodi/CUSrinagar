
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

namespace CUSrinagar.Models
{
    public class AwardFilterSettings
    {
        public Guid AwardFilterSettingsID { get; set; }
        [StringLength(10)]
        public string AwardType { get; set; }
        [StringLength(30)]
        public string FilterColumn { get; set; }
        public string Courses { get; set; }
        public string Colleges { get; set; }
        public int FilterValue { get; set; }
        public Programme Programme { get; set; }
        public Semester Semester { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        [IgnoreDBWriter]
        public string CoursesName { get; set; }
        [IgnoreDBWriter]
        public string CollegesName { get; set; }
        [IgnoreDBWriter]
        public List<Guid> CourseList { get; set; }
        [IgnoreDBWriter]
        public List<Guid> CollegeList { get; set; }
        [IgnoreDBWriter]
        public string RecieverMail { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
