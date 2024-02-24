using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class GeneralSQLQueries
    {
        internal string GetSettings(Module grievance)
        {
            string query = string.Empty;

            switch (grievance)
            {
                case Module.PersonalInformation:
                    break;
                case Module.Address:
                    break;
                case Module.ExamForm:
                    break;
                case Module.SelectedCombination:
                    break;
                case Module.AdditionalSubjects:
                    break;
                case Module.PreviousQualification:
                    break;
                case Module.CoursesApplied:
                    break;
                case Module.ReEvaluation:
                    break;
                case Module.Grievance:
                    query = " SELECT * FROM Settings WHERE Module = " + ((short)grievance);
                    break;
                default:
                    break;
            }

            return query;
        }
    }
}
