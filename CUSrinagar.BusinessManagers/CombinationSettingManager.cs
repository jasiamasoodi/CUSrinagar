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
    public class CombinationSettingManager
    {
        #region CombinationSetting
        public int GetTotalStudentsAlloted(Guid Course_ID, Guid AcceptCollege_ID, short batch, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            return new CombinationSettingDB().GetTotalStudentsAlloted(Course_ID, AcceptCollege_ID, batch, printProgramme);
        }

        public CombinationSetting GetCombinationSetting(Guid id)
        {
            var combinationSetting = new CombinationSettingDB().GetCombinationSetting(id);
            if (combinationSetting != null && combinationSetting.CompulsarySubjects.IsNotNullOrEmpty())
            {
                combinationSetting.CompulsarySubject = new SubjectDB().GetList(combinationSetting.CompulsarySubjects.ToGuidList());
            }
            return combinationSetting;
        }

        public ResponseData ToggleCollegeChangeCombination(List<Guid> combinationSetting_IDs)
        {
            ResponseData response = new ResponseData() { ErrorMessage = "No records updated" };
            response.NumberOfRecordsEffected = new CombinationSettingDB().ToggleCollegeChangeCombination(combinationSetting_IDs);
            if (response.NumberOfRecordsEffected > 0)
                response.SuccessMessage = "Update successfully";
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }

        public CombinationSetting GetCombinationSetting(Guid Course_ID, short Semester, short batch)
        {
            var model = new CombinationSettingDB().GetCombinationSetting(Course_ID, Semester, batch);
            if (model != null)
            {
                if (model.CombinationSettingStructure_ID != Guid.Empty)
                    model.CombinationSettingStructure = GetCombinationSettingStructure(model.CombinationSettingStructure_ID);
                if (model.CompulsarySubjects.IsNotNullOrEmpty())
                    model.CompulsarySubject = new SubjectDB().GetList(model.CompulsarySubjects.ToGuidList());
            }
            return model;
        }
        public List<CombinationSetting> GetCombinationSettings(Parameters parameter)
        {
            var collection = new CombinationSettingDB().GetCombinationSettings(parameter);
            if(collection.IsNotNullOrEmpty())
                foreach (var item in collection)
                {
                    if (item.CombinationSettingStructure_ID != Guid.Empty)
                        item.CombinationSettingStructure = GetCombinationSettingStructure(item.CombinationSettingStructure_ID);
                }
            return collection;
        }
        public ResponseData Save(CombinationSetting setting)
        {
            var settingdb = new CombinationSettingDB();
            if (settingdb.CombinationSettingExists(setting.Batch, setting.Course_ID, setting.Semester))
                return new ResponseData() { ErrorMessage = "Combination setting already exists" };

            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            setting.CombinationSetting_ID = Guid.NewGuid();
            //setting.TotalCreditSet = setting.TotalCredit > 0;
            setting.SetWorkFlow(RecordState.New);
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
        public ResponseData Update(CombinationSetting setting)
        {
            var settingdb = new CombinationSettingDB();
            //setting.TotalCreditSet = setting.TotalCredit > 0;
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            setting.SetWorkFlow(RecordState.Old);
            response.NumberOfRecordsEffected = settingdb.Update(setting);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = GetCombinationSetting(setting.CombinationSetting_ID);
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Delete(Guid CombinationSetting_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            response.NumberOfRecordsEffected = new CombinationSettingDB().Delete(CombinationSetting_ID);
            if (response.NumberOfRecordsEffected == 0)
            {
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }
        #endregion


        #region SubjectCombinationSetting
        public SubjectCombinationSetting GetSubjectCombinationSetting(Guid id)
        {
            var SubjectCombinationSetting = new CombinationSettingDB().SubjectCombinationSetting(id);
            return SubjectCombinationSetting;
        }

        public List<SubjectCombinationSetting> SubjectCombinationSettingList(Guid _ID)
        {
            List<SubjectCombinationSetting> list = null;
            var _BaseSubjectSetting = new CombinationSettingDB().SubjectCombinationSetting(_ID);
            if (_BaseSubjectSetting != null)
            {
                list = new List<SubjectCombinationSetting>();
                list.Add(_BaseSubjectSetting);
                var SubjectCombinationSetting = new CombinationSettingDB().SubjectCombinationSettingList(_BaseSubjectSetting.BaseSubject_ID, _BaseSubjectSetting.ForSemester);
                if (SubjectCombinationSetting.IsNotNullOrEmpty())
                    list.AddRange(SubjectCombinationSetting);
            }
            return list;
        }

        public List<SubjectCombinationSetting> SubjectCombinationSettingList(Guid BaseSubject_ID, short ForSemester)
        {
            var SubjectCombinationSetting = new CombinationSettingDB().SubjectCombinationSettingList(BaseSubject_ID, ForSemester);
            return SubjectCombinationSetting;
        }
        public SubjectCombinationSetting GetSubjectCombinationSetting(Guid Course_ID, short Semester, short batch)
        {
            var SubjectCombinationSetting = new CombinationSettingDB().GetSubjectCombinationSetting(Course_ID, Semester, batch);
            return SubjectCombinationSetting;
        }
        public List<SubjectCombinationSetting> GetSubjectCombinationSettings(Parameters parameter)
        {
            return new CombinationSettingDB().GetSubjectCombinationSettings(parameter);
        }
        public ResponseData Save(SubjectCombinationSetting setting)
        {
            var settingdb = new CombinationSettingDB();
            if (settingdb.SubjectCombinationSettingExists(setting.BaseSubject_ID, setting.ForSemester))
                return new ResponseData() { ErrorMessage = "Subject Combination setting already exists" };

            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            setting._ID = Guid.NewGuid();
            response.NumberOfRecordsEffected = settingdb.Save(setting);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = SubjectCombinationSettingList(setting._ID);
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Update(SubjectCombinationSetting setting)
        {
            var settingdb = new CombinationSettingDB();
            var response = new ResponseData() { ErrorMessage = "No change" };
            if (settingdb.SubjectCombinationSettingExists(setting.BaseSubject_ID, setting.ForSemester, setting._ID))
                return new ResponseData() { ErrorMessage = "Subject Combination setting already exists" };

            SubjectCombinationSetting dbsetting = settingdb.SubjectCombinationSetting(setting._ID);
            response.NumberOfRecordsEffected += settingdb.Update(setting, dbsetting);
            setting.Dated = DateTime.Now;
            response.NumberOfRecordsEffected += settingdb.Update(setting);
            if (response.NumberOfRecordsEffected >= 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = SubjectCombinationSettingList(setting._ID);
                response.ErrorMessage = "";
            }
            return response;
        }

        public ResponseData SaveUpdate(List<SubjectCombinationSetting> list, Guid MainBaseItem_ID)
        {
            var settingdb = new CombinationSettingDB();
            var response = new ResponseData() { ErrorMessage = "No change " };
            if (list.IsNotNullOrEmpty())
            {
                var list1 = list.Where(x => x.RecordState != RecordState.Old && x.DependentSubject_ID.HasValue)?.ToList();
                if (list1.IsNotNullOrEmpty())
                    foreach (var item in list1)
                    {
                        var dbsubject = new SubjectsManager().Get(item.DependentSubject_ID.Value);
                        if ((short)dbsubject.Semester == item.ForSemester)
                        {
                            if (item._ID == Guid.Empty)
                            {
                                item._ID = Guid.NewGuid();
                                response.NumberOfRecordsEffected += settingdb.Save(item);
                            }
                            else
                            {
                                response.NumberOfRecordsEffected += settingdb.Update(item);
                            }
                        }
                    }
            }
            if (response.NumberOfRecordsEffected >= 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = SubjectCombinationSettingList(MainBaseItem_ID);
                response.ErrorMessage = "";
            }
            return response;
        }

        public ResponseData DeleteBaseSubjectCombinationSetting(Guid _ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to delete record" };
            var dbsubjectSetting = new CombinationSettingManager().GetSubjectCombinationSetting(_ID);
            response.NumberOfRecordsEffected = new CombinationSettingDB().DeleteBaseSubjectCombinationSetting(dbsubjectSetting.BaseSubject_ID, dbsubjectSetting.ForSemester);
            if (response.NumberOfRecordsEffected >= 1)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }

        public ResponseData DeleteSubjectCombinationSetting(Guid _ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to delete record" };
            response.NumberOfRecordsEffected = new CombinationSettingDB().DeleteSubjectCombinationSetting(_ID);
            if (response.NumberOfRecordsEffected >= 1)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }


        //public ResponseData DeleteSubjectCombinationSetting(Guid _ID)
        //{
        //    var response = new ResponseData() { ErrorMessage = "Failed to delete record" };
        //    var dbsubjectSetting = new CombinationSettingManager().GetSubjectCombinationSetting(_ID);
        //    response.NumberOfRecordsEffected = new CombinationSettingDB().DeleteBaseSubjectCombinationSetting(dbsubjectSetting.BaseSubject_ID, dbsubjectSetting.ForSemester);
        //    if (response.NumberOfRecordsEffected >= 1)
        //    {
        //        response.ErrorMessage = "";
        //        response.SuccessMessage = "Record deleted successfully";
        //        response.IsSuccess = true;
        //    }
        //    return response;
        //}
        //public ResponseData DeleteBaseSubjectCombinationSetting(Guid BaseSubject_ID, short Batch, short ForSemester)
        //{
        //    var response = new ResponseData() { ErrorMessage = "Failed to delete record" };
        //    response.NumberOfRecordsEffected = new CombinationSettingDB().DeleteSubjectCombinationSetting(BaseSubject_ID, Batch, ForSemester);
        //    if (response.NumberOfRecordsEffected >= 1)
        //    {
        //        response.ErrorMessage = "";
        //        response.SuccessMessage = "Record deleted successfully";
        //        response.IsSuccess = true;
        //    }
        //    return response;
        //}

        #endregion

        #region CombinationSettingStructure
        public List<CombinationSettingStructure> GetCombinationSettingStructureList(Parameters parameter)
        {
            return new CombinationSettingDB().CombinationSettingStructureList(parameter);
        }
        public CombinationSettingStructure GetCombinationSettingStructure(Guid id)
        {
            var model = new CombinationSettingDB().GetCombinationSettingStructure(id);
            return model;
        }
        public ResponseData Save(CombinationSettingStructure model)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            model.CombinationSettingStructure_ID = Guid.NewGuid();
            model.CreatedBy = AppUserHelper.User_ID;
            model.CreatedOn = DateTime.Now;
            response.NumberOfRecordsEffected = new CombinationSettingDB().Save(model);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = GetCombinationSettingStructure(model.CombinationSettingStructure_ID);
                response.ErrorMessage = "";
            }
            return response;
        }
        public ResponseData Update(CombinationSettingStructure model)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            model.CreatedBy = AppUserHelper.User_ID;
            model.CreatedOn = DateTime.Now;
            response.NumberOfRecordsEffected = new CombinationSettingDB().Update(model);
            if (response.NumberOfRecordsEffected == 1)
            {
                response.SuccessMessage = "Record saved successfully";
                response.IsSuccess = true;
                response.ResponseObject = GetCombinationSettingStructure(model.CombinationSettingStructure_ID);
                response.ErrorMessage = "";
            }
            return response;
        }

        public List<SelectListItem> GetCombinationSettingStructureSelectList(Parameters parameter)
        {
            var collection = new CombinationSettingDB().CombinationSettingStructureList(parameter);
            List<SelectListItem> list = new List<SelectListItem>();
            if (collection != null)
                foreach (var item in collection)
                    list.Add(new SelectListItem()
                    {
                        Text = $@"{ "" + (!string.IsNullOrEmpty(item.Remark) ? "Remark:" + item.Remark : "")
                                   + " |" + (item.TotalNumberOfSubjects > 0 ? "Subjects:" + item.TotalNumberOfSubjects.ToString() : "")
                                   + " |" + (item.TotalSubjectCredits > 0 ? "SubjectCredits:" + item.TotalSubjectCredits.ToString() : "")
                                   + " |" + (item.Core_Count > 0 ? "Core_Credit:" + item.Core_Credit.ToString() : "")
                                   + " |" + (item.Core_Credit > 0 ? "Core_Count:" + item.Core_Count.ToString() : "")
                                   + " |" + (item.DCE_DSE_Count > 0 ? "DCE_DSE_Count:" + item.DCE_DSE_Count.ToString() : "")
                                   + " |" + (item.DCE_DSE_Credit > 0 ? "DCE_DSE_Credit:" + item.DCE_DSE_Credit.ToString() : "")
                                   + " |" + (item.GE_OE_Count > 0 ? "GE_OE_Count:" + item.GE_OE_Count.ToString() : "")
                                   + " |" + (item.GE_OE_Credit > 0 ? "GE_OE_Credit:" + item.GE_OE_Credit.ToString() : "")
                                   + " |" + (item.SEC_Count > 0 ? "SEC_Count:" + item.SEC_Count.ToString() : "")
                                   + " |" + (item.SEC_Credit > 0 ? "SEC_Credit:" + item.SEC_Credit.ToString() : "")
                                   + " |" + (item.MIL_Count > 0 ? "MIL_Count:" + item.MIL_Count.ToString() : "")
                                   + " |" + (item.MIL_Credit > 0 ? "MIL_Credit:" + item.MIL_Credit.ToString() : "")
                                   + " |" + (item.AE_Count > 0 ? "AE_Count:" + item.AE_Count.ToString() : "")
                                   + " |" + (item.AE_Credit > 0 ? "AE_Credit:" + item.AE_Credit.ToString() : "")}",
                        Value = item.CombinationSettingStructure_ID.ToString()
                    });
            return list;

        }

        #endregion
    }
}
