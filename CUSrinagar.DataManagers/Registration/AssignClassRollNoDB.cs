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
    public class AssignClassRollNoDB
    {
        public bool StudentExistsInCollege(string FormNoOrBoardRegnNo, PrintProgramme printProgramme, int Batch)
        {
            string Qurery = $@"SELECT COUNT(Student_ID) FROM ARGPersonalInformation_{printProgramme.ToString()} 
                                WHERE AcceptCollege_ID=@College_ID AND Batch=@Batch AND (StudentFormNo=@FormNo OR CUSRegistrationNo=@FormNo)";

            SqlCommand command = new SqlCommand(Qurery);
            command.Parameters.AddWithValue("@FormNo", FormNoOrBoardRegnNo);
            command.Parameters.AddWithValue("@Batch", Batch);
            command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);

            return new MSSQLFactory().ExecuteScalar<int>(command) > 0;
        }
        public string ClassRollNoAlreadyAssigned(string FormNoOrBoardRegnNo, PrintProgramme printProgramme, int Batch)
        {
            string Qurery = $@"SELECT a.ClassRollNo FROM ARGPersonalInformation_{printProgramme.ToString()} a
                            WHERE (a.StudentFormNo=@FormNo OR a.CUSRegistrationNo=@FormNo) AND Batch=@Batch AND a.ClassRollNo IS NOT NULL";

            SqlCommand command = new SqlCommand(Qurery);
            command.Parameters.AddWithValue("@Batch", Batch);
            command.Parameters.AddWithValue("@FormNo", FormNoOrBoardRegnNo);

            return new MSSQLFactory().ExecuteScalar<string>(command);
        }
        public string ClassRollNoAlreadyExists(string ClassRollNo, PrintProgramme printProgramme, int Batch, Guid Course_ID)
        {
            string Qurery = $@"SELECT TOP 1 a.StudentFormNo+'|'+ ISNULL(a.CUSRegistrationNo,'') FROM ARGPersonalInformation_{printProgramme.ToString()} a
                            JOIN ARGSelectedCombination_{printProgramme.ToString()} sc ON sc.Student_ID = a.Student_ID AND sc.Semester=1
                            JOIN ADMCombinationMaster cm ON cm.Combination_ID = sc.Combination_ID
                            WHERE a.AcceptCollege_ID=@College_ID
                            AND a.ClassRollNo=@ClassRollNo AND Batch=@Batch AND cm.Course_ID=@Course_ID";

            SqlCommand command = new SqlCommand(Qurery);
            command.Parameters.AddWithValue("@ClassRollNo", ClassRollNo);
            command.Parameters.AddWithValue("@Batch", Batch);
            command.Parameters.AddWithValue("@College_ID", AppUserHelper.College_ID);
            command.Parameters.AddWithValue("@Course_ID", Course_ID);

            return new MSSQLFactory().ExecuteScalar<string>(command);
        }

        public int Update(AssignClassRollNo assignClassRollNo)
        {
            string Qurery = $@"UPDATE ARGPersonalInformation_{assignClassRollNo.Programme.ToString()} SET ClassRollNo=@ClassRollNo,UpdatedOn=@UpdatedOn,UpdatedBy=@UpdatedBy
                            WHERE (StudentFormNo=@FormNo OR CUSRegistrationNo=@FormNo) AND Batch=@Batch";

            SqlCommand command = new SqlCommand(Qurery);
            command.Parameters.AddWithValue("@ClassRollNo", assignClassRollNo.ClassRollNo.ToUpper());
            command.Parameters.AddWithValue("@FormNo", assignClassRollNo.StudentFormNoOrRegnNo);
            command.Parameters.AddWithValue("@Batch", assignClassRollNo.Batch);
            command.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);
            command.Parameters.AddWithValue("@UpdatedBy", AppUserHelper.User_ID);

            return new MSSQLFactory().ExecuteNonQuery(command);
        }
    }
}
