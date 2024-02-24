using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class BillDeskRequest
    {
        /// <summary>
        /// Should be of 15-25 length
        /// </summary>
        public string CustomerID { get; set; }

        /// <summary>
        /// Should be of 1.0-999999999.99 length
        /// </summary>
        public int TotalFee { get; set; }

        /// <summary>
        /// Should be of 15-25 length
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Should be of 15-25 length
        /// </summary>
        public string PhoneNo { get; set; }
        /// <summary>
        /// AdditionalInfo3
        /// </summary>
        public PrintProgramme PrintProgramme { get; set; }
        /// <summary>
        /// AdditionalInfo4
        /// </summary>
        public Guid Entity_ID { get; set; }

        /// <summary>
        /// AdditionalInfo7 - Student_ID
        /// </summary>
        public Guid Student_ID { get; set; }

        /// <summary>
        /// AdditionalInfo5 - Student_ID
        /// </summary>
        public string Semester { get; set; } = "NA";
        public string ReturnURL { get; set; }
        public string AdditionalInfo { get; set; } = "NA";

        /// <summary>
        /// Only used for searching purpose in reconcile
        /// </summary>
        public string NonBillDeskField { get; set; }
    }
}
