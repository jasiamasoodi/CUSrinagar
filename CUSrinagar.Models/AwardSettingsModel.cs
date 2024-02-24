using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AwardSettingsModel : BaseWorkFlow

    {

        public Guid AwardSettings_ID { get; set; }
        public int Year { get; set; }
        public int Batch { get; set; }
        public int Semester { get; set; }
        public Guid SUBJECT_ID { get; set; }
        public Guid USER_ID { get; set; }
        public bool ISFinalSubmit { get; set; }
        public bool ISFinalSubmitTheory { get; set; }
        public bool Status { get; set; }
        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }
        [IgnoreDBWriter]
        public string UserName { get; set; }
        [IgnoreDBWriter]
        public Guid Course_Id { get; set; }
        [IgnoreDBWriter]
        public Guid College_Id { get; set; }
    }


}
