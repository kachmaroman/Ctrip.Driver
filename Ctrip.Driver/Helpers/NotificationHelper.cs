using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Net;

namespace Ctrip.Driver.Helpers
{
    public class NotificationHelper : Java.Lang.Object
    {
	    public const string PrimaryChannel = "Urgent";
        public const int NotifyId = 100;

        public void NotifyVersion26(Context context, Android.Content.Res.Resources res, Android.App.NotificationManager manager)
        {

            string channelName = "Secondary Channel";
            NotificationImportance importance = NotificationImportance.High;
            NotificationChannel channel = new NotificationChannel(PrimaryChannel, channelName, importance);

            Uri path = Android.Net.Uri.Parse("android.resource://com.companyname.ctrip.driver/" + Resource.Raw.alert);

            AudioAttributes audioattribute = new AudioAttributes.Builder()
	            .SetContentType(AudioContentType.Sonification)
	            .SetUsage(AudioUsageKind.Notification)
	            .Build();

            channel.EnableLights(true);
            channel.EnableLights(true);
            channel.SetSound(path, audioattribute);
            channel.LockscreenVisibility = NotificationVisibility.Public;

            manager.CreateNotificationChannel(channel);

            Intent intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.CancelCurrent);

            Notification.Builder builder = new Notification.Builder(context)
                .SetContentTitle("Ctrip Driver")
                .SetSmallIcon(Resource.Drawable.ic_location)
                .SetLargeIcon(BitmapFactory.DecodeResource(res, Resource.Drawable.iconimage))
                .SetContentText("You have a new trip request")
                .SetChannelId(PrimaryChannel)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            manager.Notify(NotifyId, builder.Build());
        }
         
        public void NotifyOtherVersions(Context context, Android.Content.Res.Resources res, Android.App.NotificationManager manager)
        {
	        Intent intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.CancelCurrent);
            var path = Android.Net.Uri.Parse("android.resource://com.companyname.ctrip.driver/" + Resource.Raw.alert);


            Notification.Builder builder = new Notification.Builder(context)
                .SetContentTitle("Ctrip Driver")
                .SetSmallIcon(Resource.Drawable.ic_location)
                .SetLargeIcon(BitmapFactory.DecodeResource(res, Resource.Drawable.iconimage))
                .SetTicker("You have a new trip request")
                .SetChannelId(PrimaryChannel)
                .SetSound(path)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            manager.Notify(NotifyId, builder.Build());
        }
    }
}
