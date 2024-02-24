using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CUSrinagar.BusinessManagers
{
    public class FeedbackManager
    {
        public Tuple<bool, string> Save(DailyLectureFeedBack dailyLectureFeedBack, PrintProgramme printProgramme)
        {
            if (dailyLectureFeedBack == null)
                return Tuple.Create(false, "Inalid details submitted");

            int batch = new StudentManager().StudentExists(dailyLectureFeedBack.Student_ID);
            if (dailyLectureFeedBack.Student_ID == Guid.Empty || batch < 2017)
                return Tuple.Create(false, "Student not found");

            if (dailyLectureFeedBack.Subject_ID == Guid.Empty)
                return Tuple.Create(false, "Subject not found");

            FeedbackDB feedbackDB = new FeedbackDB();

            printProgramme = new GeneralFunctions().MappingTable(printProgramme);

            //Save ABC ID
            feedbackDB.UpdateABCID(dailyLectureFeedBack.Student_ID, printProgramme, dailyLectureFeedBack.ABCID);

            //Check for datetime & Subject not duplicate
            Parameters parameters = new Parameters
            {
                Filters = new List<SearchFilter> {
                 new SearchFilter{
                     Column =nameof(dailyLectureFeedBack.Student_ID),
                     TableAlias="d",
                     IsSibling=false,
                     Operator=SQLOperator.EqualTo,
                     Value=dailyLectureFeedBack.Student_ID.ToString()
                 },
                 new SearchFilter{
                     Column =nameof(dailyLectureFeedBack.Subject_ID),
                     TableAlias="d",
                     IsSibling=false,
                     Operator=SQLOperator.EqualTo,
                     Value=dailyLectureFeedBack.Subject_ID.ToString()
                 },
                 new SearchFilter{
                     Column =nameof(dailyLectureFeedBack.LectureDateTime),
                     TableAlias="d",
                     IsSibling=false,
                     Operator=SQLOperator.EqualTo,
                     Value=dailyLectureFeedBack.LectureDateTime.ToString()
                 },
                },
            };

            List<DailyLectureFeedBack> dailyLectureFeedBackList = GetDailyLectureFeedBacks(parameters, printProgramme);

            if (dailyLectureFeedBackList.IsNotNullOrEmpty())
            {
                return Tuple.Create(false,
                    $"You have already submitted feedback for Subject " +
                    $"{dailyLectureFeedBackList.First().SubjectFullName} " +
                    $"({dailyLectureFeedBackList.First().SubjectType.GetEnumDescription()}) " +
                    $"and Lecture dated {dailyLectureFeedBack.LectureDateTime.ToLongDateString()}");
            }

            dailyLectureFeedBack.DailyLectureFeedBack_ID = Guid.NewGuid();
            dailyLectureFeedBack.CreatedOn = DateTime.Now;

            int result = feedbackDB.Save(dailyLectureFeedBack);

            return Tuple.Create(result > 0, "Saved. Thank you for your feedback & ABC ID");
        }

        public bool HasSubmittedDailyLectureFeedback(Guid Student_ID, short Semester, PrintProgramme printProgramme)
        {
            //Check for datetime & Subject not duplicate
            Parameters parameters = new Parameters
            {
                Filters = new List<SearchFilter> {
                 new SearchFilter{
                     Column ="Student_ID",
                     TableAlias="d",
                     IsSibling=false,
                     Operator=SQLOperator.EqualTo,
                     Value=Student_ID.ToString()
                 },
                 new SearchFilter{
                     Column ="Semester",
                     TableAlias="s",
                     IsSibling=false,
                     Operator=SQLOperator.EqualTo,
                     Value=Semester.ToString()
                 },
                },
            };

            return (GetDailyLectureFeedBacks(parameters, printProgramme)?.Count() ?? 0) > 0;
        }

        public List<DailyLectureFeedBack> GetDailyLectureFeedBacks(Parameters parameters, PrintProgramme printProgramme)
        {
            printProgramme = new RegistrationManager().MappingTable(printProgramme);

            if (parameters == null)
                return null;

            parameters.SortInfo = new Sort
            {
                ColumnName = "d.CreatedOn",
                OrderBy = System.Data.SqlClient.SortOrder.Descending,
            };

            return new FeedbackDB().GetDailyLectureFeedBacks(parameters, printProgramme);
        }

        public async Task<string> ValidateABCID(string ABCID)
        {
            bool result = new FeedbackDB().ValidateABCID(ABCID);
            if (result)
                return "ABC ID already used by another candidate";

            //Call ABC API
            result = await APIHelper.IsABCIDValidAsync(new ValidateABCID { ABCID = ABCID });

            if (!result)
                return "ABC ID is not valid";

            return null;
        }
    }
}
