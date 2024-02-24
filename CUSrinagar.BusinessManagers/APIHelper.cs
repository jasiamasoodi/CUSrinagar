using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CUSrinagar.Models;
using System.Text;
using CUSrinagar.Enums;
using CUSrinagar.DataManagers;
using System.Net;
using System.Web.Hosting;
using System.Threading.Tasks;
using System.Collections.Generic;
using CUSrinagar.Extensions;
using System.Text.RegularExpressions;
using System.Linq;
using System.Transactions;
using System.Data.SqlClient;

namespace CUSrinagar.BusinessManagers
{
    public class APIHelper
    {
        #region common API Post Method
        public static Tuple<bool, string> PostAsync<T>(string Url, T dataToPost, string token) where T : class
        {
            string result = string.Empty;
            bool isSuccess = false;

            using (HttpClient _AppClient = new HttpClient())
            {
                _AppClient.DefaultRequestHeaders.Accept.Clear();
                _AppClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(token))
                    _AppClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                string json = JsonConvert.SerializeObject(dataToPost);
                StringContent jsonDataToPost = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _AppClient.PostAsync(Url, jsonDataToPost).Result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    isSuccess = true;
                }
                else
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            return Tuple.Create(isSuccess, result);
        }

        #endregion


        #region BILLDESK S2S
        public static string PostAsync(string Url)
        {
            var result = string.Empty;
            using (HttpClient _AppClient = new HttpClient())
            {
                _AppClient.BaseAddress = new Uri(BillDeskSettings.BillDeskBaseUrlS2S);
                _AppClient.DefaultRequestHeaders.Accept.Clear();
                _AppClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = _AppClient.PostAsync(Url, null).Result;
                if (response.IsSuccessStatusCode)
                    result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
        }

        #endregion


        #region CUET/NTA
        private static string CUEToAuthAPIToken()
        {
            string token = "D9G1OGDvd3RwQkivsx1H9RDNASSHdTrY";
#if (!DEBUG)
                   token = "_3njkDmkPsvtS542Z-PYnB0UQ6QeSkmJ";
#endif
            return token;
        }

        public static CUETDocuments GetCandidatesDocuments(CUSCUETAPIRequest CUETAPIRequest)
        {
            string url = "https://apicuet.samarth.ac.in/index.php/v1/photos-api";

            CUETAPIRequestModel requestModel = new CUETAPIRequestModel
            {
                ApplicationNo = CUETAPIRequest.ApplicationNo,
                Day = CUETAPIRequest.DOB.ToString("dd"),
                Month = CUETAPIRequest.DOB.ToString("MM"),
                Year = CUETAPIRequest.DOB.ToString("yyyy"),
            };

            string token = CUEToAuthAPIToken();
            Tuple<bool, string> APIResponse = PostAsync(url, requestModel, token);

            if (APIResponse.Item1)
            {
                return JsonConvert.DeserializeObject<CUETDocuments>(APIResponse.Item2);
            }
            else
            {
                CUETError error = JsonConvert.DeserializeObject<CUETError>(APIResponse.Item2);
                return new CUETDocuments
                {
                    CUETErrors = error
                };
            }
        }

        public static CUETRegistrationData GetCandidatesRegistrationData(CUSCUETAPIRequest CUETAPIRequest)
        {
            string url = "https://apicuet.samarth.ac.in/index.php/v1/data-api";

            CUETAPIRequestModel requestModel = new CUETAPIRequestModel
            {
                ApplicationNo = CUETAPIRequest.ApplicationNo,
                Day = CUETAPIRequest.DOB.ToString("dd"),
                Month = CUETAPIRequest.DOB.ToString("MM"),
                Year = CUETAPIRequest.DOB.ToString("yyyy"),
            };

            string token = CUEToAuthAPIToken();
            Tuple<bool, string> APIResponse = PostAsync(url, requestModel, token);

            if (APIResponse.Item1)
            {
                CUETRegistrationData CUETAPIdate = JsonConvert.DeserializeObject<CUETRegistrationData>(APIResponse.Item2);
                CUETAPIdate.CandidateDetails.DOB = CUETAPIRequest.DOB.Date;
                CUETAPIdate.CandidateDetails.BoardRegistrationNo = CUETAPIRequest.BoardRegistrationNo;
                CUETAPIdate.CandidateDetails.PrintProgramme = CUETAPIRequest.SelectedPorgramme;

                return CUETAPIdate;
            }
            else
            {
                CUETError error = JsonConvert.DeserializeObject<CUETError>(APIResponse.Item2);
                return new CUETRegistrationData
                {
                    CUETErrors = error
                };
            }
        }

        public static Task DownloadCUETApplicantPhotographAsync(string photoURL, string localPath, PrintProgramme printProgramme)
        {
            return new TaskFactory().StartNew(() => DownloadCUETApplicantPhotograph(photoURL, localPath, printProgramme));
        }

        private static void DownloadCUETApplicantPhotograph(string photoURL, string localPath, PrintProgramme printProgramme)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFileAsync(new Uri(photoURL)
                    , HostingEnvironment.MapPath("~" + localPath));

            }
        }

        public static Tuple<ARGPersonalInformation, string> ConvertCUETToPersonalInformation(CUETRegistrationData registrationData, PrintProgramme printProgramme, Guid NewStudent_ID)
        {
            if (registrationData == null)
                return Tuple.Create<ARGPersonalInformation, string>(null, "invalid details");

            if (registrationData.CUETErrors != null)
                return Tuple.Create<ARGPersonalInformation, string>(null, registrationData.CUETErrors.Error.Message);


            if (printProgramme != PrintProgramme.UG) //temp until PG API will be available
                return Tuple.Create<ARGPersonalInformation, string>(null, "only UG allowed");

            //temporarily commented
            //if (registrationData.CandidateDetails.UniversityProgrammeSelection.SelectedCourses.IsNullOrEmpty())
            //    return Tuple.Create<ARGPersonalInformation, string>(null, "Applicant has not selected any course(s) in Cluster University Srinagar as per CUET");

            string cuetEntranceRollNo = null;
            if (registrationData.CandidateDetails.CUETResult != null)
            {
                cuetEntranceRollNo = registrationData.CandidateDetails.CUETResult["ROLL"].ToUpper().Trim();
                cuetEntranceRollNo = Regex.Replace(cuetEntranceRollNo, @"[^\d]", "");
            }

            ARGFormNoMaster formNoMaster = new RegistrationManager().GetFormNoMaster(printProgramme);

            ARGPersonalInformation personalInformation = new ARGPersonalInformation
            {
                Student_ID = NewStudent_ID,
                StudentFormNo = $"CUS-{printProgramme}-{registrationData.CandidateDetails.ApplicationNo.ToString().Trim().ToUpper()}",
                BoardRegistrationNo = registrationData.CandidateDetails.BoardRegistrationNo.ToUpper().Trim(),
                PreviousUniversityRegnNo = null,
                FullName = registrationData.CandidateDetails.Name.ToUpper().Trim(),
                FathersName = registrationData.CandidateDetails.FatherName.ToUpper().Trim(),
                MothersName = registrationData.CandidateDetails.MotherName.ToUpper().Trim(),
                Gender = registrationData.CandidateDetails.Gender.ToUpper(),
                DOB = registrationData.CandidateDetails.DOB,
                Religion = "NotAvailable",
                Category = registrationData.CandidateDetails.Category.ToLower().Contains("general/unreserved") ? "OM" : registrationData.CandidateDetails.Category,
                FormStatus = FormStatus.FeePaid,
                CATEntrancePoints = 0,
                EntranceRollNo = cuetEntranceRollNo != null ? (long?)Convert.ToInt64(formNoMaster.BatchToSet.ToString().Substring(2, 2) + cuetEntranceRollNo) : null,
                Batch = formNoMaster.BatchToSet,
                IsProvisional = string.IsNullOrWhiteSpace(registrationData.CandidateDetails.XiiMarksObtained),
                TotalFee = 0,
                CreatedBy = AppUserHelper.User_ID,
                CreatedOn = DateTime.Now,
                CUETApplicationNo = registrationData.CandidateDetails.ApplicationNo.ToString(),
                CUETEntranceRollNo = registrationData.CandidateDetails.CUETResult == null
                                    ? null
                                    : registrationData.CandidateDetails.CUETResult["ROLL"].ToUpper().Trim(),
            };

            personalInformation.StudentAddress = new ARGStudentAddress
            {
                Student_ID = NewStudent_ID,
                Address_ID = Guid.NewGuid(),
                Email = registrationData.CandidateDetails.Email,
                PinCode = "190001",
                AssemblyConstituency = "Other",
                ParliamentaryConstituency = "OTHER",
                Block = registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper().Length > 50
                ? registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper().Substring(0, 48)
                : registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper(),
                Mobile = "NotAvailable",
                CreatedBy = AppUserHelper.User_ID,
                CreatedOn = DateTime.Now,
                District = registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper(),
                PermanentAddress = registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper().Length > 140
                ? registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper().Substring(0, 137)
                : registrationData.CandidateDetails.AddDistrictPr.Trim().ToUpper(),
                State = registrationData.CandidateDetails.AddStatePr.Trim().ToUpper(),
                PProgramme = printProgramme,
            };

            if (!string.IsNullOrWhiteSpace(registrationData.CandidateDetails.XiiMarksObtained))
            {
                personalInformation.AcademicDetails = new List<ARGStudentPreviousQualifications>
                {
                    new ARGStudentPreviousQualifications
                    {
                        Student_ID=NewStudent_ID,
                        Qualification_ID=Guid.NewGuid(),
                        ExamBody=registrationData.CandidateDetails.XiiBoard.Trim().ToUpper().Length>49
                        ?registrationData.CandidateDetails.XiiBoard.Trim().ToUpper().Substring(0,48)
                        :registrationData.CandidateDetails.XiiBoard.Trim().ToUpper(),
                        ExamName="12TH",
                        IsProvisional=false,
                        RollNo="Not Available",
                        MarksObt=Convert.ToDouble(registrationData.CandidateDetails.XiiMarksObtained),
                        MaxMarks=Convert.ToDouble(registrationData.CandidateDetails.XiiMaximumMarks),
                        Year=registrationData.CandidateDetails.XiiPassingYear,
                        Subjects=registrationData.CandidateDetails.XiiCourseName.ToUpper().Trim(),
                        Stream=registrationData.CandidateDetails.XiiCourseName.ToUpper().Trim(),
                        Session="ANNUAL",
                        CreatedBy=AppUserHelper.User_ID,
                        CreatedOn = DateTime.Now,
                    }
                };
            }

            personalInformation.PaymentDetail = new List<PaymentDetails>
            {
                new PaymentDetails
                {
                    Payment_ID = Guid.NewGuid(),
                    Entity_ID = NewStudent_ID,
                    PaymentType = PaymentType.Offline,
                    ModuleType = PaymentModuleType.Admission,
                    TxnAmount = 0,
                    TxnDate = DateTime.Now,
                    TxnReferenceNo = DateTime.Now.Ticks.ToString(),
                    PhoneNumber = "NotAvailable",
                    Email = registrationData.CandidateDetails.Email,
                    BankID = "JKCUS",
                    TxnType = "INR",
                    CurrencyName = "INR",
                    PrintProgramme=printProgramme,
                }
            };

            //course applied/selected
            Programme programme = new GeneralFunctions().GetProgramme(printProgramme);
            personalInformation.CoursesApplied = new List<ARGCoursesApplied>();

            List<Guid> CourseID = null;
            string[] selCourse;
            foreach (string selectedCourse in registrationData.CandidateDetails.UniversityProgrammeSelection?.SelectedCourses ?? new List<string>())
            {
                selCourse = selectedCourse.Trim().ToLower().Replace("  ", " ").Replace("bachelor in ", "").Split(' ');

                CourseID = new CourseDB().GetCUETCourse(selCourse.Length > 2 ? selCourse[2].Trim() : selCourse[0].Trim(), programme);

                foreach (Guid course_ID in CourseID ?? new List<Guid>())
                {
                    personalInformation.CoursesApplied.Add(
                        new ARGCoursesApplied
                        {
                            Course_ID = course_ID,
                            Student_ID = NewStudent_ID,
                            IsActive = true,
                            AppearedInEntrance = true,
                            Preference = 0,
                            PrintProgramme = printProgramme,
                        });
                }
            }

            //if student has not selected Cluster University 
            if (personalInformation.CoursesApplied.IsNullOrEmpty() && programme == Programme.UG)
            {
                personalInformation.CoursesApplied.Add(
                       new ARGCoursesApplied
                       {
                           Course_ID = new Guid("39AB4D01-16EF-40DD-9DC7-FAA14C7801B6"),//History course
                           Student_ID = NewStudent_ID,
                           IsActive = true,
                           AppearedInEntrance = false,
                           Preference = 0,
                           PrintProgramme = printProgramme,
                       });
            }

            if (personalInformation.CoursesApplied.IsNullOrEmpty())
                return Tuple.Create<ARGPersonalInformation, string>(null, "Applicant selected courses are not present in Cluster University Srinagar yet");


            return Tuple.Create(personalInformation, "success");

        }

        public static Tuple<bool, string> ImportCUETDetails(CUSCUETAPIRequest CUETAPIRequest)
        {
            //-------------------- check if already imported ----------------
            #region Validation
            RegistrationManager registrationManager = new RegistrationManager();
            RegistrationDB registrationDB = new RegistrationDB();

            ARGFormNoMaster formNoMaster =
                registrationManager.GetFormNoMaster(CUETAPIRequest.SelectedPorgramme);

            bool alreadyExists =
                            registrationDB.CheckBoardRegNoOrCUETApplicationNoExists
                            (CUETAPIRequest.SelectedPorgramme, CUETAPIRequest.BoardRegistrationNo,
                            CUETAPIRequest.ApplicationNo, formNoMaster.BatchToSet);

            if (string.IsNullOrWhiteSpace(CUETAPIRequest.BoardRegistrationNo))
            {
                return Tuple.Create(false,
                    $"Board Registration no cannot be empty");
            }
            else if (CUETAPIRequest.BoardRegistrationNo.Length <= 6)
            {
                return Tuple.Create(false,
                    $"Board Registration no cannot be empty");
            }

            if (alreadyExists)
            {
                return Tuple.Create(false,
                    $"CUET ApplicationNo already imported with CUS Form No.:  " +
                    $"CUS-{CUETAPIRequest.SelectedPorgramme}-{CUETAPIRequest.ApplicationNo.ToString().Trim().ToUpper()} Please Save it.");
            }

            string CollegeName = registrationDB.CheckAlreadyHasLiveAdmissionInCollege(CUETAPIRequest.SelectedPorgramme, CUETAPIRequest.BoardRegistrationNo);

            if (!string.IsNullOrWhiteSpace(CollegeName))
            {
                return Tuple.Create(false,
                    $"Board Registration no {CUETAPIRequest.BoardRegistrationNo} has admission active in {CollegeName}");
            }

            #endregion
            //--------------------- call api cuet ----------------------------
            CUETRegistrationData cUETRegistrationData = GetCandidatesRegistrationData(CUETAPIRequest);

            if (cUETRegistrationData.CUETErrors != null)
                return Tuple.Create(false, "CUET Registration Data error " + cUETRegistrationData.CUETErrors.Error.Message);

            if ((cUETRegistrationData?.Status ?? "").Trim().ToLower() != "success")
                return Tuple.Create(false, "CUET Registration Data status " + cUETRegistrationData?.Status ?? "");


            CUETDocuments cUETDocuments = GetCandidatesDocuments(CUETAPIRequest);

            if (cUETDocuments.CUETErrors != null)
                return Tuple.Create(false, "CUET Documents error " + cUETDocuments.CUETErrors.Error.Message);

            if ((cUETDocuments?.Status ?? "").Trim().ToLower() != "success")
                return Tuple.Create(false, "CUET Documents status " + cUETDocuments?.Status ?? "");

            Guid newStudent_ID = Guid.NewGuid();

            Tuple<ARGPersonalInformation, string> convertionToCUS =
                ConvertCUETToPersonalInformation(cUETRegistrationData, CUETAPIRequest.SelectedPorgramme, newStudent_ID);

            if (convertionToCUS.Item1 == null)
                return Tuple.Create(false, "CUET converstion error " + convertionToCUS.Item2);

            //----------------- download photograph ---------------------
            string photographLocalPath =
                GeneralFunctions.GetStudentPhotoPath(CUETAPIRequest.SelectedPorgramme)
                + Guid.NewGuid() + cUETDocuments.CandidateDoc.PhotoExtension;

            DownloadCUETApplicantPhotographAsync(cUETDocuments.CandidateDoc.PhotoURL, photographLocalPath, CUETAPIRequest.SelectedPorgramme);
            convertionToCUS.Item1.Photograph = photographLocalPath;

            //---------------------- save to db --------------------------
            try
            {
                bool importStatus = false;
                using (var ts = new TransactionScope())
                {
                    int result = registrationDB.Save(convertionToCUS.Item1, CUETAPIRequest.SelectedPorgramme);
                    if (result > 0)
                    {
                        registrationDB.Save(convertionToCUS.Item1.StudentAddress, CUETAPIRequest.SelectedPorgramme);
                        foreach (ARGCoursesApplied item in convertionToCUS.Item1.CoursesApplied)
                        {
                            registrationDB.Save(item, CUETAPIRequest.SelectedPorgramme);
                        }

                        if (convertionToCUS.Item1.AcademicDetails.IsNotNullOrEmpty())
                        {
                            registrationDB.Save(convertionToCUS.Item1.AcademicDetails.First(), CUETAPIRequest.SelectedPorgramme);
                        }

                        new PaymentDB()
                            .SavePayment(convertionToCUS.Item1.PaymentDetail.First(), CUETAPIRequest.SelectedPorgramme, PaymentModuleType.Admission);

                        importStatus = true;
                    }
                    ts.Complete();
                }
                if (importStatus)
                {
                    return Tuple.Create(true, $"Data import was successfull. The CUS FormNo is: {convertionToCUS.Item1.StudentFormNo}");
                }
            }
            catch (TransactionAbortedException)
            {
                return Tuple.Create(false, "An error occurred. Please try again");
            }
            catch (SqlException SQLError) when (SQLError.Number == 1205)
            {
                return Tuple.Create(false, "An error occurred. Please try again");
            }

            return Tuple.Create(false, "Data not imported to CUSrinagar Server");
        }

        #endregion


        #region Academic Bank of Credits API

        public static string GetABCAPIToken()
        {
            Tuple<bool, string> APIResponse = PostAsync(ABCAPIUrls.AccessTokenURL, new ABCRequestToken(), null);

            if (APIResponse.Item1)
            {
                ABCResponseToken aBCResponseToken = JsonConvert.DeserializeObject<ABCResponseToken>(APIResponse.Item2);
                return aBCResponseToken.AccessToken;
            }
            else
            {
                return null;
            }
        }
        public static bool IsABCIDValid(ValidateABCID validateABCID)
        {
            if (validateABCID == null || string.IsNullOrWhiteSpace(validateABCID?.ABCID))
                return false;

            string token = GetABCAPIToken();
            Tuple<bool, string> APIResponse = PostAsync(ABCAPIUrls.AccountsBasicDetailsURL, validateABCID, token);

            if (APIResponse.Item1)
            {
                ValidateABCIDResponse validateABCIDResponse = JsonConvert.DeserializeObject<ValidateABCIDResponse>(APIResponse.Item2);
                return (validateABCIDResponse.Statuscode?.Trim() ?? "") == "200";
            }
            else
            {
                return false;
            }
        }
        public static Task<bool> IsABCIDValidAsync(ValidateABCID validateABCID)
        {
            return new TaskFactory().StartNew(() => IsABCIDValid(validateABCID));
        }

        #endregion

    }
}
