using System;
using System.IO;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Plugin.LocalNotifications.Abstractions;

namespace Plugin.LocalNotifications
{
    /// <summary>
    /// Notifier implementation for Android
    /// </summary>
    public class LocalNotificationsImplementation : ILocalNotifications
    {
        /// <summary>
        /// Get or Set Resource Icon to display
        /// </summary>
        public static int NotificationIconId { get; set; }

        /// <summary>
        /// Gets or sets the optional local notification intent action.
        /// </summary>
        /// <value>The local notification intent action.</value>
        public static string LocalNotificationIntentAction { get; set; }

        internal const string LocalNotificationKey = "LocalNotification";

        /// <summary>
        /// The key of the bundle element that contains notification's id.
        /// </summary>
        public const string LocalNotificationIntentKey = "NotificationId";

        /// <summary>
        /// Show a local notification in the Notification Area and Drawer.
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of notifications</param>
        public void Show(string title, string body, int id = 0)
        {
            var builder = new NotificationCompat.Builder(Application.Context);
            builder.SetContentTitle(title);
            builder.SetContentText(body);
            builder.SetAutoCancel(true);

            if (NotificationIconId != 0)
            {
                builder.SetSmallIcon(NotificationIconId);
            }
            else
            {
                builder.SetSmallIcon(Resource.Drawable.plugin_lc_smallicon);
            }

            var resultIntent = string.IsNullOrEmpty(LocalNotificationIntentAction) ?
                                     GetLauncherActivity()
                                     :
                                     new Intent(LocalNotificationIntentAction).AddFlags(ActivityFlags.NewTask);

            resultIntent.PutExtra(LocalNotificationIntentKey, id);

            var resultPendingIntent = PendingIntent.GetActivity(Application.Context, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
            builder.SetContentIntent(resultPendingIntent);

            var notificationManager = NotificationManagerCompat.From(Application.Context);

            notificationManager.Notify(id, builder.Build());
        }

        public static Intent GetLauncherActivity()
        {
            return Application.Context.PackageManager.GetLaunchIntentForPackage(Application.Context.PackageName);
        }

        /// <summary>
        /// Schedule a local notification in the Notification Area and Drawer.
        /// </summary>
        /// <param name="title">Title of the notification</param>
        /// <param name="body">Body or description of the notification</param>
        /// <param name="id">Id of the notification</param>
        /// <param name="notifyTime">The time you would like to schedule the notification for</param>
        public void Show(string title, string body, int id, DateTime notifyTime)
        {
            var intent = CreateIntent(id);

            var localNotification = new LocalNotification();
            localNotification.Title = title;
            localNotification.Body = body;
            localNotification.Id = id;
            localNotification.NotifyTime = notifyTime;

            if (NotificationIconId != 0)
            {
                localNotification.IconId = NotificationIconId;
            }
            else
            {
                localNotification.IconId = Resource.Drawable.plugin_lc_smallicon;
            }

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(LocalNotificationKey, serializedNotification);

            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
            var triggerTime = NotifyTimeInMilliseconds(localNotification.NotifyTime);
            var alarmManager = GetAlarmManager();

            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        }

        /// <summary>
        /// Show a local toast notification.  Notification will also appear in the Notification Center on Windows Phone 8.1.
        /// </summary>
        /// <param name="id">Id of the scheduled notification you'd like to cancel</param>
        public void Cancel(int id)
        {
            var intent = CreateIntent(id);
            var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);

            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(Application.Context);
            notificationManager.Cancel(id);
        }

        private Intent CreateIntent(int id)
        {
            return new Intent(Application.Context, typeof(ScheduledAlarmHandler)).SetAction("LocalNotifierIntent" + id);
        }

        private AlarmManager GetAlarmManager()
        {
            return Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
        }

        private string SerializeNotification(LocalNotification notification)
        {
            var xmlSerializer = new XmlSerializer(notification.GetType());
            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, notification);

                return stringWriter.ToString();
            }
        }

        private long NotifyTimeInMilliseconds(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDifference = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;

            return utcTime.AddSeconds(-epochDifference).Ticks / 10000;
        }
    }
}