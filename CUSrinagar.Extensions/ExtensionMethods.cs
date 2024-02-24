using Newtonsoft.Json;
using Spire.Barcode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CUSrinagar.Extensions
{
    public static class Helper
    {
        public static object ReadModelStateErrors(this ModelStateDictionary modelState)
        {
            return from ms in modelState
                   where ms.Value.Errors.Any()
                   let fieldKey = ms.Key
                   let errors = ms.Value.Errors
                   from error in errors
                   select new { PropertyName = fieldKey, ErrorMsg = error.ErrorMessage };
        }

        public static void CopyPropertiesFrom(this object self, object parent)
        {
            var copyToProperties = self.GetType().GetProperties().Where(x => x.CanWrite).ToList();
            var copyFromProperties = parent.GetType().GetProperties().Where(x => copyToProperties.Any(y => y.Name == x.Name && y.PropertyType == x.PropertyType)).ToList();

            foreach (var copyFromProp in copyFromProperties)
            {
                var copyToProperty = copyToProperties.FirstOrDefault(x => x.Name == copyFromProp.Name);
                if (copyToProperty != null)
                {
                    copyToProperty.SetValue(self, copyFromProp.GetValue(parent));
                    copyToProperties.Remove(copyToProperty);
                }
            }
        }
        public static string GetTablePFix<T>(this T PFix)
        {
            string postfix = "UG";
            switch (PFix.ToString())
            {
                case "UG":
                case "BED":
                    postfix = "UG";
                    break;
                case "PG":
                    postfix = "PG";
                    break;
                case "IH":
                case "HS":
                case "IG":
                case "Engineering":
                case "Professional":
                    postfix = "IH";
                    break;

            }
            return postfix;
        }

        public static bool IsBEdMEdAdm(this string R)
        {
            return string.IsNullOrWhiteSpace(R) ? false : (R.DecryptCookieAndURLSafe() + "").ToLower().Trim() == "ibmed" ? true : false;
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }
        public static bool IsNullOrEmpty(this Guid? val)
        {
            return val == null || val == Guid.Empty;
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items != null && items.Any();
        }

        /// <summary>
        /// Converts an enum to list of SelectListItems
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Ignore"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetSelectList<T>(params T[] Ignore) where T : IComparable
        {
            List<SelectListItem> list = (Enum.GetValues(typeof(T)).Cast<T>().Select(e =>
              new SelectListItem()
              {
                  Text = e.GetEnumDescription(),
                  Value = Convert.ToInt32(e).ToString()
              })).ToList();
            foreach (var item in Ignore ?? new T[] { })
            {
                list.Remove(list.First(x => x.Value == Convert.ToInt32(item).ToString()));
            }
            return list;

        }
        public static IEnumerable<SelectListItem> GetSelectedList<T>(params T[] Add) where T : IComparable
        {
            List<SelectListItem> list = (Enum.GetValues(typeof(T)).Cast<T>().Select(e =>
       new SelectListItem()
       {
           Text = e.GetEnumDescription(),
           Value = Convert.ToInt32(e).ToString()
       })).ToList();
            List<SelectListItem> newList = new List<SelectListItem>();
            foreach (var item in Add ?? new T[] { })
            {
                newList.Add(list.First(x => x.Value == Convert.ToInt32(item).ToString()));
            }
            return newList;

        }
   

        public static IEnumerable<SelectListItem> GetEnumAsValue<T>(params T[] Ignore) where T : IComparable
        {
            List<SelectListItem> list = (Enum.GetValues(typeof(T)).Cast<T>().Select(e =>
              new SelectListItem()
              {
                  Text = e.GetEnumDescription(),
                  Value = e.ToString()
              })).ToList();
            foreach (var item in Ignore ?? new T[] { })
            {
                list.Remove(list.First(x => x.Value == item.ToString()));
            }
            return list;

        }

        public static IEnumerable<SelectListItem> GetSelectForAdmission<T>(List<T> OrgList) where T : IComparable
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in OrgList ?? new List<T> { })
            {
                list.Add(new SelectListItem()
                {
                    Text = item.GetEnumDescription(),
                    Value = Convert.ToInt32(item).ToString()
                });
            }
            return list.Any() ? list : null;

        }

        public static IEnumerable<SelectListItem> GetSelectListDescription<T>(params T[] Ignore) where T : IComparable
        {
            List<SelectListItem> list = (Enum.GetValues(typeof(T)).Cast<T>().Select(e =>
              new SelectListItem()
              {
                  Text = e.GetEnumDescription(),
                  Value = e.GetEnumDescription()
              })).ToList();
            foreach (var item in Ignore ?? new T[] { })
            {
                list.Remove(list.First(x => x.Value == item.GetEnumDescription()));
            }
            return list;

        }

        /// <summary>
        /// Provides description of particular enum item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_Enum"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(this T _Enum) where T : IComparable
        {
            FieldInfo fi = _Enum.GetType().GetField(_Enum.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return _Enum.ToString();
        }

        static string EDKey
        {
            get
            {
                return "qJ1wUUatZ9aA5Whkc2vOteCtq4zOyErO";
            }
        }

        public static string Encrypt(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            try
            {
                byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
                AesCryptoServiceProvider tripleAES = new AesCryptoServiceProvider();
                tripleAES.Key = Convert.FromBase64String(EDKey);
                tripleAES.Mode = CipherMode.ECB;
                tripleAES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleAES.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleAES.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length).Trim();
            }
            catch (FormatException)
            {
                return null;
            }
        }
        public static string Decrypt(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            try
            {
                byte[] inputArray = Convert.FromBase64String(input);
                AesCryptoServiceProvider tripleAES = new AesCryptoServiceProvider();
                tripleAES.Key = Convert.FromBase64String(EDKey);
                tripleAES.Mode = CipherMode.ECB;
                tripleAES.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = tripleAES.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                tripleAES.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static string EncryptCookieAndURLSafe(this string input)
        {
            return HttpContext.Current.Server.UrlEncode(input.Encrypt());
        }

        public static string DecryptCookieAndURLSafe(this string input)
        {
            try
            {
                return HttpContext.Current.Server.UrlDecode(input)?.Decrypt();
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public static string ToQuotedString(this string input)
        {
            return "'" + input + "'";
        }
        public static string ToQuotedCSV(this string input)
        {
            return "'" + input.Replace(",", "','") + "'";
        }

        public static Guid ToGuid(this string stringGuid)
        {
            return Guid.TryParse(stringGuid, out Guid result) ? result : Guid.Empty;
        }

        public static List<Guid> ToGuidList(this string value, char separater = '|')
        {
            return (value.Split(separater).IsNullOrEmpty()) ? null :
            value.Split(separater).ToList().Select(x => Guid.Parse(x)).ToList();
        }

        public static List<T> ToLists<T>(this string value, char separater = '|')
        {
            if (value.Split(separater).IsNullOrEmpty())
                return null;
            List<T> _listToReturn = new List<T>();
            foreach (var item in value.Split(separater))
                _listToReturn.Add((T)Convert.ChangeType(item, typeof(T)));

            return _listToReturn;
        }

        public static List<string> ToListOfStrings(this string value, char separater = '|')
        {
            return (value.Split(separater).IsNullOrEmpty()) ? null :
                 value.Split(separater).ToList();
        }

        public static string ToIN<T>(this IEnumerable<T> value)
        {
            return (value.IsNullOrEmpty()) ? "NULL" : $"'{string.Join("','", value)}'";
        }
        public static string EnumToIN<T>(this IEnumerable<T> value) where T : IComparable
        {
            return (value.IsNullOrEmpty()) ? null : $"{string.Join(",", value)}";
        }
        public static string ToSingleStrings<T>(this IEnumerable<T> value, string separator = "|")
        {
            return (value.IsNullOrEmpty()) ? null : $"{string.Join($"{separator}", value)}";
        }
        public static string ToSingleString<T>(this IEnumerable<T> value, char separator = '|')
        {
            return (value.IsNullOrEmpty()) ? null : $"{string.Join($"{separator}", value)}";
        }

        public static string ToLike(this string value)
        {
            return value == null ? null : $"%{value}%";
        }


        public static T ToEnum<T>(this string value) where T : IComparable
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static byte[] CompressImage(Stream sourcePath, int Height, int width)
        {
            using (var imae = System.Drawing.Image.FromStream(sourcePath))
            {
                int h = Convert.ToInt32(Height * 1);
                int w = Convert.ToInt32(width * 1);
                var newWidth = w;
                var newHeight = h;
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(imae, imageRectangle);
                return (byte[])(new ImageConverter().ConvertTo(imae, typeof(byte[])));
            }
        }

        public static void CompressAndUploadImage(Stream sourcePath, int Height, int width, string targetPath)
        {
            using (var imae = System.Drawing.Image.FromStream(sourcePath))
            {
                int h = Convert.ToInt32(Height * 1);
                int w = Convert.ToInt32(width * 1);
                var newWidth = w;
                var newHeight = h;
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(imae, imageRectangle);
                thumbnailImg.Save(HttpContext.Current.Server.MapPath("~" + targetPath), imae.RawFormat);
            }
        }

        public static void UploadImage(Stream sourcePath, string targetPath)
        {
            using (var imae = System.Drawing.Image.FromStream(sourcePath))
            {
                var newWidth = imae.Width;
                var newHeight = imae.Height;
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(imae, imageRectangle);
                thumbnailImg.Save(HttpContext.Current.Server.MapPath("~" + targetPath), imae.RawFormat);
            }
        }


        public static void CompressAndUploadImage(Stream sourcePath, int Height, int width, string targetPath, HttpServerUtility httpServer)
        {
            using (var imae = System.Drawing.Image.FromStream(sourcePath))
            {
                int h = Convert.ToInt32(Height * 1);
                int w = Convert.ToInt32(width * 1);
                var newWidth = w;
                var newHeight = h;
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(imae, imageRectangle);
                thumbnailImg.Save(httpServer.MapPath("~" + targetPath), imae.RawFormat);
            }
        }

        public static void UploadImage(Stream sourcePath, string targetPath, HttpServerUtility httpServer)
        {
            using (var imae = System.Drawing.Image.FromStream(sourcePath))
            {
                var newWidth = imae.Width;
                var newHeight = imae.Height;
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(imae, imageRectangle);
                thumbnailImg.Save(httpServer.MapPath("~" + targetPath), imae.RawFormat);
            }
        }

        public static List<SelectListItem> StatusDDL()
        {
            return new List<SelectListItem> {
                 new SelectListItem
                 {
                      Text="Active",
                      Value="True"
                 },new SelectListItem
                 {
                      Text="In-Active",
                      Value="False"
                 }
            };
        }
        public static List<SelectListItem> ProvisionalDDL()
        {
            return new List<SelectListItem> {
                 new SelectListItem
                 {
                      Text="Declared",
                      Value="False"
                 }
                 ,new SelectListItem
                 {
                      Text="Awaited",
                      Value="True"
                 }
            };
        }
        public static List<SelectListItem> GenderDDL()
        {
            return new List<SelectListItem> {
                 new SelectListItem
                 {
                      Text="Male",
                      Value="MALE"
                 },new SelectListItem
                 {
                      Text="Female",
                      Value="FEMALE"
                 },new SelectListItem
                 {
                      Text="Transgender",
                      Value="TRANSGENDER"
                 }
            };
        }
        public static List<SelectListItem> ReligionDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem { Text="Islam",Value="ISLAM"},
                new SelectListItem { Text="Hinduism",Value="HINDUISM"},
                new SelectListItem { Text="Christianity",Value="CHRISTIANITY"},
                new SelectListItem { Text="Sikhism",Value="SIKHISM"},
                new SelectListItem { Text="Buddhism",Value="BUDDHISM"},
                new SelectListItem { Text="Other",Value="OTHER"}
            };
        }
        public static List<SelectListItem> SessionDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem { Text="Annual",Value="ANNUAL"},
                new SelectListItem { Text="Bi-Annual",Value="BI-ANNUAL"},
                new SelectListItem { Text="JAN-FEB",Value="JAN-FEB"},
                new SelectListItem { Text="FEB-MAR",Value="FEB-MAR"},
                new SelectListItem { Text="MAR-APR",Value="MAR-APR"},
                new SelectListItem { Text="APR-MAY",Value="APR-MAY"},
                new SelectListItem { Text="MAY-JUNE",Value="MAY-JUNE"},
                new SelectListItem { Text="JUNE-JULY",Value="JUNE-JULY"},
                new SelectListItem { Text="JULY-AUG",Value="JULY-AUG"},
                new SelectListItem { Text="AUG-SEP",Value="AUG-SEP"},
                new SelectListItem { Text="SEP-OCT",Value="SEP-OCT"},
                new SelectListItem { Text="OCT-NOV",Value="OCT-NOV"},
                new SelectListItem { Text="NOV-DEC",Value="NOV-DEC"},
                new SelectListItem { Text="DEC-JAN",Value="DEC-JAN"}
            };
        }

        public static List<SelectListItem> BoardsDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem { Text="JK BOSE",Value="JKBOSE"},
                new SelectListItem { Text="CBSE",Value="CBSE"},
                new SelectListItem { Text="JK SBOTE",Value="JKSBOTE"},
                new SelectListItem { Text="Other",Value="OTHER"}
            };
        }
        public static List<SelectListItem> UniversitiesDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem { Text="University Of Kashmir",Value="University Of Kashmir"},
                new SelectListItem { Text="University Of Jammu",Value="University Of Jammu"},
                new SelectListItem { Text="Central University Of Kashmir",Value="Central University Of Kashmir"},
                new SelectListItem { Text="Central University Of Jammu",Value="Central University Of Jammu"},
                new SelectListItem { Text="Cluster University Srinagar",Value="Cluster University Srinagar"},
                new SelectListItem { Text="Cluster University Jammu",Value="Cluster University Jammu"},
                new SelectListItem { Text="SKUAST",Value="SKUAST"},
                new SelectListItem { Text="IGNOU",Value="IGNOU"},
                new SelectListItem { Text="IUST Awantipora",Value="IUST Awantipora"},
                new SelectListItem { Text="Other",Value="OTHER"}
            };
        }

        public static List<SelectListItem> I2thStreamDDL()
        {
            return new List<SelectListItem> {
                 new SelectListItem
                 {
                      Text="Arts",
                      Value="ARTS"
                 },
                new SelectListItem
                 {
                      Text="Arts With Math",
                      Value="ARTS WITH MATH"
                 }
                 ,new SelectListItem
                 {
                      Text="Commerce",
                      Value="COMMERCE"
                 }
                 ,new SelectListItem
                 {
                      Text="Home-Science",
                      Value="HOME-SCIENCE"
                 }
                 ,new SelectListItem
                 {
                      Text="Science",
                      Value="SCIENCE"
                 }
                 ,new SelectListItem
                 {
                      Text="Science With Math",
                      Value="SCIENCE WITH MATH"
                 }
                 ,new SelectListItem
                 {
                      Text="Other",
                      Value="OTHER"
                 }
            };
        }

        public static List<SelectListItem> PreferenceDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem { Text="Integrated",Value="Integrated"},
                new SelectListItem { Text="Honor's",Value="Honor's"},
                new SelectListItem { Text="Professional",Value="Professional"}
            };
        }

        /// <summary>
        /// Method to get Encrypted the password 
        /// </summary>
        /// <param name="password"> password as String"
        public static string SMSEncryptedPasswod(this string password)
        {
            byte[] encPwd = Encoding.UTF8.GetBytes(password);
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA1");
            byte[] pp = sha1.ComputeHash(encPwd);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in pp)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Provides Grid for excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_dataSource"></param>
        /// <param name="_modelPropertiesAsColumns"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Tuple<GridView, string> DownloadExcel<T>(this IEnumerable<T> _dataSource, List<string> _modelPropertiesAsColumns, string fileName = null)
        {
            if (_dataSource.IsNullOrEmpty() || _modelPropertiesAsColumns.IsNullOrEmpty())
                return Tuple.Create(new GridView(), "invalid");

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"{typeof(T).Name}_{DateTime.UtcNow.Ticks}";

            fileName = fileName.Replace("/", "_").Replace("\\", "_").Replace(".", "_").Replace("'", "_").Replace("|", "_");
            List<string> _headerColumns = new List<string>();
            _headerColumns.AddRange(_modelPropertiesAsColumns);

            _modelPropertiesAsColumns = _modelPropertiesAsColumns.ConvertAll(_ => _.ToLower());
            PropertyInfo[] propertiesInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => !x.PropertyType.FullName.Contains(nameof(System.Collections))).ToArray();

            List<PropertyInfo> propertiesInfoFinal = new List<PropertyInfo>();
            foreach (var item in propertiesInfo)
            {
                if (_modelPropertiesAsColumns.Contains(item.Name.ToLower()))
                {
                    propertiesInfoFinal.Add(item);
                    if (propertiesInfoFinal.Count() == _modelPropertiesAsColumns.Count())
                        break;
                }
            }

            DataTable _dataTable = new DataTable();
            _headerColumns.ForEach(x => { _dataTable.Columns.Add(x, typeof(string)); });

            propertiesInfo = null;//dispose
            _headerColumns = null;//dispose

            foreach (var itemData in _dataSource)
            {
                DataRow dataRow = _dataTable.NewRow();
                foreach (var propertyInfo in propertiesInfoFinal)
                {
                    dataRow[propertyInfo.Name] = propertyInfo.GetValue(itemData);
                }
                _dataTable.Rows.Add(dataRow);
            }
            GridView gridView = new GridView();
            gridView.DataSource = _dataTable;
            gridView.DataBind();

            _dataTable.Dispose();

            return Tuple.Create(gridView, fileName);
        }

        public static byte[] ToQRCode(this string QRValue)
        {
            var _BarcodeSettings = new BarcodeSettings
            {
                BackColor = Color.White,
                FontColor = Color.Blue,
                Data = QRValue,
                TopText = string.Empty,
                ShowTopText = false,
                ShowText = false,
                ShowTextOnBottom = false,
                Type = BarCodeType.QRCode,
            };
            BarCodeGenerator barCodeGenerator = new BarCodeGenerator(_BarcodeSettings);
            System.Drawing.Image img = barCodeGenerator.GenerateImage();
            return (byte[])(new ImageConverter()).ConvertTo(img, typeof(byte[]));
        }
        public static byte[] ToBarCode(this string BarCodeValue)
        {
            var _BarcodeSettings = new BarcodeSettings
            {
                BackColor = Color.White,
                FontColor = Color.Blue,
                Data = BarCodeValue,
                TopText = string.Empty,
                ShowTopText = false,
                ShowText = true,
                ShowTextOnBottom = true,
                Type = BarCodeType.Code25
            };
            BarCodeGenerator barCodeGenerator = new BarCodeGenerator(_BarcodeSettings);
            System.Drawing.Image img = barCodeGenerator.GenerateImage();
            return (byte[])(new ImageConverter()).ConvertTo(img, typeof(byte[]));
        }


        public static string[] SplitDate(this string dateToSplit)
        {
            string[] splitDate = null;
            try
            {
                splitDate = new string[3];
                if (dateToSplit.Contains("-"))
                    splitDate = dateToSplit.Split('-');
                else if (dateToSplit.Contains("/"))
                    splitDate = dateToSplit.Split('/');
                else if (dateToSplit.Contains("."))
                    splitDate = dateToSplit.Split('.');

            }
            catch (FormatException)
            {
                return null;
            }
            return splitDate;
        }

        public static decimal IsNonNegativeNumber(this decimal number)
        {
            return number < 0 ? 0 : number;
        }


        public static bool IsBoardRegNoIsValid(this string BoardRegistrationNo)
        {
            if (string.IsNullOrWhiteSpace(BoardRegistrationNo))
            {
                return false;
            }
            else
            {
                string FirstChar = BoardRegistrationNo.Substring(0, 1) + string.Empty;
                string LastChar = BoardRegistrationNo.Substring(BoardRegistrationNo.Length - 1, 1) + string.Empty;

                if (BoardRegistrationNo.Length >= 4 && (new Regex("^[a-zA-Z0-9]*$").IsMatch(FirstChar) || new Regex("^[a-zA-Z0-9]*$").IsMatch(LastChar)))
                {
                    return true;
                }
            }
            return false;
        }


        public static T DeepCloneObject<T>(this T _object) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(_object));
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        public static string SortAlphaNumeric(this string input)
        {
            return Regex.Replace(input + "", "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        public static List<SelectListItem> AlominiEmploymentStatusDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Text="Private Employee",Value="Private Employee" },
                new SelectListItem{ Text="Govt. Employee",Value="Govt. Employee" },
                new SelectListItem{ Text="Self-Employed",Value="Self-Employed" },
                new SelectListItem{ Text="Other",Value="Other" },
            };
        }

        public static IEnumerable<SelectListItem> GetYearsDDL()
        {
            for (int i = 2017; i <= DateTime.Now.Year; i++)
                yield return new SelectListItem { Text = i.ToString(), Value = i.ToString() };
        }

        public static List<SelectListItem> EmploymentStatusDDL()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Text="Private Employee",Value="Private Employee" },
                new SelectListItem{ Text="Govt. Employee",Value="Govt. Employee" },
                new SelectListItem{ Text="Self-Employed",Value="Self-Employed" },
                new SelectListItem{ Text="None",Value="None" },
            };
        }

        public static byte[] GradeTableQRCode()
        {
            //string gradeTableFull = $@"O(Outstanding){Environment.NewLine}- Grade Point: 10{Environment.NewLine}- Courses Pass %age 40%: 90 to ≤ 100{Environment.NewLine}- Courses Pass %age > 40%: 93 to ≤ 100{Environment.NewLine}____________________{Environment.NewLine}A*(Excellent){Environment.NewLine}- Grade Point: 9{Environment.NewLine}- Courses Pass %age 40%: 80 to < 90{Environment.NewLine}- Courses Pass %age > 40%: 86 to < 93{Environment.NewLine}____________________{Environment.NewLine}A(Very Good){Environment.NewLine}- Grade Point: 8{Environment.NewLine}- Courses Pass %age 40%: 70 to < 80{Environment.NewLine}- Courses Pass %age > 40%: 79 to < 86{Environment.NewLine}____________________{Environment.NewLine}B*(Good){Environment.NewLine}- Grade Point: 7{Environment.NewLine}- Courses Pass %age 40%: 60 to < 70{Environment.NewLine}- Courses Pass %age > 40%: 71 to < 79{Environment.NewLine}____________________{Environment.NewLine}B(Above Average){Environment.NewLine}- Grade Point: 6{Environment.NewLine}- Courses Pass %age 40%: 55 to < 60{Environment.NewLine}- Courses Pass %age > 40%: 63 to < 71{Environment.NewLine}____________________{Environment.NewLine}C(Average){Environment.NewLine}- Grade Point: 5{Environment.NewLine}- Courses Pass %age 40%: 49 to < 55{Environment.NewLine}- Courses Pass %age > 40%: 55 to < 63{Environment.NewLine}____________________{Environment.NewLine}P(Pass){Environment.NewLine}- Grade Point: 4{Environment.NewLine}- Courses Pass %age 40%: 40 to < 49{Environment.NewLine}- Courses Pass %age > 40%: 47 to < 55{Environment.NewLine}____________________{Environment.NewLine}F(Fail){Environment.NewLine}- Grade Point: 0{Environment.NewLine}- Courses Pass %age 40%: Below 40{Environment.NewLine}- Courses Pass %age > 40%: Below 47{Environment.NewLine}____________________{Environment.NewLine}AB(Absent){Environment.NewLine}- Grade Point: 0{Environment.NewLine}";
            return ToQRCode("https://www.cusrinagar.edu.in/foldermanager/Downloads/GradeTableCUS.jpeg");
        }
        public static string GetDegreeCompletionDateFormat(this DateTime degreeDate)
        {
            string suffix;
            switch (degreeDate.Day)
            {
                case 1:
                case 21:
                case 31:
                    suffix = "st";
                    break;
                case 2:
                case 22:
                    suffix = "nd";
                    break;
                case 3:
                case 23:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
            return $"{degreeDate.Day.ToString("d")}<sup>{suffix}</sup>,{degreeDate.ToString("MMMM")} {degreeDate.Year}";
        }
    }
}
