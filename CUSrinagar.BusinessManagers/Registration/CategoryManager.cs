using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using CUSrinagar.DataManagers;
using CUSrinagar.Models;
using GeneralModels;

namespace CUSrinagar.BusinessManagers
{
    public class CategoryManager
    {
        public List<SelectListItem> GetCategoryList()
        {
            return new CategoriesDB().GetCategoryList();
        }

        public List<SelectListItem> GetCategoryList(Parameters parameters)
        {
            return new CategoriesDB().GetCategoryList(parameters);
        }
        public List<SelectListItem> GetCategoryListALL()
        {
            return new CategoriesDB().GetCategoryListALL();
        }
    }
}
