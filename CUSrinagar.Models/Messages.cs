using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class Message
    {
        public Guid Message_ID { get; set; }
        public DateTime Date { get; set; }
        public bool GWC { get; set; }
        public bool SPC { get; set; }
        public bool AAA { get; set; }
        public bool IASE { get; set; }
        public bool ASC { get; set; }
        public bool HYD { get; set; }
        public bool EDG { get; set; }
        public bool ALB { get; set; }
        public string MessageTitle { get; set; }
        public string MessageBody { get; set; }
        public string Courses_ID { get; set; }
    }

}
