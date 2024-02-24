using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class AssignSubjectManager
    {
        public List<ProfessorCRClasses> GetAllProfessorSubjects(Parameters parameter)
        {
            return new AssignSubjectDB().GetAllProfessorORCRSubjects(parameter,"Professor_ID");
        }

        public ProfessorCRClasses GetDetails(Parameters parameter,string Column)
        {
            return new AssignSubjectDB().GetDetails(parameter, Column);
        }

        public List<ProfessorCRClasses> GetAllProfessorList(Parameters parameter)
        {
            return new AssignSubjectDB().GetAllProfessorList(parameter);
        }

        public int AddOrEdit(ProfessorCRClassDetails professorCRClassDetails)
        {
            if (professorCRClassDetails.ProfessorCRClassDetails_ID == Guid.Empty)
            {
                professorCRClassDetails.ProfessorCRClassDetails_ID = Guid.NewGuid();
                professorCRClassDetails.SetWorkFlow(RecordState.New);
                return new AssignSubjectDB().Add(professorCRClassDetails);
            }
            else
            {
                professorCRClassDetails.SetWorkFlow(RecordState.Old);
                return new AssignSubjectDB().Edit(professorCRClassDetails);
            }

        }
        public List<ProfessorCRClasses> GetAllCRSubjects(Parameters parameter)
        {
            return new AssignSubjectDB().GetAllProfessorORCRSubjects(parameter,"CR_ID");
        }
    }

}
