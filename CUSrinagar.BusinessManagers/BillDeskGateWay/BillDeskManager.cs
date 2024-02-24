using CUSrinagar.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using CUSrinagar.Enums;
using CUSrinagar.DataManagers;
using CUSrinagar.Extensions;
using System.Web;
using System.Text.RegularExpressions;

namespace CUSrinagar.BusinessManagers
{
    public class BillDeskManager
    {
        #region NormalPayment
        private NameValueCollection Inputs = new NameValueCollection();
        private void Add(string name, string value) => Inputs.Add(name, value);
        public StringBuilder GenerateHTMLForm(NameValueCollection Inputs)
        {
            StringBuilder sbHTML = new StringBuilder();

            sbHTML.Append("<html><head>");

            sbHTML.Append(string.Format("</head><body style='background-color:#e5eff1;' onload=\"document.{0}.submit()\">", BillDeskSettings.FormName));
            sbHTML.Append(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", BillDeskSettings.FormName, BillDeskSettings.FormMethod, BillDeskSettings.BillDeskBaseUrl));
            for (int i = 0; i < Inputs.Keys.Count; i++)
            {
                sbHTML.Append(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i].Trim(), Inputs[Inputs.Keys[i].Trim()].Trim()));
            }
            sbHTML.Append("</form>");
            sbHTML.Append("<p style='text-align:center;font-size:30px;margin-bottom: -7px'>Please Wait... While we redirect you to Payment Gateway.<br/>Make sure you have good internet connection.<br/>Don't use Google Assistant App for payment. <br/><br/><span style='color:red'>Do not Refresh at any point while payment is in process</span></p><br/><div style='margin: auto;width: 50%'>" +
                "<img style='' src='/Content/ThemePublic/Libraries/images/loading-icon-large.gif' />" +
                "</div>");
            sbHTML.Append("</body></html>");
            return sbHTML;
            //System.Web.HttpContext.Current.Response.Clear();
            //System.Web.HttpContext.Current.Response.Write(sbHTML.ToString());
            //System.Web.HttpContext.Current.Response.End();
        }

        /// <summary>
        /// MerchantID|CustomerID|NA|TxnAmount|NA|NA|NA|CurrencyType|NA|TypeField1|SecurityID|NA|NA|TypeField2|AdditionalInfo1|AdditionalInfo2|AdditionalInfo3|AdditionalInfo4|AdditionalInfo5|AdditionalInfo6|AdditionalInfo7|RU|Checksum 
        /// MerchantID - 0
        /// CustomerID - 1
        /// NA - 2
        /// TxnAmount - 3
        /// NA - 4
        /// NA - 5
        /// NA - 6
        /// CurrencyType - 7
        /// NA - 8
        /// TypeField1 - 9
        /// SecurityI D - 10
        /// NA - 11
        /// NA - 12
        /// TypeField2 - 13
        /// AdditionalInfo1 - 14
        /// AdditionalInfo2 - 15
        /// AdditionalInfo3 - 16
        /// AdditionalInfo4 - 17
        /// AdditionalInfo5 - 18
        /// AdditionalInfo6 - 19
        /// AdditionalInfo7 - 20
        /// RU - 21
        /// Checksum - 22
        /// </summary>
        /// <param name="billDeskRequest"></param>
        /// <returns></returns>
        public NameValueCollection GenerateRequestString(BillDeskRequest billDeskRequest, BillDeskPaymentType billDeskPaymentType)
        {
            billDeskRequest.CustomerID = DateTime.Now.Ticks.ToString();
            string amountToSent = Convert.ToDecimal(billDeskRequest.TotalFee).ToString("F2");
            string CheckSumString = $"{BillDeskSettings.MerchantID}|{billDeskRequest.CustomerID}|NA|{amountToSent}|NA|NA|NA|INR|NA|R|{BillDeskSettings.SecurityID}|NA|NA|F|{billDeskRequest.PhoneNo}|{billDeskRequest.Email}|{(short)billDeskRequest.PrintProgramme}|{billDeskRequest.Entity_ID}|{billDeskRequest.Semester}|{billDeskRequest.AdditionalInfo}|{billDeskPaymentType}|{BillDeskSettings.CUSBASEURL.Trim()}/{billDeskRequest.ReturnURL.Trim()}";

            CheckSumString = CheckSumString.Trim() + "|" + CheckSumString.GenerateCheckSum();
            Inputs.Add("msg", CheckSumString);

            if (!string.IsNullOrWhiteSpace(billDeskRequest.NonBillDeskField))
            {
                CheckSumString += $"|{billDeskRequest.NonBillDeskField}";
            }

            HttpCookie authCookie = HttpContext.Current.Request.Cookies[AuthCookie.Name];
            string oAuthTokenValue = null;
            if (authCookie != null)
            {
                oAuthTokenValue = authCookie.Value;
            }
            new PaymentDB().SaveBDRequest(CheckSumString, oAuthTokenValue, billDeskRequest.Student_ID);
            return Inputs;
        }


        /// <summary>
        /// MerchantID|CustomerID|TxnReferenceNo|BankReferenceNo|TxnAmount|BankID|BankMerchantID|TxnType|CurrencyName|ItemCode|SecurityType|SecurityID|SecurityPassword|TxnDate|AuthStatus|SettlementType|AdditionalInfo1|AdditionalInfo2|AdditionalInfo3|AdditionalInfo4|AdditionalInfo5|AdditionalInfo6|AdditionalInfo7|ErrorStatus|ErrorDescription|CheckSum
        /// MerchantID - 0
        /// CustomerID - 1
        /// TxnReferenceNo - 2
        /// BankReferenceNo - 3
        /// TxnAmount - 4
        /// BankID - 5
        /// BankMerchantID - 6
        /// TxnType - 7
        /// CurrencyName - 8
        /// ItemCode - 9
        /// SecurityType - 10
        /// SecurityID - 11
        /// SecurityPassword - 12
        /// TxnDate - 13
        /// AuthStatus - 14
        /// SettlementType - 15
        /// AdditionalInfo1 - 16
        /// AdditionalInfo2 - 17
        /// AdditionalInfo3 - 18
        /// AdditionalInfo4 - 19
        /// AdditionalInfo5 - 20
        /// AdditionalInfo6 - 21
        /// Student_ID - 22
        /// ErrorStatus - 23
        /// ErrorDescription - 24
        /// CheckSum - 25
        /// </summary>
        /// <param name="_Stream"></param>
        /// <returns></returns>
        public Tuple<bool, string, PaymentDetails, Guid, Guid> BillDeskResponse(Stream _Stream)
        {
            string ErrorDesc = string.Empty;
            try
            {
                //Request.InputStream
                string data = HttpUtility.UrlDecode(new StreamReader(_Stream)?.ReadToEnd() ?? string.Empty);
                if (string.IsNullOrWhiteSpace(data))
                {
                    data = HttpContext.Current.Request.Form.Get("msg");
                }

                string[] BillDeskResponse = data.Split('|');

                string hash_seq = data.Replace("msg=", "");
                string CheckSumFromBillDesk = hash_seq.Substring(hash_seq.LastIndexOf("|") + 1);
                string StringFromBillDesk = hash_seq.Substring(0, hash_seq.LastIndexOf("|"));
                PrintProgramme printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), BillDeskResponse[18]);


                Guid student_ID = Guid.Empty;
                if (BillDeskResponse[14].Trim() == "0300")
                {
                    if (CheckSumFromBillDesk.Trim().ToUpper().Contains(StringFromBillDesk.GenerateCheckSum()))
                    {
                        ErrorDesc = BillDeskResponse[23] + " - " + BillDeskResponse[24];

                        string[] email = Regex.Split(BillDeskResponse[17], @"__") ?? null;

                        string emailtoUse = BillDeskResponse[17];

                        if (email?.Length > 1)
                        {
                            emailtoUse = email[1];
                        }

                        student_ID = new PaymentDB().GetStudentIDFromBillDeskRequest(BillDeskResponse[1]);

                        PaymentDetails aRGPaymentDetails = new PaymentDetails
                        {
                            Payment_ID = Guid.NewGuid(),
                            Entity_ID = Guid.Parse(BillDeskResponse[19]),
                            EntityID = BillDeskResponse[21],
                            TxnReferenceNo = BillDeskResponse[2],
                            BankReferenceNo = BillDeskResponse[3],
                            TxnAmount = Convert.ToDecimal(BillDeskResponse[4]),
                            BankID = BillDeskResponse[5],
                            BankMerchantID = BillDeskResponse[6],
                            TxnType = BillDeskResponse[7],
                            CurrencyName = BillDeskResponse[8],
                            ItemCode = BillDeskResponse[9],
                            SecurityType = BillDeskResponse[10],
                            SecurityID = BillDeskResponse[11],
                            TxnDate = DateTime.Now,
                            AuthStatus = BillDeskResponse[14],
                            SettlementType = BillDeskResponse[15],
                            PhoneNumber = BillDeskResponse[16],
                            Email = emailtoUse,
                            ErrorStatus = BillDeskResponse[23],
                            ErrorDescription = BillDeskResponse[24],
                            PaymentType = PaymentType.Online,
                            Student_ID = student_ID,
                            Semester = short.TryParse(BillDeskResponse[20], out short sem) ? sem : (short?)null,
                            PrintProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), BillDeskResponse[18].ToString()),
                            AdditionalInfo = BillDeskResponse[21]
                        };
                        return Tuple.Create(true, "", aRGPaymentDetails, Guid.Parse(BillDeskResponse[19]), student_ID);
                    }
                    else
                        return Tuple.Create<bool, string, PaymentDetails, Guid, Guid>(false, "CheckSum did not match on CUS Side. Please try again.", null, Guid.Parse(BillDeskResponse[19]), student_ID);
                }
                else
                {
                    string ErrorMsg = (string.IsNullOrEmpty(BillDeskResponse[23]) || BillDeskResponse[23].Trim().ToLower() == "na") || (string.IsNullOrEmpty(BillDeskResponse[24]) || BillDeskResponse[24].Trim().ToLower() == "na") ? "Sorry.  We were unable to process your transaction.  We apologise for the inconvenience and request you to try again later." : BillDeskResponse[23] + "-" + BillDeskResponse[24];
                    //errorcode and errordescription
                    return Tuple.Create<bool, string, PaymentDetails, Guid, Guid>(false, ErrorMsg, null, Guid.Parse(BillDeskResponse[19]), student_ID);
                }
            }
            catch (Exception ex)
            {
                return Tuple.Create<bool, string, PaymentDetails, Guid, Guid>(false, ErrorDesc + " - " + ex.Message, null, Guid.Empty, Guid.Empty);
            }
        }

        public List<PrintProgramme> GetPaymentCategory()
        {
            List<PrintProgramme> list = new List<PrintProgramme>();
            foreach (var item in new PaymentDB().GetPaymentCategory() ?? new List<short>())
            {
                list.Add((PrintProgramme)Enum.Parse(typeof(PrintProgramme), item.ToString()));
            }
            return list.IsNullOrEmpty() ? null : list;
        }
        #endregion


        #region Server-To-Server
        public string GenerateRequestString(BillDeskS2SRequest billDeskS2SRequest)
        {
            string CheckSumString = $"{billDeskS2SRequest.RequestType}|{billDeskS2SRequest.MerchantID}|{billDeskS2SRequest.CustomerID}|{billDeskS2SRequest.CurrentDate}";
            return CheckSumString.Trim() + "|" + CheckSumString.GenerateCheckSum();
        }

        public Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> BillDeskS2SResponse(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "No details provided", null, null, Guid.Empty);

            return S2SResponse(data);
        }

        /// <summary>
        /// RequestType|MerchantID|CustomerID|TxnReferenceNo|BankReferenceNo|TxnAmount|BankID|BankMerchantID|TxnType|CurrencyName|ItemCode|SecurityType|SecurityID|SecurityPassword|TxnDate|AuthStatus|SettlementType|AdditionalInfo1|AdditionalInfo2|AdditionalInfo3|AdditionalInfo4|AdditionalInfo5|AdditionalInfo6|AdditionalInfo7|ErrorStatus|ErrorDescription|Filler1|RefundStatus|TotalRefundAmount|LastRefundDate|LastRefundRefNo|QueryStatus|CheckSum
        /// RequestType - 0
        /// MerchantID - 1
        /// CustomerID - 2
        /// TxnReferenceNo - 3
        /// BankReferenceNo - 4
        /// TxnAmount - 5
        /// BankID - 6
        /// BankMerchantID - 7
        /// TxnType - 8
        /// CurrencyName - 9
        /// ItemCode - 10
        /// SecurityType - 11
        /// SecurityID - 12
        /// SecurityPassword - 13
        /// TxnDate - 14
        /// AuthStatus - 15
        /// SettlementType - 16
        /// AdditionalInfo1 - 17
        /// AdditionalInfo2 - 18
        /// AdditionalInfo3 - 19
        /// AdditionalInfo4 - 20
        /// AdditionalInfo5 - 21
        /// AdditionalInfo6 - 22
        /// AdditionalInfo7 - 23
        /// ErrorStatus - 24
        /// ErrorDescription - 25
        /// Filler1 - 26
        /// RefundStatus - 27
        /// TotalRefundAmount - 28
        /// LastRefundDate - 29
        /// LastRefundRefNo - 30
        /// QueryStatus - 31
        /// CheckSum - 32
        /// </summary>
        /// <param name="PipeSeparatedData"></param>
        /// <returns></returns>
        private Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> S2SResponse(string PipeSeparatedData)
        {
            string[] BillDeskResponse = PipeSeparatedData.Split('|');
            try
            {
                string hash_seq = PipeSeparatedData.Replace("msg=", "");
                string CheckSumFromBillDesk = hash_seq.Substring(hash_seq.LastIndexOf("|") + 1);
                string StringFromBillDesk = hash_seq.Substring(0, hash_seq.LastIndexOf("|"));
                PrintProgramme printProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), BillDeskResponse[19]);

                if (BillDeskResponse[15].Trim() == "0399")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "0399 - Invalid Authentication at Bank - Failed Transaction", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].ToUpper().Trim() == "NA")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "NA - Invalid Input in the Request Message - Cancel Transaction", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].ToUpper().Trim() == "0002")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "0002 - BillDesk is waiting for Response from Bank - Pending Transaction", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].ToUpper().Trim() == "0001")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "0001 - Error at BillDesk - Cancel Transaction", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].Trim() == "0300" && BillDeskResponse[27].Trim() == "0699")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "Payment status (0300) is success but it has been process for cancellation(0699) i.e.refunded back to customer", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].Trim() == "0300" && BillDeskResponse[27].Trim() == "0799")
                {
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "Payment status (0300) is success and a refund [either partial / full] was initiated for this transaction", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else if (BillDeskResponse[15].Trim() == "0300" && BillDeskResponse[27].ToUpper().Trim() == "NA")
                {
                    if (CheckSumFromBillDesk.Trim().ToUpper().Contains(StringFromBillDesk.GenerateCheckSum()))
                    {
                        string[] email = Regex.Split(BillDeskResponse[18], @"__") ?? null;
                        string emailtoUse = BillDeskResponse[18];

                        if (email?.Length > 1)
                        {
                            emailtoUse = email[1];
                        }

                        #region create objects
                        BillDeskS2SResponse S2SResponse = new BillDeskS2SResponse
                        {
                            RequestType = BillDeskResponse[0],
                            MerchantID = BillDeskResponse[1],
                            CustomerID = BillDeskResponse[2],
                            TxnReferenceNo = BillDeskResponse[3],
                            TxnAmount = decimal.Parse(BillDeskResponse[5]),
                            AuthStatus = BillDeskResponse[15],
                            Filler1 = BillDeskResponse[26],
                            RefundStatus = BillDeskResponse[27],
                            TotalRefundAmount = decimal.Parse(BillDeskResponse[28]),
                            LastRefundDate = BillDeskResponse[29],
                            LastRefundRefNo = BillDeskResponse[30],
                            QueryStatus = char.Parse(BillDeskResponse[31]),
                            CheckSum = BillDeskResponse[32],
                            PaymentType = PaymentType.Reconciled,
                            PrintProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), BillDeskResponse[19].ToString()),
                            Entity_ID = Guid.Parse(BillDeskResponse[20])
                        };

                        PaymentDetails aRGPaymentDetails = new PaymentDetails
                        {
                            Payment_ID = Guid.NewGuid(),
                            Entity_ID = Guid.Parse(BillDeskResponse[20]),
                            EntityID = BillDeskResponse[22],
                            TxnReferenceNo = BillDeskResponse[3],
                            BankReferenceNo = BillDeskResponse[4],
                            TxnAmount = Convert.ToDecimal(BillDeskResponse[5]),
                            BankID = BillDeskResponse[6],
                            BankMerchantID = BillDeskResponse[7],
                            TxnType = BillDeskResponse[8],
                            CurrencyName = BillDeskResponse[9],
                            ItemCode = BillDeskResponse[10],
                            SecurityType = BillDeskResponse[11],
                            SecurityID = BillDeskResponse[12],
                            TxnDate = new DateTime(Convert.ToInt64(BillDeskResponse[2] ?? DateTime.Now.Ticks.ToString())),
                            AuthStatus = BillDeskResponse[15],
                            SettlementType = BillDeskResponse[16],
                            PhoneNumber = BillDeskResponse[17],
                            Email = emailtoUse,
                            ErrorStatus = BillDeskResponse[24],
                            ErrorDescription = BillDeskResponse[25],
                            PaymentType = PaymentType.Reconciled,
                            Student_ID = new PaymentDB().GetStudentIDFromBillDeskRequest(BillDeskResponse[2]),
                            Semester = short.TryParse(BillDeskResponse[21], out short sem) ? sem : (short?)null,
                            PrintProgramme = (PrintProgramme)Enum.Parse(typeof(PrintProgramme), BillDeskResponse[19].ToString()),
                            AdditionalInfo = BillDeskResponse[22]
                        };
                        #endregion
                        return Tuple.Create(true, "Payment status (0300) is success and is currently not refunded or cancelled.", aRGPaymentDetails, S2SResponse, Guid.Parse(BillDeskResponse[20]));
                    }
                    else
                        return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, "CheckSum did not match on CUS Side. Please try again.", null, null, Guid.Parse(BillDeskResponse[20]));
                }
                else
                {
                    string ErrorMsg = (string.IsNullOrEmpty(BillDeskResponse[24]) || BillDeskResponse[24].Trim().ToLower() == "na") || (string.IsNullOrEmpty(BillDeskResponse[25]) || BillDeskResponse[25].Trim().ToLower() == "na") ? "Sorry.  We were unable to process your transaction, Or it may have been refunded" : BillDeskResponse[24] + "-" + BillDeskResponse[25];
                    //errorcode and errordescription
                    return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, ErrorMsg, null, null, Guid.Parse(BillDeskResponse[20]));
                }
            }
            catch (Exception)
            {
                return Tuple.Create<bool, string, PaymentDetails, BillDeskS2SResponse, Guid>(false, BillDeskResponse[25] + "".ToString(), null, null, Guid.Empty);
            }
        }

        public List<BillDeskStoredRequest> GetRequests(S2SSearch s2SSearch)
        {
            return new S2SDB().GetCustomerID(s2SSearch);
        }

        public bool PaymentExistsInDB(string TxnReferenceNo)
        {
            return new S2SDB().PaymentExistsInDB(TxnReferenceNo);
        }

        #endregion


        #region Reconcilation Process 

        public Tuple<bool, string> DoReconcilation(string BillDeskResponse, PaymentModuleType FeeType)
        {
            Tuple<bool, string> result = Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
            switch (FeeType)
            {
                case PaymentModuleType.ReEvaluation:
                case PaymentModuleType.Xerox:
                case PaymentModuleType.Examination:
                    result = ReconcilationForExamainationReEvaluation(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.Admission:
                    result = ReconcilationForRegistration(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.AddMoreAdmCourses:
                    result = ReconcilationForAdmMoreCourses(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.SelfFinanced:
                    result = ReconcilationForSelfFinanced(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.CancelRegistration:
                case PaymentModuleType.Migration:
                    result = ReconcilationForMigration(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.SemesterAdmission:
                    result = ReconcilationForSemesterAdm(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.CertificateCoursesAdmission:
                    result = ReconcilationForCertificateCoursesAdmission(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.FirstSemesterAdmissionReconcileOnly:
                    result = ReconcilationForSemesterI_IIAdmission(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.CertificateVerification:
                    result = ReconcilationForCertificationVerfication(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.IssuingOfDegreeCertificate:
                    result = ReconcilationForIssuingOfDegreeForm(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.AttemptCertificate:
                    result = ReconcilationForAttemptCertificate(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.NEPAdmission:
                    result = ReconcilationForNEPRegistration(BillDeskResponse, FeeType);
                    break;
                case PaymentModuleType.CorrectionForm:
                    result = ReconcilationForCorrectionForm(BillDeskResponse, FeeType);
                    break;
            }
            return result;
        }

        private Tuple<bool, string> ReconcilationForSemesterI_IIAdmission(string DataForReconcilation, PaymentModuleType feeType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.FirstSemesterAdmissionReconcileOnly == feeType)
            {
                if (new S2SDB().IsReconcilationPossibleForSemester_I_Admission(billdeskResponse.Item3))
                {
                    Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponseforSave
                       = new Tuple<bool, string, PaymentDetails, Guid, Guid>(billdeskResponse.Item1, billdeskResponse.Item2, billdeskResponse.Item3, Guid.Empty, Guid.Empty);

                    bool result = new PaymentManager().SavePayment(billdeskResponseforSave, billdeskResponseforSave.Item3.PrintProgramme, true, PaymentModuleType.SemesterAdmission).IsSuccess;
                    if (result)
                        return Tuple.Create(result, "");
                    else
                        return Tuple.Create(false, "Reconcilation Failed");
                }
                else
                {
                    return Tuple.Create(false, "Reconcilation not possible because there is already one payment in DB");
                }
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        private Tuple<bool, string> ReconcilationForCertificateCoursesAdmission(string DataForReconcilation, PaymentModuleType feeType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.CertificateCoursesAdmission == feeType)
            {
                bool result = new PaymentManager().SavePaymentForCertificateCourses(billdeskResponse.Item3) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        private Tuple<bool, string> ReconcilationForSemesterAdm(string DataForReconcilation, PaymentModuleType feeType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.SemesterAdmission == feeType)
            {
                bool result = new PaymentManager().SaveSemesterAdmissionPaymentDetails(billdeskResponse.Item3) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForExamainationReEvaluation(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.Examination == moduleType || PaymentModuleType.ReEvaluation == moduleType || PaymentModuleType.Xerox == moduleType)
            {
                Tuple<bool, string, PaymentDetails, Guid, Guid> billdeskResponseforSave
                    = new Tuple<bool, string, PaymentDetails, Guid, Guid>(billdeskResponse.Item1, billdeskResponse.Item2, billdeskResponse.Item3, Guid.Empty, Guid.Empty);

                bool IsReconcilationPossible = false;
                if (moduleType == PaymentModuleType.Examination)
                    IsReconcilationPossible = new S2SDB().IsReconcilationPossibleForExaminationForm(billdeskResponse.Item3);
                else
                    IsReconcilationPossible = new S2SDB().IsReconcilationPossibleForREandXeroxForm(billdeskResponse.Item3);


                if (IsReconcilationPossible)
                {
                    var response = new PaymentManager().SavePayment(billdeskResponseforSave, billdeskResponseforSave.Item3.PrintProgramme, true, moduleType);
                    return Tuple.Create(response.IsSuccess, "");
                }
                else
                    return Tuple.Create(false, "A payment is already associated with this Entity. Reconcilation Aborted");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForRegistration(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.Admission == moduleType)
            {
                bool result = new PaymentManager().SaveRegistrationPaymentDetails(billdeskResponse.Item3) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForNEPRegistration(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.NEPAdmission == moduleType)
            {
                bool result = new NEPManager().Save(billdeskResponse.Item3, billdeskResponse.Item3.PrintProgramme).Item1;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForSelfFinanced(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.SelfFinanced == moduleType)
            {
                bool result = new PaymentManager().SaveSelfFinancedPaymentDetails(billdeskResponse.Item3, true) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }


        protected Tuple<bool, string> ReconcilationForMigration(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.Migration == moduleType || PaymentModuleType.CancelRegistration == moduleType)
            {
                bool result = new PaymentManager().SaveMigrationPaymentDetails(billdeskResponse.Item3) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }


        protected Tuple<bool, string> ReconcilationForAdmMoreCourses(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);
            if (PaymentModuleType.AddMoreAdmCourses == moduleType)
            {
                bool result = new PaymentManager().SaveAdmMoreCoursesPaymentDetails(billdeskResponse.Item3) > 0;
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForCertificationVerfication(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.CertificateVerification == moduleType)
            {
                bool result = new ApplicationFormsManager().UpdatePayment(billdeskResponse.Item3);
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForIssuingOfDegreeForm(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.IssuingOfDegreeCertificate == moduleType)
            {
                bool result = new ApplicationFormsManager().UpdateDegreeIssuingFormPayment(billdeskResponse.Item3);
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForAttemptCertificate(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.AttemptCertificate == moduleType)
            {
                bool result = new CertificateManager().UpdateCertificateStatus(billdeskResponse.Item3.Entity_ID, billdeskResponse.Item3);
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        protected Tuple<bool, string> ReconcilationForCorrectionForm(string DataForReconcilation, PaymentModuleType moduleType)
        {
            Tuple<bool, string, PaymentDetails, BillDeskS2SResponse, Guid> billdeskResponse = BillDeskS2SResponse(DataForReconcilation);

            if (PaymentModuleType.CorrectionForm == moduleType)
            {
                bool result = new ApplicationFormsManager().UpdateCorrectionFormPayment(billdeskResponse.Item3);
                return Tuple.Create(result, "");
            }
            return Tuple.Create(false, "Payment Module Type did not match with BillDesk transaction Payment Module Type. Reconcilation Aborted");
        }

        #endregion
    }
}
