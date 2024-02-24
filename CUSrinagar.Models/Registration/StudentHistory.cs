using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ARGStudentHistory : BaseWorkFlow
    {
        public Guid StudentHistoryID { get; set; }  
        public Guid StudentID { get; set; }  
        public Guid AcceptCollegeID { get; set; }   
        public Guid CourseID { get; set; }  
        public Guid CombinationID { get; set; }  

        public List<ARGSelectedCombination> Combination { get; set; } = new List<ARGSelectedCombination>();
    }

}
