using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class CourseSQLQueries
    {
        internal string GetOldCourseList = @" SELECT Name AS Text , convert(nvarchar(50),Course_ID) AS Value FROM Courses_New ";

    }
}
