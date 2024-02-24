using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class BIL_PaperSetterSettings
    {
        public Guid Setting_ID { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public SetterFileType Type { get; set; }

        [MaxLength(100)]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "File is required")]
        [DisplayName("Attachment (Only doc or pdf)")]
        [ValidateFileForm(1, 4000, new string[] { ".doc", ".docx", ".DOC", ".DOCX",".pdf",".PDF" })]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase FileToUpload { get; set; }

        public string FilePath { get; set; }

        [IgnoreDBWriter]
        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }

        public List<BIL_PaperSetterSettings> papersettingList { get; set; }


    }

}
