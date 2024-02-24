using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.DataManagers;
using CUSrinagar.Models;

namespace CUSrinagar.BusinessManagers
{
    public class GeneralDDLManager
    {
        public SelectList GetCourseDDL(bool hasCombination) => new SelectList(new GeneralDDLDB().GetCourseDDL(hasCombination), "Value", "Text");
        public SelectList GetCourseOptLabelDDL(bool hasCombination) => new SelectList(new GeneralDDLDB().GetCourseOptLabelDDL(hasCombination), "Value", "Text", "DataGroupField", null, null);

        public SelectList GetCourseOptLabelDDL(bool hasCombination,Guid College_ID) 
            => new SelectList(new GeneralDDLDB().GetCourseOptLabelDDL(hasCombination,College_ID), "Value", "Text", "DataGroupField", null, null);
        public SelectList GetDistrictList()
        {
            List<DropDownOptLabelGeneral> Districts =new GeneralDDLDB().GetDistrictList();
            #region MyRegion
            //List<DropDownOptLabelGeneral> JkDistricts = Districts.Where(x => x.DataGroupField.ToLower() == "jammu and kashmir").ToList();
            //Districts = Districts.Where(x => x.DataGroupField.ToLower() != "jammu and kashmir").ToList();
            //Districts.InsertRange(0, JkDistricts);
            //JkDistricts = null; 
            #endregion
            Districts.Add(new DropDownOptLabelGeneral { Text="Other",Value="OTHER|OTHER",DataGroupField="Other"});
            return new SelectList(Districts, "Value", "Text", "DataGroupField", new object());
        }
        public SelectList GetAssemblyList()
        {
            List<DropDownOptLabelGeneral> Assemblies = new GeneralDDLDB().GetAssemblyList();
            List<DropDownOptLabelGeneral> AssembliesTemp = Assemblies.Where(x => x.DataGroupField.ToLower() == "other" || x.DataGroupField.ToLower() == "others").ToList();
            Assemblies = Assemblies.Where(x => x.DataGroupField.ToLower() != "other" && x.DataGroupField.ToLower() != "others").ToList();
            Assemblies.AddRange(AssembliesTemp);
            AssembliesTemp = null;
            return new SelectList(Assemblies, "Value", "Text", "DataGroupField", null, null);
        }

        public SelectList GetDistrictListOnly()
        {
            List<DropDownOptLabelGeneral> Districts = new GeneralDDLDB().GetDistrictListOnly();
            #region MyRegion
            //List<DropDownOptLabelGeneral> JkDistricts = Districts.Where(x => x.DataGroupField.ToLower() == "jammu and kashmir").ToList();
            //Districts = Districts.Where(x => x.DataGroupField.ToLower() != "jammu and kashmir").ToList();
            //Districts.InsertRange(0, JkDistricts);
            //JkDistricts = null; 
            #endregion
            Districts.Add(new DropDownOptLabelGeneral { Text = "Other", Value = "OTHER", DataGroupField = "Other" });
            return new SelectList(Districts, "Value", "Text", "DataGroupField", new object());
        }
    }
}
