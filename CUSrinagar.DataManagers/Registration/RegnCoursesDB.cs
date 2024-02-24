using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class RegnCoursesDB
    {
        public bool IsRegistrationOpen(PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<int>($@"SELECT COUNT(Course_ID) FROM ADMCourseMaster WHERE PrintProgramme={(short)printProgramme} AND RegistrationOpen=1") > 0;
        }

        public int UpdateCertificates(UpdateCertificates updateCertificates)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE dbo.Certificate SET CertificateUrl='{updateCertificates.OldMarksCardPath}' WHERE Certificate_ID='{updateCertificates.Certificate_ID}';");
        }

        public int UpdateCourseCGPA(ARGCoursesApplied item, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE dbo.ARGCoursesApplied_{printProgramme} SET SubjectCGPA={item.SubjectCGPA} WHERE Student_ID='{item.Student_ID}' AND Course_ID='{item.Course_ID}';");
        }
    }
}
