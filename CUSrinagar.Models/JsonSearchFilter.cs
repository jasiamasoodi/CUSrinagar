using CUSrinagar.Enums;
using System.Collections.Generic;

namespace CUSrinagar.Models
{
    public class Filters
    {
        public LogicalOperator GroupOperation { get; set; }
        public List<Filter> FilterList { get; set; }
    }
    /// <summary>
    /// this class represents the property returned from grid view
    /// </summary>
    public class Filter
    {
        public string Column { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public LogicalOperator GroupOperation { get; set; }
    }

}