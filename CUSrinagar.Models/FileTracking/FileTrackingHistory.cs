using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class FTFileTrackingHistory
    {
        public Guid Tracking_ID { get; set; }

        [DisplayName("File No")]
        [Required(ErrorMessage = " Required")]
        public Guid File_ID { get; set; }

        public FTFileStatus FileStatus { get; set; }
        public DateTime Dated { get; set; }

        [DisplayName("Remarks (If any)")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(500, ErrorMessage = " Max 500 chars")]
        public string Remarks { get; set; }

        public string Section { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("Forward to")]
        public Guid User_ID { get; set; }

        [IgnoreDBWriter]
        public string UserID { get; set; }
    }
}
