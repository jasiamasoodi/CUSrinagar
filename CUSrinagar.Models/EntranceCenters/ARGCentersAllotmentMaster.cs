using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class ARGCentersAllotmentMaster : BaseWorkFlow
    {
        public Guid CenterAllowment_ID { get; set; }
        public Guid Entity_ID { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }
        public Guid Center_ID { get; set; }

        [DisplayName("Entrance Date (dd-mm-yyyy)")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string EnteredEntranceDate { get; set; }

        public DateTime EntranceDate { get; set; }
        

        [DisplayName("Entrance Time")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^\d{1,2}:\d{2}\s[AM|PM]{2}",ErrorMessage ="Invalid (AM / PM should be in Upper Case e,g: 12:00 PM)")]
        public string Time { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string DisplayCourseName { get; set; }

        public bool Appeared { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public Int64 EntranceRollNo { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public FormStatus EntranceFormStatus { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PrintProgramme Programme { get; set; }

    }
    public class CentersAllotmentMaster
    {
        public List<ARGCentersAllotmentMaster> CenterAllotmentMaster { get; set; }
    }
}
