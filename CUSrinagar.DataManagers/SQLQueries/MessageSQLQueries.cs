using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.DataManagers.SQLQueries
{
    public class MessageSQLQueries
    {
        internal string GetMessageList()
        {
            return @" SELECT TOP 10 * FROM Messages";
        }
    }
}
