using Messenjoor.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Messenjoor.Services
{
    public static  class NotificationService
    {
        public static WebApplication MapNotificationApi(this WebApplication app)
        {

            // Subscribe to notifications
            app.MapPut("/notifications/subscribe", [Authorize] async (
                HttpContext context,
                ChatContext db,
                NotificationSubscriptionModel subscription) => {

                    // We're storing at most one subscription per user, so delete old ones.
                    // Alternatively, you could let the user register multiple subscriptions from different browsers/devices.
                    var userId = GetUserId(context);
                    if (userId is null)
                    {
                        return Results.Unauthorized();
                    }
                    var oldSubscriptions = db.NotificationSubscriptions.Where(e => e.UserId == userId);
                    db.NotificationSubscriptions.RemoveRange(oldSubscriptions);

                    // Store new subscription
                    subscription.UserId = userId ?? 0;
                    db.NotificationSubscriptions.Attach(new NotificationSubscription() { //Id= subscription.Id,
                                                                                         UserId = subscription.UserId,
                                                                                         Url = subscription.Url,
                                                                                        P256dh = subscription.P256dh,
                                                                                        Auth = subscription.Auth});

                    await db.SaveChangesAsync();
                    return Results.Ok(subscription);

                });

            // Specials
            //app.MapGet("/specials", async (ChatContext db) => {

            //    var specials = await db.Specials.ToListAsync();
            //    return Results.Ok(specials);

            //});

            //// Toppings
            //app.MapGet("/toppings", async (ChatContext db) => {
            //    var toppings = await db.Toppings.OrderBy(t => t.Name).ToListAsync();
            //    return Results.Ok(toppings);
            //});

            return app;

        }

        public static int? GetUserId(HttpContext context) => int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    }
}
