using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terex;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class CollegeSQLQueries
    {
        internal string GetCollegeList = @" SELECT CollegeFullName AS Text , CAST(College_ID AS Varchar(50)) AS Value FROM ADMCollegeMaster ";
        internal string GetADMCollegeMasterList = @"SELECT Convert(Varchar(50),College_ID) AS Value,CollegeFullName AS Text  FROM ADMCollegeMaster where [Status]=1 ORDER BY CollegeCode ASC";

        internal string GetADMCollegeMasterAllList = @"SELECT  *  FROM ADMCollegeMaster where [Status]=1";


        internal string GetCollegeWiseNewRegistrations(PrintProgramme programme,short batchToAssign)
        {
            programme = new GeneralFunctions().MappingTable(programme);

            return $@"SELECT CollegeFullName [Text], CAST(COUNT(Student_ID) AS VARCHAR(10))+'$'+CAST(College_ID AS VARCHAR(50)) [Value]
                                                                FROM ARGPersonalInformation_{programme}
                                                                    INNER JOIN ADMCollegeMaster
                                                                        ON AcceptCollege_ID = College_ID
                                                                WHERE Batch ={batchToAssign}
                                                                 AND CUSRegistrationNo IS NULL
                                                                GROUP BY CollegeFullName,College_ID";
        }

        internal SqlCommand GetAllCollegeList(Parameters parameter)
        {
            string query = @"SELECT  *  FROM ADMCollegeMaster ";
            FilterHelper helper = new GeneralFunctions().GetWhereClause<ADMCollegeMaster>(parameter.Filters);
            query += helper.WhereClause;
            query += new GeneralFunctions().GetPagedQuery(query, parameter);
            helper.Command.CommandText = query;
            return helper.Command;
        }
    }
}
