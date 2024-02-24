using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using System.Transactions;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Data;
using GeneralModels;
using CUSrinagar.Models;

namespace CUSrinagar.BusinessManagers
{
    public class ReEvaluationSettingManager
    {
        public List<ReEvaluationSetting> List(Parameters parameter)
        {
            return new ReEvaluationSettingDB().List(parameter);
        }
        public ReEvaluationSetting Get(Guid ReEvaluationSetting_ID)
        {
            var item = new ReEvaluationSettingDB().Get(ReEvaluationSetting_ID);
            if (item != null && item.ResultNotification_IDs.IsNotNullOrEmpty())
            {
                item.ResultNotifications = new ResultNotificationDB().GetList(item.ResultNotification_IDs.ToGuidList());
            }
            return item;
        }

        public ResponseData Save(ReEvaluationSetting model)
        {
            var settingdb = new ReEvaluationSettingDB();
            if (settingdb.ReEvaluationSettingExists(model.SubmittedYear, model.CourseCategory, model.Semester, model.IsReEvaluation))
                return new ResponseData() { ErrorMessage = "ReEvaluation/Xerox setting already exists" };

            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            model.ReEvaluationSetting_ID = Guid.NewGuid();
            response.NumberOfRecordsEffected = settingdb.Save(model);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = model;
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Update(ReEvaluationSetting model)
        {
            var settingdb = new ReEvaluationSettingDB();
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = settingdb.Update(model);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = Get(model.ReEvaluationSetting_ID);
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Delete(Guid ReEvaluationSetting_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = new ReEvaluationSettingDB().Delete(ReEvaluationSetting_ID);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.ErrorMessage = null;
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }
        public ResponseData ChangeStatus(Guid ReEvaluationSetting_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = new ReEvaluationSettingDB().ChangeStatus(ReEvaluationSetting_ID);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.ErrorMessage = null;
                response.SuccessMessage = "Record updated successfully";
                response.IsSuccess = true;
            }
            return response;
        }

        public List<ReEvaluationSetting> List(PrintProgramme printProgramme, bool IsReEvaluation = true, short? SubmittedYear = null, bool WithResultNotificationDetail = false)
        {
            var collection = new ReEvaluationSettingDB().List(printProgramme, IsReEvaluation, SubmittedYear);
            //if (WithResultNotificationDetail)
            //{
            //    if (collection.IsNotNullOrEmpty())
            //        foreach (var item in collection)
            //        {
            //            if (item.ResultNotification_IDs.IsNotNullOrEmpty())
            //            {
            //                item.ResultNotifications = new ResultNotificationDB().GetList(item.ResultNotification_IDs.ToGuidList());
            //            }
            //        }
            //}
            return collection;
        }

    }
}
