using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using CUSrinagar.DataManagers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using System.Diagnostics;
using System.Web;
using GeneralModels;
using CUSrinagar.Enums;
using System.Transactions;
using CUSrinagar.Extensions;

namespace CUSrinagar.BusinessManagers
{
    public class MigrationManager
    {

        public List<MigrationPI> GetMigrationFormList(Parameters parameter)
        {
            List<MigrationPI> listMigration = new MigrationDB().GetMigrationFormList(parameter);
            return listMigration;

        }
        public List<MigrationPI> GetALLMigrationFormList(Parameters parameter)
        {
            List<MigrationPI> listMigration = new MigrationDB().GetALLMigrationFormList(parameter);
            return listMigration;

        }
        //public bool UploadDownloadForm(PaymentDetails paymentDetails, PrintProgramme printProgramme,MigrationE formType)//(ARGPersonalInformation model, DownLoadFormType formType, PrintProgramme printProgramme, string NewCollege)

        //{
        //    string[] sformnoAI = paymentDetails.AdditionalInfo.Split('@');
        //    ARGPersonalInformation student = new StudentManager().GetStudent(paymentDetails.Entity_ID, paymentDetails.PrintProgramme, false);
        //    MigrationDetail formsToDownload = new MigrationDetail()
        //    {

        //        Student_ID = paymentDetails.Entity_ID,
        //        Form_ID = Guid.NewGuid(),
        //        FormType=formType,
        //        Programme = new GeneralFunctions().GetProgramme(paymentDetails.PrintProgramme),//printProgramme
        //        FormStatus = FormStatus.InProcess,
        //        Status = true,
        //        CreatedOn = DateTime.Now,
        //        NewCollege = sformnoAI[1],
        //        OldCollege = student.AcceptCollege_ID ?? Guid.Empty,
        //        Semester = student.CurrentSemesterOrYear,
        //        Remarks = sformnoAI[2]
        //        };
        //    return MigrationDB.SaveDownloadForm(formsToDownload, paymentDetails.PrintProgramme) > 0;
        //}
        public bool SaveDownloadForm(MigrationPI migrationPI)//(ARGPersonalInformation model, DownLoadFormType formType, PrintProgramme printProgramme, string NewCollege)

        {
            return MigrationDB.SaveDownloadForm(migrationPI) > 0;
        }


        //public bool UpdateForm(Guid Form_ID, int AcceptReject,string REMARKSBYCOLLEGE)
        //{
        //    bool result = false;

        //    List<string> ignoreQuery = new List<string>();
        //    result = new MigrationDB().UpdateFormStatus(Form_ID);

        //    return result;
        //}


        public bool UpdateDownloadForm(Guid Form_ID)
        {
            bool result = false;

            List<string> ignoreQuery = new List<string>();
            result = new MigrationDB().UpdateFormStatus(Form_ID);

            return result;
        }



        public MigrationPI GetData(Guid Form_ID)
        {
            return new MigrationDB().GetData(Form_ID);

        }

        public MigrationPI GetMigrationStudent(string formNumberOrRegNo, DateTime DOB, PrintProgramme printProgramme, bool WithDetails = true)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);
            if (DOB == DateTime.MinValue || DOB == DateTime.MaxValue)
                return null;

            MigrationPI studentInfo = new StudentDB().GetStudentMigration(formNumberOrRegNo, DOB, printProgramme);
            if (studentInfo != null && WithDetails)
            {
                studentInfo.StudentAddress = new StudentDB().GetStudentAddress(studentInfo.Student_ID, printProgramme);
                studentInfo.SelectedCombinations = new StudentDB().GetSelectedCombinations(studentInfo.Student_ID, printProgramme);
                if (studentInfo.SelectedCombinations != null)
                    foreach (var selectedCombination in studentInfo.SelectedCombinations)
                    {
                        selectedCombination.Subjects = selectedCombination.Course_ID == null
                            ? new SubjectDB().GetList(selectedCombination.CombinationSubjects.ToGuidList())
                            : new SubjectDB().GetList(selectedCombination.Course_ID, selectedCombination.CombinationSubjects.ToGuidList());
                    }
            }
            return studentInfo;
        }
        public MigrationPI GetMigrationStudent(Guid student_ID, PrintProgramme printProgramme, string MigrationType, bool WithDetails = true)
        {
            MigrationPI studentInfo = new StudentDB().GetStudentMigration(student_ID, printProgramme, MigrationType);
            if (studentInfo != null && WithDetails)
            {
                studentInfo.StudentAddress = new StudentDB().GetStudentAddress(studentInfo.Student_ID, printProgramme);
                studentInfo.SelectedCombinations = new StudentDB().GetSelectedCombinations(studentInfo.Student_ID, printProgramme);
                if (studentInfo.SelectedCombinations != null)
                    foreach (var selectedCombination in studentInfo.SelectedCombinations)
                    {
                        selectedCombination.Subjects = selectedCombination.Course_ID == null
                            ? new SubjectDB().GetList(selectedCombination.CombinationSubjects.ToGuidList())
                            : new SubjectDB().GetList(selectedCombination.Course_ID, selectedCombination.CombinationSubjects.ToGuidList());
                    }
            }
            return studentInfo;
        }
        public bool DeleteMigrationStudent(Guid student_ID, PrintProgramme printProgramme, string MigrationType)
        {
            return new StudentDB().DeleteStudentMigration(student_ID, printProgramme, MigrationType) > 1;

        }

        public MigrationPI GetMigrationDetail(Guid student_ID, PrintProgramme programme, string formType)
        {
            return new StudentDB().GetMigrationDetail(student_ID,programme,formType);
        }

        public int GetLastSerialNo()
        {
            return new MigrationDB().GetLastSerialNo();
        }
    }
}
