using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.DataManagers.SQLQueries;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using Terex;

namespace CUSrinagar.DataManagers
{
    public class SalaryDB
    {
        public int EditEmployeeSalary(SalaryBill salaryBill, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(salaryBill, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE SalaryBill_ID=@SalaryBill_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public int AddEmployeeSalary(SalaryBill salaryBill)
        {
            return new MSSQLFactory().InsertRecord(salaryBill);
        }

        public SalaryBill GetEmployeeSalary(Guid employee_Id)
        {
            return new MSSQLFactory().GetObject<SalaryBill>(new SalarySQLQueries().GetEmployeeSalary(employee_Id));
        }

        public List<SalaryBill> GetEmployeeSalaryList()
        {
            return new MSSQLFactory().GetObjectList<SalaryBill>(new SalarySQLQueries().GetEmployeeSalaryList());
        }

        public int ArchiveEmployeeSalary(SalaryBillArchive salaryBillArchive)
        {
            return new MSSQLFactory().InsertRecord(salaryBillArchive, "SalaryBillArchive");
        }

        public List<SalaryCalculationColumns> FetchSalaryCalculationColumns()
        {
            return new MSSQLFactory().GetObjectList<SalaryCalculationColumns>(new SalarySQLQueries().FetchSalaryBill());
        }

        public int GenerateSalaryBill()
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText += $@"  UPDATE SB
                                            SET DA=CASE When((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='DA')*BasicPay/CAST(100 AS Decimal))>0
                                              Then ROUND( ((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='DA')*BasicPay/CAST(100 AS Decimal)),0)
                                                Else DA
                                                End
                                            ,HRA=CASE When((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='HRA')*BasicPay/CAST(100 AS Decimal))>0
                                              Then ROUND( ((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='HRA')*BasicPay/CAST(100 AS Decimal)),0)
                                                Else HRA
                                                End
                                            ,MA=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='MA')>0
                                              Then  ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='MA'),0)
                                                Else MA
                                                End
                                            ,CCA=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='CCA')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='CCA'),0)
                                                Else CCA
                                                End
                                            ,SPL_Pay=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SPL_Pay')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SPL_Pay'),0)
                                                Else SPL_Pay
                                                End
                                            ,ChargeAllow=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='ChargeAllow')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='ChargeAllow'),0)
                                                Else ChargeAllow
                                                End
                                            ,GPF_Sub=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GPF_Sub')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GPF_Sub'),0)
                                                Else GPF_Sub
                                                End
                                            ,GPF_ref=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GPF_ref')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GPF_ref'),0)
                                                Else GPF_ref
                                                End
                                            ,GroupIns=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GroupIns')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='GroupIns'),0)
                                                Else GroupIns
                                                End
                                            ,SLI_I=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SLI_I')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SLI_I'),0)
                                                Else SLI_I
                                                End
                                            ,SLI_II=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SLI_II')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='SLI_II'),0)
                                                Else SLI_II
                                                End
                                            ,I_Tax=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='I_Tax')>0
                                              Then  ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='I_Tax'),0)
                                                Else I_Tax
                                                End
                                            ,RecoveryExcess=CASE When(SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='RecoveryExcess')>0
                                              Then ROUND((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='RecoveryExcess'),0)
                                                Else RecoveryExcess
                                                End
                                            ,NPSShare=CASE WHEN (ISNPS=1) THEN( CASE When((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='NPSShare')*(BasicPay+((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='DA')*BasicPay)/CAST(100 AS Decimal))/CAST(100 AS Decimal))>0 
                                              Then ROUND(((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='NPSShare')*(BasicPay+(ROUND((((SELECT ColumnValue FROM SalaryCalculationColumns WHERE SalaryCalculationColumn='DA')*BasicPay)/CAST(100 AS Decimal)),0)))/CAST(100 AS Decimal)),0)
                                                Else NPSShare
                                                END)
												ELSE 0 END
												 From SalaryBill SB JOIN Employees ON Employees.Employee_ID=SB.Emp_ID
                                              Where EmployeeType Not IN ('{(int)EmployeeType.Need_Basis}','{((int)EmployeeType.Contractual)}') 
                                            ";
            new MSSQLFactory().ExecuteNonQuery(sqlCommand);

            sqlCommand = new SqlCommand();
            sqlCommand.CommandText += @" UPDATE SalaryBill
                                            SET NetPay = (BasicPay
                                            + DA
                                            +HRA
                                            +MA
                                            +CCA
                                            +SPL_Pay
                                            +ChargeAllow
                                            +OtherAllowances)
                                           - 
                                           ( GPF_Sub
                                            +GPF_ref
                                            +GroupIns
                                            +SLI_I
                                            +SLI_II
                                            +I_Tax
                                            +RecoveryExcess
                                            +NPSShare
                                            +OtherDeductions)
                                            ";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }

        public List<SalaryBillArchive> FetchEmployeeHistoryList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<SalaryBillArchive>(new SalarySQLQueries().FetchArchiveSalaryList(parameters));
        }

        public List<SalaryBillArchive> FetchSalaryList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<SalaryBillArchive>(new SalarySQLQueries().FetchSalaryList(parameters));
        }

        public List<SalaryBillArchive> FetchArchiveSalaryList(Parameters parameters)
        {
            return new MSSQLFactory().GetObjectList<SalaryBillArchive>(new SalarySQLQueries().FetchArchiveSalaryList(parameters));
        }


        public int EditSalarySettings(SalaryCalculationColumns row, List<string> ignoreParameter, List<string> ignoreQuery)
        {
            SqlCommand sqlCommand = new MSSQLFactory().UpdateRecord(row, ignoreQuery, ignoreParameter);
            sqlCommand.CommandText += " WHERE SalaryCalculationColumn_ID=@SalaryCalculationColumn_ID";
            return new MSSQLFactory().ExecuteNonQuery(sqlCommand);
        }
    }
}
