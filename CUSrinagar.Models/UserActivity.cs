using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class UserActivity
    {
        public string CreatedByUser { get; set; }
        public DateTime? CreationDate { get; set; }
        public string UpdatedByUser { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
