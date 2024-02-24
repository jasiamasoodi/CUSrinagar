
using System;

namespace CUSrinagar.Models
{
    public class PushMessageSubscribers
    {
        public string DeviceToken { get; set; }
        public string DeviceInfo { get; set; }
        public DateTime CreatedOn { get; set; }

        [IgnoreDBWriter]
        public int CMGroup { get; set; }
    }

    public static class FireBaseConfigSettings
    {
        public static string ServerAPIKey
        {
            get
            {
                return "AAAA6_5s1qI:APA91bHl4VVbcYm043KWLC8sktpdKMHEsUaJhAk4jxIH01EXxkcj1TMadkpOzujlVIKntsa9jt9cUWaWzScdV7oNceVtHer2cEVmuSL2Y-zkfdhOQAiHWqKlhT_L_-4AAHc08mbiBSie";
            }
        }

        public static string SenderID
        {
            get
            {
                return "1013585860258";
            }
        }
        public static string CloudAPIUrl
        {
            get
            {
                return "https://fcm.googleapis.com/fcm/send";
            }
        }
    }

    public class PushNotification
    {
        public string[] registration_ids { get; set; }
        public BasePushNotification notification { get; set; }

    }
    public class BasePushNotification
    {
        public string title { get; set; }
        public string body { get; set; }
        public string icon
        {
            get
            {
                return "http://www.cusrinagar.edu.in/Content/ThemePublic/PrintImages/MarksSheetLogo.png";
            }
        }
        public string click_action { get; set; } = "http://www.cusrinagar.edu.in/Notification/Notification";
        public string sound
        {
            get
            {
                return "default";
            }
        }
    }
}
