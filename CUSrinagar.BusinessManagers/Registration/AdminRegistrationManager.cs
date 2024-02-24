using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CUSrinagar.Extensions;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System.Data.SqlClient;
using CUSrinagar.DataManagers;
using System.Web.Mvc;
using Terex;
using System.Transactions;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;

namespace CUSrinagar.BusinessManagers
{
    public class AdminRegistrationManager
    {
        public Guid GetStudentIDByFormNo(ARGReprint studentDetail)
        {
            if (studentDetail != null && (string.IsNullOrWhiteSpace(studentDetail.FormNo)))
                return Guid.Empty;
            studentDetail.PrintProgrammeOption = new RegistrationManager().MappingTable(studentDetail.PrintProgrammeOption);
            return new AdminRegistrationDB().GetStudentIDByFormNo(studentDetail);
        }
        public ARGPersonalInformation GetStudentByID(Guid ID, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            ARGPersonalInformation ARGPersonalInformation = new RegistrationDB().GetStudentByID(ID, printProgramme);
            if (ARGPersonalInformation == null)
                return null;
            ARGPersonalInformation.StudentAddress = new RegistrationDB().GetStudentAddress(ID, printProgramme);
            return ARGPersonalInformation;
        }
        public List<ARGCoursesApplied> GetStudentCoursesAppliedIDs(Guid ID, PrintProgramme printProgramme)
        {
            return new AdminRegistrationDB().GetStudentCoursesAppliedIDs(ID, printProgramme);
        }
        public int EditCourses(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            int result = 0;
            RegistrationManager registrationManager = new RegistrationManager();
            printProgramme = registrationManager.MappingTable(printProgramme);
            AdminRegistrationDB registrationDB = new AdminRegistrationDB();

            List<ARGCoursesApplied> list = GetStudentCoursesAppliedIDs(aRGPersonalInformation.Student_ID, printProgramme);

            bool IsJkStudent = (new RegistrationDB().GetStudentAddress(aRGPersonalInformation.Student_ID, printProgramme).State.ToLower().Trim() != "other");
            bool AlreadyPaidFee = (printProgramme == PrintProgramme.IH) ? true : aRGPersonalInformation.FormStatus != FormStatus.InProcess;
            ARGFormNoMaster formNoMaster = registrationManager.GetFormNoMaster(printProgramme);

            decimal EntrancePointsIfAny = 0;
            bool UpdateCourse = false, appearedInEntrance = false;

            using (var ts = new TransactionScope())
            {
                if (aRGPersonalInformation.FormStatus == FormStatus.InProcess)
                {
                    registrationDB.DeleteCourse(aRGPersonalInformation.Student_ID, printProgramme);

                    registrationManager.AddCommonCourses(ref aRGPersonalInformation);

                    foreach (var item in aRGPersonalInformation.CoursesApplied)
                    {
                        EntrancePointsIfAny = (list?.FirstOrDefault(x => x.Course_ID == item.Course_ID)?.SubjectEntrancePoints) ?? 0;
                        appearedInEntrance = (list?.FirstOrDefault(x => x.Course_ID == item.Course_ID)?.AppearedInEntrance) ?? false;
                        registrationDB.Save(new ARGCoursesApplied
                        {
                            Course_ID = item.Course_ID,
                            Student_ID = aRGPersonalInformation.Student_ID,
                            Preference = item.Preference,
                            SubjectEntrancePoints = EntrancePointsIfAny,
                            AppearedInEntrance = appearedInEntrance,
                        }, printProgramme);
                        UpdateCourse = true;
                    }
                }

                bool UpdatePhoto = false;

                if (aRGPersonalInformation.PhotographPath != null)
                {
                    UpdatePhoto = true;
                    if (File.Exists(HttpContext.Current.Server.MapPath("~" + aRGPersonalInformation.Photograph)))
                    {
                        File.Delete(HttpContext.Current.Server.MapPath("~" + aRGPersonalInformation.Photograph));
                    }
                    aRGPersonalInformation.Photograph = GeneralFunctions.GetStudentPhotoPath(printProgramme) + aRGPersonalInformation.Student_ID + System.IO.Path.GetExtension(aRGPersonalInformation.PhotographPath.FileName);
                    Helper.CompressAndUploadImage(aRGPersonalInformation.PhotographPath.InputStream, 259, 194, aRGPersonalInformation.Photograph);
                }
                if (printProgramme == PrintProgramme.IH && aRGPersonalInformation.Batch > 2017)
                {
                    aRGPersonalInformation.TotalFee = registrationManager.CalculateFeeFor_IHP_Only(aRGPersonalInformation.CoursesApplied, formNoMaster, IsJkStudent, AlreadyPaidFee);
                }
                else if (printProgramme == PrintProgramme.PG)
                {
                    aRGPersonalInformation.TotalFee = registrationManager.CalculateFeeFor_PG_Only(aRGPersonalInformation.CoursesApplied.Select(x => x.Course_ID).ToList(), formNoMaster, IsJkStudent, AlreadyPaidFee);
                }
                else if (printProgramme == PrintProgramme.UG)
                {
                    aRGPersonalInformation.TotalFee = registrationManager.CalculateFeeFor_UG_Only(IsJkStudent, aRGPersonalInformation.BoardRegistrationNo, aRGPersonalInformation.Batch, formNoMaster, AlreadyPaidFee, aRGPersonalInformation.CoursesApplied.Select(x => x.Course_ID).ToList());
                }
                if (UpdateCourse || UpdatePhoto)
                {
                    registrationDB.UpdateFeeANDPhoto(aRGPersonalInformation, printProgramme, UpdatePhoto, UpdateCourse);
                }
                result = 1;
                ts.Complete();
            }

            return result;
        }

        public bool CheckStudentBelongsToCollege(Guid Student_ID, PrintProgramme printProgramme)
        {
            if (Student_ID == Guid.Empty)
                return false;

            return new AdminRegistrationDB().CheckStudentBelongsToCollege(Student_ID, new RegistrationManager().MappingTable(printProgramme));
        }

        public int EditStudentDetails(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme, Guid? RemoveQ_ID = null)
        {
            int result = 0;
            if (aRGPersonalInformation == null)
                return result;
            var AdminRegistrationDB = new AdminRegistrationDB();
            printProgramme = new RegistrationManager().MappingTable(printProgramme);

            if (string.IsNullOrWhiteSpace(aRGPersonalInformation.PreviousUniversityRegnNo) || aRGPersonalInformation.PreviousUniversityRegnNo.ToUpper().Trim() == "NULL")
                aRGPersonalInformation.PreviousUniversityRegnNo = null;

            bool BoardRegnNoIsPresentInBatch = false;
            if (!string.IsNullOrWhiteSpace(aRGPersonalInformation.BoardRegistrationNo))
                BoardRegnNoIsPresentInBatch = new RegistrationDB().CheckBoardRegNoExists(aRGPersonalInformation.BoardRegistrationNo, aRGPersonalInformation.Student_ID, aRGPersonalInformation.Batch, printProgramme);
            if (BoardRegnNoIsPresentInBatch)
                return -1;

            try
            {
                using (var ts = new TransactionScope())
                {
                    #region Update PersonalInfo and Address
                    if (aRGPersonalInformation.EditPhotograph != null)
                    {
                        if (File.Exists(HttpContext.Current.Server.MapPath("~" + aRGPersonalInformation.Photograph))
                            && !(aRGPersonalInformation.Photograph ?? "").ToLower().Contains("defaultstudentphoto.jpeg"))
                        {
                            File.Delete(HttpContext.Current.Server.MapPath("~" + aRGPersonalInformation.Photograph));
                        }
                        aRGPersonalInformation.Photograph = GeneralFunctions.GetStudentPhotoPath(printProgramme) + aRGPersonalInformation.Student_ID + System.IO.Path.GetExtension(aRGPersonalInformation.EditPhotograph.FileName);
                        Helper.CompressAndUploadImage(aRGPersonalInformation.EditPhotograph.InputStream, 259, 194, aRGPersonalInformation.Photograph);
                    }

                    aRGPersonalInformation.SetWorkFlow(RecordState.Old);
                    aRGPersonalInformation.StudentAddress.SetWorkFlow(RecordState.Old);
                    aRGPersonalInformation.DOB = new RegistrationManager().ConvertEnterdDOBToDateTime(aRGPersonalInformation.EnteredDOB);
                    result = AdminRegistrationDB.UpdateStudentDetails(aRGPersonalInformation, printProgramme);
                    if (result > 0)
                    {
                        aRGPersonalInformation.StudentAddress.Student_ID = aRGPersonalInformation.Student_ID;

                        string[] disttAndState = aRGPersonalInformation.StudentAddress.District.Split('|');
                        aRGPersonalInformation.StudentAddress.District = disttAndState[0];
                        aRGPersonalInformation.StudentAddress.State = disttAndState[1];

                        string[] assembly = aRGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
                        aRGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
                        aRGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];

                        if (AdminRegistrationDB.UpdateStudentAddress(aRGPersonalInformation.StudentAddress, printProgramme) <= 0)
                        {
                            result = 0;
                            throw new TransactionAbortedException();
                        }

                        //update qualifications
                        if (!RemoveQ_ID.IsNullOrEmpty() && !aRGPersonalInformation.IsLateralEntry)
                        {
                            AdminRegistrationDB.DeleteStudentQualification((Guid)RemoveQ_ID, printProgramme);
                            aRGPersonalInformation.AcademicDetails.RemoveAt(aRGPersonalInformation.AcademicDetails.FindIndex(x => x.Qualification_ID == RemoveQ_ID));
                        }
                        else if (aRGPersonalInformation.IsLateralEntry && aRGPersonalInformation.AcademicDetails.IsNullOrEmpty())
                        {
                            new RegistrationDB().DeleteAcademicDetailsAll(aRGPersonalInformation.Student_ID, printProgramme);
                        }
                        if (aRGPersonalInformation.AcademicDetails.IsNotNullOrEmpty())
                        {
                            foreach (var item in aRGPersonalInformation.AcademicDetails)
                            {
                                if (item.Qualification_ID != Guid.Empty)
                                {
                                    item.SetWorkFlow(RecordState.Old);
                                    AdminRegistrationDB.UpdateStudentQualification(item, printProgramme);
                                }
                                else
                                {
                                    item.SetWorkFlow(RecordState.New);
                                    item.Student_ID = aRGPersonalInformation.Student_ID;
                                    item.Qualification_ID = Guid.NewGuid();
                                    new RegistrationDB().Save(item, printProgramme);
                                }
                            }
                        }
                    }
                    #endregion

                    ts.Complete();
                }
            }
            catch (TransactionAbortedException) { }
            return result;
        }
    }
}
