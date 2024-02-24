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
using System.Transactions;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Data;
using GeneralModels;

namespace CUSrinagar.BusinessManagers
{
    public class CombinationManager
    {
        public Task<List<ADMCombinationMaster>> GetADMCombinationMastersForDateSheetClashAsync(Programme programme, short Semester, string Subjects, List<SubjectType> subjectTypes, short examYear)
        {
            return new TaskFactory().StartNew(() => GetADMCombinationMastersForDateSheetClash(programme, Semester, Subjects, subjectTypes, examYear));
        }
        public List<ADMCombinationMaster> GetADMCombinationMastersForDateSheetClash(Programme programme, short Semester, string Subjects, List<SubjectType> subjectTypes, short examYear)
        {
            if (string.IsNullOrWhiteSpace(Subjects))
            {
                return null;
            }

            if (subjectTypes.IsNullOrEmpty())
            {
                subjectTypes = new List<SubjectType>();
            }
            List<string> SubjectsEntered = Subjects.ToListOfStrings('|');

            List<ADMCombinationMaster> combinationMasters =
                new CombinationDB().GetADMCombinationMastersForDateSheetClash(programme, Semester, examYear)
                ?.Where(x => string.IsNullOrWhiteSpace(x.CombinationSubjects) == false)?.ToList();

            if (combinationMasters.IsNotNullOrEmpty())
            {
                SubjectDB _SubjectsManager = new SubjectDB();
                int resultOfSearchCount = 0;

                List<ADMSubjectMaster> combinationSubjects = null;

                foreach (var item in combinationMasters)
                {
                    resultOfSearchCount = 0;

                    combinationSubjects =
                    _SubjectsManager.GetSubjectNamesTypeOnly(item.CombinationSubjects.ToGuidList(), subjectTypes) ?? new List<ADMSubjectMaster>();

                    if (combinationSubjects.IsNotNullOrEmpty())
                    {
                        combinationSubjects.AddRange(
                            _SubjectsManager.GetAdditionalSubjectNamesTypeOnly(programme, Semester, subjectTypes, item.Course_ID.Value)
                            ?? new List<ADMSubjectMaster>());
                    }

                    item.CombinationSubjects = null;

                    combinationSubjects.ForEach(x =>
                    {
                        item.CombinationSubjects += x.SubjectFullName + $" - (<i style='color:#2c70ba'>{x.SubjectType.GetEnumDescription()}</i>) |";
                    });

                    if (!string.IsNullOrWhiteSpace(item.CombinationSubjects))
                    {
                        foreach (string SubjectEntered in SubjectsEntered)
                        {
                            if (item.CombinationSubjects.ToLower().Trim().Contains(SubjectEntered.ToLower().Trim()))
                            {
                                resultOfSearchCount++;
                            }
                            if (resultOfSearchCount > 1)
                                break;
                        }
                        if (resultOfSearchCount < 2)
                        {
                            item.CombinationSubjects = null; //ie no clash found
                        }
                    }
                }
            }
            return combinationMasters?.Where(x => x.CombinationSubjects != null)?.ToList() ?? new List<ADMCombinationMaster>();
        }

        public CombinationHelper GetCombinationsHelper(Guid Course_ID, int combinationSemester)
        {
            ADMCourseMaster aDMCourseMaster = new CourseManager().GetCompactItem(Course_ID);

            List<ADMSubjectMaster> _subjectList = new SubjectsManager().GetList(Course_ID, combinationSemester)?.Where(x => x.IsCompulsory)?.ToList();
            if (aDMCourseMaster == null)
                return new CombinationHelper();

            int _minCombinationSubjects = aDMCourseMaster.MinCombinationSubjects;
            CombinationHelper combinationHelper = new CombinationHelper
            {
                MaxSubjectsAllowed = aDMCourseMaster.MaxCombinationSubjects,
                MinSubjectsAllowed = aDMCourseMaster.MinCombinationSubjects
            };
            combinationHelper.FinalCombinations = new List<BaseCombinationHelper>();
            foreach (var item in _subjectList ?? new List<ADMSubjectMaster>())
            {
                combinationHelper.FinalCombinations.Add(
                    new BaseCombinationHelper
                    {
                        IsCompulsary = item.IsCompulsory,
                        SubjectsName = item.SubjectFullName,// + " - " + item.SubjectCode,
                        Subject_ID = item.Subject_ID
                    }
                 );
            }
            _minCombinationSubjects = _minCombinationSubjects - combinationHelper.FinalCombinations.Count();
            for (int i = 0; i < _minCombinationSubjects; i++)
                combinationHelper.FinalCombinations.Add(new BaseCombinationHelper());

            return combinationHelper;
        }

        public int Save(ADMCombinationMaster aDMCombinationMaster)
        {
            int result = 0;
            if (aDMCombinationMaster == null)
                throw new CUSException("No data submitted");
            if (aDMCombinationMaster.CombinationHelper?.FinalCombinations.IsNullOrEmpty() ?? false)
                throw new CUSException("Submitted Data is not valid");
            if ((new GeneralDDLManager().GetCourseOptLabelDDL(true).FirstOrDefault(x => x.Value == Convert.ToString(aDMCombinationMaster.Course_ID))) == null)
                throw new CUSException("Course does not exist");

            List<ADMSubjectMaster> subjects = new SubjectsManager().GetList(aDMCombinationMaster.Course_ID ?? Guid.Empty);
            if (subjects.IsNullOrEmpty())
                throw new CUSException("Submitted Data is not valid");

            foreach (var item in aDMCombinationMaster.CombinationHelper.FinalCombinations)
            {
                if (subjects.FirstOrDefault(x => x.Subject_ID != item.Subject_ID) == null)
                    throw new CUSException("Subjects are not valid");
            }
            CombinationDB combinationDB = new CombinationDB();
            IEnumerable<Guid?> FinalCombinations =
                aDMCombinationMaster.CombinationHelper.FinalCombinations.Select(i => i.Subject_ID).OrderBy(x => x);
            if (FinalCombinations.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                throw new CUSException("Duplicate Subjects found");

            if ((aDMCombinationMaster?.Semester ?? 0) > 8)
            {
                if (FinalCombinations?.Count() < 1)
                    throw new CUSException("Atleast One subjects should be choosen for the said combination");
            }
            else
            {
                if (FinalCombinations?.Count() < 2)
                    throw new CUSException("Atleast two subjects should be choosen for the said combination");
            }

            aDMCombinationMaster.CombinationSubjects = FinalCombinations.ToSingleString();
            var _dbMasterComb = combinationDB.CombinationAlreadyExists(aDMCombinationMaster.College_ID, aDMCombinationMaster.CombinationSubjects);
            if (_dbMasterComb != null)
            {
                if (_dbMasterComb.Course_ID == aDMCombinationMaster.Course_ID)
                    throw new CUSException($"Combination already exists having code:{_dbMasterComb.CombinationCode}");
            }

            if (combinationDB.CombinationCodeAlreadyExists(aDMCombinationMaster.College_ID, aDMCombinationMaster.CombinationCode))
                throw new CUSException($"Combination Code {aDMCombinationMaster.CombinationCode.ToUpper()} already exists");

            using (var ts = new TransactionScope())
            {
                aDMCombinationMaster.Combination_ID = Guid.NewGuid();
                aDMCombinationMaster.SetWorkFlow(RecordState.New);
                aDMCombinationMaster.Status = true;
                aDMCombinationMaster.College_ID = aDMCombinationMaster.College_ID;
                aDMCombinationMaster.Semester = aDMCombinationMaster.Semester;
                aDMCombinationMaster.CombinationCode = aDMCombinationMaster.CombinationCode.ToUpper();
                result = combinationDB.Save(aDMCombinationMaster);
                ts.Complete();
            }

            return result;
        }

        public ResponseData Delete(Guid Combination_ID)
        {
            var response = new ResponseData() { ErrorMessage = "Failed to save record" };
            var dbCombination = new CombinationDB().GetCombinationByID(Combination_ID);
            if (dbCombination.College_ID != AppUserHelper.College_ID && AppUserHelper.AppUsercompact.UserRoles.Any(x => x == AppRoles.College))
                return new ResponseData() { ErrorMessage = "This combination does not belong to your college" };
            if (new CombinationDB().IsCombinationInUser(Combination_ID))
                return new ResponseData() { ErrorMessage = "Combination in Use" };
            //var programme = new CourseManager().GetCourseById(dbCombination.Course_ID.Value).Programme;
            response.NumberOfRecordsEffected = new CombinationDB().Delete(Combination_ID);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.SuccessMessage = "Record deleted successfully";
                response.IsSuccess = true;
            }
            return response;
        }

        public int Edit(ADMCombinationMaster aDMCombinationMaster, bool iscompact = false)
        {

            int result = 0;
            if (aDMCombinationMaster == null && aDMCombinationMaster.Combination_ID != Guid.Empty)
                throw new CUSException("No data submitted");
            if (aDMCombinationMaster.CombinationHelper?.FinalCombinations.IsNullOrEmpty() ?? false)
                throw new CUSException("Submitted Data is not valid");
            if ((new GeneralDDLManager().GetCourseOptLabelDDL(true).FirstOrDefault(x => x.Value == Convert.ToString(aDMCombinationMaster.Course_ID))) == null)
                throw new CUSException("Course does not exist");

            List<ADMSubjectMaster> subjects = new SubjectsManager().GetList(aDMCombinationMaster.Course_ID ?? Guid.Empty);
            if (subjects.IsNullOrEmpty())
                throw new CUSException("Submitted Data is not valid");
            if (!iscompact)
            {
                foreach (var item in aDMCombinationMaster.CombinationHelper.FinalCombinations)
                {
                    if (subjects.FirstOrDefault(x => x.Subject_ID != item.Subject_ID) == null)
                        throw new CUSException("Subjects are not valid");
                }
            }
            CombinationDB combinationDB = new CombinationDB();
            if (!iscompact)
            {
                IEnumerable<Guid?> FinalCombinations =
                aDMCombinationMaster.CombinationHelper.FinalCombinations.Select(i => i.Subject_ID).OrderBy(x => x);
                if (FinalCombinations.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                    throw new CUSException("Duplicate Subjects found");

                if (FinalCombinations?.Count() < 2)
                    throw new CUSException("Atleast three subjects should be choosen for the said combination");

                aDMCombinationMaster.CombinationSubjects = FinalCombinations.ToSingleString();
            }
            var _dbMasterComb = combinationDB.CombinationAlreadyExists(aDMCombinationMaster.College_ID, aDMCombinationMaster.CombinationSubjects, aDMCombinationMaster.Combination_ID);
            if (_dbMasterComb != null)
                throw new CUSException($"Combination already exists having code:{_dbMasterComb.CombinationCode}");

            if (combinationDB.CombinationCodeAlreadyExists(aDMCombinationMaster.College_ID, aDMCombinationMaster.CombinationCode, aDMCombinationMaster.Combination_ID))
                throw new CUSException($"Combination Code {aDMCombinationMaster.CombinationCode.ToUpper()} already exists");
            //ADMCombinationMaster _ADMCombinationMasterDB = GetCombinationByID(aDMCombinationMaster.Combination_ID);

            // using (var ts = new TransactionScope())
            // {
            aDMCombinationMaster.CombinationCode = aDMCombinationMaster.CombinationCode.ToUpper();
            aDMCombinationMaster.SetWorkFlow(RecordState.Old);
            result = combinationDB.Edit(aDMCombinationMaster);
            //   ts.Complete();
            //}
            return result;
        }

        public ResponseData UpdateByIDs(ADMCombinationMaster aDMCombinationMaster)
        {
            var response = new ResponseData();
            var combinationDB = new CombinationDB();
            var _dbMasterComb = combinationDB.CombinationAlreadyExists(aDMCombinationMaster.College_ID, aDMCombinationMaster.CombinationSubjects, aDMCombinationMaster.Combination_ID);
            if (_dbMasterComb != null)
                return new ResponseData() { ErrorMessage = $"Combination already exists having code:{_dbMasterComb.CombinationCode}" };

            response.NumberOfRecordsEffected = combinationDB.UpdateByIDs(aDMCombinationMaster);
            if (response.NumberOfRecordsEffected > 0)
            {
                response.IsSuccess = true;
                response.ErrorMessage = null;
                response.SuccessMessage = "Records Updated Successfully.";
            }
            return response;
        }
        //public ResponseData BatchUpdateCombinationMaster(BatchUpdateCombinationMaster model)
        //{
        //    var response = new ResponseData();
        //    bool Editable = false;


        //    ADMCombinationMaster aDMCombinationMaster = new CombinationManager().GetCombinationByID(model.comb);
        //    var coursemaster = new CourseManager().GetCourseById(aDMCombinationMaster.Course_ID.Value);
        //    //aDMCombinationMaster.CombinationHelper.SubjectSelectListItems = new SubjectsManager().GetSubjectsOf(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.College_ID, aDMCombinationMaster.Semester.Value, coursemaster.Programme, null);
        //    aDMCombinationMaster.CombinationHelper = new CombinationManager().GetCombinationsHelper(aDMCombinationMaster.Course_ID.Value, aDMCombinationMaster.Semester.Value);




















        //    ADMCombinationMaster dbCombinationMaster = new CombinationManager().GetCombinationByID(model.Combination_ID);

        //    if (!dbCombinationMaster.SubjectsDetails.Any(x => x.Subject_ID == model.FindSubject_ID))
        //        return new ResponseData() { ErrorMessage = "Corresponding subjects not found in this particular combination" };

        //    var dbCourseMaster = new CourseManager().GetCourseById(dbCombinationMaster.Course_ID.Value);

        //    var _cs = dbCombinationMaster.CombinationSubjects.ToLower();
        //    if (!model.ReplaceCombinationSubject_IDWith.IsNullOrEmpty() && dbCombinationMaster.CombinationSubjects.ToLower().Contains(model.ReplaceCombinationSubject_IDWith.ToString().ToLower()))
        //    {
        //        Editable = true;
        //        dbCombinationMaster.CombinationSubjects=dbCombinationMaster.CombinationSubjects.ToLower().Replace(model.FindSubject_ID.ToString().ToLower(), model.ReplaceCombinationSubject_IDWith.ToString().ToLower());
        //        //if (dbCombinationMaster.CombinationSubjects.Contains("||"))
        //        //{
        //        //    dbCombinationMaster.CombinationSubjects = dbCombinationMaster.CombinationSubjects.Replace("||", "|");
        //        //}
        //        //else if (dbCombinationMaster.CombinationSubjects.IndexOf('|') == 0)
        //        //{
        //        //    dbCombinationMaster.CombinationSubjects = dbCombinationMaster.CombinationSubjects.Substring(1, dbCombinationMaster.CombinationSubjects.Length);
        //        //}else if (dbCombinationMaster.CombinationSubjects.Substring(dbCombinationMaster.CombinationSubjects.Length-1,1)=="|")
        //        //{
        //        //    dbCombinationMaster.CombinationSubjects = dbCombinationMaster.CombinationSubjects.Substring(0, dbCombinationMaster.CombinationSubjects.Length-1);
        //        //}
        //    }

        //    if (model.RemoveSubject_ID)
        //    {
        //        Editable = true;
        //        dbCombinationMaster.CombinationSubjects=dbCombinationMaster.CombinationSubjects.ToLower().Replace(model.FindSubject_ID.ToString().ToLower(), "").Replace("||", "|");
        //    }

        //    dbCombinationMaster.CombinationSubjects = string.Join("|", _cs.Split('|').OrderBy(x => x).ToList());
        //    response.NumberOfRecordsEffected = new CombinationDB().Edit(dbCombinationMaster);

        //    if (response.NumberOfRecordsEffected > 0)
        //    {
        //        response.SuccessMessage = response.NumberOfRecordsEffected + " combination subjects updated. ## ";
        //        if (model.UpdateResult)
        //        {
        //            if (!model.ReplaceCombinationSubject_IDWith.IsNullOrEmpty() && Editable)
        //            {
        //                var _recordsEffected = new ResultDB().ReplaceSubjectInResult(dbCourseMaster.PrintProgramme, dbCombinationMaster.Semester.Value, model.Combination_ID, model.FindSubject_ID, model.ReplaceCombinationSubject_IDWith.Value);
        //                if (_recordsEffected > 0)
        //                    response.SuccessMessage += _recordsEffected + " records of results Updated Successfully. ##";
        //            }
        //        }
        //    }
        //    if (model.DeleteCombinationSubject_ID)
        //    {
        //        //var _deleteResponse = new SubjectsManager().Delete(model.CombinationSubject_ID);
        //        // if (_deleteResponse.IsSuccess)
        //        //     response.SuccessMessage += _deleteResponse.NumberOfRecordsEffected + " Subject deleted succesfully ##";
        //    }
        //    response.IsSuccess = response.NumberOfRecordsEffected >= 0;
        //    return response;
        //}

        public ResponseData BatchUpdateCombinationMaster(BatchUpdateCombinationMaster model)
        {
            var response = new ResponseData();
            var NoOfRecordsUpdated = 0;
            foreach (var Combination_ID in model.Combination_IDs)
            {
                ADMCombinationMaster _dbCombinationMaster = new CombinationManager().GetCombinationByID(Combination_ID);
                var dbCourseMaster = new CourseManager().GetCourseById(_dbCombinationMaster.Course_ID.Value);
                List<ADMSubjectMaster> _NewCombinationSubjects = new List<ADMSubjectMaster>();

                if (!model.ReplaceWithSubject_ID.IsNullOrEmpty())
                {
                    if (!_dbCombinationMaster.CombinationSubjects.ToLower().Contains(model.FindSubject_ID.ToString().ToLower()))
                    {
                        continue;
                    }

                    if (_dbCombinationMaster.SubjectsDetails.FirstOrDefault(x => x.Subject_ID == model.FindSubject_ID)?.Subject_ID == model.ReplaceWithSubject_ID)
                    {
                        continue;
                    }

                    _dbCombinationMaster.SubjectsDetails = _dbCombinationMaster.SubjectsDetails.Where(x => x.Subject_ID != model.FindSubject_ID).ToList();
                    var _NewSubject = new SubjectsManager().Get(model.ReplaceWithSubject_ID.Value);
                    _dbCombinationMaster.SubjectsDetails.Add(_NewSubject);

                    IEnumerable<Guid> FinalCombinations = _dbCombinationMaster.SubjectsDetails.Select(i => i.Subject_ID).OrderBy(x => x);
                    if (FinalCombinations.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                        continue;
                    _dbCombinationMaster.CombinationSubjects = FinalCombinations.ToSingleString();
                }


                if (model.RemoveFindedSubject == true)
                {
                    _dbCombinationMaster.SubjectsDetails = _dbCombinationMaster.SubjectsDetails.Where(x => x.Subject_ID != model.FindSubject_ID).ToList();
                    IEnumerable<Guid> FinalCombinations = _dbCombinationMaster.SubjectsDetails.Select(i => i.Subject_ID).OrderBy(x => x);
                    _dbCombinationMaster.CombinationSubjects = FinalCombinations.ToSingleString();
                }
                _dbCombinationMaster.SetWorkFlow(RecordState.Old);
                response.NumberOfRecordsEffected = new CombinationDB().Edit(_dbCombinationMaster);
                if (response.NumberOfRecordsEffected > 0)
                    response.SuccessMessage += "1 Combination updated successfully.";
                if (model.InActiveFindedSubject_ID == true && (new SubjectsManager().Get(model.FindSubject_ID.Value)).Status == true)
                {
                    new SubjectsManager().ChangeStatus(model.FindSubject_ID.Value);
                    response.SuccessMessage += NoOfRecordsUpdated + " Change status of subject to InActive";
                }
                if (model.ReplaceSubjectInResult == true)
                {
                    //var printProgrammelist = Helper.GetSelectList<PrintProgramme>().Select(x => (PrintProgramme)Convert.ToInt16(x.Value));
                    //foreach (PrintProgramme printProgramme in printProgrammelist)
                    //{
                    //  NoOfRecordsUpdated = new ResultManager().ReplaceSubjectInResult(dbCourseMaster.PrintProgramme, _dbCombinationMaster.Semester.Value, _dbCombinationMaster.Combination_ID, model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);
                    //  if (NoOfRecordsUpdated > 1)
                    //     response.SuccessMessage += NoOfRecordsUpdated + " Result updated successfully.";
                    //}
                }
                if (model.ReplaceSubjectInTranscript == true)
                {
                    NoOfRecordsUpdated = new TranscriptDB().ReplaceSubject(dbCourseMaster.PrintProgramme, _dbCombinationMaster.Combination_ID, model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);
                    if (NoOfRecordsUpdated > 1)
                        response.SuccessMessage += NoOfRecordsUpdated + " Transcript updated successfully.";
                }
                if (model.ReplaceSubjectInSyllabus == true && model.FindSubject_ID.HasValue && model.ReplaceWithSubject_ID.HasValue)
                {
                    NoOfRecordsUpdated = new SyllabusDB().ReplaceSubject(model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);
                    if (response.NumberOfRecordsEffected > 1)
                        response.SuccessMessage += NoOfRecordsUpdated + " Replaced in syllabus successfully.";
                }
                response.NumberOfRecordsEffected += NoOfRecordsUpdated;
            }

            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }
        public ResponseData ReplaceSubject(ReplaceSubject model)
        {
            var response = new ResponseData();
            var NoOfRecordsUpdated = 0;
            var TNoOfRecordsUpdated = 0;
            if (!model.FindSubject_ID.IsNullOrEmpty() && !model.ReplaceWithSubject_ID.IsNullOrEmpty() && model.Programme.IsNotNullOrEmpty())
            {
                foreach (var programme in model.Programme)
                {
                    List<short> otherBatchs = new RegistrationDB().GetAllBatchs(programme).Where(x => !model.Batch.Contains(x)).ToList();
                    //Check combinations having this subject
                    List<Guid> allCombinationsHavingSubjectonlytheseBatchs = new CombinationDB().GetCombinationsBySubject(programme, model.FindSubject_ID.Value, model.Batch, otherBatchs, true) ?? new List<Guid>();
                    List<Guid> allCombinationsHavingSubjectthesBatchsinUnion = new CombinationDB().GetCombinationsBySubject(programme, model.FindSubject_ID.Value, model.Batch, otherBatchs, false) ?? new List<Guid>();
                    allCombinationsHavingSubjectthesBatchsinUnion = allCombinationsHavingSubjectthesBatchsinUnion.Where(x => !allCombinationsHavingSubjectonlytheseBatchs.Contains(x)).ToList();
                    List<Guid> allCombinationsHavingSubjectToReplace = allCombinationsHavingSubjectonlytheseBatchs;
                    if (allCombinationsHavingSubjectthesBatchsinUnion.Count() > 0)
                    {
                        DateTime createdon = DateTime.Now;
                        List<Guid> CopyCombinations = new CombinationDB().CreateCopyOfCombinations(allCombinationsHavingSubjectthesBatchsinUnion, createdon);
                        int isupdated = new CombinationDB().AssignCopiedCombinationsToStudents(programme, model.Batch, model.Semester, allCombinationsHavingSubjectthesBatchsinUnion, createdon);
                        allCombinationsHavingSubjectToReplace.AddRange(CopyCombinations);
                    }
                    foreach (Guid id in allCombinationsHavingSubjectToReplace)
                    {
                        ADMCombinationMaster aDMCombinationMaster = new CombinationDB().GetCombinationByID(id);
                        aDMCombinationMaster.CombinationSubjects = new CombinationDB().ReplaceCombinationsSubject(model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value, aDMCombinationMaster.CombinationSubjects);
                        TNoOfRecordsUpdated += new CombinationDB().Edit(aDMCombinationMaster);
                        NoOfRecordsUpdated += new ResultManager().ReplaceSubjectInResult(programme, model.Semester, id, model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);

                    }
                }
            }

            //if (model.ReplaceSubjectInTranscript == true)
            //{
            //    NoOfRecordsUpdated = new TranscriptDB().ReplaceSubject(dbCourseMaster.PrintProgramme, _dbCombinationMaster.Combination_ID, model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);
            //    if (NoOfRecordsUpdated > 1)
            //        response.SuccessMessage += NoOfRecordsUpdated + " Transcript updated successfully.";
            //}
            //if (model.ReplaceSubjectInSyllabus == true && model.FindSubject_ID.HasValue && model.ReplaceWithSubject_ID.HasValue)
            //{
            //    NoOfRecordsUpdated = new SyllabusDB().ReplaceSubject(model.FindSubject_ID.Value, model.ReplaceWithSubject_ID.Value);


            response.NumberOfRecordsEffected += NoOfRecordsUpdated + TNoOfRecordsUpdated;
            if (response.NumberOfRecordsEffected > 1)
                response.SuccessMessage += TNoOfRecordsUpdated + " combination subjects replaced and " + response.NumberOfRecordsEffected + " records updated  successfully.";
            else response.ErrorMessage = "No Record Found.";
            response.IsSuccess = response.NumberOfRecordsEffected > 0;
            return response;
        }
        public List<ADMCombinationMaster> GetList(ADMCourseMaster aDMCourseMaster, Guid? College_ID = null, string CombinationSemester = null)
        {
            College_ID = (College_ID == null || College_ID == Guid.Empty) ? aDMCourseMaster.College_ID : College_ID;
            List<ADMCombinationMaster> listCombinationMaster = new CombinationDB().GetList((Guid)College_ID, aDMCourseMaster, CombinationSemester);
            List<ADMSubjectMaster> listSubjects = null;
            foreach (var item in listCombinationMaster ?? new List<ADMCombinationMaster>())
            {
                if (!string.IsNullOrWhiteSpace(item.CombinationSubjects))
                {

                    listSubjects = aDMCourseMaster.Course_ID == null
                        ? new SubjectDB().GetList(item.CombinationSubjects.ToGuidList())
                        : new SubjectDB().GetList(aDMCourseMaster.Course_ID, item.CombinationSubjects.ToGuidList());

                    if (listSubjects.IsNotNullOrEmpty())
                        item.SubjectsDetails.AddRange(listSubjects);
                }
            }
            return listCombinationMaster?.OrderBy(x => x.CombinationCode.SortAlphaNumeric()).ToList();
        }

        public List<ADMCombinationMaster> List(Parameters parameter)
        {
            List<ADMCombinationMaster> list = new CombinationDB().List(parameter);
            if (list.IsNotNullOrEmpty())
            {
                foreach (var item in list)
                {
                    item.SubjectsDetails = new SubjectDB().GetList(item.CombinationSubjects.ToGuidList());
                }
            }
            return list;
        }
        public List<ADMCombinationMaster> UsedCombinationList(Parameters parameter)
        {
            List<ADMCombinationMaster> list = new CombinationDB().UsedCombinationList(parameter);
            if (list.IsNotNullOrEmpty())
            {
                foreach (var item in list)
                {
                    item.SubjectsDetails = new SubjectDB().GetList(item.CombinationSubjects.ToGuidList());
                }
            }
            return list;
        }
        public bool CombinationCodeAlreadyExists(string CombinationCode, Guid College_ID, Guid? Combination_Id = null)
        {
            return new CombinationDB().CombinationCodeAlreadyExists(College_ID, CombinationCode, Combination_Id);
        }
        public bool CombinationExistsInCollege(Guid College_ID, Guid Combination_ID, Guid Course_ID, short Semester)
        {
            return new CombinationDB().CombinationExistsInCollege(College_ID, Combination_ID, Course_ID, Semester);
        }
        public ADMCombinationMaster GetCombination(Guid College_ID, string CombinationCode)
        {
            return new CombinationDB().GetCombination(College_ID, CombinationCode);
        }
        public ADMCombinationMaster GetCombinationBySubjects(Guid College_ID, IEnumerable<Guid?> subjects, Guid Course_ID)
        {
            IEnumerable<Guid?> FinalCombinations = subjects.OrderBy(x => x);
            ADMCombinationMaster aDMCombinationMaster = new CombinationDB().GetCombinationBySubjects(College_ID, FinalCombinations.ToSingleString(), Course_ID);
            if (aDMCombinationMaster != null)
                aDMCombinationMaster.SubjectsDetails = new CombinationDB().GetCombinationSubjects(aDMCombinationMaster.Combination_ID);
            return aDMCombinationMaster;
        }

        public int ChangeStatus(List<Guid> Combination_IDs, Status status)
        {
            if (Combination_IDs.IsNullOrEmpty())
                return 0;
            return new CombinationDB().ChangeStatus(Combination_IDs, status, AppUserHelper.User_ID);
        }

        public Tuple<GridView, string> DownloadExcel(ADMCourseMaster _Search, string CombinationSemester = null)
        {
            ADMCourseMaster _ADMCourseMaster = new CourseManager().GetItem(_Search, false, null, CombinationSemester);
            if (_ADMCourseMaster == null)
                return null;

            string fileName = _ADMCourseMaster.CourseCode.Replace("/", "_").Replace("\\", "_").Replace(".", "_").Replace("'", "_");
            GridView grid = new GridView();
            if (_ADMCourseMaster.CombinationDetails.IsNotNullOrEmpty())
            {
                grid.DataSource = from d in _ADMCourseMaster.CombinationDetails
                                  select new
                                  {
                                      Programme = _ADMCourseMaster.Programme.ToString(),
                                      CourseName = _ADMCourseMaster.CourseFullName,
                                      CourseCode = _ADMCourseMaster.CourseCode,
                                      CombinationCode = d.CombinationCode,
                                      Semester = d.Semester,
                                      // todo Subjects = d.SubjectsDetails.Select(x => x.SubjectFullName + "(" + x.SubjectCode + ")" + Environment.NewLine).ToList().ToSingleString(','),
                                      Subjects = d.SubjectsDetails.Select(x => x.SubjectFullName + Environment.NewLine).ToList().ToSingleString(','),
                                      Status = d.Status ? "Active" : "InActive"
                                  };
                grid.DataBind();
            }
            return Tuple.Create(grid, fileName);
        }

        public ADMCombinationMaster GetCombinationByID(Guid combination_ID)
        {
            ADMCombinationMaster aDMCombinationMaster = new CombinationDB().GetCombinationByID(combination_ID);
            if (aDMCombinationMaster != null)
                aDMCombinationMaster.SubjectsDetails = new CombinationDB().GetCombinationSubjects(combination_ID);
            return aDMCombinationMaster;
        }

        public List<SelectListItem> GetCombinationsDDL(Parameters Parameter)
        {
            List<SelectListItem> finalCombinationDDL = new List<SelectListItem>();
            List<ADMCombinationMaster> combinationMasters = new CombinationDB().CombinationDDLDB(Parameter);
            if (combinationMasters.IsNullOrEmpty())
                return finalCombinationDDL;

            int index = 0; string DisplayAs = string.Empty, CombinationCode = string.Empty; IEnumerable<ADMCombinationMaster> combination = null;

            foreach (Guid item in combinationMasters.Select(x => x.Combination_ID).Distinct())
            {
                combination = combinationMasters.Where(x => x.Combination_ID == item);
                index = combination?.First().CombinationSubjects?.IndexOf("$") ?? -1;
                if (index >= 0)
                {
                    CombinationCode = $"{combination.First().CombinationSubjects.Substring(0, index)}";

                    DisplayAs = CombinationCode + " = " + string.Join(" - ", combination.Select(_ => _.CombinationSubjects)).Replace(CombinationCode + "$", "") + "    [" + combination.First().CourseFullName + $":Sem-{combination.First().Semester}" + "  " + "]";

                    finalCombinationDDL.Add(new SelectListItem
                    {
                        Value = item.ToString(),
                        Text = DisplayAs
                    });
                }
                combination = null;//dispose off
            }
            return finalCombinationDDL;

        }

        public List<SelectListItem> GetCombinationsDDL(Guid College_ID, Guid Course_ID, int semester = 0)
        {
            List<SelectListItem> finalCombinationDDL = new List<SelectListItem>();
            List<ADMCombinationMaster> combinationMasters = new CombinationDB().CombinationDDLDB(College_ID, Course_ID, semester);
            if (combinationMasters.IsNullOrEmpty())
                return finalCombinationDDL;

            int index = 0; string DisplayAs = string.Empty, CombinationCode = string.Empty; IEnumerable<ADMCombinationMaster> combination = null;

            foreach (Guid item in combinationMasters.Select(x => x.Combination_ID).Distinct())
            {
                combination = combinationMasters.Where(x => x.Combination_ID == item);
                index = combination.First().CombinationSubjects?.IndexOf("$") ?? -1;

                CombinationCode = index != -1 ? $"{combination?.First().CombinationSubjects?.Substring(0, index)}" : combination?.First()?.CombinationCode;

                DisplayAs = CombinationCode + " = " + string.Join(" - ", combination.Select(_ => _.CombinationSubjects)).Replace(CombinationCode + "$", "");

                finalCombinationDDL.Add(new SelectListItem
                {
                    Value = item.ToString(),
                    Text = DisplayAs
                });
                combination = null;//dispose off
            }
            return finalCombinationDDL;

        }
        /// <summary>
        /// Method used to compare the subjects of a student belonging to a particular combination for insert update purposes
        /// </summary>
        /// <param name="Student_ID"></param>
        /// <param name="printProgramme"></param>
        /// <param name="semester"></param>
        /// <returns></returns>
        public ARGSelectedCombinationSubject GetSelectedCombinationCompact(Guid Student_ID, PrintProgramme printProgramme, short semester)
        {
            return new CombinationDB().GetSelectedCombinationCompact(Student_ID, printProgramme, semester);
        }
        public ARGSelectedCombination GetSelectedCombination(Guid student_ID, PrintProgramme printProgramme, short semester)
        {
            var selectedComb = new StudentDB().GetSelectedCombination(student_ID, printProgramme, semester);
            if (selectedComb != null)
                selectedComb.Subjects = new SubjectDB().GetList(selectedComb.CombinationSubjects.ToGuidList());
            return selectedComb;
        }

        public UserActivity GetUserActivity(Guid combination_ID)
        {
            return new CombinationDB().GetUserActivity(combination_ID);
        }

        public bool SetCombinationEndDate(int? batch, DateTime? endDate, PrintProgramme? printProgramme, int? semester)
        {
            return new CombinationDB().SetCombinationEndDate(batch, endDate, printProgramme, semester);
        }

        public void CloseCombinationSettingsLink()
        {
            try
            {
                new CombinationDB().CloseCombinationSettingsLink();
            }
            catch (Exception) { }
        }
    }
}
