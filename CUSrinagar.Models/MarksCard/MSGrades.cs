using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class MSGrades
    {
        public Guid Grade_ID { get; set; }
        public string LetterGrade { get; set; }
        public int GradePoint { get; set; }
        public int FourtyPassPercentageFrom { get; set; }
        public int FourtyPassPercentageTo { get; set; }
        public int GtrFourtyPassPercentageFrom { get; set; }

        public int GtrFourtyPassPercentageTo { get; set; }
    }
    }