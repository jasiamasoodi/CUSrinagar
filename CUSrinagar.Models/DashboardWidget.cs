using CUSrinagar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{

    public class CourseSummaryWidget
    {
        public string CourseFullName { get; set; }

        public int Male { get; set; }

        public int Female { get; set; }

        public int TotalStudents { get { return (Male + Female); } }
    }


    public class GrievanceWidget
    {
        public int Received { get; set; }

        public int Resolved { get; set; }

        public int Forwarded { get; set; }

        public int Pending { get { return ((Received + Forwarded) - Resolved) < 0 ? 0 : Math.Abs((Received + Forwarded) - Resolved); } }

        public GrievanceCategoryCount Summary { get; set; }
    }

    public class GrievanceCategoryCount
    {
        public int Admission { get; set; }

        public int Examination { get; set; }

        public int Result { get; set; }

        public int Syllabus { get; set; }

        public int TeachingClassWork { get; set; }

        public int Other { get; set; }
    }
}
