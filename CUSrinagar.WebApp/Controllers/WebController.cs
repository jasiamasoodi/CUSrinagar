using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.Controllers
{
    public class WebController : Controller
    {
        public void ExportToCSV<T>(List<T> list,string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "List" + DateTime.Now.ToString();

            string csv= GetCSV(list);
            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.csv", fileName));
            HttpContext.Response.ContentType = "text/csv";
            HttpContext.Response.AddHeader("CU", "public");
            HttpContext.Response.Write(csv);
            HttpContext.Response.End();
        }

        private string GetCSV<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            //Get the properties for type T for the headers
            PropertyInfo[] propInfos = typeof(T).GetProperties();
            for (int i = 0; i <= propInfos.Length - 1; i++)
            {
                sb.Append(propInfos[i].Name);

                if (i < propInfos.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
            //Loop through the collection, then the properties and add the values
            for (int i = 0; i <= list.Count - 1; i++)
            {
                T item = list[i];
                for (int j = 0; j <= propInfos.Length - 1; j++)
                {
                    object o = item.GetType().GetProperty(propInfos[j].Name).GetValue(item, null);
                    if (o != null)
                    {
                        string value = o.ToString();
                        //Check if the value contans a comma and place it in quotes if so
                        //if (value.Contains(","))
                        //{
                        //    value = string.Concat("\"", value, "\"");
                        //}
                        if (value.Contains(","))
                        {
                            value.Replace("\"", "\"\"");
                            value = string.Concat("\"", value, "\"");
                        }
                        //Replace any \r or \n special characters from a new line with a space
                        if (value.Contains("\r"))
                        {
                            value = value.Replace("\r", " ");
                        }
                        if (value.Contains("\n"))
                        {
                            value = value.Replace("\n", " ");
                        }
                        sb.Append(value);
                    }
                    if (j < propInfos.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }
}