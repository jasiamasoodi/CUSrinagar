using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using System;
using System.Transactions;

namespace CUSrinagar.BusinessManagers
{
    public class AssignClassRollNoManager
    {
        public bool StudentExistsInCollege(string FormNoOrBoardRegnNo, PrintProgramme printProgramme, int Batch)
        {
            return new AssignClassRollNoDB().StudentExistsInCollege(FormNoOrBoardRegnNo, new RegistrationManager().MappingTable(printProgramme), Batch);
        }
        public string ClassRollNoAlreadyAssigned(string FormNoOrBoardRegnNo, PrintProgramme printProgramme, int Batch)
        {
            return new AssignClassRollNoDB().ClassRollNoAlreadyAssigned(FormNoOrBoardRegnNo, new RegistrationManager().MappingTable(printProgramme), Batch);
        }
        public string ClassRollNoAlreadyExists(string ClassRollNo, PrintProgramme printProgramme, int Batch, Guid Course_ID)
        {
            return new AssignClassRollNoDB().ClassRollNoAlreadyExists(ClassRollNo, new RegistrationManager().MappingTable(printProgramme), Batch, Course_ID);
        }

        public Tuple<int, string> Update(AssignClassRollNo assignClassRollNo)
        {
            if (assignClassRollNo == null)
                return Tuple.Create(0, "Please provide details");

            if (!System.Text.RegularExpressions.Regex.IsMatch(assignClassRollNo.ClassRollNo + "", "^[a-zA-Z0-9-]*$"))
                return Tuple.Create(0, "Class Roll No is not valid");

            assignClassRollNo.Programme = new RegistrationManager().MappingTable(assignClassRollNo.Programme);
            if (!StudentExistsInCollege(assignClassRollNo.StudentFormNoOrRegnNo, assignClassRollNo.Programme, assignClassRollNo.Batch))
                return Tuple.Create(0, $"Form No. Or Regn. No. {assignClassRollNo.StudentFormNoOrRegnNo} either does not exist or not admitted in your college. Check details and try again");

            Guid Course_ID = new CourseManager().GetStudentCourse_ID(assignClassRollNo.StudentFormNoOrRegnNo, assignClassRollNo.Programme);

            string ClassRollNo = ClassRollNoAlreadyExists(assignClassRollNo.ClassRollNo, assignClassRollNo.Programme, assignClassRollNo.Batch, Course_ID);
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && !(ClassRollNo?.ToLower().Contains(assignClassRollNo.StudentFormNoOrRegnNo.ToLower().Trim()) ?? true))
                return Tuple.Create(0, $"Class Roll No. {assignClassRollNo.ClassRollNo} already assigned to Form No. Or Regn. No. {ClassRollNo.Substring(0,ClassRollNo.IndexOf('|')).ToUpper()}");

            ClassRollNo = ClassRollNoAlreadyAssigned(assignClassRollNo.StudentFormNoOrRegnNo, assignClassRollNo.Programme, assignClassRollNo.Batch)?.ToString();
            if (!string.IsNullOrWhiteSpace(ClassRollNo) && !assignClassRollNo.OverrideExistingClassRollNo && assignClassRollNo.ClassRollNo.ToLower() !=ClassRollNo.ToLower())
                return Tuple.Create(0, $"Form No. Or Regn. No. {assignClassRollNo.StudentFormNoOrRegnNo} has been already assigned the Roll No. as {ClassRollNo.ToUpper()}. Check ( Override Existing Class RollNo checkbox) to override existing.");

            ClassRollNo = "Something went wrong. Please try again.";
            int result = 0;
            using (var ts = new TransactionScope())
            {
                assignClassRollNo.Programme = new RegistrationManager().MappingTable(assignClassRollNo.Programme);
                result = new AssignClassRollNoDB().Update(assignClassRollNo);
                ts.Complete();
                if (result > 0)
                    ClassRollNo = string.Empty;
            }
            return Tuple.Create(result, ClassRollNo);
        }
    }
}
