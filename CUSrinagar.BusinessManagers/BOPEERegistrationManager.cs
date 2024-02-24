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
    public class BOPEERegistrationManager
    {
        public int Save(ARGPersonalInformation aRGPersonalInformation, PrintProgramme printProgramme)
        {
            int result = 0;
            if (new CategoryManager().GetCategoryList().FindIndex(x => x.Value.Trim().ToLower() == aRGPersonalInformation.Category.Trim().ToLower()) == -1)
                return 0;
            RegistrationManager _RegistrationManager = new RegistrationManager();
            printProgramme = _RegistrationManager.MappingTable(printProgramme);
            RegistrationDB registrationDB = new RegistrationDB();
            aRGPersonalInformation.Student_ID = Guid.NewGuid();

            aRGPersonalInformation.Photograph = GeneralFunctions.GetStudentPhotoPath(printProgramme) + aRGPersonalInformation.Student_ID + System.IO.Path.GetExtension(aRGPersonalInformation.PhotographPath.FileName);
            Helper.CompressAndUploadImage(aRGPersonalInformation.PhotographPath.InputStream, 259, 194, aRGPersonalInformation.Photograph);

            //fill board data
            string[] disttAndState = aRGPersonalInformation.StudentAddress.District.Split('|');
            aRGPersonalInformation.StudentAddress.District = disttAndState[0];
            aRGPersonalInformation.StudentAddress.State = disttAndState[1];
            string[] assembly = aRGPersonalInformation.StudentAddress.AssemblyConstituency.Split('|');
            aRGPersonalInformation.StudentAddress.AssemblyConstituency = assembly[0];
            aRGPersonalInformation.StudentAddress.ParliamentaryConstituency = assembly[1];
            try
            {
                using (var ts = new TransactionScope())
                {
                    if (CheckBoardRegNoExists(aRGPersonalInformation.BoardRegistrationNo, printProgramme, aRGPersonalInformation.Batch))
                        result = -1;
                    else
                    {
                        aRGPersonalInformation.DOB = _RegistrationManager.ConvertEnterdDOBToDateTime(aRGPersonalInformation.EnteredDOB);
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
                        aRGPersonalInformation.CreatedBy = AppUserHelper.User_ID;


                        ARGFormNoMaster aRGFormNoMaster = registrationDB.GetFromNoMaster(printProgramme);
                        aRGPersonalInformation.StudentFormNo = ($"{aRGFormNoMaster.FormPrefix}{aRGFormNoMaster.FormNoCount}").ToUpper().Trim();
                        aRGPersonalInformation.CurrentSemesterOrYear = 0;

                        //add common courses 
                        //_RegistrationManager.AddCommonCourses(ref aRGPersonalInformation);
                        aRGPersonalInformation.TotalFee = 0;
                        aRGPersonalInformation.FormStatus = FormStatus.FeePaid;

                        result = registrationDB.Save(aRGPersonalInformation, printProgramme);
                        if (result > 0)
                        {
                            registrationDB.Save(aRGPersonalInformation.StudentAddress, printProgramme);
                            foreach (var item in aRGPersonalInformation.CoursesApplied)
                                registrationDB.Save(item, printProgramme);

                            foreach (var item in aRGPersonalInformation.AcademicDetails)
                                registrationDB.Save(item, printProgramme);
                            registrationDB.UpdateFromNoMaster(printProgramme);
                        }
                        else
                        {
                            throw new TransactionAbortedException();
                        }
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

        public bool CheckBoardRegNoExists(string BoardRegistrationNo, PrintProgramme programme, short Batch)
        {
            if (string.IsNullOrWhiteSpace(BoardRegistrationNo) || Batch < 2017)
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
                    programme = new RegistrationManager().MappingTable(programme);
                    return new RegistrationDB().CheckBoardRegNoExists(BoardRegistrationNo.Trim(), Batch, programme);
                }
            }
        }

        public List<short> GetBatches(PrintProgramme programme)
        {
            programme = new RegistrationManager().MappingTable(programme);
            return new RegistrationDB().GetBatchs(programme);
        }
    }
}
