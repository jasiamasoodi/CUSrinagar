using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class ExaminationWhiteListDB
    {
        public List<ExaminationWhiteList> GetAllApplications(Parameters parameters)
        {
            string query = @"SELECT 
                                ExaminationWhiteList.*,AppUsers.FullName AllowedByName,ARGPersonalInformation.FullName StudentName,CUSRegistrationNo
                                FROM ExaminationWhiteList 
                                INNER JOIN 
                                (
                                SELECT Student_ID,FullName,CUSRegistrationNo FROM ARGPersonalInformation_IH
                                UNION                     
                                SELECT Student_ID,FullName,CUSRegistrationNo FROM ARGPersonalInformation_UG
                                UNION                      
                                SELECT Student_ID,FullName,CUSRegistrationNo FROM ARGPersonalInformation_PG
                                ) ARGPersonalInformation ON  ARGPersonalInformation.Student_ID = ExaminationWhiteList.Student_ID
                                INNER JOIN AppUsers ON AllowedBy_ID = User_ID ";

            var helper = new GeneralFunctions().GetWhereClause<ExaminationWhiteList>(parameters.Filters);

            SqlCommand command = helper.Command;
            query += helper.WhereClause;

            if (parameters.PageInfo != null)
                query += new GeneralFunctions().GetPagedQuery(query, parameters);

            command.CommandText = query;

            return new MSSQLFactory().GetObjectList<ExaminationWhiteList>(command);
        }

        public int SaveApplication(ExaminationWhiteListCompact application)
        {
            return new MSSQLFactory().InsertRecord(application, "ExaminationWhiteList");
        }

        public int DeleteApplication(Guid _ID)
        {
            return new MSSQLFactory().ExecuteNonQuery($"DELETE FROM ExaminationWhiteList WHERE WHiteList_ID ='{_ID}'");
        }

        public int EditApplication(ExaminationWhiteListCompact application)
        {
            throw new NotImplementedException();
        }
    }
}
