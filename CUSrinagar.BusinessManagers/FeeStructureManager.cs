using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class FeeStructureManager
    {
        public List<FeeStructureViewModel> GetFeeStructureAdmissionForIntegratedAndPGCourses(int Programme,int Year)
        {
            List<FeeStructureViewModel> itemsList = new List<FeeStructureViewModel>();
            var FeeStructureAdmissionList = new FeeStructureDB().GetFeeStructureAdmissionForIntegratedAndPGCourses(Programme,Year);

            var NoOfStudents = new StudentManager().GetStudentsForFeeStructure(Programme,Year);

            foreach (var MasterCourseCategory in FeeStructureAdmissionList?.Select(i => i.MasterCourseCategory)?.ToList()?? new List<MasterCourseCategory>())
            {
                var feeStructureItem = FeeStructureAdmissionList.Where(i => i.MasterCourseCategory == MasterCourseCategory);

                foreach (var property in typeof(FeeStructureAdmission).GetProperties().Where(p => p.PropertyType == typeof(int)).ToList())
                {
                    FeeStructureViewModel viewModel = new FeeStructureViewModel
                    {
                        FeeComponent = property.Name,
                        FeeComponentAmount = (int)property.GetValue(feeStructureItem.FirstOrDefault()),
                        MasterCourseCategory = MasterCourseCategory,
                        NoOfStudents = NoOfStudents
                    };
                    viewModel.SubTotal = viewModel.NoOfStudents * viewModel.FeeComponentAmount;
                    itemsList.Add(viewModel);
                }
            }

            return itemsList;
        }

    }
}
