using CUSrinagar.DataManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.Models;
using GeneralModels;
using CUSrinagar.Enums;

namespace CUSrinagar.BusinessManagers
{
    public class CollegeManager
    {

        public List<SelectListItem> CollegeList()
        {
            return CollegeDB.CollegeList();
        }
        public List<SelectListItem> GetADMCollegeMasterList()
        {
            return CollegeDB.GetADMCollegeMasterList();
        }
        public List<SelectListItem> GetAllCollegeMasterList()
        {
            return new CollegeDB().GetALLCollegeMasterList();
        }

        public ADMCollegeMaster GetItem(Guid College_ID)
        {
            if (College_ID == Guid.Empty)
                return null;
            return new CollegeDB().GetItem(College_ID);
        }

        public List<CUSCollegeInfoDashboard> GetCollegeListForDashboard(string year)
        {
            return CollegeDB.CUSCollegeInfoDashboard(year);
        }

        public List<ADMCollegeMaster> GetCollegeList(Parameters parameter)
        {
            return new CollegeDB().GetAllCollegeList(parameter);
        }

        public List<ADMCollegeMaster> GetCollegeList()
        {
            return CollegeDB.GetADMCollegeMasterAllList();
        }

        public IEnumerable<SelectListItem> GetCollegeWiseNewRegistrations(PrintProgramme programme, short batchToAssign)
        {
            return CollegeDB.GetCollegeWiseNewRegistrations(programme, batchToAssign);
        }
        public List<SelectListItem> GetCollegePreference(List<Guid> Course_IDs)
        {
            return new CollegeDB().GetCollegePreference(Course_IDs);
        }
        internal string GetColleges(string colleges)
        {
            return new CollegeDB().GetColleges(colleges);
        }
    }
}
