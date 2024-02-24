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
using GeneralModels;

namespace CUSrinagar.BusinessManagers
{
    public class RegistrationManager
    {
        #region AssignRegistrationNumbers
        public ResponseData AssignRegistrationNumbers(Guid college_ID, PrintProgramme programme, int GapBetweenCourses)
        {
            short batchToAssign = new RegistrationManager().GetFormNoMaster(programme).BatchToSet;
            ResponseData response = ValidateRequestForAssignment(college_ID, programme, batchToAssign);

            if (response.IsSuccess)
            {
                ADMCollegeMaster college = null;
                string NoStudentsForCourses = string.Empty; List<string> allotedRegistrationCourses = new List<string>();
                var NoOfRecordsEffected = 0;

                int Count;
                if (programme == PrintProgramme.PG)
                    Count = 11001;
                else
                    Count = 10001;

                if (college_ID != Guid.Empty)
                    college = new CollegeManager().GetCollegeList(new Parameters() { Filters = new List<SearchFilter>() { new SearchFilter() { Column = nameof(ADMCollegeMaster.College_ID), Operator = SQLOperator.EqualTo, Value = college_ID.ToString() } } }).First();

                if (college == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Please click on any college to assign it the Registration Numbers or if you see no college listed, The Colleges may have been alloted the Registration Numbers.";
                    return response;
                }

                var collegeCourses = new CourseManager().GetAllCourseListByCollegeID(college_ID, programme);

                if (collegeCourses == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = $" No Course found for <strong>{programme.GetEnumDescription()}</strong> in the college <strong>{college.CollegeFullName}</strong>. Please try with different programme for this college.";
                    return response;
                }
                else
                {
                    string registrationCode, registrationNumber;
                    List<PersonalInformationCompact> studentsOfCourse;
                    foreach (var course in collegeCourses)
                    {
                        registrationCode = GetRegistrationCode(programme, course, college.CollegeCode);
                        studentsOfCourse = GetStudentsForCourse(programme, course.Course_ID, college.College_ID, batchToAssign);

                        if (studentsOfCourse == null)
                        {
                            NoStudentsForCourses += $" {course.CourseFullName},";
                        }
                        else
                        {
                            registrationNumber = "CUS-" + batchToAssign.ToString().Substring(2, 2) + "-" + registrationCode.Trim().ToUpper() + "-";
                            programme = new GeneralFunctions().MappingTable(programme);
                            NoOfRecordsEffected += UpdateRegistrationNumbers(registrationNumber, ref Count, GapBetweenCourses, programme, studentsOfCourse);
                            allotedRegistrationCourses.Add(course.CourseFullName);
                        }
                    }
                }

                if (college != null)
                {
                    response.SuccessMessage = $" <strong> Registration Numbers for <span style=\"color:red \">{NoOfRecordsEffected} students </span> alloted successfully to <span style=\"color:red \">" + college.CollegeFullName + "</span></strong>";

                    if (NoStudentsForCourses.IsNotNullOrEmpty() && NoOfRecordsEffected <= 0)
                        response.ErrorMessage += $" No student was found for the courses <strong>{NoStudentsForCourses.TrimEnd(',')}</strong>";
                }

                response.SecondaryResponseObject = allotedRegistrationCourses;
            }
            return response;
        }

        private int UpdateRegistrationNumbers(string registrationNumber, ref int Count, int GapBetweenCourses, PrintProgramme programme, List<PersonalInformationCompact> studentsOfCourse)
        {
            string sqlQuery = string.Empty;
            int result = 0;
            int previousCount = new RegistrationDB().GetPreviousSchoolRegistrationCount(registrationNumber);


            if (previousCount > 0 && studentsOfCourse.Count > 20)           // If new registrations are not assigned in bulk 
                Count = ((int)Math.Ceiling(previousCount / 100.0) * 100);   //i.e assignment is for individual student or few students.
            else if (previousCount > 0)
            {
                Count = previousCount + 1;
            }

            foreach (var student in studentsOfCourse)
            {
                student.PreviousUniversityRegnNo = new RegistrationDB().GetPreviousRegnNoOfCUS(student.Batch, student.BoardRegistrationNo.ToLower().Trim().Replace("sf-",""));
                if (string.IsNullOrWhiteSpace(student.PreviousUniversityRegnNo))
                {
                    //assign new
                    sqlQuery += $"Update ARGPersonalInformation_{programme} SET CUSRegistrationNo='{(registrationNumber + Count).Trim().ToUpper()}' WHERE Student_ID = '{student.Student_ID}';" + Environment.NewLine;
                }
                else
                {
                    //assing old
                    sqlQuery += $@"Update ARGPersonalInformation_{programme} SET CUSRegistrationNo='{student.PreviousUniversityRegnNo.Trim().ToUpper()}' WHERE Student_ID = '{student.Student_ID}'" + Environment.NewLine;
                }
                Count++;
            }
            Count += GapBetweenCourses;
            Count = ((int)Math.Ceiling(Count / 100.0)) * 100;

            using (var ts = new TransactionScope())
            {
                result = new RegistrationDB().AssignRegistrationNumbersToStudent(sqlQuery);
                ts.Complete();
            }
            return result;
        }

        private List<PersonalInformationCompact> GetStudentsForCourse(PrintProgramme programme, Guid Course_ID, Guid College_ID, short batchToAssign)
        {
            Parameters parameters = new Parameters();
            parameters.Filters = new List<SearchFilter>();

            parameters.Filters.Add(new SearchFilter()
            {
                Column = nameof(PersonalInformationCompact.Course_ID),
                Operator = SQLOperator.EqualTo,
                Value = Course_ID.ToString(),
                TableAlias = nameof(ADMCombinationMaster)
            });

            parameters.Filters.Add(new SearchFilter()
            {
                Column = nameof(PersonalInformationCompact.AcceptCollege_ID),
                Operator = SQLOperator.EqualTo,
                Value = College_ID.ToString()
            });

            parameters.Filters.Add(new SearchFilter()
            {
                Column = nameof(PersonalInformationCompact.CUSRegistrationNo),
                Operator = SQLOperator.ISNULL,
                Value = ""
            });

            return new StudentManager().GetStudentsForCourse(parameters, programme, batchToAssign);
        }

        private string GetRegistrationCode(PrintProgramme programme, CourseCollegeWidget course, string CollegeCode)
        {
            string registrationCode = string.Empty;

            switch (programme)
            {
                case PrintProgramme.UG:
                    registrationCode = CollegeCode;
                    break;
                case PrintProgramme.BED:
                    registrationCode = "ISE";
                    break;
                case PrintProgramme.PG:
                    registrationCode = new CourseManager().GetItem(course.Course_ID)?.SchoolCode;
                    break;
                case PrintProgramme.IH:
                    registrationCode = new CourseManager().GetItem(course.Course_ID)?.SchoolCode;
                    break;

                default:
                    break;
            }

            return registrationCode;
        }

        public int GetNewAdmissionsStudentCountWithFees()
        {
            return new RegistrationDB().GetNewAdmissionsStudentCountWithFees();
        }

        public int GetNewAdmissionsStudentCountWithoutFees()
        {
            return new RegistrationDB().GetNewAdmissionsStudentCountWithoutFees();
        }
        public int GetGraduationOnlyNewAdmissionsStudentCountWithFees()
        {
            return new RegistrationDB().GetGraduationOnlyNewAdmissionsStudentCountWithFees();
        }
        private ResponseData ValidateRequestForAssignment(Guid college_ID, PrintProgramme programme, short batchToAssign)
        {
            ResponseData response = new ResponseData() { IsSuccess = true };

            var studentsWithoutRegistrationNumbers = new RegistrationDB().GetTotalNewStudentsForCollege(college_ID, programme, batchToAssign);
            var RegistrationNumberAssignedFor = new RegistrationDB().GetRegistrationNumberAssignedFor(college_ID, programme, batchToAssign);

            if ((studentsWithoutRegistrationNumbers == RegistrationNumberAssignedFor || studentsWithoutRegistrationNumbers == 0) && college_ID != Guid.Empty)
            {
                response.IsSuccess = false;
                response.ErrorMessage = " The Registration Numbers for this College and the chosen Programme are already assigned.";
            }

            return response;
        }
        #endregion

        #region New Registration
        public Tuple<bool, ARGStudentPreviousQualifications> ValidatePGFormAcademicDetails(Guid Course_ID, List<ARGStudentPreviousQualifications> studentPreviousAcademics)
        {
            //validate academic details
            if (Course_ID == Guid.Parse("307ae1db-9bb5-4e9c-87d6-608579142401"))
            {
                if (!studentPreviousAcademics.Any(x => x.ExamName.Trim() == "PG")) // PG Integrated B.Ed-M.Ed
                {
                    return Tuple.Create<bool, ARGStudentPreviousQualifications>(false, null);
                }
                else
                {
                    return Tuple.Create(true, studentPreviousAcademics.First(x => x.ExamName.Trim() == "PG"));
                }
            }

            if (Course_ID == Guid.Parse("c23dd7f4-a933-4deb-ba8e-f933830bccf8"))
            {
                if (!studentPreviousAcademics.Any(x => x.ExamName.Trim() == "B.Ed")) // PG M.Ed
                {
                    return Tuple.Create<bool, ARGStudentPreviousQualifications>(false, null);
                }
                else
                {
                    return Tuple.Create(true, studentPreviousAcademics.First(x => x.ExamName.Trim() == "B.Ed"));
                }
            }

            if ((Course_ID != Guid.Parse("c23dd7f4-a933-4deb-ba8e-f933830bccf8")
                && Course_ID != Guid.Parse("307ae1db-9bb5-4e9c-87d6-608579142401"))
                && !studentPreviousAcademics.Any(x => x.ExamName.Trim() == "GRADUATION"))  // PG others
            {
                return Tuple.Create<bool, ARGStudentPreviousQualifications>(false, null);
            }
            else
            {
                return Tuple.Create(true, studentPreviousAcademics.First(x => x.ExamName.Trim() == "GRADUATION"));
            }
        }

        public int Save(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme, Guid? User_ID = null)
        {
            int result = 0;
            if (new CategoryManager().GetCategoryList().FindIndex(x => x.Value.Trim().ToLower() == aRGPersonalInformation.Category.Trim().ToLower()) == -1)
                return 0;

            printProgramme = MappingTable(printProgramme);
            RegistrationDB registrationDB = new RegistrationDB();

            aRGPersonalInformation.Student_ID = Guid.NewGuid();

            //fill board data
            string[] disttAndState = aRGPersonalInformation.StudentAddress.District.Split('|');
            aRGPersonalInformation.StudentAddress.District = disttAndState[0];
            aRGPersonalInformation.StudentAddress.State = disttAndState[1];
            string[] assembly = aRGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
            aRGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
            aRGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];

            try
            {
                if (CheckBoardRegNoExists(aRGPersonalInformation.BoardRegistrationNo, printProgramme))
                    return -1;

                FillPersonalInfoWithBoardData(aRGPersonalInformation);
                aRGPersonalInformation.DOB = ConvertEnterdDOBToDateTime(aRGPersonalInformation.EnteredDOB);
                aRGPersonalInformation.BoardRegistrationNo = aRGPersonalInformation.BoardRegistrationNo.Trim();


                aRGPersonalInformation.AcademicDetails.ForEach(x =>
                {
                    x.Student_ID = aRGPersonalInformation.Student_ID;
                    x.CreatedOn = DateTime.UtcNow;
                    x.Qualification_ID = Guid.NewGuid();
                });


                aRGPersonalInformation.CoursesApplied.ForEach(x =>
                {
                    x.Student_ID = aRGPersonalInformation.Student_ID;
                });


                aRGPersonalInformation.StudentAddress.Student_ID = aRGPersonalInformation.Student_ID;
                aRGPersonalInformation.StudentAddress.CreatedOn = DateTime.UtcNow;
                aRGPersonalInformation.CreatedOn = DateTime.UtcNow;
                aRGPersonalInformation.StudentAddress.Address_ID = Guid.NewGuid();
                aRGPersonalInformation.CurrentSemesterOrYear = 0;
                aRGPersonalInformation.FormStatus = FormStatus.InProcess;
                aRGPersonalInformation.Photograph = GeneralFunctions.GetStudentPhotoPath(printProgramme) + aRGPersonalInformation.Student_ID + System.IO.Path.GetExtension(aRGPersonalInformation.PhotographPath.FileName);

                if (User_ID != null && User_ID != Guid.Empty)
                    aRGPersonalInformation.CreatedBy = User_ID;

                bool IsJkStudent = (aRGPersonalInformation.StudentAddress.State.ToLower().Trim() != "other");

                //add common courses 
                AddCommonCourses(ref aRGPersonalInformation);

                ARGFormNoMaster aRGFormNoMaster = registrationDB.GetFromNoMaster(printProgramme);
                aRGPersonalInformation.StudentFormNo = ($"{aRGFormNoMaster.FormPrefix}{aRGFormNoMaster.FormNoCount}").ToUpper().Trim();
                aRGPersonalInformation.Batch = (aRGFormNoMaster.BatchToSet <= 0 || aRGFormNoMaster.BatchToSet.ToString().Length < 4) ? Convert.ToInt16(DateTime.UtcNow.Year) : aRGFormNoMaster.BatchToSet;

                if (printProgramme == PrintProgramme.IH)
                    aRGPersonalInformation.TotalFee = CalculateFeeFor_IHP_Only(aRGPersonalInformation.CoursesApplied.ToList(), aRGFormNoMaster, IsJkStudent, false);
                else if (printProgramme == PrintProgramme.UG)
                    aRGPersonalInformation.TotalFee = CalculateFeeFor_UG_Only(IsJkStudent, aRGPersonalInformation.BoardRegistrationNo, aRGPersonalInformation.Batch, aRGFormNoMaster, false, aRGPersonalInformation.CoursesApplied.Select(x => x.Course_ID).ToList());
                else if (printProgramme == PrintProgramme.PG)
                    aRGPersonalInformation.TotalFee = CalculateFeeFor_PG_Only(aRGPersonalInformation.CoursesApplied.Select(x => x.Course_ID).ToList(), aRGFormNoMaster, IsJkStudent, false);

                using (var ts = new TransactionScope())
                {
                    registrationDB.UpdateFromNoMaster(printProgramme);
                    result = registrationDB.Save(aRGPersonalInformation, printProgramme);
                    if (result > 0)
                    {
                        registrationDB.Save(aRGPersonalInformation.StudentAddress, printProgramme);
                        foreach (var item in aRGPersonalInformation.CoursesApplied)
                            registrationDB.Save(item, printProgramme);

                        foreach (var item in aRGPersonalInformation.AcademicDetails)
                            registrationDB.Save(item, printProgramme);

                        #region Upload Form Certificate
                        Certificate certificate = new Certificate
                        {
                            Student_ID = aRGPersonalInformation.Student_ID,
                            VerificationStatus = VerificationStatus.Pending,
                            UploadingDate = DateTime.Now
                        };

                        if (aRGPersonalInformation.CategoryCertificate != null)
                        {
                            string CategoryCertificatePath = GeneralFunctions.GetStudentCertificatePath(printProgramme) + aRGPersonalInformation.StudentFormNo + "_Adm_" + CertificateType.Category.ToString() + "_" + aRGPersonalInformation.Batch + System.IO.Path.GetExtension(aRGPersonalInformation.CategoryCertificate.FileName);

                            certificate.Certificate_ID = Guid.NewGuid();
                            certificate.CertificateType = CertificateType.Category;
                            certificate.CertificateUrl = CategoryCertificatePath;

                            if (new StudentDB().Save(certificate, printProgramme) > 0)
                            {
                                Helper.UploadImage(aRGPersonalInformation.CategoryCertificate.InputStream, CategoryCertificatePath);
                            }
                        }
                        if (aRGPersonalInformation.MarksCertificate != null)
                        {
                            string MarksCardPath = GeneralFunctions.GetStudentCertificatePath(printProgramme) + aRGPersonalInformation.StudentFormNo + "_Adm_" + CertificateType.MarksCard.ToString() + "_" + aRGPersonalInformation.Batch + System.IO.Path.GetExtension(aRGPersonalInformation.MarksCertificate.FileName);

                            certificate.Certificate_ID = Guid.NewGuid();
                            certificate.CertificateType = CertificateType.MarksCard;
                            certificate.CertificateUrl = MarksCardPath;

                            if (new StudentDB().Save(certificate, printProgramme) > 0)
                            {
                                Helper.UploadImage(aRGPersonalInformation.MarksCertificate.InputStream, MarksCardPath);
                            }
                        }
                        #endregion

                        Helper.CompressAndUploadImage(aRGPersonalInformation.PhotographPath.InputStream, 259, 194, aRGPersonalInformation.Photograph);
                    }
                    else
                    {
                        throw new TransactionAbortedException();
                    }
                    ts.Complete();
                }
            }
            catch (TransactionAbortedException)
            {
                result = 0;
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                result = 0;
            }
            return result;
        }
        public void AddGraduationCourse(ARGPersonalInformation _ARGPersonalInformation)
        {
            //add Graduation Course if not selected
            if (_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e")))
            {
                if (!_ARGPersonalInformation.CoursesApplied.Any(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e") && x.IsClicked))
                {
                    _ARGPersonalInformation.CoursesApplied.First(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e")).IsClicked = true;
                }
            }
        }

        public void AddGraduationCourse(List<ARGCoursesApplied> AdmissionCoursesApplied)
        {
            //add Graduation Course if not selected
            if (AdmissionCoursesApplied.Any(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e")))
            {
                if (!AdmissionCoursesApplied.Any(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e") && x.IsClicked))
                {
                    AdmissionCoursesApplied.First(x => x.Course_ID == Guid.Parse("a3ee7f98-7b82-4d95-a2c0-faba7a18240e")).IsClicked = true;
                }
            }
        }
        public void AddCommonCourses(ref ARGPersonalInformation personalInformation)
        {
            return;
            //Tuple<string, List<Guid[]>> courseGroups = new EntranceCentersDB().GetCourseGrouping();
            //if (personalInformation?.CoursesApplied.IsNotNullOrEmpty() ?? false)
            //{
            //    CourseManager courseManager = new CourseManager();
            //    List<ARGCoursesApplied> TempCoursesApplieds = new List<ARGCoursesApplied>();
            //    TempCoursesApplieds.AddRange(personalInformation.CoursesApplied);

            //    Guid[] Course_IDs = null;
            //    ADMCourseMaster CourseDetails = null;
            //    Guid Course_ID = Guid.Empty;
            //    foreach (ARGCoursesApplied item in personalInformation.CoursesApplied)
            //    {
            //        Course_IDs = courseGroups.Item2.FirstOrDefault(x => x.Contains(item.Course_ID));

            //        if (Course_IDs.IsNotNullOrEmpty())
            //        {
            //            Course_ID = Course_IDs.First(x => x != item.Course_ID);
            //            if (!TempCoursesApplieds.Any(x => x.Course_ID == Course_ID))
            //            {
            //                CourseDetails = courseManager.GetCompactItem(Course_ID);
            //                TempCoursesApplieds.Add(
            //                    new ARGCoursesApplied
            //                    {
            //                        Course_ID = Course_ID,
            //                        Preference = 0,
            //                        Student_ID = personalInformation.Student_ID,
            //                        CourseName = CourseDetails.CourseFullName,
            //                        CourseCode = CourseDetails.CourseCode
            //                    });
            //            }
            //        }

            //    }
            //    personalInformation.CoursesApplied = TempCoursesApplieds;
            //}
        }

        public ARGFormNoMaster GetFormNoMaster(PrintProgramme programme)
        {
            programme = MappingTable(programme);
            return new RegistrationDB().GetFromNoMaster(programme);
        }

        public int GetBatchForAssigningRegistrationNumbers()
        {
            return new RegistrationDB().GetBatchForAssigningRegistrationNumbers();
        }

        public List<SelectListItem> GetStudentsSelectListItem()
        {
            return new RegistrationDB().GetStudentsSelectListItem();
        }

        public List<NewAdmissionsWidget> GetNewAdmissionsStatistics(Parameters parameters, PrintProgramme programme)
        {
            var list = new RegistrationDB().GetNewAdmissionsStatistics(parameters, programme);

            if (programme == PrintProgramme.IH && list.IsNotNullOrEmpty())
            {
                var commonAptitudeTest = new RegistrationDB().GetCountForAptitude(list.First().Batch);
                list.Insert(0, commonAptitudeTest);
            }
            return list;
        }

        public List<CourseWiseGenderCountWidget> GetCourseWiseGenderCount(PrintProgramme? programme, int? batch, Guid? college_ID, string courseFullName)
        {
            return new RegistrationDB().GetCourseWiseGenderCount(batch, college_ID, programme, courseFullName);
        }

        public List<AdmissionFormWidgetCourseWise> GetCourseWiseCountForProgramme(PrintProgramme? programme, int? batch, Guid? college_ID)
        {
            return new RegistrationDB().GetCourseWiseCountForProgramme(batch, college_ID, programme);
        }

        public List<AdmissionFormWidgetCourseWise> GetProgrammeWiseCount(PrintProgramme programme, int? batch)
        {
            return new RegistrationDB().GetProgrammeWiseCount(batch, programme);
        }

        public List<AdmissionFormWidget> GetAdmissionFormMainWidget(PrintProgramme? programme, int? batch, Guid? college_ID)
        {
            return new RegistrationDB().GetAdmissionFormMainWidget(batch, college_ID, programme);
        }

        public int UpdateFormStatus(Guid student_ID, Programme programme)
        {
            return new RegistrationDB().UpdateFormStatus(student_ID, programme);
        }

        public int UpdateStudentInfo(Guid acceptCollege_ID, Guid acceptedBy_ID, Guid student_ID, Programme Programme)
        {
            return new RegistrationDB().UpdateAppUserInfo(acceptCollege_ID, acceptedBy_ID, student_ID, Programme);
        }

        internal int UpdateFormStatus(FormStatus status, Guid student_ID)
        {
            return new RegistrationDB().UpdateFormStatus(status, student_ID, PrintProgramme.UG);
        }

        internal List<ARGSelectedCombination> GetSelectedCombinations(Guid student_ID, PrintProgramme programme)
        {
            return new RegistrationDB().GetSelectedCombinations(student_ID, programme);
        }

        public bool ValidateDOB(string EnteredDOB)
        {
            if (!string.IsNullOrWhiteSpace(EnteredDOB))
            {
                string[] splitDate = EnteredDOB.SplitDate();
                if (splitDate != null && splitDate.Count() == 3)
                {
                    try
                    {
                        new DateTime(Convert.ToInt32(splitDate[2]), Convert.ToInt32(splitDate[1]), Convert.ToInt32(splitDate[0]));
                        return true;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return false;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                }
            }
            return false;
        }


        public DateTime ConvertEnterdDOBToDateTime(string EnteredDOB)
        {
            if (!string.IsNullOrWhiteSpace(EnteredDOB))
            {
                string[] splitDate = EnteredDOB.SplitDate();
                if (splitDate != null && splitDate.Count() == 3)
                {
                    try
                    {
                        DateTime dateTime = new DateTime(Convert.ToInt32(splitDate[2]), Convert.ToInt32(splitDate[1]), Convert.ToInt32(splitDate[0]));
                        if (dateTime.Date <= new DateTime(1920, 01, 01).Date)
                            return DateTime.MinValue;
                        else
                            return dateTime;
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }
            return DateTime.MinValue;
        }

        public ARGBoardRegistrationData GetBoardRegistrationData(string BoardRegistrationNo)
        {
            if (string.IsNullOrWhiteSpace(BoardRegistrationNo))
                return new ARGBoardRegistrationData();
            return new RegistrationDB().GetBoardRegistrationData(BoardRegistrationNo.Trim()) ?? new ARGBoardRegistrationData();
        }

        private void FillPersonalInfoWithBoardData(ARGPersonalInformation aRGPersonalInformation)
        {
            ARGBoardRegistrationData _ARGBoardRegistrationData = new RegistrationDB().GetBoardRegistrationData(aRGPersonalInformation.BoardRegistrationNo);
            if (_ARGBoardRegistrationData == null)
                return;
            if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.BoardRegistrationNo))
            {
                if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.FullName))
                {
                    aRGPersonalInformation.FullName = _ARGBoardRegistrationData.FullName.ToUpper();
                }
                if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.FathersName))
                {
                    aRGPersonalInformation.FathersName = _ARGBoardRegistrationData.FathersName.ToUpper();
                }
                if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.MothersName))
                {
                    aRGPersonalInformation.MothersName = _ARGBoardRegistrationData.MothersName.ToUpper();
                }
                if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.DisplayDOBAs))
                {
                    aRGPersonalInformation.EnteredDOB = _ARGBoardRegistrationData.DisplayDOBAs.Trim();
                }
                if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.Gender))
                {
                    aRGPersonalInformation.Gender = _ARGBoardRegistrationData.Gender.Trim();
                }

                foreach (var item in aRGPersonalInformation.AcademicDetails)
                {
                    if (item.ExamName.ToLower().Trim() != "12th")
                        continue;
                    if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.Session))
                    {
                        item.Session = _ARGBoardRegistrationData.Session.Trim();
                    }
                    if (_ARGBoardRegistrationData.YearOfPassing > 0)
                    {
                        item.Year = (int)_ARGBoardRegistrationData.YearOfPassing;
                    }
                    if (!string.IsNullOrWhiteSpace(_ARGBoardRegistrationData.RollNo))
                    {
                        item.RollNo = _ARGBoardRegistrationData.RollNo.ToUpper().Trim();
                    }
                    if (_ARGBoardRegistrationData.MaxMarks > 0)
                    {
                        item.MaxMarks = (int)_ARGBoardRegistrationData.MaxMarks;
                    }
                    if (_ARGBoardRegistrationData.MarksObt > 0)
                    {
                        item.MarksObt = (int)_ARGBoardRegistrationData.MarksObt;
                    }
                    break;
                }
            }
        }

        public Guid GetStudentIDByFormNoAndDOB(ARGReprint aRGReprint)
        {
            if (aRGReprint != null && (string.IsNullOrWhiteSpace(aRGReprint.FormNo) || aRGReprint.DOB == DateTime.MinValue))
                return Guid.Empty;
            aRGReprint.PrintProgrammeOption = MappingTable(aRGReprint.PrintProgrammeOption);
            return new RegistrationDB().GetStudentIDByFormNoAndDOB(aRGReprint);
        }


        public Guid GetStudentIDByFormNoAndDOBCUS(ARGReprint aRGReprint)
        {
            if (aRGReprint != null && (string.IsNullOrWhiteSpace(aRGReprint.FormNo) || aRGReprint.DOB == DateTime.MinValue))
                return Guid.Empty;
            aRGReprint.PrintProgrammeOption = MappingTable(aRGReprint.PrintProgrammeOption);
            return new RegistrationDB().GetStudentIDByFormNoAndDOBCUS(aRGReprint);
        }

        public Guid GetStudentIDByFormNoEntranceRollNoAndDOBCUS(ARGReprint aRGReprint)
        {
            if (aRGReprint != null && (string.IsNullOrWhiteSpace(aRGReprint.FormNo) || aRGReprint.DOB == DateTime.MinValue))
                return Guid.Empty;
            aRGReprint.PrintProgrammeOption = MappingTable(aRGReprint.PrintProgrammeOption);
            return new RegistrationDB().GetStudentIDByFormNoEntranceRollNoAndDOBCUS(aRGReprint);
        }

        public ARGPersonalInformation GetStudentByID(Guid ID, PrintProgramme printProgramme, short Semester = 0)
        {
            printProgramme = MappingTable(printProgramme);
            RegistrationDB _RegistrationDB = new RegistrationDB();
            ARGPersonalInformation ARGPersonalInformation = _RegistrationDB.GetStudentByID(ID, printProgramme);
            if (ARGPersonalInformation == null)
                return null;

            if (Semester > 0)
            {
                ARGPersonalInformation.Batch = new StudentManager().getStudentSemesterBatch(ID, printProgramme, Semester);
            }

            ARGPersonalInformation.StudentAddress = _RegistrationDB.GetStudentAddress(ID, printProgramme);
            ARGPersonalInformation.AcademicDetails = _RegistrationDB.GetStudentAcademicDetails(ID, printProgramme);
            ARGPersonalInformation.CoursesApplied = _RegistrationDB.GetStudentCoursesApplied(ID, printProgramme);
            ARGPersonalInformation.PaymentDetail = new PaymentManager().GetPaymentDetails(ID, PaymentModuleType.Admission, printProgramme);
            return ARGPersonalInformation;
        }

        public ARGPersonalInformation GetStudent(Guid Student_ID, PrintProgramme printProgramme, bool WithAddress = false
            , bool WithAcademicDetail = false, bool WithCoursesApplied = false, bool WithPaymentDetails = false)
        {
            printProgramme = MappingTable(printProgramme);
            RegistrationDB _RegistrationDB = new RegistrationDB();
            ARGPersonalInformation ARGPersonalInformation = _RegistrationDB.GetStudentByID(Student_ID, printProgramme);
            if (ARGPersonalInformation == null)
                return null;
            ARGPersonalInformation.StudentAddress = WithAddress ? _RegistrationDB.GetStudentAddress(Student_ID, printProgramme) : null;
            ARGPersonalInformation.AcademicDetails = WithAcademicDetail ? _RegistrationDB.GetStudentAcademicDetails(Student_ID, printProgramme) : null;
            ARGPersonalInformation.CoursesApplied = WithCoursesApplied ? _RegistrationDB.GetStudentCoursesApplied(Student_ID, printProgramme) : null;
            ARGPersonalInformation.PaymentDetail = WithPaymentDetails ? new PaymentManager().GetPaymentDetails(Student_ID, PaymentModuleType.SemesterAdmission, printProgramme) : null;
            return ARGPersonalInformation;
        }

        public List<ARGCoursesApplied> GetStudentCourses(Guid student_ID, PrintProgramme programme)
        {
            programme = MappingTable(programme);
            return new RegistrationDB().GetStudentCoursesApplied(student_ID, programme);
        }

        public ARGStudentAddress GetStudentAddress(Guid student_ID, PrintProgramme printProgramme)
        {
            return new RegistrationDB().GetStudentAddress(student_ID, printProgramme);
        }

        public List<ARGStudentPreviousQualifications> GetStudentAcademicDetails(Guid student_ID, PrintProgramme printProgramme)
        {
            return new RegistrationDB().GetStudentAcademicDetails(student_ID, printProgramme);
        }

        public List<ARGCoursesApplied> GetStudentCoursesApplied(List<Guid> course_Ids)
        {
            return new RegistrationDB().GetStudentCoursesApplied(course_Ids);
        }

        public List<PrintProgramme> GetAdmitCardCategory()
        {
            List<PrintProgramme> list = new List<PrintProgramme>();
            foreach (var item in new RegistrationDB().GetAdmitCardCategory() ?? new List<short>())
            {
                list.Add((PrintProgramme)Enum.Parse(typeof(PrintProgramme), item.ToString()));
            }
            return list.IsNullOrEmpty() ? null : list;
        }
        public int GetAdmitCardCategoryYear(PrintProgramme printProgramme)
            => new RegistrationDB().GetAdmitCardCategoryYear(printProgramme);

        public bool IsEngLateralEntry(PrintProgramme printProgramme)
            => new RegistrationDB().IsEngLateralEntry(printProgramme);


        public ARGPersonalInformation GetStudentPersonalInfoOnly(Guid ID, PrintProgramme printProgramme, bool IncludeAddress = false)
        {
            printProgramme = MappingTable(printProgramme);
            ARGPersonalInformation _ARGPersonalInformation = new RegistrationDB().GetStudentByID(ID, printProgramme);
            if (_ARGPersonalInformation == null)
                return null;
            if (IncludeAddress)
                _ARGPersonalInformation.StudentAddress = GetStudentAddress(ID, printProgramme);
            return _ARGPersonalInformation;

        }
        public PrintProgramme MappingTable(PrintProgramme programme)
        {
            if (programme == PrintProgramme.BED)
                programme = PrintProgramme.UG;
            return programme;
        }

        public int CalculateFeeFor_IHP_Only(List<ARGCoursesApplied> coursesApplied, ARGFormNoMaster formNoMaster, bool IsJKResident, bool hasAlreadyFeePaid)
        {
            if (coursesApplied.IsNullOrEmpty() || formNoMaster == null)
                throw new Exception("Calculate Fee For IHP Only, Courses Applied/ARGFormNoMaster should not be null or empty");

            int PercourseFee = formNoMaster.PerCourse;
            int basicFee = formNoMaster.BasicFee;

            if (coursesApplied.Any(x => string.IsNullOrWhiteSpace(x.GroupName)))
            {
                coursesApplied.ForEach(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.GroupName))
                    {
                        x.GroupName = x.Course_ID.ToString();
                    }
                });
            }

            int CourseCount = coursesApplied.GroupBy(x => x.GroupName.ToLower().Trim()).Count();

            if (coursesApplied.Count == 1 && coursesApplied.Any(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
            {
                return basicFee + 50;
            }
            if (coursesApplied.Count > 1 && coursesApplied.Any(x => x.Course_ID == Guid.Parse("A3EE7F98-7B82-4D95-A2C0-FABA7A18240E")))
            {
                CourseCount--;
            }

            int totalFee = (PercourseFee * CourseCount) + basicFee;


            if (!hasAlreadyFeePaid && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }

            if (hasAlreadyFeePaid)
            {
                totalFee -= basicFee;
            }

            return totalFee;
        }
        public int CalculateFeeFor_UG_Only(bool IsJKResident, string BoardRegnNo, short Batch, ARGFormNoMaster formNoMaster, bool hasAlreadyFeePaid, List<Guid> coursesApplied)
        {
            bool hasAlreadyAppliedInIG = formNoMaster.FeeForAlreadyInIH > 0 ? new RegistrationDB().HasAlreadyAppliedInIG(BoardRegnNo, Batch) : false;
            if (hasAlreadyAppliedInIG)
                return formNoMaster.FeeForAlreadyInIH;

            int basicFee = formNoMaster.BasicFee;
            int PerSubject = formNoMaster.PerCourse;
            int totalFee = basicFee + (PerSubject * coursesApplied.Count);

            if (totalFee > 0 && !hasAlreadyFeePaid && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }
            return totalFee;
        }
        public int CalculateFeeFor_PG_Only(List<Guid> coursesApplied, ARGFormNoMaster formNoMaster, bool IsJKResident, bool hasAlreadyFeePaid)
        {
            int basicFee = formNoMaster.BasicFee;
            int PerSubject = formNoMaster.PerCourse;
            int totalFee = basicFee + (PerSubject * coursesApplied.Count);

            if (totalFee > 0 && !hasAlreadyFeePaid && !IsJKResident)
            {
                totalFee += formNoMaster.AdditionalFeeForNonJK;
            }
            return totalFee;
        }

        public void SendEmailAndSMSAsync(ARGPersonalInformation _aRGPersonalInformation, PrintProgramme printProgramme)
        {
            new TaskFactory().StartNew(() => SendEmailAndSMS(_aRGPersonalInformation, printProgramme));
        }
        public void SendEmailAndSMS(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
#if (!DEBUG)
            //SMSModel sms = new SMSModel { MobileNos = aRGPersonalInformation.StudentAddress.Mobile };
            //sms.Message = $"Form submitted successfully. {Environment.NewLine}Form No. : {aRGPersonalInformation.StudentFormNo} {Environment.NewLine}DOB : {aRGPersonalInformation.EnteredDOB}. {Environment.NewLine}Save them for future reference. Please make online payment to complete your registration.";
            //new SMSHttpPostClient().SendOTPMSG(sms);

            new EmailSystem().FormSuccessMail(aRGPersonalInformation, printProgramme);
#endif
        }

        public List<string> SendBulkSMS()
        => new RegistrationDB().SendBulkSMS();
        #endregion

        #region Get New colleges
        public List<SelectListItem> GetCollegeCourses()
        {
            return new RegistrationDB().GetCollegeCourses();
        }
        public List<string> GetColleges(Guid Course_ID)
        {
            return new RegistrationDB().GetColleges(Course_ID);
        }
        #endregion

        public void GetDDLForIntMed(ref ProceedOptions proceed)
        {
            #region ForIntMed
            new RegistrationManager().CloseNewAdmission(PrintProgramme.IH);
            PrintProgramme? programmeIntMEd = new CourseManager().GetClosedResistrationProgrammeCategoriesIBEdMEd();
            if (programmeIntMEd != null)
            {
                if (proceed.SelectListItemOptions.IsNullOrEmpty())
                    proceed.SelectListItemOptions = new List<SelectListItem>();
                proceed.SelectListItemOptions = proceed.SelectListItemOptions.Concat(new List<SelectListItem> {
                     new SelectListItem{
                          Text="Integrated B.Ed-M.Ed",
                          Value="5"
                     }
                }).ToList();
            }
            #endregion
        }

        #region CloseAdmission
        public int CloseNewAdmission(PrintProgramme printProgramme)
        {
            printProgramme = MappingTable(printProgramme);
            RegistrationDB registrationDB = new RegistrationDB();
            int result = 0;

            ARGFormNoMaster aRGFormNoMaster = registrationDB.GetFromNoMaster(printProgramme);
            if (DateTime.Now > aRGFormNoMaster.ClosingDate)
            {
                result = registrationDB.CloseNewAdmission(printProgramme);
            }
            return result;
        }
        #endregion

        #region Already Exists
        public bool CheckAlreadyExistsInAffiliated(string BoardRegnNo, int Batch, PrintProgramme PrintProgrammeOption)
        {
            return new RegistrationDB().CheckAlreadyExistsInAffiliated(BoardRegnNo, Batch, PrintProgrammeOption);
        }

        public Task<bool> CheckBoardRegNoExistsAsync(string BoardRegistrationNo, PrintProgramme programme)
        {
            return new TaskFactory().StartNew(() => CheckBoardRegNoExists(BoardRegistrationNo, programme));
        }
        public bool CheckBoardRegNoExists(string BoardRegistrationNo, PrintProgramme programme)
        {
            if (string.IsNullOrWhiteSpace(BoardRegistrationNo))
            {
                return true;
            }
            else
            {
                string FirstChar = BoardRegistrationNo.Substring(0, 1) + string.Empty;
                string LastChar = BoardRegistrationNo.Substring(BoardRegistrationNo.Length - 1, 1) + string.Empty;

                if (!(new Regex("^[a-zA-Z0-9]*$").IsMatch(FirstChar)) || !(new Regex("^[a-zA-Z0-9]*$").IsMatch(LastChar)))
                {
                    return true;
                }
                else
                {
                    programme = MappingTable(programme);
                    RegistrationDB registrationDB = new RegistrationDB();
                    ARGFormNoMaster aRGFormNoMaster = registrationDB.GetFromNoMaster(programme);
                    return registrationDB.CheckBoardRegNoExists(BoardRegistrationNo.Trim(), aRGFormNoMaster.BatchToSet, programme);
                }
            }
        }

        public Task<bool> CheckMobileExistsAsync(string MobileNo, Guid Student_ID, PrintProgramme programme, bool isdegreeIssuingForm)
        {
            return new TaskFactory().StartNew(() => CheckMobileExists(MobileNo, Student_ID, programme, isdegreeIssuingForm));
        }
        public bool CheckMobileExists(string MobileNo, Guid Student_ID, PrintProgramme programme, bool isDegreeIssuingForm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MobileNo))
                {
                    return true;
                }
                else
                {
                    if (!new Regex(@"\d{10,10}").IsMatch(MobileNo))
                    {
                        return true;
                    }
                    else
                    {
                        programme = MappingTable(programme);
                        RegistrationDB registrationDB = new RegistrationDB();

                        int BatchToSet = 0;
                        if (isDegreeIssuingForm)
                        {
                            BatchToSet = registrationDB.GetStudentNormalBatch(Student_ID, programme);
                        }
                        else
                        {
                            BatchToSet = registrationDB.GetFromNoMaster(programme).BatchToSet;
                        }
                        return registrationDB.CheckMobileNoExists(MobileNo.Trim(), BatchToSet, Student_ID, programme);
                    }
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return false;
            }
        }

        public Task<bool> CheckEmailExistsAsync(string Email, Guid Student_ID, PrintProgramme programme, bool isdegreeIssuingForm)
        {
            return new TaskFactory().StartNew(() => CheckEmailExists(Email, Student_ID, programme, isdegreeIssuingForm));
        }
        public bool CheckEmailExists(string Email, Guid Student_ID, PrintProgramme programme, bool isDegreeIssuingForm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    return true;
                }
                else
                {
                    if (!new Regex(@"^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+").IsMatch(Email))
                    {
                        return true;
                    }
                    else
                    {
                        programme = MappingTable(programme);
                        RegistrationDB registrationDB = new RegistrationDB();
                        int BatchToSet = 0;
                        if (isDegreeIssuingForm)
                        {
                            BatchToSet = registrationDB.GetStudentNormalBatch(Student_ID, programme);
                        }
                        else
                        {
                            BatchToSet = registrationDB.GetFromNoMaster(programme).BatchToSet;
                        }
                        return registrationDB.CheckEmailExists(Email.Trim(), BatchToSet, Student_ID, programme);
                    }
                }
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return false;
            }
        }
        #endregion
    }
}
