using CUSrinagar.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CUSrinagar.DataManagers;

namespace CUSrinagar.BusinessManagers
{
    public class PushNotificationManager
    {
        public Task<int> SavePushMessageSubscribersAsync(PushMessageSubscribers messageSubscribers)
        {
            return new TaskFactory().StartNew(() => SavePushMessageSubscribers(messageSubscribers));
        }

        private int SavePushMessageSubscribers(PushMessageSubscribers messageSubscribers)
        {
            return new GeneralDDLDB().SavePushMessageSubscribers(messageSubscribers);
        }

        public Task SendAsync(PushNotification pushNotification)
        {
            return new TaskFactory().StartNew(() => ComposeMessage(pushNotification));
        }

        public void ComposeMessage(PushNotification pushNotification)
        {
            var GeneralDDLDB = new GeneralDDLDB();
            int offSet = 0;

            string[] Registration_ids = GeneralDDLDB.GetPushMessageSubscribers(offSet).ToArray();

            if (Registration_ids.Length > 0)
            {
                do
                {
                    pushNotification.registration_ids = Registration_ids;
                    Send(pushNotification);
                    offSet += 1000;

                    Registration_ids = GeneralDDLDB.GetPushMessageSubscribers(offSet).ToArray();
                } while (Registration_ids.Length > 0);
            }
        }

        private string Send(PushNotification pushNotification)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(FireBaseConfigSettings.CloudAPIUrl);

                webRequest.Method = "post";
                webRequest.ContentType = "application/json";
                webRequest.Headers.Add(string.Format("Authorization: key={0}", FireBaseConfigSettings.ServerAPIKey));
                //webRequest.Headers.Add(string.Format("Sender: id={0}", FireBaseConfigSettings.SenderID));

                string JSON = new JavaScriptSerializer().Serialize(pushNotification);

                byte[] byteArray = Encoding.UTF8.GetBytes(JSON);
                webRequest.ContentLength = byteArray.Length;

                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse _webResponse = webRequest.GetResponse();
                dataStream = _webResponse.GetResponseStream();
                StreamReader _Reader = new StreamReader(dataStream);
                string sResponseFromServer = _Reader.ReadToEnd();

                _Reader.Close();
                dataStream.Close();
                _webResponse.Close();
                pushNotification.registration_ids = null;//dispose
                return sResponseFromServer;
            }
            catch (Exception ex)
            {
                new EmailSystem().LogErrorMail(ex);
                return null;
            }
        }
    }
}
