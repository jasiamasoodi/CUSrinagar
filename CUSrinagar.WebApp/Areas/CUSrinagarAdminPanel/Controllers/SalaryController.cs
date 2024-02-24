using CUSrinagar.BusinessManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using CUSrinagar.OAuth;
using GeneralModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CUSrinagar.WebApp.CUSrinagarAdminPanel.Controllers
{
    [OAuthorize(AppRoles.University_Salary,AppRoles.University_CAO)]
    public class SalaryController : Controller
    {
        string errorMsg = $"<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> <a href='#' class='alert-link'>##</a></div>";

        // GET: CUSrinagarAdminPanel/Salary
        public ActionResult EmployeeList()
        {
            return View();
        }
        public ActionResult FetchEmployeeList(Parameters parameters)
        {
            List<Employee> employeeList = new EmployeeManager().List(parameters);
            return View(employeeList);
        }
        [HttpGet]
        public ActionResult EditEmployeeSalary(Guid Employee_Id)
        {
            SalaryBill salaryBill = new SalaryManager().GetEmployeeSalary(Employee_Id);
            salaryBill.Emp_ID = Employee_Id;
            return View(salaryBill);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrEditEmployeeSalary(SalaryBill salaryBill)
        {
            if (ModelState.IsValid)
            {
                salaryBill.NetPay = new SalaryManager().calculateNetPay(salaryBill);
                if (salaryBill.SalaryBill_ID == null || salaryBill.SalaryBill_ID == Guid.Empty)
                {
                    if (new SalaryManager().AddEmployeeSalary(salaryBill) > 0)
                    {
                        TempData["response"] = errorMsg.Replace("##", $"Saved successfully.").Replace("alert-danger", "alert-success");

                    }
                    else
                    {
                        TempData["response"] = errorMsg.Replace("##", $"Some Error Occured.");
                    }
                }
                else
                {
                    if (new SalaryManager().EditEmployeeSalary(salaryBill) > 0)
                    {
                        TempData["response"] = errorMsg.Replace("##", $"Saved successfully.").Replace("alert-danger", "alert-success");

                    }
                    else
                    {
                        TempData["response"] = errorMsg.Replace("##", $"Some Error Occured.");
                    }
                }
            }
            return View("EditEmployeeSalary", salaryBill);
        }
          public string CalculateSalary(int BasicPay, bool isNPS)
        {

            List<SalaryCalculationColumns> salaryCalculationcolumns = new SalaryManager().FetchSalaryCalculationColumns();
            int DA = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "da").FirstOrDefault().IsPercent ? (BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "da").FirstOrDefault().ColumnValue / 100) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "da").FirstOrDefault().ColumnValue;

            SalaryBill salaryBill = new SalaryBill()
            {
                DA = DA,
                HRA = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "hra").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "hra").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "hra").FirstOrDefault().ColumnValue,
                MA = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "ma").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "ma").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "ma").FirstOrDefault().ColumnValue,
                CCA = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "cca").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "cca").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "cca").FirstOrDefault().ColumnValue,
                SPL_PAY = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "spl_pay").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "spl_pay").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "spl_pay").FirstOrDefault().ColumnValue,
                ChargeAllow = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "chargeallow").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "chargeallow").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "chargeallow").FirstOrDefault().ColumnValue,
                GPF_Sub = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_sub").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_sub").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_sub").FirstOrDefault().ColumnValue,
                GPF_ref = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_ref").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_ref").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "gpf_ref").FirstOrDefault().ColumnValue,
                GroupIns = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "groupins").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "groupins").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "groupins").FirstOrDefault().ColumnValue,
                SLI_I = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_i").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_i").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_i").FirstOrDefault().ColumnValue,
                SLI_II = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_ii").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_ii").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "sli_ii").FirstOrDefault().ColumnValue,
                I_Tax = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "i_tax").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "i_tax").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "i_tax").FirstOrDefault().ColumnValue,
                RecoveryExcess = salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "recoveryexcess").FirstOrDefault().IsPercent ? (int)Math.Round((double)(BasicPay * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "recoveryexcess").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "recoveryexcess").FirstOrDefault().ColumnValue,
                NPSShare = isNPS ? (salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "npsshare").FirstOrDefault().IsPercent ? (int)Math.Round((double)((BasicPay + DA) * salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "npsshare").FirstOrDefault().ColumnValue / (decimal)100)) : salaryCalculationcolumns.Where(x => x.SalaryCalculationColumn.ToLower() == "npsshare").FirstOrDefault().ColumnValue) : 0
            };
            return JsonConvert.SerializeObject(salaryBill);
        }
        [HttpGet]
        public ActionResult EditSalarySettings()
        {
            List<SalaryCalculationColumns> salaryCalculationColumns = new SalaryManager().FetchSalaryCalculationColumns();
            SetViewBags();
            return View(salaryCalculationColumns);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSalarySettings(List<SalaryCalculationColumns> salaryCalculationColumn)
        {

            new SalaryManager().EditSalarySettings(salaryCalculationColumn);
            SetViewBags();
            return View(salaryCalculationColumn);
        }
        public void SetViewBags(ADMCourseMaster _ADMCourseMaster = null)
        {

            IEnumerable<SelectListItem> SalaryColumnTypeDDL = new List<SelectListItem>();
            SalaryColumnTypeDDL = Helper.GetSelectList<SalaryCalculationColumn>();
            ViewBag.SalaryColumnType = SalaryColumnTypeDDL ?? SalaryColumnTypeDDL;

        }
        public ActionResult SalaryList()
        {
            ViewBag.EmployeeTypesList = Helper.GetSelectList<EmployeeType>();
            return View();
        }
        public ActionResult FetchSalaryList(Parameters parameter)
        {
            List<SalaryBillArchive> salaryList = new SalaryManager().FetchArchiveSalaryList(parameter);
             if (parameter.Filters != null)
            {
                SearchFilter yearfilter = parameter.Filters.Where(x => x.Column == "Year").FirstOrDefault();
                ViewBag.Year = yearfilter.Value;
                SearchFilter monfilter = parameter.Filters.Where(x => x.Column == "Month").FirstOrDefault();
                ViewBag.Month = monfilter.Value;
            }
            if (salaryList == null || salaryList.Count() == 0)
            { TempData["response"] = errorMsg.Replace("##", $"Salary Bill doesn't exist."); }
            else
            { TempData["response"] = null; }
            return View(salaryList);
        }
       
        public ActionResult CurrentSalaryList()
        {
            return View();
        }
        public ActionResult EmployeeCurrentSalaryList(Parameters parameter)
        {
            List<SalaryBillArchive> salaryList = new List<SalaryBillArchive>();
            if (parameter.Filters != null)
            {
                SearchFilter yearfilter = parameter.Filters.Where(x => x.Column == "Year").FirstOrDefault();
                ViewBag.Year = yearfilter.Value;
                SearchFilter monfilter = parameter.Filters.Where(x => x.Column == "Month").FirstOrDefault();
                ViewBag.Month = monfilter.Value;
                salaryList = new SalaryManager().FetchSalaryList(parameter);
                if (salaryList == null || salaryList.Count() == 0)
                { TempData["response"] = errorMsg.Replace("##", $"Salary Bill Already Generated."); }
                else
                { TempData["response"] = null; }
            }

           
      
            return View("FetchSalaryList", salaryList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ArchiveSalary(SalaryBillFilter salaryBillFilter)
        {
            if (ModelState.IsValid)
            {
               
                if (new SalaryManager().ArchiveEmployeeSalary(salaryBillFilter))
                { TempData["response"] = errorMsg.Replace("##", $"Salary Bill Generated Succesfully.").Replace("alert-danger", "alert-success"); }
                else
                { TempData["response"] = errorMsg.Replace("##", $"Salary Bill Already Generated."); }
               
            }
            return View("CurrentSalaryList");
        }

        public ActionResult EmployeeHistory()
        {
            return View();
        }
        public ActionResult EmployeeHistoryList(Parameters parameter)
        {
            List<SalaryBillArchive> salaryList = new List<SalaryBillArchive>();
            if (parameter.Filters != null)
            {
                salaryList = new SalaryManager().FetchEmployeeHistoryList(parameter);
                if (salaryList == null || salaryList.Count() == 0)
                { TempData["response"] = errorMsg.Replace("##", $"Employee History Not Found."); }
                else
                { TempData["response"] = null; }
            }



            return View(salaryList);
        }
        public ActionResult PrintSalary(Parameters parameter)
        {
            List<SalaryBillArchive> salaryList = new List<SalaryBillArchive>();
            if (parameter.Filters != null)
            {
                SearchFilter yearfilter = parameter.Filters.Where(x => x.Column == "Year").FirstOrDefault();
                ViewBag.Year = yearfilter?.Value??"";
                SearchFilter monfilter = parameter.Filters.Where(x => x.Column == "Month").FirstOrDefault();
                ViewBag.Month = monfilter?.Value??"";
                salaryList = new SalaryManager().FetchArchiveSalaryList(parameter);
             
            }
            return View(salaryList);
        }
    }
}