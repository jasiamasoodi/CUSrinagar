using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CUSrinagar.Models
{
    public class ARGBoardRegistrationData
    {
        public string BoardRegistrationNo { get; set; }
        public string FullName { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string Gender { get; set; }

        public DateTime? DOB { get; set; }
        public string Session { get; set; }
        public int MaxMarks { get; set; }
        public int? MarksObt { get; set; }
        public int? YearOfPassing { get; set; }
        public string RollNo { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string DisplayDOBAs
        {
            get
            {
                if (DOB != null && DOB != DateTime.MinValue && DOB?.Year > 1920)
                {
                    return DOB?.ToString("dd-MM-yyyy");
                }
                return string.Empty;
            }
        }
    }


}
