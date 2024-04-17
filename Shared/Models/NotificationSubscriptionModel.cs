using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenjoor.Shared.Models
{
    public class NotificationSubscriptionModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? Url { get; set; }

        public string? P256dh { get; set; }

        public string? Auth { get; set; }
    }
}
