using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Extensions;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class SelfFinancedDB
    {        
        public int UpdateAppliedForSelfFinancedSeat(Guid Payment_ID, Guid Student_ID, List<Guid> CoursesApplied, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGCoursesApplied_{printProgramme.ToString()} SET SelfFinancedPayment_ID='{Payment_ID}' WHERE Student_ID='{Student_ID}' AND Course_ID IN({CoursesApplied.ToIN()})");
        }
   
        public int UpdateAppliedForSelfFinancedSeat(Guid Payment_ID, Guid Student_ID, PrintProgramme printProgramme)
        {
            return new MSSQLFactory().ExecuteNonQuery($"UPDATE ARGCoursesApplied_{printProgramme.ToString()} SET SelfFinancedPayment_ID='{Payment_ID}' WHERE Student_ID='{Student_ID}'");
        }

        public List<Guid> GetCourse_IDs(List<string> CourseCodes)
        {
            return new MSSQLFactory().GetSingleValues<Guid>($"SELECT Course_ID FROM ADMCourseMaster WHERE CourseCode IN({CourseCodes.ToIN()})");
        }
        public List<ARGCoursesApplied> GetCoursePrefIDs(List<string> CourseCodes)
        {
            return new MSSQLFactory().GetObjectList<ARGCoursesApplied>($"SELECT Course_ID,CourseCode FROM ADMCourseMaster WHERE CourseCode IN({CourseCodes.ToIN()})");
        }


        public PaymentDetails GetAppliedSelfFinancedPayment(PrintProgramme printProgramme, Guid Payment_ID)
        {
            string query = $@"SELECT * FROM PaymentDetails_{printProgramme.ToString()} WHERE Payment_ID='{Payment_ID}' ORDER BY TxnDate DESC";
            return new MSSQLFactory().GetObject<PaymentDetails>(query);
        }

        public ARGFormNoMaster GetFromNoMasterForSF(PrintProgramme printProgramme)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@Programme", printProgramme);
            cmd.CommandText = "SELECT Top 1 * FROM ARGFormNoMaster WHERE PrintProgramme=@Programme AND AllowApplyForSelfFinancedSeat=1 ORDER BY BatchToSet DESC";
            return new MSSQLFactory().GetObject<ARGFormNoMaster>(cmd);
        }
    }
}
