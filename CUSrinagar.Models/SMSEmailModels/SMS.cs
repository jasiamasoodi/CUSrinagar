using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class SMS
    {
        [Required(ErrorMessage = " Required")]
        [DisplayName("SMS Regarding")]
        public SMSRegarding SMSRegarding { get; set; }

        [DisplayName("Center / College")]
        public Guid? College_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        public Programme Programme { get; set; }

        [Required(ErrorMessage = " Required")]
        [Range(2016, 9999, ErrorMessage = " Should be > 2015")]
        public short Batch { get; set; }

        [DisplayName("Course")]
        public List<Guid> Course_ID { get; set; }

        [DisplayName("RollNo's separated by , (comma)")]
        [RegularExpression(@"^(\d{6},)*\d{6}$", ErrorMessage = "Invalid (RollNo's should be 6 digits or remove comma OR NewLine at the end of list)")]
        public string RollNos { get; set; }

        [DisplayName("OtherPhoneNos separated by , (comma)")]
        [RegularExpression(@"^(\d{10},)*\d{10}$", ErrorMessage = "Invalid (phone no. should be 10 digits or remove comma OR NewLine at the end of list)")]
        public string OtherPhoneNo { get; set; }

        [DisplayName("Message (1-300 chars)")]
        [Required(ErrorMessage = " Required")]
        [StringLength(300, ErrorMessage = "Message should be 1-300 in length", MinimumLength = 1)]
        public string Message { get; set; }
    }
}
