using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace avSVAW.Helpers
{
    public class FCMHelper
    {
        public async Task Send(string notification)
        {
            var fcmKey = WebConfigurationManager.AppSettings["FcmKey"].Trim();
            var senderKey = WebConfigurationManager.AppSettings["SenderKey"].Trim();
            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + fcmKey);
            http.DefaultRequestHeaders.TryAddWithoutValidation("Sender", "id=" + senderKey);
            http.DefaultRequestHeaders.TryAddWithoutValidation("content-length", notification.Length.ToString());
            var content = new StringContent(notification, System.Text.Encoding.UTF8, "application/json");

            var response = await http.PostAsync("https://fcm.googleapis.com/fcm/send", content);

        }

        public static string getAndroidMessage(string title, string message, object data, string regId)
        {
            Dictionary<string, object> androidMessageDic = new Dictionary<string, object>();
            androidMessageDic.Add("title", title);
            androidMessageDic.Add("collapse_key", message);
            androidMessageDic.Add("message", message);
            androidMessageDic.Add("body", message);
            androidMessageDic.Add("data", data);
            androidMessageDic.Add("to", regId);
            androidMessageDic.Add("delay_while_idle", true);
            androidMessageDic.Add("time_to_live", 125);
            androidMessageDic.Add("dry_run", false);
            return JsonConvert.SerializeObject(androidMessageDic);
        }

        public static string getAppledMessage(string title, string message, object data, string regId)
        {
            Dictionary<string, object> notification = new Dictionary<string, object>();
            Dictionary<string, object> appMessageDic = new Dictionary<string, object>();

            notification.Add("title", title);
            notification.Add("body", message);
            notification.Add("sound", "adcmover_notify_sound.m4r");
            notification.Add("mutable_content", true);
            notification.Add("badge", 1);

            appMessageDic.Add("priority", "high");
            appMessageDic.Add("notification", notification);
            appMessageDic.Add("data", data);
            appMessageDic.Add("to", regId);

            return JsonConvert.SerializeObject(appMessageDic);
        }

    }
}