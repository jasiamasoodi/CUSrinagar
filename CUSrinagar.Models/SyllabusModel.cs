using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace CUSrinagar.Models
{
    //public class Syllabus
    //{
    //    public Programme Programme { get; set; }
    //    public Guid Course_ID { get; set; }
    //    public string CourseFullName { get; set; }
    //    public string SubjectFullName { get; set; }
    //    public string Session { get; set; }
    //    public short Semester { get; set; }
    //    public bool Status { get; set; }
    //    public SubjectType SubjectType { get; set; }
    //    public string DepartmentFullName { get; set; }
    //    public string SyllabusFileName { get; set; }
    //    public short? FromBatch { get; set; }
    //    public virtual string BaseUrl
    //    {
    //        get { return "/FolderManager/Syllabus/" + SyllabusFileName; }
    //    }
    //}

    public class Syllabus : BaseWorkFlow
    {
        public Guid Syllabus_ID { get; set; }

        [IgnoreDBWriter]
        public Programme Programme { get; set; }

        //[Required(ErrorMessage = "Course required")]
        // [Display(Name = "Course")]
        public Guid? Course_ID { get; set; }

        [IgnoreDBWriter]
        public Guid Department_ID { get; set; }
        public string SyllabusFileName { get; set; }

        public Guid? Subject_ID { get; set; }

        [Required(ErrorMessage = "Session Year required")]
        [Display(Name = "Session Year")]
        public string Session { get; set; }

        public short? Semester { get; set; }

        public bool Status { get; set; }

        [Required(ErrorMessage = "Syllabus required")]
        [Display(Name = "Syllabus")]
        [IgnoreDBRead]
        [IgnoreDBWriter]
        public HttpPostedFileBase Files { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public virtual string SyllabusUrl
        {
            get
            {
                return "/FolderManager/Syllabus/" + SyllabusFileName;
            }
        }

        [IgnoreDBWriter]
        public string SubjectFullName { get; set; }

        [IgnoreDBWriter]
        public string DepartmentFullName { get; set; }

        [IgnoreDBWriter]
        public string CourseFullName { get; set; }

        [IgnoreDBWriter]
        public SubjectType SubjectType { get; set; }

        public string Remark { get; set; }

        [IgnoreDBWriter]
        public string SubjectCode { get; set; }

        [IgnoreDBWriter]
        public bool IsNEP { get; set; }


    }




}