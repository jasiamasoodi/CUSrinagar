using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CUSrinagar.Extensions;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class PrintExamAdmitCards
    {
        [DisplayName("Semester ")]
        [Required(ErrorMessage = " Required")]
        public short Semester { get; set; }
    }
}
