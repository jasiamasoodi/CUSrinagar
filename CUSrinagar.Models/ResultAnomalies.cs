using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;

namespace CUSrinagar.Models
{
    public class ResultAnomalies
    {
        public Guid ResultAnomalies_Id { get; set; }
        public Guid Result_Id { get; set; }
        public Guid Student_ID { get; set; }
        public Guid Subject_Id { get; set; }
        public Guid? ResultNotification_Id { get; set; }
        public Guid ExamForm_Id { get; set; }
        public ResultAnomalyType AnomalyType { get; set; }
        public int TimeSpan { get; set; }
        public bool IsActive { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
        [IgnoreDBWriter]
        public PrintProgramme PrintProgramme { get; set; }
        [IgnoreDBWriter]
        public int Semester { get; set; }

    }
}
