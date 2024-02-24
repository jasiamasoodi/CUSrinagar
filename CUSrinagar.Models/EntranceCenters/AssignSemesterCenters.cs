using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class AssignSemesterCenters
    {
        [DisplayName("Students of College")]
        [Required(ErrorMessage = " Required")]
        public Guid StudentesOfCollege_ID { get; set; }

        [DisplayName("Exam Forms")]
        [Required(ErrorMessage = " Required")]
        public Guid ARGExamForm_ID { get; set; }

        [DisplayName("Course")]
        [Required(ErrorMessage = " Required")]
        public Guid Course_ID { get; set; }

        [DisplayName("Center Location")]
        [Required(ErrorMessage = " Required")]
        public Guid CenterLocationCollege_ID { get; set; }

        [DisplayName("No. Of Students To Be Assigned Per Center")]
        [Required(ErrorMessage = " Required")]
        [Range(1, 999, ErrorMessage = "Should be 1-999")]
        public short NoOfStudentsToBeAssignedPerCenter { get; set; }

        [DisplayName("Exam Category")]
        [Required(ErrorMessage = " Required")]
        public SemesterCentersCategory SemesterCenterCategory { get; set; }

        public int TotalExamFormsAvailable { get; set; }

        public List<AssignCentersAs> AssignCentersAs { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public RelocateCenter RelocateCenters { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public ArchiveCenter ArchiveCenters { get; set; }
    }

    public class AssignCentersAs
    {
        public bool HasToBeAssigned { get; set; }

        public Guid Center_ID { get; set; }

        public string CenterCode { get; set; }

        [DisplayName("Assignment Order")]
        [Range(1, 999, ErrorMessage = "Should be 1-999")]
        public short? AssignmentOrder { get; set; }

        [DisplayName("Has Already Some Students")]
        public bool HasAlreadySomeStudents { get; set; }
    }

    public class AssignCentersInBulk
    {
        public Guid User_ID { get; set; }

        public Guid Center_ID { get; set; }
        public Guid Course_ID { get; set; }
        public Guid StudentesOfCollege_ID { get; set; }
        public short Semester { get; set; }
        public short Year { get; set; }
        public DateTime ExaminationDate { get; set; }
        public string ExaminationTime { get; set; }
        public PrintProgramme PrintProgramme { get; set; }
        public short FetchNextCount { get; set; }
        public short IndexCount { get; set; }
        public int OFFSETCount
        {
            get
            {
                if (IndexCount > 0)
                {
                    return (IndexCount * FetchNextCount);
                }
                return 0;
            }
        }
        public int OFFSETCountForEntrance { get; set; }
        public SemesterCentersCategory SemesterCenterCategory { get; set; }
        public List<string> Districts { get; set; }
    }
}
