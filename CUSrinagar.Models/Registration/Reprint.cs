using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CUSrinagar.Extensions;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class ARGReprint
    {
        [DisplayName("CUS Form No. Or CUS Regn. No. Or Board Regn No.")]
        [Required(ErrorMessage = " Required")]
        [MinLength(4, ErrorMessage = " Invalid")]
        [MaxLength(150, ErrorMessage = " Max 150 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string FormNo { get; set; }

        [DisplayName("Date of birth (dd-mm-yyyy)")]
        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$", ErrorMessage = " Invalid (remove spaces or special chars)")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string EnteredDOB { get; set; }

        public DateTime DOB
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EnteredDOB))
                {
                    string[] splitDate = EnteredDOB.SplitDate();

                    if (splitDate != null && splitDate.Count() == 3)
                    {
                        try
                        {
                            return new DateTime(Convert.ToInt32(splitDate[2]), Convert.ToInt32(splitDate[1]), Convert.ToInt32(splitDate[0]));
                        }
                        catch (ArgumentOutOfRangeException) { }
                        catch (FormatException) { }
                    }
                }
                return DateTime.MinValue;
            }
        }

        [DisplayName("Applied For ")]
        [Required(ErrorMessage = " Required")]
        public PrintProgramme PrintProgrammeOption { get; set; }


        [Required(ErrorMessage = " Required")]
        [RegularExpression(@"\d{4,4}", ErrorMessage = "Invalid")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public short Batch { get; set; }

        [IgnoreDBWriter]
        public DateTime? AllowFormsToBeEditFromDate { get; set; }
    }
    public class ProceedOptions
    {
        [DisplayName("Apply For ")]
        [Required(ErrorMessage = " Required")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public PrintProgramme ProceedProgrammeOption { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public IEnumerable<SelectListItem> SelectListItemOptions { get; set; }
    }
}
