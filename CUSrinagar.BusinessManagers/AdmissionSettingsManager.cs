using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.BusinessManagers
{
    public class AdmissionSettingsManager
    {
        #region Regular Admission Settings
        public List<ARGFormNoMaster> GetFormNoMasterList()
        {
            return new AdmissionSettingsDB().GetFormNoMasterList();
        }

        public List<SelectListItem> GetCourses(PrintProgramme printProgramme)
        {
            return new AdmissionSettingsDB().GetCourses(printProgramme);
        }

        public ARGFormNoMasterSettings GetFormNoMasterSettings(PrintProgramme printProgramme)
        {
            ARGFormNoMasterSettings formNoMasterSettings = new AdmissionSettingsDB().GetFromNoMaster(printProgramme);
            formNoMasterSettings.DownloadAdmitCards = new AdmissionSettingsDB().GetAllowDownloadEntranceAdmitCards(printProgramme, formNoMasterSettings.BatchToSet);
            return formNoMasterSettings;
        }

        public int UpdateSettings(ARGFormNoMasterSettings formNoMasterSettings)
        {
            if (formNoMasterSettings == null)
                return 0;

            AdmissionSettingsDB admissionSettingsDB = new AdmissionSettingsDB();
            if (formNoMasterSettings.AllowProgrammesInSelfFinance?.Any(x => x == Guid.Empty) ?? false)
                formNoMasterSettings.AllowProgrammesInSelfFinance = null;

            ARGFormNoMaster formNoMaster = new ARGFormNoMaster
            {
                FormPrefix = $"CUS-{formNoMasterSettings.PrintProgramme.ToString()}-{formNoMasterSettings.BatchToSet.ToString().Substring(2, 2)}0",
                PrintProgramme = formNoMasterSettings.PrintProgramme,
                BatchToSet = formNoMasterSettings.BatchToSet,
                AllowOnlinePayment = formNoMasterSettings.AllowOnlinePayment,
                AllowApplyForSelfFinancedSeat = formNoMasterSettings.AllowApplyForSelfFinancedSeat,
                SelfFinancedApplicationFee = formNoMasterSettings.SelfFinancedApplicationFee,
                AllowProgrammesInSelfFinance = formNoMasterSettings.AllowProgrammesInSelfFinance?.ToSingleString('|'),
                BasicFee = formNoMasterSettings.BasicFee,
                PerCourse = formNoMasterSettings.PerCourse,
                AdditionalFeeForNonJK = formNoMasterSettings.AdditionalFeeForNonJK,
                FeeForAlreadyInIH = formNoMasterSettings.FeeForAlreadyInIH,
                ClosingDate = formNoMasterSettings.ClosingDate,
                ValidatePaymentByCourse = formNoMasterSettings.ValidatePaymentByCourse,
                ShowLateralEntry = formNoMasterSettings.ShowLateralEntry,
                AllowEditAfterPayment = formNoMasterSettings.AllowEditAfterPayment,
                AllowNonCUET = formNoMasterSettings.AllowNonCUET,
                AllowFormsToBeEditFromDate = formNoMasterSettings.AllowFormsToBeEditFromDate,
            };
            admissionSettingsDB.UpdateFromNoMaster(formNoMaster);
            admissionSettingsDB.CloseRegistration(formNoMasterSettings.PrintProgramme);

            if (formNoMasterSettings.OpenRegistrationCourses.IsNotNullOrEmpty())
            {
                admissionSettingsDB.OpenRegistration(formNoMasterSettings.OpenRegistrationCourses);
            }

            if (formNoMasterSettings.ResetFormCount)
                admissionSettingsDB.ResetFormNoCount(formNoMasterSettings.PrintProgramme);

            admissionSettingsDB.UpdateAllowDownloadEntranceAdmitCards(formNoMasterSettings.PrintProgramme, formNoMasterSettings.DownloadAdmitCards, formNoMasterSettings.ShowLateralEntry);

            return 1;
        }
        #endregion
    }
}
