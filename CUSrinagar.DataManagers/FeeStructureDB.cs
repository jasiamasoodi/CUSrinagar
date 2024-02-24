using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Models;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class FeeStructureDB
    {
        public int Save(FeeStructureMaster feeStructureMaster)
        {
            return new MSSQLFactory().InsertRecord(feeStructureMaster);
        }

        public List<FeeStructureAdmission> GetFeeStructureAdmissionForIntegratedAndPGCourses(int Programme, int Year)
        {
            return new MSSQLFactory().GetObjectList<FeeStructureAdmission>(new FeeStructureSQLQueries().FeeStructureAdmissionForIntegratedAndPGCourses(Programme,Year));
        }
    }
}
