using Android.Content;
using AndroidX.Core.App;
using AndroidApp=Android.App;
using Android.OS;
using Android.Graphics;




namespace Client.Platforms.Android.PushServices
{
    public interface INotifyHelper
    {
        int ShowNotification(string title, string text);
        int UpdateNotification(int notifyID, string title, string text);
        void SkipAllNotifications();
        void SkipNotification(int notifyID);
    }

    public class NotificationHelper: INotifyHelper//отображает уведомления и позволяет тапать по ним
    {
        private readonly Context _context;
        private const string _channelId = "penta_msg_channel_v3";
        private static int _counterID = 0;//чтобы между экземплярами сохранялся
        private int smallIconID;
        private int largeIconID;

        public NotificationHelper()
        {
            _context = AndroidApp.Application.Context;
            CreateNotificationChannel();

            smallIconID = _context.Resources.GetIdentifier("hands", "drawable", _context.PackageName);//объявляется в манифесте, живет в папке Images
            largeIconID = _context.Resources.GetIdentifier("message", "drawable", _context.PackageName);//drawable -это ТИП ресурса
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new AndroidApp.NotificationChannel(_channelId, "PentaChannel", AndroidApp.NotificationImportance.High);
                channel.LockscreenVisibility = AndroidApp.NotificationVisibility.Public;

                var manager = (AndroidApp.NotificationManager)_context.GetSystemService(Context.NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }

        /// <summary>
        /// Создает новое уведомление
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <returns>Возврвщает идентификатор отображенного уведомления</returns>
        public int ShowNotification(string title, string text)
        {
            var notificationManager = NotificationManagerCompat.From(_context);
            notificationManager.Notify(GenerateNextID(), CreateNotify(title, text));

            return _counterID;
        }

        /// <summary>
        /// Обновлеят уже существующее уведомление(для сжатия нескольких уведомлений в одно, например?)
        /// </summary>
        /// <param name="notifyID"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public int UpdateNotification(int notifyID, string title, string text)
        {
            var notificationManager = NotificationManagerCompat.From(_context);
            notificationManager.Notify(notifyID, CreateNotify(title, text));

            return notifyID;
        }

        private AndroidApp.Notification CreateNotify(string title, string text)
        {
            var intent = _context.PackageManager.GetLaunchIntentForPackage(_context.PackageName);
            intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
            var pendingIntent = AndroidApp.PendingIntent.GetActivity(_context, 0, intent, AndroidApp.PendingIntentFlags.UpdateCurrent | AndroidApp.PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(_context, _channelId)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(smallIconID) // Иконка(типа системной) в левом верхнем углу экрана
                .SetLargeIcon(BitmapFactory.DecodeResource(_context.Resources, largeIconID))//иконка в уведомлении
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityMax) // важное уведомление
                .SetDefaults(NotificationCompat.DefaultAll)//показ поверх всего
                .SetVisibility(NotificationCompat.VisibilityPublic)//показ на заблокированном экране 
                .SetCategory(NotificationCompat.CategoryMessage);

            return builder.Build();
        }

        private int GenerateNextID()
        {
            if (_counterID >= int.MaxValue)
                _counterID = 0;

            return _counterID++;
        }

        /// <summary>
        /// Удаляет все уведомления
        /// </summary>
        public void SkipAllNotifications()
        {
            var manager = (AndroidApp.NotificationManager)Platform.CurrentActivity.GetSystemService(Context.NotificationService);
            manager?.CancelAll();
        }
        public void SkipNotification(int id)
        {
            var manager = (AndroidApp.NotificationManager)Platform.CurrentActivity.GetSystemService(Context.NotificationService);
            manager?.Cancel(id);
        }
    }
}
