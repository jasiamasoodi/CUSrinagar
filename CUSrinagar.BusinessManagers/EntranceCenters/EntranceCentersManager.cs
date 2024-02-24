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
using System.Text.RegularExpressions;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Web;
using System.IO;
using System.Web.Hosting;

namespace CUSrinagar.BusinessManagers
{
    public class EntranceCentersManager
    {
        #region DisplayCenterDetails

        public List<ARGEntranceCentersMaster> GetEntranceCentersMaster(Guid College_ID)
        {
            List<ARGEntranceCentersMaster> centersMasters = new SemesterCentersDB().GetSemesterCentersMaster(College_ID, true);
            List<DisplayEntranceCenterAllotmentDetail> centerAllotmentDetails = null;
            foreach (var item in centersMasters ?? new List<ARGEntranceCentersMaster>())
            {
                centerAllotmentDetails = GetCenterAllotmentDetails(item.Center_ID);
                if (centerAllotmentDetails.IsNotNullOrEmpty())
                {
                    item.DisplayEntranceCenterAllotmentDetail = new List<DisplayEntranceCenterAllotmentDetail>();
                    item.DisplayEntranceCenterAllotmentDetail.AddRange(centerAllotmentDetails);
                }
            }
            return centersMasters;
        }
        public List<DisplayEntranceCenterAllotmentDetail> GetCenterAllotmentDetails(Guid Center_ID)
        {
            return new EntranceCentersDB().GetCenterAllotmentDetails(Center_ID);
        }


        #endregion

        #region AssignRollNosInBulk
        private Tuple<bool, string> AssignRollNosInBulk(PrintProgramme _Programme, int Batch)
        {
            if (_Programme != PrintProgramme.PG && _Programme != PrintProgramme.IH && _Programme != PrintProgramme.UG)
                return Tuple.Create(false, "Invalid programme found");

            EntranceCentersDB _EntranceCentersDB = new EntranceCentersDB();

            if (!_EntranceCentersDB.BatchExists(_Programme, Batch))
                return Tuple.Create(false, $"Batch does not exist.");

            if (_EntranceCentersDB.IsAdmissionStillOpen(_Programme))
                return Tuple.Create(false, $"Admission process is still open for {_Programme.GetEnumDescription()} courses");

            int Result = Convert.ToInt32(Batch.ToString().Substring(2, 2) + "00000");
            int startRollNo = Convert.ToInt32(Batch.ToString().Substring(2, 2) + "00000");

            //check if Rollnos are already assigned
            if (_EntranceCentersDB.CheckIfRollnosAreAlreadyAssigned(_Programme, Batch))
            {
                Result = _EntranceCentersDB.GetMaxEntranceRollNo(Batch, _Programme);
                startRollNo = Result;
            }

            using (var ts = new TransactionScope())
            {
                List<EntranceFormsCourseCount> formsCourseCounts = _EntranceCentersDB.GetEntranceFormsCourseCounts(_Programme, Batch) ?? new List<EntranceFormsCourseCount>();
                if (_Programme == PrintProgramme.IH)
                {
                    //for Graduation
                    if (formsCourseCounts.FirstOrDefault(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")) != null)
                        formsCourseCounts.FirstOrDefault(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")).TotalForms = -1;
                    //for Integrated B.Ed-M.Ed
                    if (formsCourseCounts.FirstOrDefault(x => x.Course_ID == Guid.Parse("FC32E138-4EE2-4DA2-9453-5C8368180BC3")) != null)
                        formsCourseCounts.FirstOrDefault(x => x.Course_ID == Guid.Parse("FC32E138-4EE2-4DA2-9453-5C8368180BC3")).TotalForms = -2;
                }

                foreach (EntranceFormsCourseCount item in formsCourseCounts.OrderByDescending(EF => EF.TotalForms))
                {
                    Result += _EntranceCentersDB.AssignBulkRollNos(_Programme, Batch, item.Course_ID, Result);
                }
                ts.Complete();
            }

            return Tuple.Create(true, $"{Result - startRollNo} Roll Nos Assigned successfully");
        }

        public Task<Tuple<bool, string>> AssignRollNosInBulkAsync(PrintProgramme _Programme, int Batch)
        {
            return new TaskFactory().StartNew(() => AssignRollNosInBulk(_Programme, Batch));
        }
        #endregion

        #region AssignCenterInBulk       
        public AssignEntranceCenters GetDispalyAvaliableCenters(AssignEntranceCenters CentersToAssign)
        {
            if (CentersToAssign == null)
                return new AssignEntranceCenters();

            EntranceCentersDB entranceCentersDB = new EntranceCentersDB();

            AssignEntranceCenters entranceCenters = new AssignEntranceCenters
            {
                AssignEntranceCentersAs = entranceCentersDB.GetDispalyAvaliableCenters(),
                TotalEntranceFormsAvailable = entranceCentersDB.GetTotalEntranceFormsAvailable(CentersToAssign.Batch, CentersToAssign.Course_ID, CentersToAssign.PrintProgramme),
                DistrictsDDL = new GeneralDDLManager().GetDistrictListOnly()
            };
            return entranceCenters;
        }


        public List<SelectListItem> GetCoursesDDL(PrintProgramme? Id, short? Id1)
        {

            if ((Id == null || (Id != PrintProgramme.PG && Id != PrintProgramme.IH && Id != PrintProgramme.UG)) || Id1 == null)
                return new List<SelectListItem>();

            List<SelectListItem> selectListItem = new EntranceCentersDB().GetCoursesDDL((PrintProgramme)Id, (short)Id1);
            if (selectListItem.IsNotNullOrEmpty())
            {
                #region Check for General Appitude to include
                Dictionary<PrintProgramme, bool> IncludeGeneralAppitude =
                       JsonConvert.DeserializeObject<Dictionary<PrintProgramme, bool>>(
                       Convert.ToString(ConfigurationManager.AppSettings["IncludeGeneralApitude"]));
                if (IncludeGeneralAppitude.IsNotNullOrEmpty())
                {
                    IncludeGeneralAppitude.TryGetValue((PrintProgramme)Id, out bool AddGP);
                    if (AddGP)
                    {
                        Guid GeneralAppitudeCourse_ID =
                            Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

                        selectListItem.Add(new SelectListItem
                        {
                            Value = GeneralAppitudeCourse_ID.ToString(),
                            Text = "Common Aptitude Test"
                        });
                    }
                }
                #endregion
            }
            return selectListItem ?? new List<SelectListItem>();
        }

        public Task<Tuple<bool, string>> AssignCentersInBulkAsync(AssignEntranceCenters CentersToAssign)
        {
            Guid User_ID = AppUserHelper.User_ID;
            return new TaskFactory().StartNew(() => AssignCentersInBulk(CentersToAssign, User_ID));
        }

        private Tuple<bool, string> AssignCentersInBulk(AssignEntranceCenters CentersToAssign, Guid User_ID)
        {
            #region Validations
            if (CentersToAssign == null || CentersToAssign.AssignEntranceCentersAs.IsNullOrEmpty())
                return Tuple.Create(false, "Please Choose the centers to be alloted using checkboxes.");

            EntranceCentersDB _EntranceCentersDB = new EntranceCentersDB();

            if (!_EntranceCentersDB.BatchExists(CentersToAssign.PrintProgramme, CentersToAssign.Batch))
                return Tuple.Create(false, $"Batch does not exist.");

            if (!_EntranceCentersDB.CheckIfRollnosAreAlreadyAssigned(CentersToAssign.PrintProgramme, CentersToAssign.Batch))
                return Tuple.Create(false, $"Entrance Roll No.'s has not been assigned yet for {CentersToAssign.PrintProgramme.GetEnumDescription()} courses.");

            if (_EntranceCentersDB.CheckIfCentersAlreadyAlloted(CentersToAssign.PrintProgramme, CentersToAssign.Batch, CentersToAssign.Course_ID))
                return Tuple.Create(false, $"Centers already alloted");


            if (CentersToAssign.TotalEntranceFormsAvailable <= 0)
                return Tuple.Create(false, $"No Entrance forms are avaliable for {CentersToAssign.PrintProgramme.GetEnumDescription()} Batch = {CentersToAssign.Batch}");

            if (CentersToAssign.AssignEntranceCentersAs.All(x => x.HasToBeAssigned == false))
                return Tuple.Create(false, "Please Choose the centers to be alloted using checkboxes.");

            if (CentersToAssign.AssignEntranceCentersAs.Any(x => x.HasToBeAssigned && x.AssignmentOrder == null))
                return Tuple.Create(false, "Please provide Assignment Order for each selected Centers.");

            if (CentersToAssign.AssignEntranceCentersAs.Any(x => x.HasToBeAssigned && (x.NoOfStudentsToBeAssignedPerCenter ?? 0) <= 0))
                return Tuple.Create(false, "Please provide No Of Students To Be Assigned Per Center for each selected Centers.");

            List<AssignEntranceCentersAs> _AssignCentersAs = CentersToAssign.AssignEntranceCentersAs.Where(f => f.HasToBeAssigned).ToList();

            if (_AssignCentersAs.GroupBy(x => x.AssignmentOrder).Where(d => d.Count() > 1).Select(y => y.Key)?.Any() ?? false)
                return Tuple.Create(false, "Assignment Order for each selected Centers should be unique.");

            int TotalEntranceFormsToAssign = _AssignCentersAs.Sum(x => Convert.ToInt32(x.NoOfStudentsToBeAssignedPerCenter));
            if (CentersToAssign.TotalEntranceFormsAvailable > TotalEntranceFormsToAssign)
                return Tuple.Create(false, "There are still " + (CentersToAssign.TotalEntranceFormsAvailable - TotalEntranceFormsToAssign) + " Entrance Forms available which needs to be assigned. Please select more centers OR Increase No. Of Students To Be Assigned Per Center.");

            #endregion

            #region SaveProcess

            short index = 0;
            int result = 0, QueryResult = 0, NextOFFSet = 0;

            Guid GeneralAppitudeCourse_ID =
                          Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));

            if (GeneralAppitudeCourse_ID == CentersToAssign.Course_ID)
            {
                _AssignCentersAs.Where(x => x.Districts.IsNotNullOrEmpty()).ToList().ForEach(y =>
                {
                    y.AssignmentOrder = Convert.ToInt16(y.AssignmentOrder * (-1));
                });
                List<AssignEntranceCentersAs> _AssignCentersAsNegative = _AssignCentersAs.Where(x => x.AssignmentOrder < 0).OrderByDescending(x => x.AssignmentOrder).ToList();
                List<AssignEntranceCentersAs> _AssignCentersAsPositive = _AssignCentersAs.Where(x => x.AssignmentOrder >= 0).OrderBy(x => x.AssignmentOrder).ToList();
                _AssignCentersAs = new List<AssignEntranceCentersAs>();
                _AssignCentersAs.AddRange(_AssignCentersAsNegative);
                _AssignCentersAs.AddRange(_AssignCentersAsPositive);
            }
            else
            {
                _AssignCentersAs = _AssignCentersAs.OrderBy(AO => AO.AssignmentOrder).ToList();
            }

            RegistrationManager regn = new RegistrationManager();
            Tuple<string, List<Guid[]>> tuple;


            Guid optCourseOfGroup = Guid.Empty;
            if (CentersToAssign.PrintProgramme == PrintProgramme.IH)
            {
                tuple = _EntranceCentersDB.GetCourseGrouping(PrintProgramme.IH);
                if (tuple != null && tuple.Item2.Any())
                {
                    optCourseOfGroup =
                        (tuple?.Item2?.Find(x => x[0] == CentersToAssign.Course_ID || x[1] == CentersToAssign.Course_ID)?.ToList()?.FindIndex(x => x == CentersToAssign.Course_ID)??0) == 0
                        ? tuple?.Item2?.Find(x => x[0] == CentersToAssign.Course_ID || x[1] == CentersToAssign.Course_ID)?.ElementAt(1)??Guid.Empty
                        : tuple?.Item2?.Find(x => x[0] == CentersToAssign.Course_ID || x[1] == CentersToAssign.Course_ID)?.ElementAt(0)??Guid.Empty;
                }
            }

            //using (var ts = new TransactionScope())
            //{
            AssignCentersInBulk assignCentersInBulk = null;
            foreach (AssignEntranceCentersAs center in _AssignCentersAs)
            {
                assignCentersInBulk = new AssignCentersInBulk
                {
                    Center_ID = center.Center_ID,
                    PrintProgramme = CentersToAssign.PrintProgramme,
                    Course_ID = CentersToAssign.Course_ID,
                    Semester = 0,
                    Year = CentersToAssign.Batch,
                    ExaminationDate = regn.ConvertEnterdDOBToDateTime(CentersToAssign.EnteredEntranceDate),
                    ExaminationTime = CentersToAssign.Time,
                    FetchNextCount = Convert.ToInt16(center.NoOfStudentsToBeAssignedPerCenter),
                    User_ID = User_ID,
                    OFFSETCountForEntrance = 0,
                    Districts = center.Districts
                };
                NextOFFSet += Convert.ToInt16(center.NoOfStudentsToBeAssignedPerCenter);

                QueryResult = _EntranceCentersDB.AssignEntranceCenterInBulk(assignCentersInBulk);

                //------------------------- insert for group courses as well ---------------------
                if (CentersToAssign.PrintProgramme == PrintProgramme.IH)
                {
                    if (optCourseOfGroup != Guid.Empty)
                    {
                        _EntranceCentersDB.AssignEntranceCenterInBulkForGroupCourses(new AssignCentersInBulk
                        {
                            PrintProgramme = CentersToAssign.PrintProgramme,
                            Center_ID = center.Center_ID,
                            ExaminationDate = regn.ConvertEnterdDOBToDateTime(CentersToAssign.EnteredEntranceDate),
                            ExaminationTime = CentersToAssign.Time,
                            Course_ID = CentersToAssign.Course_ID,
                            User_ID = User_ID
                        }, optCourseOfGroup);
                    }

                    if (QueryResult < assignCentersInBulk.FetchNextCount && optCourseOfGroup != Guid.Empty)
                    {
                        assignCentersInBulk.Course_ID = optCourseOfGroup;
                        assignCentersInBulk.FetchNextCount = Convert.ToInt16(assignCentersInBulk.FetchNextCount - QueryResult);
                        QueryResult += _EntranceCentersDB.AssignEntranceCenterInBulk(assignCentersInBulk);
                    }
                }

                result += QueryResult;
                if (QueryResult == 0 || result >= CentersToAssign.TotalEntranceFormsAvailable)
                {
                    break;
                }
                else
                {
                    index++;
                }
            }
            //ts.Complete();
            //}

            result = (result >= CentersToAssign.TotalEntranceFormsAvailable) ? 0 : (CentersToAssign.TotalEntranceFormsAvailable - result);
            return Tuple.Create(true, $"Students assigned successfully in {index + 1} Centers. {result} Entrance Forms remain unassigned for this course for now.");
            #endregion
        }
        #endregion

        #region IndividualCenterAssignment And AdmitCards
        public List<ARGEntranceCentersMaster> GetCenters()
        {
            List<ARGEntranceCentersMaster> _Centers = new EntranceCentersDB().GetCenters();
            foreach (var item in _Centers ?? new List<ARGEntranceCentersMaster>())
                item.CenterContactDetails = new CollegeManager().GetItem(item.College_ID);
            return _Centers;
        }
        public void GetCenterDetails(ref ARGPersonalInformation _PersonalInformation, PrintProgramme programme)
        {
            if (_PersonalInformation == null || (_PersonalInformation?.CoursesApplied.IsNullOrEmpty() ?? true))
                return;
            var _entranceCentersDB = new EntranceCentersDB();
            _PersonalInformation.EntranceCenters = GetCenters();


            #region Check for General Appitude to include
            Dictionary<PrintProgramme, bool> IncludeGeneralAppitude =
                   JsonConvert.DeserializeObject<Dictionary<PrintProgramme, bool>>(
                   Convert.ToString(ConfigurationManager.AppSettings["IncludeGeneralApitude"]));
            if (IncludeGeneralAppitude.IsNotNullOrEmpty())
            {
                if (!_PersonalInformation.CoursesApplied.Any(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")))
                {
                    IncludeGeneralAppitude.TryGetValue(programme, out bool AddGP);
                    if (AddGP && !_PersonalInformation.IsLateralEntry)
                    {
                        Guid GeneralAppitudeCourse_ID =
                            Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));
                        _PersonalInformation.CoursesApplied.Insert(0,
                            new ARGCoursesApplied
                            {
                                Course_ID = GeneralAppitudeCourse_ID,
                                Student_ID = _PersonalInformation.Student_ID,
                                CourseName = "Common Aptitude Test",
                                CourseCode = "GENAPTITUDE",
                                Preference = 0
                            });
                    }
                }
            }
            #endregion

            #region remove graduation Course if applied
            ARGCoursesApplied coursesAppliedGraduation = _PersonalInformation.CoursesApplied.FirstOrDefault(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e"));
            if (coursesAppliedGraduation != null)
            {
                _PersonalInformation.CoursesApplied.Remove(coursesAppliedGraduation);
            }
            #endregion

            foreach (var item in _PersonalInformation.CoursesApplied)
            {
                item.EntranceCentersAllotment = _entranceCentersDB.GetCentersAllotment(_PersonalInformation.Student_ID, item.Course_ID);
            }

            _PersonalInformation.AcademicDetails = null;//dispose
            IncludeGeneralAppitude = null;//dispose
        }

        public SelectList GetCenterList(bool IsEntrance = true) =>
        new SelectList(new EntranceCentersDB().GetCenterList(IsEntrance)?? new List<DropDownOptLabelGeneral>(), "Value", "Text", "DataGroupField", null, null);

        public void AddGeneralAptitudeCourse(ref ARGPersonalInformation _PersonalInformation, PrintProgramme programme)
        {
            if (_PersonalInformation == null || (_PersonalInformation?.CoursesApplied.IsNullOrEmpty() ?? true))
                return;


            #region Check for General Appitude to include
            Dictionary<PrintProgramme, bool> IncludeGeneralAppitude =
                   JsonConvert.DeserializeObject<Dictionary<PrintProgramme, bool>>(
                   Convert.ToString(ConfigurationManager.AppSettings["IncludeGeneralApitude"]));
            if (IncludeGeneralAppitude.IsNotNullOrEmpty())
            {
                IncludeGeneralAppitude.TryGetValue(programme, out bool AddGP);
                if (AddGP)
                {
                    if (!_PersonalInformation.CoursesApplied.Any(x => x.Course_ID == new Guid("FC32E138-4EE2-4DA2-9453-5C8368180BC3")))
                    {
                        Guid GeneralAppitudeCourse_ID =
                            Guid.Parse(Convert.ToString(ConfigurationManager.AppSettings["GeneralApitudeCourse_ID"]));
                        _PersonalInformation.CoursesApplied.Insert(0,
                            new ARGCoursesApplied
                            {
                                Course_ID = GeneralAppitudeCourse_ID,
                                Student_ID = _PersonalInformation.Student_ID,
                                CourseName = "Common Aptitude Test",
                                CourseCode = "GENAPTITUDE"
                            });
                    }
                }
            }
            #endregion


            _PersonalInformation.AcademicDetails = null;//dispose
            IncludeGeneralAppitude = null;//dispose
        }

        public int Save(CentersAllotmentMaster _CentersAllotmentMaster, PrintProgramme printProgramme)
        {
            int result = 0;
            if (_CentersAllotmentMaster?.CenterAllotmentMaster.IsNullOrEmpty() ?? true)
                return result;
            using (var ts = new TransactionScope())
            {
                EntranceCentersDB entranceCentersDB = new EntranceCentersDB();
                RegistrationManager registrationManager = new RegistrationManager();

                if (entranceCentersDB.CheckIfEntranceCenterAlreadyAssinged(_CentersAllotmentMaster.CenterAllotmentMaster[0].Entity_ID))
                    result = -1;
                else if (entranceCentersDB.CheckIfAlreadyHasEntranceRollNo(_CentersAllotmentMaster.CenterAllotmentMaster[0].Entity_ID, registrationManager.MappingTable(printProgramme)))
                    result = entranceCentersDB.UpdateEntranceRollNoAndFormStatus(_CentersAllotmentMaster.CenterAllotmentMaster[0].Entity_ID, FormStatus.FeePaid, registrationManager.MappingTable(printProgramme));
                else
                    result = 1;

                if (result > 0)
                {
                    foreach (var item in _CentersAllotmentMaster.CenterAllotmentMaster)
                    {
                        item.EntranceDate = registrationManager.ConvertEnterdDOBToDateTime(item.EnteredEntranceDate);
                        item.SetWorkFlow(RecordState.New);
                        item.CenterAllowment_ID = Guid.NewGuid();
                        result += entranceCentersDB.Save(item);
                    }
                }
                ts.Complete();
            }
            return result;
        }

        public int DeleteCentersForIndividual(string formNumber, PrintProgramme programme)
        {
            if (string.IsNullOrWhiteSpace(formNumber))
                return 0;
            programme = new RegistrationManager().MappingTable(programme);

            Guid Student_ID = new AssignCombinationDB().GetStudentDetails(formNumber, programme)?.Student_ID ?? Guid.Empty;
            if (Student_ID == Guid.Empty)
                return 0;
            return new EntranceCentersDB().DeleteCentersForIndividual(Student_ID);
        }
        #endregion


        #region Generate CenterNotice
        public List<CenterNotice> GetCenterNoticeDetails(RePrintCenterNotice printCenterNotice)
        {
            if (printCenterNotice == null)
                return null;
            EntranceCentersDB entranceCentersDB = new EntranceCentersDB();
            List<CenterNotice> CenterDetails = entranceCentersDB.GetCenterAndCourseInfo(printCenterNotice);

            List<CenterNotice> DispalyCenterDetails = new List<CenterNotice>();

            Tuple<string, List<Guid[]>> tuple = entranceCentersDB.GetCourseGrouping(GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory));

            List<string> IsCourseInGroup = new List<string>();

            CenterNotice FullCenterDetail = null;
            bool ReplaceName = false;

            if (GeneralFunctions.ProgrammeToPrintProgrammeMapping(printCenterNotice.CourseCategory) == PrintProgramme.IH)
                ReplaceName = true;

            foreach (var detailForSearch in CenterDetails ?? new List<CenterNotice>())
            {
                FullCenterDetail = entranceCentersDB.GetCenterNoticeDetail(detailForSearch);
                if (FullCenterDetail != null)
                {
                    if (ReplaceName)
                    {
                        FullCenterDetail.CourseFullName = FullCenterDetail.CourseFullName.Replace("Integrated", "").Replace("Bachelor of ", "").Replace("Honors", "").Replace("Honor's", "").Replace("MCA", "IMCA").Replace("MBA", "IMBA");
                    }

                    FullCenterDetail.TotalStudentesAssigned = entranceCentersDB.GetStudentsAssignedToCenter(detailForSearch);
                    DispalyCenterDetails.Add(FullCenterDetail);
                }
            }
            return DispalyCenterDetails.IsNullOrEmpty() ? null : DispalyCenterDetails.DistinctBy(x => new { x.Course_ID, x.CenterCode }).ToList();
        }
        #endregion


        #region GenerateExcel
        public Task<DataTable> GetCenterWiseCourseCountAsync(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetCenterWiseCourseCount(_PrintCenterNotice));
        }

        public Task<DataTable> GetAdmissionFormCount(RePrintCenterNotice printCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetAdmissionFormCount(printCenterNotice));
        }

        public Task<DataTable> GetOMRMasterFile(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetOMRMasterFile(_PrintCenterNotice));
        }

        public Task<DataTable> GetAttendanceSheetFileAsync(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetAttendanceSheetFile(_PrintCenterNotice));
        }

        public Task<DataTable> GetGeneralMeritList(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetGeneralMeritList(_PrintCenterNotice));
        }

        public Task<DataTable> GetCategoryMeritList(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetCategoryMeritList(_PrintCenterNotice));
        }


        public Task<DataTable> GetDateSheet(RePrintCenterNotice printCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetDateSheet(printCenterNotice));
        }
        public Task<DataTable> GetDistrictWiseCount(RePrintCenterNotice printCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetDistrictWiseCount(printCenterNotice));
        }
        public Task<DataTable> GetDistrictWiseCenterAllotmentCount(RePrintCenterNotice _PrintCenterNotice)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().GetDistrictWiseCenterAllotmentCount(_PrintCenterNotice));
        }
        #endregion

        #region Upload result
        public Task<Tuple<bool, string>> UploadEntranceResultAsync(RePrintCenterNotice _PrintCenterNotice, HttpPostedFileBase CSVResultFile)
        {
            return new TaskFactory().StartNew(() => UploadEntranceResult(_PrintCenterNotice, CSVResultFile));
        }


        public Tuple<bool, string> UploadEntranceResult(RePrintCenterNotice printCenterNotice, HttpPostedFileBase _CSVResultFile)
        {
            if (_CSVResultFile == null || _CSVResultFile?.ContentLength <= 0 || System.IO.Path.GetExtension(_CSVResultFile?.FileName).ToLower().Trim() != ".csv")
            {
                return Tuple.Create(false, "Please choose valid result file");
            }

            EntranceResult entranceResults = null;
            List<string> ScriptToUploadList = new List<string>();
            StringBuilder ScriptToUpload = new StringBuilder();
            var _EntranceCentersDB = new EntranceCentersDB();
            int Counter = 0;

            #region CSVParsing Work
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            using (StreamReader csvreader = new StreamReader(_CSVResultFile.InputStream))
            {
                while (!csvreader.EndOfStream)
                {
                    string[] Fields = CSVParser.Split(csvreader.ReadLine());
                    if (Fields.Length != 2)
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some row are not valid. Please check again.<br/> Operation aborted.");
                    }

                    if (!long.TryParse(Fields[0], out long _EntranceRollNo))
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some Roll Nos are not valid (Entrance RollNo Should be a number). Please check again.<br/> Operation aborted.");
                    }

                    if (!decimal.TryParse(Fields[1], out decimal _EntrancePoints))
                    {
                        _CSVResultFile = null;//dispose
                        return Tuple.Create(false, $"Some Entrace points are not valid (Entrance Points Should be a fraction/number). Please check again.<br/> Operation aborted.");
                    }

                    entranceResults = new EntranceResult
                    {
                        Course_ID = printCenterNotice.College_ID,
                        EntracePoints = _EntrancePoints,
                        EntranceRollNo = _EntranceRollNo,
                        Batch = printCenterNotice.ExaminationYear,
                        Programme = printCenterNotice.CourseCategory
                    };

                    ScriptToUpload.Append(_EntranceCentersDB.GetUploadEntranceResultTSQL(entranceResults));
                    ScriptToUpload.Append(Environment.NewLine);
                    Counter++;

                    if (Counter >= 1000)
                    {
                        ScriptToUploadList.Add(ScriptToUpload.ToString());
                        ScriptToUpload.Clear();
                        Counter = 0;
                    }
                }
                if (Counter > 0)
                {
                    ScriptToUploadList.Add(ScriptToUpload.ToString());
                }
            }
            #endregion


            _CSVResultFile = null;//dispose
            int result = 0;
            using (var ts = new TransactionScope())
            {
                foreach (var itemTSQL in ScriptToUploadList)
                {
                    result += _EntranceCentersDB.UploadEntranceResult(itemTSQL);
                }
                ts.Complete();
            }

            entranceResults = null;//despose
            return Tuple.Create(result > 0, result > 0 ? $"{result} Entrance Results Uploaded Successfully" : "Failed to upload result. Please check details and try again.");
        }
        #endregion


        #region SelfFinanced List
        public Task<DataTable> GetSelfFinancedListAsync(SelfFinancedList selfFinancedList)
        {
            return new TaskFactory().StartNew(() => GetSelfFinancedList(selfFinancedList));
        }
        public DataTable GetSelfFinancedList(SelfFinancedList selfFinancedList)
        {
            if (selfFinancedList == null)
                return new DataTable();
            switch (selfFinancedList.SelfFinancedDownloadType)
            {
                case SelfFinancedDownloadType.ByCourse:
                    return new EntranceCentersDB().GetSelfFinancedListByCourse(selfFinancedList);
                case SelfFinancedDownloadType.ByIndividual:
                    return new EntranceCentersDB().GetSelfFinancedListByIndividual(selfFinancedList);
                default:
                    return new DataTable();
            }
        }
        #endregion

        #region VerifyFirstSemesterRR
        public List<SelectListItem> GetCoursesDDL(Programme? Id, short? Id1)
        {
            if ((Id == null || (Id != Programme.PG && Id != Programme.IG && Id != Programme.Professional && Id != Programme.HS && Id != Programme.UG)) || Id1 == null)
                return new List<SelectListItem>();

            List<SelectListItem> selectListItem = new EntranceCentersDB().GetCoursesDDL((Programme)Id, (short)Id1);
            return (selectListItem ?? new List<SelectListItem>()).OrderBy(x => x.Text).ToList();
        }

        public Task<List<FirstSemesterRR>> GetFirstSemesterRRAsync(SearchFirstSemesterRR searchFirstSemesterRR)
        {
            return new TaskFactory().StartNew(() => GetFirstSemesterRR(searchFirstSemesterRR));
        }

        public List<FirstSemesterRR> GetFirstSemesterRR(SearchFirstSemesterRR searchFirstSemesterRR)
        {
            if (searchFirstSemesterRR == null || !string.IsNullOrWhiteSpace(searchFirstSemesterRR.EntranceRollNos) && !new Regex(@"^(\d{6},)*\d{6}$").IsMatch(searchFirstSemesterRR.EntranceRollNos))
                return new List<FirstSemesterRR>();

            EntranceCentersDB entranceCentersDB = new EntranceCentersDB();
            List<FirstSemesterRR> RRList = entranceCentersDB.GetFirstSemesterRR(searchFirstSemesterRR);
            if (RRList.IsNotNullOrEmpty())
            {
                foreach (FirstSemesterRR item in RRList)
                {
                    item.CoursesAppliedByStudent = entranceCentersDB.GetCoursesAppliedByStudent(searchFirstSemesterRR.Programme, item.Student_ID, item.CATEntrancePoints ?? 0m)?.ToSingleString(';');
                }
            }
            return RRList ?? new List<FirstSemesterRR>();
        }
        #endregion

        #region DeleteJunkData

        public Tuple<bool, string> DeleteJunk(PrintProgramme printProgramme, bool onlyLateralEntry, DateTime appliedBefore)
        {
            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(printProgramme);
            if (formNoMaster == null || formNoMaster.AllowOnlinePayment)
                return Tuple.Create(true, $"{printProgramme.GetEnumDescription()} Batch {formNoMaster.BatchToSet} new Admission is currently open. Please close admission before proceeding with this option.");

            short BatchToDeleteFrom = formNoMaster.BatchToSet;

            int result = 0, finalResult = 0;
            EntranceCentersDB entranceCentersDB = new EntranceCentersDB();
            List<string> PhotographsToDelete = entranceCentersDB.GetJunkPhotoPaths(printProgramme, BatchToDeleteFrom, onlyLateralEntry, appliedBefore);
            List<string> CertificatesToDelete = entranceCentersDB.GetJunkCertificatesPaths(printProgramme, BatchToDeleteFrom, onlyLateralEntry, appliedBefore);

            if (PhotographsToDelete.IsNotNullOrEmpty())
            {
                while (PhotographsToDelete.IsNotNullOrEmpty())
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        result = entranceCentersDB.DeleteJunk(printProgramme, BatchToDeleteFrom, onlyLateralEntry, appliedBefore);
                        ts.Complete();
                    }

                    if (result > 0)
                    {
                        //delete photographs
                        foreach (string path in PhotographsToDelete ?? new List<string>())
                        {
                            if (!path.ToLower().Contains("themepublic") && File.Exists(HostingEnvironment.MapPath("~" + path)))
                            {
                                File.Delete(HostingEnvironment.MapPath("~" + path));
                            }
                        }
                        foreach (string cpath in CertificatesToDelete ?? new List<string>())
                        {
                            if (!cpath.ToLower().Contains("themepublic") && File.Exists(HostingEnvironment.MapPath("~" + cpath)))
                            {
                                File.Delete(HostingEnvironment.MapPath("~" + cpath));
                            }
                        }
                        PhotographsToDelete = entranceCentersDB.GetJunkPhotoPaths(printProgramme, BatchToDeleteFrom, onlyLateralEntry, appliedBefore);
                        CertificatesToDelete = entranceCentersDB.GetJunkCertificatesPaths(printProgramme, BatchToDeleteFrom, onlyLateralEntry, appliedBefore);
                        finalResult += result;
                    }
                }
                if (finalResult > 0)
                {
                    return Tuple.Create(true, $"{printProgramme.GetEnumDescription()} Batch {BatchToDeleteFrom} Junk deleted successfully");
                }
            }
            else
            {
                return Tuple.Create(false, $"{printProgramme.GetEnumDescription()} Batch {BatchToDeleteFrom} already archived.");
            }

            return Tuple.Create(false, $"UnKnown error occurred.");
        }

        public Tuple<bool, string> DeleteJunk()
        {
            int result = 0;
            List<string> PhotographsToDelete = null;
            EntranceCentersDB entranceCentersDB = new EntranceCentersDB();

            using (TransactionScope ts = new TransactionScope())
            {
                PhotographsToDelete = entranceCentersDB.GetJunkPhotoPaths();

                if (PhotographsToDelete.IsNotNullOrEmpty())
                    result = entranceCentersDB.DeleteJunk();

                ts.Complete();
            }

            if (PhotographsToDelete.IsNullOrEmpty())
                return Tuple.Create(false, $"Already been archived.");

            if (result > 0)
            {
                //delete photographs
                foreach (string path in PhotographsToDelete ?? new List<string>())
                {
                    if (!path.ToLower().Contains("themepublic") && File.Exists(HostingEnvironment.MapPath("~" + path)))
                    {
                        File.Delete(HostingEnvironment.MapPath("~" + path));
                    }
                }
                PhotographsToDelete = null; //dispose
                return Tuple.Create(true, $"Certificate Courses Junk deleted successfully");
            }

            return Tuple.Create(false, $"UnKnown error occurred.");
        }


        public Task<Tuple<bool, string>> DeleteJunkAsync(PrintProgramme printProgramme, bool onlyLateralEntry, DateTime appliedBefore)
        {
            return new TaskFactory().StartNew(() => DeleteJunk(printProgramme, onlyLateralEntry, appliedBefore));
        }

        public Task<Tuple<bool, string>> DeleteJunkAsync()
        {
            return new TaskFactory().StartNew(() => DeleteJunk());
        }
        #endregion

        #region PDF Merit Lists
        public Task<MeritListsAsPDF> PDFMeritListsAsync(PDFMeritListFilter meritLists)
        {
            return new TaskFactory().StartNew(() => new EntranceCentersDB().PDFMeritLists(meritLists));
        }
        #endregion

        #region Relocate Centers
        public SelectList GetRelocateCenterList() =>
         new SelectList(new EntranceCentersDB().GetRelocateCenterList() ?? new List<DropDownOptLabelGeneral>(), "Value", "Text", "DataGroupField", null, null);
        public List<SelectListItem> GetRelocateCenterCourseIds(Guid Center_ID) => new EntranceCentersDB().GetRelocateCenterCourseIds(Center_ID);
        public Tuple<bool, string> RelocateCenter(RelocateCenter relocateCenter)
        {
            if (relocateCenter == null)
                return Tuple.Create(false, "Invalid Details");

            if (relocateCenter.FromCenter_ID == relocateCenter.ToCenter_ID)
                return Tuple.Create(false, "No change");

            if (relocateCenter.FromCourse_ID == Guid.Empty)
                return Tuple.Create(false, "No course selected");

            int result = 0;
            using (var ts = new TransactionScope())
            {
                result = new EntranceCentersDB().RelocateCenter(relocateCenter);
                ts.Complete();
            }
            if (result > 0)
            {
                return Tuple.Create(true, $"{result} students relocated successfully");
            }
            else
            {
                return Tuple.Create(false, "Operation was unsuccessfull");
            }
        }
        #endregion
    }
}
