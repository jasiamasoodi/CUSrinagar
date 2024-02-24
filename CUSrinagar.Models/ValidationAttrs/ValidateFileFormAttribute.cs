using CUSrinagar.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CUSrinagar.ValidationAttrs
{
    public class ValidateFileFormAttribute : ValidationAttribute
    {
        int minContent = 0;
        int maxContent = 0;
        int orgminContent = 0;
        int orgmaxContent = 0;
        bool orgIsImage = false;
        string[] sAllowedExt = new string[15];

        /// <summary>
        /// Validate file
        /// </summary>
        /// <param name="_minSize">in Kb</param>
        /// <param name="_maxSize">in Kb</param>
        /// <param name="allowedTypes"> like .pdf</param>
        /// <param name="isImage"></param>
        public ValidateFileFormAttribute(int _minSize, int _maxSize, string[] allowedTypes, bool isImage = true)
        {
            maxContent = 1024 * _maxSize;
            minContent = 1024 * _minSize;
            sAllowedExt = allowedTypes;
            orgminContent = _minSize;
            orgmaxContent = _maxSize;
            orgIsImage = isImage;
        }

        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;

            if (file == null)
            {
                return false;
            }
            else if (!sAllowedExt.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower()))
            {
                ErrorMessage = "Invalid type";
                return false;
            }
            else if (file.ContentLength > maxContent)
            {
                ErrorMessage = " Max allowed size is : " + (orgmaxContent).ToString() + " KB";
                return false;
            }
            else if (file.ContentLength < minContent)
            {
                ErrorMessage = "Min allowed size is : " + (orgminContent).ToString() + " KB";
                return false;
            }
            else
            {
                if (orgIsImage && !sAllowedExt.Any(x => x.ToLower().Trim() == ".pdf") 
                    && !sAllowedExt.Any(x => x.ToLower().Trim() == ".csv")
                    && !sAllowedExt.Any(x => x.ToLower().Trim() == ".doc")
                    && !sAllowedExt.Any(x => x.ToLower().Trim() == ".docx")
                    )
                {
                    try
                    {
                        Helper.CompressImage(file.InputStream, 259, 194);
                    }
                    catch (ArgumentException)
                    {
                        ErrorMessage = "Image is corrupted";
                        return false;
                    }
                }
                return true;
            }
        }
    }

    public class ValidateFileFormEditAttribute : ValidationAttribute
    {
        int minContent = 0;
        int maxContent = 0;
        int orgminContent = 0;
        int orgmaxContent = 0;
        bool orgIsImage = false;
        string[] sAllowedExt = new string[15];

        public ValidateFileFormEditAttribute(int _minSize, int _maxSize, string[] allowedTypes, bool isImage = true)
        {
            maxContent = 1024 * _maxSize;
            minContent = 1024 * _minSize;
            sAllowedExt = allowedTypes;
            orgminContent = _minSize;
            orgmaxContent = _maxSize;
            orgIsImage = isImage;
        }

        public override bool IsValid(object value)
        {
            var file = value as HttpPostedFileBase;

            if (file == null)
            {
                return true;
            }
            else if (!sAllowedExt.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.')).ToLower()))
            {
                ErrorMessage = "Invalid type";
                return false;
            }
            else if (file.ContentLength > maxContent)
            {
                ErrorMessage = " Max allowed size is : " + (orgmaxContent).ToString() + " KB";
                return false;
            }
            else if (file.ContentLength < minContent)
            {
                ErrorMessage = "Min allowed size is : " + (orgminContent).ToString() + " KB";
                return false;
            }
            else
            {
                if (orgIsImage)
                {
                    try
                    {
                        Helper.CompressImage(file.InputStream, 259, 194);
                    }
                    catch (ArgumentException)
                    {
                        ErrorMessage = "Image is corrupted";
                        return false;
                    }
                }
                return true;
            }
        }
    }



}
