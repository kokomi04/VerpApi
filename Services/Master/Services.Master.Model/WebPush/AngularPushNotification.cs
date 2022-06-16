using Lib.Net.Http.WebPush;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Model.WebPush
{
    public class AngularPushNotification
    {
        private const string WRAPPER_START = "{\"notification\":";
        private const string WRAPPER_END = "}";

        public class NotificationAction
        {
            public string Action { get; }

            public string Title { get; }

            public NotificationAction(string action, string title)
            {
                Action = action;
                Title = title;
            }
        }

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Title { get; set; }

        public string Body { get; set; }

        public string Icon { get; set; }

        public IList<int> Vibrate { get; set; } = new List<int>();

        public IDictionary<string, object> Data { get; set; }

        public IList<NotificationAction> Actions { get; set; } = new List<NotificationAction>();

        public NotificationAdditionalModel NotifyData { get; set; }

        public PushMessage ToPushMessage(string topic = null, int? timeToLive = null, PushMessageUrgency urgency = PushMessageUrgency.Normal)
        {
            return new PushMessage(WRAPPER_START + JsonConvert.SerializeObject(this, _jsonSerializerSettings) + WRAPPER_END)
            {
                Topic = topic,
                TimeToLive = timeToLive,
                Urgency = urgency
            };
        }
    }
}