using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CUSrinagar.Models
{
    public class Enrollment
    {
        public int Batch { get; set; }

        public int AAA { get; set; }
        public int ASC { get; set; }
        public int GWC { get; set; }
        public int SPC { get; set; }
        public int IASE { get; set; }

        public string CollegeFullName { get; set; }
        public int Count { get; set; }
    }
}
