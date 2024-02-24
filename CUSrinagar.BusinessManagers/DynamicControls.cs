using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CUSrinagar.BusinessManagers
{
    public class DynamicControls
    {
        /// <summary>
        /// Fills values of dynamic controls created by JS or JQuery
        /// </summary>
        /// <param name="formCollection"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public FormCollection FillModel(FormCollection formCollection, string PropertyName)
        {
            if (formCollection == null || !formCollection.AllKeys.Any())
                return null;
            int index = 0;
            string prevKey = null;
            string itemName = null;
            int subStringIndex = 0;
            FormCollection newFormCollection = new FormCollection();
            PropertyName = PropertyName + "[";
            foreach (var Key in formCollection.AllKeys.Where(x => x.Contains(PropertyName)))
            {
                subStringIndex = Key.LastIndexOf("].");

                if (subStringIndex == -1 || !Key.Contains(PropertyName))
                    continue;

                if (!string.IsNullOrWhiteSpace(prevKey) && !prevKey.Contains(Key.Substring(0, subStringIndex).ToLower()))
                    index++;
                itemName = $"{PropertyName + index}" + Key.Substring(subStringIndex);
                newFormCollection.Set(itemName, formCollection[Key]);
                prevKey = Key.ToLower();
            }
            return newFormCollection.AllKeys.Count() > 0 ? newFormCollection : null;
        }
    }
}
