using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CUSrinagar.API.Models
{
    public class ButtonsLinks
    {
        /// <summary>
        /// Default color black
        /// </summary>
        public string HexBackgroundColor { get; set; } = "#202021";

        /// <summary>
        /// Default color white
        /// </summary>
        public string HexTextColor { get; set; } = "#ffffff";

        /// <summary>
        /// default is button = false
        /// </summary>
        public bool DisplayASLink { get; set; }

        /// <summary>
        /// Web URL to Open
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Text to display on button
        /// </summary>
        public string ButtonText { get; set; }
    }

    public class ButtonsLinksManager
    {
        public static List<ButtonsLinks> GetButtonsLinks()
        {
            return new List<ButtonsLinks> {
                new ButtonsLinks{ ButtonText="3rd Semester Admission",URL="http://www.cusrinagar.edu.in/Account/StudentZone",HexBackgroundColor="#5cb85c"},
                new ButtonsLinks{ ButtonText="Result UG-PG 1st-Semester",URL="http://www.cusrinagar.edu.in/Result/ResultNotification",HexBackgroundColor="#337ab7"}
            };
        }
    }
}