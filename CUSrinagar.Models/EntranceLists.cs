using CUSrinagar.Enums;
using CUSrinagar.ValidationAttrs;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CUSrinagar.Models
{
    public class ADMEntranceLists : BaseWorkFlow
    {
        public Guid EntranceList_ID { get; set; }

        [DisplayName("Entrance List Type")]
        [Required(ErrorMessage = " Required")]
        public EntranceListType ListType { get; set; }


        [Required(ErrorMessage = " Required")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public Programme Programme { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        public string ListURL { get; set; }

        [DisplayName("File (PDF)")]
        [Required(ErrorMessage = " Required")]
        [ValidateFileForm(1, 100000, new string[] { ".PDF", ".pdf" }, false)]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase File { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public static string DirectoryPath { get; } = "~/FolderManager/SelectionLists";
    }

}
