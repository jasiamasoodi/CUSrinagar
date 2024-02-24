using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class QuestionOptions
    {

        public Guid Option_Id { get; set; }
        public Guid Question_Id { get; set; }
        public string OptionLabel { get; set; }
        [IgnoreDBWriter]
        public int StudentCount { get; set; }

    }
}
