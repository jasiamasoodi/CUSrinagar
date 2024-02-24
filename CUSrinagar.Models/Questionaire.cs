using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class Questionaire
    {
        [Required]
        public Guid Question_Id { get; set; }
        public string Question { get; set; }
        [IgnoreDBWriter]
        [IgnoreDBRead]
        public List<QuestionOptions> QuestionOptions { get; set; }
        [IgnoreDBWriter]
        [IgnoreDBRead]
        [Required]
        public Semester Semester { get; set; }
        [IgnoreDBWriter]
        [Required]
        public Guid IsCheckedOption { get; set; }
        [IgnoreDBWriter]
        [IgnoreDBRead]
        public int TotalStudents { get; set; }
    }
}