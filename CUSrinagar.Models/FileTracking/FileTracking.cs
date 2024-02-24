using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CUSrinagar.Models
{
    public class FTFileTracking
    {
        public Guid File_ID { get; set; }

        [Required(ErrorMessage = " Required")]
        [DisplayName("File No")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(80, ErrorMessage = " Max 80 chars")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = " Invalid(remove spaces)")]
        public string FileNo { get; set; }

        [Required(ErrorMessage = " Required")]
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(300, ErrorMessage = " Max 300 chars")]
        public string Subject { get; set; }

        [MinLength(2, ErrorMessage = " Invalid")]
        [DisplayName("Description (if any)")]
        [MaxLength(500, ErrorMessage = " Max 500 chars")]
        public string Description { get; set; }

        public string Section { get; set; }

        public Guid CurrentlyWithUser_ID { get; set; }

        [IgnoreDBWriter]
        public string UserID { get; set; }

        [IgnoreDBWriter]
        public FTFileStatus CurrentFileStatus { get; set; }

        [IgnoreDBWriter]
        public DateTime CreatedOn { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public FTFileTrackingHistory FileTrackingHistory { get; set; }
        public List<FTFileTrackingHistory> FileTrackingHistoryDetails { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public FTFilters Filters { get; set; }
    }
    public class FTFilters
    {
        [MinLength(2, ErrorMessage = " Invalid")]
        [MaxLength(500, ErrorMessage = " Max 500 chars")]
        [DisplayName("Search Query")]
        public string SearchQuery { get; set; }
        public DateTime? From { get; set; } = DateTime.Now.Date.AddMonths(-3);
        public DateTime? To { get; set; } = DateTime.Now.Date;
    }

    public class FTUserSectionMapping
    {

        public Guid User_ID { get; set; }

        public Guid Department_ID { get; set; }

    }


}
