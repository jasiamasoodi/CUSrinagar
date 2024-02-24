using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;
using CUSrinagar.Models;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers
{
    public class CertificateCoursesDB
    {
        public bool ValidateBoardRegnNoExists(string BoardRegnNo, short Batch)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT COUNT(BoardRegnNo) FROM CertificateCoursePersonalInfo
                                              WHERE BoardRegnNo=@BoardRegnNo AND Batch=@Batch");
            cmd.Parameters.AddWithValue("@BoardRegnNo", BoardRegnNo);
            cmd.Parameters.AddWithValue("@Batch", Batch);
            return new MSSQLFactory().ExecuteScalar<int>(cmd) > 0;
        }
        public int Save(CertificateCoursePersonalInfo certificateCoursePersonalInfo)
        {
            return new MSSQLFactory().InsertRecord(certificateCoursePersonalInfo);
        }
        public int Save(CertificateCoursePrevQualifications certificateCoursePrevQualifications)
        {
            return new MSSQLFactory().InsertRecord(certificateCoursePrevQualifications);
        }


        public CertificateCoursePersonalInfo GetItem(Guid Student_ID)
        {
            SqlCommand cmd = new SqlCommand($"SELECT * FROM CertificateCoursePersonalInfo WHERE Student_ID='{Student_ID}'");
            return new MSSQLFactory().GetObject<CertificateCoursePersonalInfo>(cmd);
        }
        public CertificateCoursePersonalInfo GetItem(string BoardRegnNo, short Batch)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT Top 1 * FROM CertificateCoursePersonalInfo
                                              WHERE BoardRegnNo=@BoardRegnNo AND Batch=@Batch ORDER BY Batch DESC");
            cmd.Parameters.AddWithValue("@BoardRegnNo", BoardRegnNo);
            cmd.Parameters.AddWithValue("@Batch", Batch);
            return new MSSQLFactory().GetObject<CertificateCoursePersonalInfo>(cmd);
        }

        public List<CertificateCoursePrevQualifications> GetCertificateCoursePrevQualifications(Guid Student_ID)
        {
            SqlCommand cmd = new SqlCommand($"SELECT * FROM CertificateCoursePrevQualifications WHERE Student_ID='{Student_ID}' ORDER BY YearOfPassing ASC");
            return new MSSQLFactory().GetObjectList<CertificateCoursePrevQualifications>(cmd);
        }

        public int UpdateFormStatus(Guid Student_ID)
        {
            SqlCommand cmd = new SqlCommand($"UPDATE CertificateCoursePersonalInfo SET FormStatus={(int)FormStatus.FeePaid} WHERE Student_ID='{Student_ID}'");
            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

    }
}
