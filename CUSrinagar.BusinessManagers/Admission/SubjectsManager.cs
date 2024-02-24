using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using GeneralModels;
using System.Activities.Statements;

namespace CUSrinagar.BusinessManagers
{
    public class SubjectsManager
    {
        public List<ADMSubjectMaster> GetList(Guid Course_ID, int combinationSemester = 0)
        {
            return new SubjectDB().GetList(Course_ID, combinationSemester);
        }
        public List<ADMSubjectMaster> GetList(Guid Course_ID, List<Guid> subject_IDs)
        {
            return new SubjectDB().GetList(Course_ID, subject_IDs);
        }
        public List<ADMSubjectMaster> GetList(List<Guid> subject_IDs)
        {
            if (subject_IDs.IsNullOrEmpty())
                return null;
            return new SubjectDB().GetList(subject_IDs);
        }

        public List<ADMSubjectMasterCompact> List(Parameters parameter)
        {
            return new SubjectDB().List(parameter);
        }

        public List<ADMSubjectMaster> GetAllSubjects(Parameters parameter)
        {
            List<ADMSubjectMaster> listSubjects = new SubjectDB().GetAllSubjects(parameter);
            return listSubjects;
        }

        //Get All Subjects whether passed or failed(resultnotification id or not) for Resultview
        public List<ADMSubjectMaster> GetAllSubjectsSemester(string SearchText, int semester, PrintProgramme Programme)
        {
            List<ADMSubjectMaster> listSubjects = new SubjectDB().GetAllSubjectsSemester(SearchText, semester, Programme);
            return listSubjects;
        }

        public List<SubjectCompact> SubjectListCompact(Parameters parameter)
        {
            return new SubjectDB().SubjectListComact(parameter);
        }

        public Parameters GetDefaultParameter(Parameters parameter)
        {
            parameter.SortInfo = new Sort() { ColumnName = "CreatedOn", OrderBy = SortOrder.Descending };
            if (parameter.Filters == null)
                parameter.Filters = new List<SearchFilter>();
            parameter.Filters.AddRange(GetDefaultSearchFilter());

            return parameter;
        }
        public List<SearchFilter> GetDefaultSearchFilter()
        {
            List<SearchFilter> listfilter = new List<SearchFilter>();
            SearchFilter defaultFilter = new SearchFilter() { Column = "Status", Operator = Enums.SQLOperator.EqualTo, Value = "true", GroupOperation = Enums.LogicalOperator.AND };
            listfilter.Add(defaultFilter);
            return listfilter;
        }

        public Guid GetSubjectsCourse(Guid subject_ID)
        {
            return (new SubjectDB().GetSubjectsCourse(subject_ID) ?? new ADMSubjectMaster()).Course_ID ?? Guid.Empty;
        }

        public SelectList GetListDDL(Guid Course_ID, bool _withCompulsary = false)
        {
            return new SelectList(new SubjectDB().GetListDDL(Course_ID, _withCompulsary), "Value", "Text");
        }

        public SelectList GetListDDL(Guid Course_ID, int Semester, bool _withCompulsary = false)
        {
            return new SelectList(new SubjectDB().GetListDDL(Course_ID, Semester, _withCompulsary) ?? new List<DropDown>(), "Value", "Text");
        }

        #region old
        public List<SelectListItem> GetOldCollegeSubjectList()
        {
            return new SubjectDB().GetOldCollegeSubjectList();
        }

        //[Obsolete("Use another method - GetCombinationByCollege", false)]
        //public List<SubjectCombinations> GetCombinationList(Parameters parameter, string courseCode, string collegeCode)
        //{
        //    return new SubjectDB().GetCombinationList(parameter, courseCode, collegeCode);
        //}
        #endregion
        public bool checkSubjectExists(ADMSubjectMaster input, string columnName, bool IsEdit)
        {
            string columnValue = input.GetType().GetProperty(columnName).GetValue(input, null).ToString();
            return new SubjectDB().ExistSubject(columnName, columnValue, input, IsEdit) != null;

        }
        public int AddSubject(ADMSubjectMaster input)
        {
            int result = 0;
            input.Subject_ID = Guid.NewGuid();
            input.Status = true;
            input.SetWorkFlow(RecordState.New);
            result = new SubjectDB().AddSubject(input);
            return result;
        }
        //public ADMSubjectMaster Get(Guid id)
        //{
        //    return new SubjectDB().GetSubjectById(id);
        //}

        public ADMSubjectMaster GetOldSubjectById(Guid id)
        {
            return new SubjectDB().GetOldSubjectById(id);
        }

        public int EditSubject(ADMSubjectMaster input)
        {
            int result = 0;

            List<string> ignoreQuery = new List<string>() {

                nameof(input.Subject_ID),
                nameof(input.CreatedOn),
                nameof(input.CreatedBy),
                nameof(input.Status),
                nameof(input.Course_ID)

            };
            input.SetWorkFlow(RecordState.Old);
            result = new SubjectDB().EditSubject(input, ignoreQuery, null);

            return result;
        }

        public ResponseData Delete(Guid subject_ID)
        {
            var dbSubject = new SubjectDB().Get(subject_ID);
            if (new SubjectDB().HasStudentsAssociated(subject_ID))
                return new ResponseData() { ErrorMessage = "Has students associated, hence can't be deleted." };
            if (new SubjectDB().HasSyllabus(subject_ID))
                return new ResponseData() { ErrorMessage = "Has syllabus associated, hence can't be deleted." };
            if (new SubjectDB().HasResult(dbSubject.Programme, (short)dbSubject.Semester, subject_ID))
                return new ResponseData() { ErrorMessage = "Has result associated, hence can't be deleted." };
            ResponseData response = new ResponseData();
            response.NumberOfRecordsEffected = new SubjectDB().Delete(subject_ID);
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            response.SuccessMessage = response.IsSuccess ? " Record deleted successfully" : "";
            return response;
        }

        public string ChangeStatus(Guid id)
        {
            string msg = string.Empty;
            ADMSubjectMaster subject = new SubjectsManager().Get(id);
            if (subject != null)
            {


                string deactivationMessage = subject.SubjectFullName + " Syllabus Deactivated!";
                string activationMessage = subject.SubjectFullName + " Syllabus Activated!";
                subject.Status = !subject.Status;
                msg = subject.Status ? activationMessage : deactivationMessage;
                // subject.UpdatedOn = DateTime.Now;
                //    List<string> ignoreQuery = new List<string>() {

                //    nameof(subject.Subject_ID)
                //};
                var count = new SubjectDB().UpdateSubjectStatus(subject.Status, subject.Subject_ID);
                //new SubjectDB().EditSubject(subject, ignoreQuery, null);
            }
            return msg;
        }
        public List<SelectListItem> FetchChildDDlValues(string id, string Type)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Course":
                    {
                        int ProgrammeId = Convert.ToInt32(id);
                        childDDL = new CourseManager().GetAllCoursesByProgramme(ProgrammeId);
                        break;
                    }



            }


            return childDDL;
        }

        public List<SelectListItem> GetAllSubjects(Guid Course_Id)
        {
            return new SubjectDB().GetAllSubjects(Course_Id);
        }

        public List<SelectListItem> DepartmentDataList(string SchoolFullName)
        {
            return new SubjectDB().DepartmentDataList(SchoolFullName);
        }
        public List<SelectListItem> SchoolSelectList(Guid? College_ID)
        {
            return new SubjectDB().SchoolSelectList(College_ID);
        }
        public List<SelectListItem> SubjectDDL(Parameters parameters)
        {
            return new SubjectDB().SubjectDDL(parameters) ?? new List<SelectListItem>();
        }
        /// <summary>
        /// Subject name suffixed with subject type
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public List<SelectListItem> SubjectDDLWithDetail(Parameters parameters)
        {
            var list = new SubjectDB().SubjectListComact(parameters);
            List<SelectListItem> subjectlist = new List<SelectListItem>();
            if (list != null)
            {
                foreach (var item in list)
                {
                    subjectlist.Add(new SelectListItem() { Text = $"{(string.IsNullOrEmpty(item.CollegeCode) ? "" : item.CollegeCode + ": ")}{item.SubjectFullName}({item.SubjectType.ToString()})-{item.DepartmentFullName} Sem-{((short)item.Semester)}:Credit-{item.TotalCredit}:[{item.FromBatch}-{item.ToBatch}]-{item.CourseFullName}", Value = item.Subject_ID.ToString() });
                }
            }
            return subjectlist;
        }
        public List<SelectListItem> VWSCSubjectDDLWithDetail(Parameters parameters)
        {
            var list = new SubjectDB().VWSCSubjectListComact(parameters);
            List<SelectListItem> subjectlist = new List<SelectListItem>();
            if (list.IsNotNullOrEmpty())
            {
                var collection = list.Select(x => x.Subject_ID).Distinct().ToList<Guid>();
                foreach (var _ID in collection)
                {
                    var items = list.Where(x => x.Subject_ID == _ID).ToList();
                    subjectlist.Add(new SelectListItem()
                    {
                        Text = $@"{(items.Any(x => !string.IsNullOrEmpty(x.CollegeCode)) ? items.FirstOrDefault()?.CollegeCode + ": " : "")}
                        {items.First().SubjectFullName} 
                        ({items.First().SubjectType.ToString()})-
                        {items.First().DepartmentFullName} 
                        {Helper.GetEnumDescription(items.First().Semester)}:
                        Credit-{items.First().TotalCredit}"//:[{items.First().FromBatch}-{items.First().ToBatch}]-
                        //{string.Join(",", items.Select(x => x.CourseFullName).ToArray())}"
                        ,
                        Value = items.First().Subject_ID.ToString()
                    });
                }
            }
            return subjectlist?.ToList();//?.DistinctBy(x => x.Text)?.ToList();
        }
        public List<SelectListItem> GetAllSubjects(Guid Course_Id, int Semester, bool checkcollege)
        {
            return new SubjectDB().GetAllSubjects(Course_Id, Semester, checkcollege);
        }
        public List<SelectListItem> GetAllSubjects(string CourseFullName, Guid College_ID)
        {
            return new SubjectDB().GetAllSubjects(CourseFullName, College_ID);
        }
        public List<SelectListItem> SubjectSelectList(Parameters parameter)
        {
            return new SubjectDB().SubjectSelectList(parameter);
        }

        public string GetSubjectCode(ADMSubjectMaster SubjectMaster)
        {
            string code = string.Empty;
            try { code = new SubjectDB().GetSubjectCode(SubjectMaster); }
            catch (Exception) { }
            return code;
        }

        public string GetSubjectNumber(Guid Course_Id, int Semester)
        {
            string code = string.Empty;
            try { code = new SubjectDB().GetSubjectNumber(Course_Id, Semester); }
            catch (Exception) { }
            return code;
        }

        public string GetSubjectNumber(Guid? course_Id, int semester, SubjectType subjectType, Programme programme, Guid? College_ID = null)
        {
            return new SubjectDB().GetSubjectNumber(course_Id, semester, subjectType, programme, College_ID).ToString();
        }

        public IEnumerable<ADMSubjectMaster> GetSubjectByType(Guid college_ID, SubjectType SubjectType, short? semester)
        {
            List<ADMSubjectMaster> listSubjects = new SubjectDB().GetSubjectByType(college_ID, SubjectType, semester);
            return listSubjects;
        }

        #region SubjectInfo
        public List<SubjectDetails> SubjectInfo()
        {
            return new SubjectDB().SubjectInfo();
        }

        public List<SelectListItem> FetchChildDDlValuesByCollegeID(string ID, string Type)
        {
            List<SelectListItem> childDDL = new List<SelectListItem>();
            switch (Type)
            {
                case "Course":
                    {

                        Guid CollegeID = Guid.NewGuid();//convert string to guid                        
                        CollegeID = new Guid(ID);
                        childDDL = new CourseManager().GetAllCourseListByCollegeID(CollegeID);
                        childDDL.AddRange(new CourseManager().GetOtherSubjectCourse() ?? new List<SelectListItem>());
                        break;
                    }

                case "Semester":
                    {
                        Guid CourseId = new Guid(ID);
                        childDDL = new SyllabusManager().GetAllSemesters(CourseId);
                        break;
                    }

            }


            return childDDL;
        }

        public SearchFilter GetDefaultSearchFilterForSubjectInfo()
        {
            SearchFilter defaultFilter = new SearchFilter() { Column = "Status", Operator = Enums.SQLOperator.EqualTo, Value = "true", GroupOperation = Enums.LogicalOperator.AND };
            return defaultFilter;
        }

        public List<ADMSubjectMaster> GetAllSubjectList(Parameters parameter)
        {
            List<ADMSubjectMaster> listSyllabus = new SubjectDB().GetAllSubjectList(parameter);
            return listSyllabus;
        }

        public List<SubjectDetails> GetAllSubjectDetails(Parameters parameter)
        {
            List<SubjectDetails> listSubject = new SubjectDB().GetAllSubjectDetails(parameter);
            return listSubject;

        }

        public ADMSubjectMaster Get(Guid Subject_ID)
        {
            return new SubjectDB().Get(Subject_ID);
        }
        public ADMSubjectMaster Get(Programme programme, Guid? Course_ID, short Semester, SubjectType subjectType)
        {
            return new SubjectDB().Get(programme, Course_ID, Semester, subjectType);
        }
        public ResponseData Save(ADMSubjectMaster model)
        {
            var subjectDB = new SubjectDB();
            if (model == null)
                return new ResponseData() { ErrorMessage = "Invalid details" };

            if ((model.SubjectType == SubjectType.SEC || model.SubjectType == SubjectType.GE || model.SubjectType == SubjectType.OE) && (!model.College_ID.HasValue || new CollegeManager().GetItem(model.College_ID.Value) == null))
                return new ResponseData() { ErrorMessage = "College is missing" };

            if (model.SubjectType == SubjectType.GE || model.SubjectType == SubjectType.OE || model.SubjectType == SubjectType.SEC)
            {
                if (!model.College_ID.HasValue)
                    return new ResponseData() { ErrorMessage = "College is required for subject type" + model.SubjectType.ToString() };
                if (subjectDB.SubjectNameAlreadyExists(model.Programme, model.College_ID.Value, model.Semester, model.SubjectFullName, model.Subject_ID))
                    return new ResponseData() { ErrorMessage = "Subject Name Already Exist" };
            }
            else if (subjectDB.SubjectNameAlreadyExists(model.Course_ID.Value, model.Semester, model.SubjectFullName))
                return new ResponseData() { ErrorMessage = "Subject Name Already Exist" };

            //if (subjectDB.SubjectCodeAlreadyExists(model.SubjectCode))
            //    return new ResponseData() { ErrorMessage = "Subject Code Already Exist" };

            ResponseData response = new ResponseData() { ErrorMessage = "Unable to update record." };
            model.Subject_ID = Guid.NewGuid();
            model.SetWorkFlow(RecordState.New);
            //model.Department_ID = SaveUpdateDepartment(model.SchoolFullName, model.DepartmentFullName)?.Department_ID;
            response.NumberOfRecordsEffected = subjectDB.Save(model);

            if (response.NumberOfRecordsEffected == -2601)
            {
                return new ResponseData() { ErrorMessage = "Subject Code Already Exist" };
            }

            if (response.NumberOfRecordsEffected == 1)
            {
                response.ErrorMessage = null;
                response.SuccessMessage = "Record saved successfully.";
                response.IsSuccess = true;
                response.ResponseObject = subjectDB.Get(model.Subject_ID);
            }
            return response;
        }

        public ResponseData Update(ADMSubjectMaster model)
        {
            var SUBJECTDB = new SubjectDB();
            var subjectdb = SUBJECTDB.Get(model?.Subject_ID ?? Guid.Empty);
            if (subjectdb == null)
                return new ResponseData() { ErrorMessage = "Subject not found." };

            if ((model.SubjectType == SubjectType.SEC || model.SubjectType == SubjectType.GE || model.SubjectType == SubjectType.OE) && (!model.College_ID.HasValue || new CollegeManager().GetItem(model.College_ID.Value) == null))
                return new ResponseData() { ErrorMessage = "College is missing" };

            if (model.SubjectType == SubjectType.GE || model.SubjectType == SubjectType.OE || model.SubjectType == SubjectType.SEC)
            {
                if (!model.College_ID.HasValue)
                    return new ResponseData() { ErrorMessage = "College is required for subject type" + model.SubjectType.ToString() };
                if (SUBJECTDB.SubjectNameAlreadyExists(model.Programme, model.College_ID.Value, model.Semester, model.SubjectFullName, model.Subject_ID))
                    return new ResponseData() { ErrorMessage = "Subject Name Already Exist" };
            }
            else if (SUBJECTDB.SubjectNameAlreadyExists(model.Course_ID.Value, model.Semester, model.SubjectFullName, model.Subject_ID))
                return new ResponseData() { ErrorMessage = "Subject Name Already Exist" };


            //if (SUBJECTDB.SubjectCodeAlreadyExists(model.SubjectCode, model.Subject_ID))
            //    return new ResponseData() { ErrorMessage = "Subject Code Already Exist" };

            ResponseData response = new ResponseData() { ErrorMessage = "Unable to update record." };

            model.SetWorkFlow(RecordState.Old);
            //model.Department_ID = SaveUpdateDepartment(model.SchoolFullName, model.DepartmentFullName)?.Department_ID;
            response.NumberOfRecordsEffected = new SubjectDB().Update(model);

            if (response.NumberOfRecordsEffected == -2601)
            {
                return new ResponseData() { ErrorMessage = "Subject Code Already Exist" };
            }

            if (response.NumberOfRecordsEffected == 1)
            {
                response.ErrorMessage = "";
                response.SuccessMessage = "Record saved successfully.";
                response.IsSuccess = true;
                response.ResponseObject = Get(model.Subject_ID);
            }
            return response;
        }

        public List<ADMSubjectMaster> GetSkillSubjectsByCollege(Guid Id, int semester = 0)
        {
            List<ADMSubjectMaster> listSubjects = new SubjectDB().GetSkillSubjectsByCollege(Id, semester);
            return listSubjects;
        }
        public SelectList GetSubjectsOf(Guid Course_ID, Guid College_ID, short semester, Programme programme, string[] SubjectType)
        {
            string programmes = ((int)programme).ToString();
            if (programme == Programme.UG || programme == Programme.IG || programme == Programme.HS || programme == Programme.Professional)
            { programmes = (int)Programme.UG + "," + (int)Programme.IG + "," + (int)Programme.HS + "," + (int)Programme.Professional; }
            var list = new SubjectDB().GetSubjectsOf(Course_ID, College_ID, semester, programmes, SubjectType);
            List<SelectListItem> subjectlist = new List<SelectListItem>();
            if (list != null)
            {
                foreach (var item in list)
                {
                    subjectlist.Add(new SelectListItem() { Text = $"{item.SubjectFullName} ({item.SubjectType.ToString()})-{item.DepartmentFullName}", Value = item.Subject_ID.ToString() });
                }
            }
            SelectList ll = new SelectList(subjectlist, "Value", "Text");
            return ll;
        }

        #endregion


        #region DeparmentSection
        public Department GetDepartment(Guid Department_ID)
        {
            return new SubjectDB().GetDepartment(Department_ID);
        }
        public Department GetDepartment(string SchoolFullName, string DepartmentFullName)
        {
            return new SubjectDB().GetDepartment(SchoolFullName, DepartmentFullName);
        }

        private Department SaveUpdateDepartment(string SchoolFullName, string DepartmentFullName)
        {
            Department model = new SubjectDB().GetDepartment(SchoolFullName, DepartmentFullName);
            if (model == null)
            {
                model = new Department() { Department_ID = Guid.NewGuid(), SchoolFullName = SchoolFullName, DepartmentFullName = DepartmentFullName };
                Save(model);
            }
            return new SubjectDB().GetDepartment(SchoolFullName, DepartmentFullName);
        }
        public int Save(Department model)
        {
            return new SubjectDB().Save(model);
        }
        public int Update(Department model)
        {
            return new SubjectDB().Update(model);
        }
        #endregion


        #region ADMSubjectMasterMarksStructure
        public List<MSSubjectMarksStructure> Structures(Parameters parameter)
        {
            return new SubjectDB().Structures(parameter);
        }
        #endregion


    }
}
