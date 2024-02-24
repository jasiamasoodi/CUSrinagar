using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.BusinessManagers
{
    public class CUSException : Exception
    {
        public CUSException(){}
        public CUSException(string message) : base(message){}
    }
}
