using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    public class InternalExam
    {

        public Guid Exam_Id { get; set; }
        public string ExamName { get; set; }
        public int Batch { get; set; }
        public int TimeLimit { get; set; }
        public DateTime ExamDate { get; set; }
        public int NoOfQuestions { get; set; }
        public int Semester { get; set; }
        public Guid College_Id { get; set; }
        public Guid Subject_ID { get; set; }
        [IgnoreDBWriter]
        public Guid Course_Id { get; set; }
    }

    public class InternalQuestions
    {
        public Guid Question_Id { get; set; }
        public string Question { get; set; }
        public Guid Exam_Id { get; set; }
        public List<InternalQuestionOptions> InternalQuestionOptions { get; set; }
        [IgnoreDBWriter]
        public Guid IsCheckedOption { get; set; }
    }


    public class InternalQuestionOptions
    {
        public Guid Option_Id { get; set; }
        public Guid Question_Id { get; set; }
        public string OptionName { get; set; }
        public bool IsCorrect { get; set; }

    }
    public class InternalExamAnswers
    {
        public Guid InternalExamAnswer_Id { get; set; }
        public Guid Student_Id { get; set; }
        public Guid Question_Id { get; set; }
        public Guid Option_Id { get; set; }
    }
}
