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
    public class ExaminationSettingManager
    {
        public ExaminationSetting GetExaminationSetting(Guid id)
        {
            var ExaminationSetting = new ExaminationSettingDB().GetExaminationSetting(id);

            return ExaminationSetting;
        }
        public ExaminationSetting GetExaminationSetting(string FormNumberPrefix, ExaminationCourseCategory courseCat, Semester Semester)
        {
            var ExaminationSetting = new ExaminationSettingDB().GetExaminationSetting(FormNumberPrefix, courseCat, Semester);
            return ExaminationSetting;
        }
        public List<ExaminationSetting> GetExaminationSettings(Parameters parameter)
        {
            return new ExaminationSettingDB().GetExaminationSettings(parameter);
        }
        public ResponseData Save(ExaminationSetting setting)
        {
            var settingdb = new ExaminationSettingDB();
            if (settingdb.ExaminationSettingExists(setting.CourseCategory, setting.Semester, setting.Year, setting.IsBacklogSetting))
                return new ResponseData() { ErrorMessage = "Examination setting already exists" };

            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            setting.ARGExamForm_ID = Guid.NewGuid();

            if (setting.ValidateByMinMaxFee)
            {
                if (setting.MinimumFee <= 0)
                {
                    response.ErrorMessage = "when Validate By MinMax Fee is checked then MinimumFee should be greater than 0";
                    return response;
                }
                if (setting.MaximumFee <= 0)
                {
                    response.ErrorMessage = "when Validate By MinMax Fee is checked then MaximumFee should be greater than 0";
                    return response;
                }
            }

            response.NumberOfRecordsEffected = settingdb.Save(setting);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = setting;
                response.ErrorMessage = "";
            }
            return response;
        }

        public ResponseData AllowExamFormApplications(DateTime startDate, DateTime endDate, Guid notification_ID, Guid ARGExamForm_ID)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };

            if (startDate < DateTime.Now.AddHours(-1))
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Invalid Start Date";
                return response;
            }

            if (endDate == DateTime.MinValue)
            {
                response.IsSuccess = false;
                response.ErrorMessage = "Invalid End Date";
                return response;
            }

            if (notification_ID == Guid.Empty)
            {

            }

            if (new ExaminationSettingDB().AllowExamFormApplications(startDate, endDate, notification_ID, ARGExamForm_ID) > 0)
            {
                response.IsSuccess = true;
                response.SuccessMessage = "Successfully allowed the Examination Form Applications";
            }

            return response;
        }

        public ResponseData Update(ExaminationSetting setting)
        {
            var settingdb = new ExaminationSettingDB();
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            //setting.GracePeriodDays1 = 0;
            //setting.GracePeriodDays2 = 0;
            //setting.IsGracePeriod = false;

            if (setting.ValidateByMinMaxFee)
            {
                if (setting.MinimumFee <= 0)
                {
                    response.ErrorMessage = "when Validate By MinMax Fee is checked then MinimumFee should be greater than 0";
                    return response;
                }
                if (setting.MaximumFee <= 0)
                {
                    response.ErrorMessage = "when Validate By MinMax Fee is checked then MaximumFee should be greater than 0";
                    return response;
                }
            }

            response.NumberOfRecordsEffected = settingdb.Update(setting);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = GetExaminationSetting(setting.ARGExamForm_ID);
                response.ErrorMessage = "";
            }
            return response;
        }



        public ResponseData Delete(Guid ExaminationSetting_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = new ExaminationSettingDB().Delete(ExaminationSetting_ID);
            if (response.NumberOfRecordsEffected == 0)
            {
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }

        public IEnumerable<SelectListItem> GetExamFeeStructure()
        {
            return new ExaminationFormDB().GetExamFeeStructure();
        }
    }
}
