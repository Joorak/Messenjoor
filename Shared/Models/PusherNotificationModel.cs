using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Messenjoor.Shared.Models
{
        public class PusherNotificationModel
        {
            public string[]? interests { get; set; }
            
            public Notification? web { get; set; }    
            public Notification? apns { get; set; }
            public Notification? fcm { get; set;}

            
        }
        //public record PusherNotification(string[]? interests, Notification? web, Notification? apns, Notification? fcm);
        public record Notification(NotificationContent notification);
        public record NotificationContent(string title,string body);

}
