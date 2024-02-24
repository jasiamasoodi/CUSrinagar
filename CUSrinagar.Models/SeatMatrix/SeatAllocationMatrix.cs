using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class SeatAllocationMatrix
    {
        public Guid SeatAllocationMatrix_Id { get; set; }
        public Guid Student_Id { get; set; }
        public Guid Course_IdAssigned { get; set; }
        public int PreferenceOfCourse { get; set; }
        public int PreferenceOfCollege { get; set; }
        public int Phase { get; set; }
        public decimal MeritObtained { get; set; }
        public bool AssignedUnderCategory { get; set; }
        public int SelectionListNo { get; set; }
        public bool ISFinallySelected { get; set; }
        public Guid? College_IdAssigned { get; set; }
    }
}
