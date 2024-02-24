using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace CUSrinagar.Models.ValidationAttrs
{
    public class DBColumnNameAttribute : Attribute
    {
        public string name;

        public DBColumnNameAttribute(string columnName)
        {
            name = columnName;
        }
    }
    public class StringValueAttribute : System.Attribute
    {
        public StringValueAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }

    }

}
