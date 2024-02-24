
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{

    public class AssignSubjectDB
    {
        public List<ProfessorCRClasses> GetAllProfessorORCRSubjects(Parameters parameter,string Column)
        {
            return new MSSQLFactory().GetObjectList<ProfessorCRClasses>(new AssignSubjectSQLQueries().GetAllProfessorSubjects(parameter, Column));
        }

        public ProfessorCRClasses GetDetails(Parameters parameter,string Column)
        {
            Parameters parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameters>(Newtonsoft.Json.JsonConvert.SerializeObject(parameter));
            List<ProfessorCRClasses> listProfessorCRClasses = new MSSQLFactory().GetObjectList<ProfessorCRClasses>(new AssignSubjectSQLQueries().GetAllProfessorSubjects(parameter,Column));
            listProfessorCRClasses.FirstOrDefault().ProfessorCRClassDetails = new MSSQLFactory().GetObjectList<ProfessorCRClassDetails>(new AssignSubjectSQLQueries().GetDetail(parameters));
            if (listProfessorCRClasses.FirstOrDefault().ProfessorCRClassDetails == null)
            { listProfessorCRClasses.FirstOrDefault().ProfessorCRClassDetails = new List<ProfessorCRClassDetails>(); }
            if (AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College_ClassRepresentative) || AppUserHelper.AppUsercompact.UserRoles.Any(y => y == AppRoles.College_AssistantProfessor))
            {

                listProfessorCRClasses.FirstOrDefault().ProfessorCRClassDetails.Insert(0, (new ProfessorCRClassDetails()
                {
                    ProfessorCRClasses_ID = listProfessorCRClasses.FirstOrDefault().ProfessorCRClasses_ID,
                }));
            }
            return listProfessorCRClasses.FirstOrDefault();
        }

        public List<ProfessorCRClasses> GetAllProfessorList(Parameters parameter)
        {
            return new MSSQLFactory().GetObjectList<ProfessorCRClasses>(new AssignSubjectSQLQueries().GetAllProfessorList(parameter));
        }

        public int Edit(ProfessorCRClassDetails professorCRClassDetails)
        {
            List<string> ignoreinquery = new List<string>() { "CreatedOn", "CreatedBy" };
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(professorCRClassDetails, ignoreinquery, null);
            sqlCommand.CommandText += " WHERE ProfessorCRClassDetails_ID=@ProfessorCRClassDetails_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int Add(ProfessorCRClassDetails professorCRClassDetails)
        {
            return new MSSQLFactory().InsertRecord(professorCRClassDetails);
        }


    }
}
