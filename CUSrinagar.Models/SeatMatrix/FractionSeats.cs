using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class FractionSeats
    {
        public Guid FractionSeat_ID { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
        public Guid Course_ID { get; set; }
        public int AssignedSeats { get; set; }
    }
}
