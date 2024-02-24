using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
   public class AttemptCertificateDetails
    {
        public Guid Details_ID { get; set; }
        public Guid Certificate_ID { get; set; }
        [Required]
        public int Semester { get; set; }
        [Required]
        public int? ExpectedYearPassing { get; set; }
        [Required]
        public int? ActualYearPassing { get; set; }
        public int? NoOfReappearSubjects { get; set; }
        public string ReapperSubjects { get; set; }
        public int? NoOfAttemptsToClear { get; set; }
        public string Remarks { get; set; }
    }
}

