using CUSrinagar.Enums;
using CUSrinagar.Extensions;
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
    public class ApplicationFormsDB
    {
        public CertificateVerification GetByStudentID(Guid student_ID)
        {
            return new MSSQLFactory().GetObject<CertificateVerification>($"SELECT TOP 1 * FROM CertificateVerification WHERE Student_ID='{student_ID}' ORDER BY AppliedOn DESC");
        }

        public int Save(CertificateVerification certificateVerification)
        {
            return new MSSQLFactory().InsertRecord(certificateVerification);
        }

        public int DeletePreviousCV(Guid student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM dbo.CertificateVerification WHERE Student_ID='{student_ID}' AND FeePaid=0");
        }

        public int UpdateFeePaidInCV(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE dbo.CertificateVerification SET FeePaid=1 WHERE Student_ID='{Student_ID}' AND FeePaid=0");
        }

        public IssuingOfDegreeCertificate GetForIssuingOfDegreeCertificate(Guid student_ID, PrintProgramme printProgramme)
        {
            IssuingOfDegreeCertificate issuingOfDegreeCertificate = new MSSQLFactory().GetObject<IssuingOfDegreeCertificate>($@"SELECT DISTINCT p.Photograph,p.CUSRegistrationNo,p.FullName,p.FathersName,
                                                    p.DOB,c.CourseFullName,cm.CollegeFullName,
                                                    r.CGPA,r.DateofDeclaration,
                                                    p.Student_ID,i.Specializations,i.PerusingHigherEduInAndProgramme,
                                                    i.EmploymentStatus,i.PaymentStatus,i.AppliedOn,r.TCourse_ID AS Course_ID
                                                    FROM dbo.ARGPersonalInformation_{printProgramme} p
                                                        JOIN dbo.MSCGPA r
                                                            ON r.Student_ID = p.Student_ID	AND p.IsPassout=1 AND r.SGPAType={(short)SGPAType.Type1}	
		                                                    JOIN dbo.ADMCollegeMaster cm ON p.AcceptCollege_ID=cm.College_ID
		                                                    JOIN dbo.ARGStudentExamForm_{printProgramme} ex ON ex.Student_ID = p.Student_ID AND p.IsPassout=1
                                                        JOIN dbo.ADMCourseMaster c ON 
                                                            c.Course_ID = r.TCourse_ID
                                                        LEFT JOIN dbo.IssuingOfDegreeCertificate i
                                                            ON i.Student_ID = p.Student_ID
		                                                    WHERE p.Student_ID='{student_ID}'");

            if (issuingOfDegreeCertificate == null && printProgramme == PrintProgramme.IH)
            {
                issuingOfDegreeCertificate = new MSSQLFactory()
                    .GetObject<IssuingOfDegreeCertificate>($@"
                                    SELECT DISTINCT
                                           p.Photograph,
                                           p.CUSRegistrationNo,
                                           p.FullName,
                                           p.FathersName,
                                           p.DOB,
                                           c.CourseFullName,
                                           cm.CollegeFullName,
                                           0 CGPA,       
                                           p.Student_ID,
                                           i.Specializations,
                                           i.PerusingHigherEduInAndProgramme,
                                           i.EmploymentStatus,
                                           i.PaymentStatus,
                                           i.AppliedOn,
                                           c.Course_ID
                                    FROM dbo.ARGPersonalInformation_{printProgramme} p
                                            JOIN dbo.ADMCollegeMaster cm
                                            ON p.AcceptCollege_ID = cm.College_ID
                                        JOIN dbo.ARGStudentExamForm_{printProgramme} ex
                                            ON ex.Student_ID = p.Student_ID
                                               AND p.IsPassout = 1
                                        JOIN dbo.VWStudentCourse c
                                            ON c.Student_ID = p.Student_ID
                                        LEFT JOIN dbo.IssuingOfDegreeCertificate i
                                            ON i.Student_ID = p.Student_ID
                                    WHERE p.Student_ID = '{student_ID}'
                                     AND p.AcceptCollege_ID='9D03A374-4398-4A48-BE2A-FD9911EC6F82';");
            }
            return issuingOfDegreeCertificate;
        }

        public List<SelectListItem> GetQualifyingExamNames(Guid course_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObjectList<SelectListItem>
                ($@"SELECT DISTINCT q.ExamName [Text],q.ExamName [Value] FROM dbo.ARGStudentPreviousQualifications_{printProgramme} q
                    JOIN dbo.VWStudentCourse c ON c.Student_ID = q.Student_ID
                    WHERE c.Course_ID='{course_ID}'");
        }

        public string GetCUSExamRollNo(Guid student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteScalar<string>($@"SELECT TOP 1 ExamRollNumber FROM dbo.ARGStudentExamForm_{printProgramme} WHERE Student_ID='{student_ID}' AND ExamRollNumber IS NOT NULL ORDER BY Semester Desc;");
        }

        public int UpdateIssuingDegreeForm(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE dbo.IssuingOfDegreeCertificate SET PaymentStatus={(short)FormStatus.FeePaid} WHERE Student_ID='{Student_ID}'");
        }

        public ARGStudentPreviousQualifications GetStudentPreviousQualification(Guid student_ID, string examName, PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = $@"SELECT TOP 1 * FROM dbo.ARGStudentPreviousQualifications_{printProgramme} q
                    WHERE q.Student_ID=@student_ID AND q.ExamName=@ExamName";
            cmd.Parameters.AddWithValue("@student_ID", student_ID);
            cmd.Parameters.AddWithValue("@ExamName", examName);

            return new MSSQLFactory().GetObject<ARGStudentPreviousQualifications>(cmd);
        }

        public int UpdateContactDetails(Guid address_ID, PrintProgramme printProgramme, string mobile, string email, Guid student_ID)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = $@"UPDATE dbo.ARGStudentAddress_{printProgramme} SET Mobile=@m ,Email=@e,UpdatedOn=GETDATE(),UpdatedBy=@u WHERE Address_ID=@id";
            cmd.Parameters.AddWithValue("@u", student_ID);
            cmd.Parameters.AddWithValue("@id", address_ID);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@m", mobile);

            return new MSSQLFactory().ExecuteNonQuery(cmd);
        }

        public int Save(IssuingOfDegreeCertificate issuingOfDegreeCertificate)
        {
            try
            {
                new MSSQLFactory().ExecuteNonQuery($@"DELETE FROM IssuingOfDegreeCertificate 
                                                WHERE Student_ID='{issuingOfDegreeCertificate.Student_ID}'
                                                AND PaymentStatus<>{(short)FormStatus.FeePaid}");
                return new MSSQLFactory().InsertRecord(issuingOfDegreeCertificate);
            }
            catch (SqlException ex) when (ex.Number == 2627)//primary key violation
            {
                return 1;
            }
        }

        public bool DegreeAlreadyHandedOverOn(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>
                                        ($@"SELECT COUNT(DegreeCertificate_ID) FROM dbo.DegreeCertificate
                                          WHERE Student_ID='{Student_ID}' AND HandedOverOn IS NOT NULL") > 0;
        }

        public bool DegreeAlreadyPrinted(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteScalar<int>
                                        ($@"SELECT COUNT(DegreeCertificate_ID) FROM dbo.DegreeCertificate
                                          WHERE Student_ID='{Student_ID}' AND PrintedOn IS NOT NULL") > 0;
        }

        public CorrectionForm GetCorrectionForm(Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().GetObject<CorrectionForm>
                                        ($@"SELECT p.Student_ID,
                                               p.Photograph,
                                               ISNULL(p.CUSRegistrationNo, p.StudentFormNo) CUSRegistrationNo,
                                               {(short)printProgramme} PrintProgramme,
                                               p.FullName PresentFullName,
                                               p.FathersName PresentFathersName,
                                               p.DOB PresentDOB,
                                               c.NewFullName,
                                               c.NewFathersName,c.ReasonAndDocumentaryProof,
                                               ISNULL(c.NewDOB, '0001-01-01') NewDOB,
                                               ISNULL(c.PaymentStatus, 1) PaymentStatus,
                                               c.AppliedOn,
                                               CASE
                                                   WHEN d.PrintedOn IS NULL THEN
                                                       0
                                                   ELSE
                                                       1
                                               END HasDegreeCertificatePrinted,
                                               CASE
                                                   WHEN (m.PrintedOn IS NULL AND MS.PrintedOn IS NULL) THEN
                                                       0
                                                   ELSE
                                                       1
                                               END HasTranscriptPrinted
                                        FROM dbo.ARGPersonalInformation_{printProgramme} p
                                            LEFT JOIN dbo.CorrectionForm c
                                                ON c.Student_ID = p.Student_ID
                                            LEFT JOIN dbo.DegreeCertificate d
                                                ON d.Student_ID = p.Student_ID
                                            LEFT JOIN dbo.MSStatus MS ON MS.Student_ID = p.Student_ID
                                            LEFT JOIN dbo.MSCGPA m
                                                ON m.Student_ID = p.Student_ID
                                        WHERE p.Student_ID = '{Student_ID}' AND p.FormStatus=9;");
        }

        public int SaveCorrectionForm(CorrectionForm correctionForm)
        {
            new MSSQLFactory().ExecuteNonQuery($@"DELETE FROM dbo.CorrectionForm WHERE Student_ID='{correctionForm.Student_ID}'");
            return new MSSQLFactory().InsertRecord(correctionForm);
        }

        public int UpdateCorrectionForm(Guid Student_ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($@"UPDATE dbo.CorrectionForm SET PaymentStatus={(short)FormStatus.FeePaid} WHERE Student_ID='{Student_ID}'");
        }
    }
}
