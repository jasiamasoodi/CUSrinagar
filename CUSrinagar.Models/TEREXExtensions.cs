using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUSrinagar.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreDBReadAttribute : Attribute
    {
        //Ignore DataBase Read
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreDBWriterAttribute : Attribute
    {
        //Ignore DataBase Write
    }
}
