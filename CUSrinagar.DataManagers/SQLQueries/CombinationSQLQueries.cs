using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class CombinationSQLQueries
    {
        public SqlCommand GetSelectedCombinationCompact(Guid Student_ID, PrintProgramme printProgramme, short semester)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $@" SELECT STDCOMB.Combination_ID,STDCOMB.Student_ID,COMB.CombinationSubjects,COMB.Course_ID from 
                                        ARGSelectedCombination_{printProgramme.ToString()} STDCOMB
                                        JOIN ADMCombinationMaster COMB on STDCOMB.Combination_ID=COMB.Combination_ID
                                        WHERE STDCOMB.Student_ID=@Student_ID AND STDCOMB.Semester=@Semester";
            command.Parameters.AddWithValue("@Student_ID", Student_ID);
            command.Parameters.AddWithValue("@Semester", semester);
            return command;
        }
        
    }
}
