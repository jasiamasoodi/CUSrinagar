using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
	public class MSSGPA
	{
		public Guid ID { get; set; }
    	public Guid Student_ID { get; set; }
		public short Semester { get; set; }
		public decimal SGPA { get; set; }
        public SGPAType SGPAType { get; set; }
        public string Formula { get; set; }
		public decimal Credits { get; set; }
		public decimal CreditPoints { get; set; }
	}
}
