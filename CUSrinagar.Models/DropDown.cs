using CUSrinagar.Enums;
using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.Models
{
    public class DropDown
    {
        public object Text { get; set; }
        public object Value { get; set; }
    }
    public class DropDownOptLabel
    {
        public object Text { get; set; }
        public object Value { get; set; }
        public Programme DataGroupFieldEnum { get; set; }

        [IgnoreDBRead]
        [IgnoreDBWriter]
        public string DataGroupField
        {
            get
            {
                return DataGroupFieldEnum.GetEnumDescription();
            }
        }
    }
    public class DropDownOptLabelGeneral
    {
        public object Text { get; set; }
        public object Value { get; set; }
        public string DataGroupField { get; set; }
    }

}
