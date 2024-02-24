using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    public class UniversityAdminBaseController : Controller
    {
        protected EmptyResult DownloadExcel<T>(List<T> _dataSource, List<string> _modelPropertiesAsColumns, string fileName = null)
        {
            try
            {
                var resultsExcel = _dataSource.DownloadExcel(_modelPropertiesAsColumns, fileName);
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + resultsExcel.Item2 + ".xls");
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                resultsExcel.Item1.HeaderRow.Style.Add("background-color", "#438EB9");
                resultsExcel.Item1.HeaderRow.Style.Add("color", "white");
                Response.Charset = "";
                resultsExcel.Item1.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            catch (NullReferenceException) { }
            return new EmptyResult();
        }
        protected EmptyResult DownloadExcel(DataTable dataTable, string fileName = null)
        {
            try
            {
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    try
                    {
                        DataRow dr = dataTable.NewRow();
                        dr[0] = "Downloaded On " + DateTime.Now;
                        dataTable.Rows.InsertAt(dr, 0);
                    }
                    catch (Exception)
                    {

                    }
                }

                GridView gridView = new GridView();
                gridView.DataSource = dataTable;

                gridView.DataBind();
                dataTable.Dispose();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                Response.ContentType = "application/ms-excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                gridView.HeaderRow.Style.Add("background-color", "#438EB9");
                gridView.HeaderRow.Style.Add("color", "white");

                Response.Charset = "";
                gridView.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
            catch (NullReferenceException) { }
            return new EmptyResult();
        }
        protected void ExportToCSV<T>(List<T> list, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "List" + DateTime.Now.ToString();

            string csv = GetCSV(list);
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