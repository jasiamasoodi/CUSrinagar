using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class CategoriesDB
    {
        public List<SelectListItem> GetCategoryList()
        {
            string Qurery = "SELECT CategoryCode AS [Value],CategoryName AS [Text] FROM CategoryMaster  WHERE Isvisible=1";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }

        public List<SelectListItem> GetCategoryList(Parameters parameters)
        {
            string Query = "SELECT CategoryCode AS [Value],CategoryName AS [Text] FROM CategoryMaster " ;
            var whereClause = new GeneralFunctions().GetWhereClause<SelectListItem>(parameters.Filters);
            SqlCommand sqlCommand = whereClause.Command;
            Query += whereClause.WhereClause;
            sqlCommand.CommandText = Query;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }

        public List<SelectListItem> GetCategoryListALL()
        {
            string Qurery = "SELECT CategoryCode AS [Value],CategoryName AS [Text] FROM CategoryMaster";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = Qurery;
            return new MSSQLFactory().GetObjectList<SelectListItem>(sqlCommand);
        }
    }
}
