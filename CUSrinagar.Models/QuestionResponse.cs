using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class QuestionResponse
    {
        public Guid Response_Id { get; set; }
        public Guid Student_Id { get; set; }
        public int Semester { get; set; }
        public int Year { get; set; }
        public Guid Option_Id { get; set; }
        public Guid Question_Id { get; set; }
    }
}
