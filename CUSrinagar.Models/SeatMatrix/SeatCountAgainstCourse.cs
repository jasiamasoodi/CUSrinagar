using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;

namespace CUSrinagar.Models
{
    public class SeatCountAgainstCourse
    {
        public Guid SeatCountAgainstCourse_ID { get; set; }
        public Guid Course_Id { get; set; }
        public Guid? College_Id { get; set; }
        
        public string Category { get; set; }
        public decimal NoofSeats { get; set; }
        public int AllocatedSeats { get; set; }
        public decimal ActualNoofSeats { get; set; }
        public Programme PrintProgramme { get; set; }
        [IgnoreDBWriter]
        public int Preference { get; set; }
        [IgnoreDBWriter]
        public int PreferenceNo { get; set; }
        [IgnoreDBWriter]
        public int Count { get; set; }
        [IgnoreDBWriter]
        public int AssignedSeats  { get; set; }

        [IgnoreDBWriter]
        public decimal SubjectEntrancePoints { get; set; }

        [IgnoreDBWriter]
        public int Percentage { get; set; }
       
    }
}
