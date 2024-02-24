using CUSrinagar.DataManagers;
using CUSrinagar.Enums;
using CUSrinagar.Models;
using GeneralModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class DashboardManager
    {

        public CourseSummaryWidget GetCourseSummaryWidget(PrintProgramme printProgramme)
        {
            return new DashboardDB().GetCourseSummaryWidget(printProgramme);
        }


        public GrievanceWidget GetGrievanceWidget()
        {
            GrievanceWidget widget = new DashboardDB().GetGrievanceWidget();
            if (widget != null)
                widget.Summary = new DashboardDB().GetGrievanceCategoryCount();
            return widget;
        }

        public List<MenuList> GetAllMenuItems()
        {
            return new DashboardDB().GetAllMenuItems();
        }

        public List<ExaminationDashboard> GetOpenExaminations()
        {
            return new DashboardDB().GetOpenExaminations();
        }

        public List<ExaminationDashboard> GetOpenAwardLinks()
        {
            return new DashboardDB().GetOpenAwardLinks();
        }
                
        public List<ExaminationDashboard> CurrentRunningSemesters()
        {
            return new DashboardDB().CurrentRunningSemesters();
        }

        public object GetProgrammeWiseCount()
        {
            return new DashboardDB().GetProgrammeWiseCount();
        }

        public List<CoursesDashboard> GetPassingCourses()
        {
            return new DashboardDB().GetPassingCourses();
        }

        public List<CoursesDashboard> GetFailingCourses()
        {
            return new DashboardDB().GetFailingCourses();
        }
    }
}
