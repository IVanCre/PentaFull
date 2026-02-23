using Android.Content;
using AndroidX.Core.App;
using Android.App;
using Android.OS;
using Android.Graphics;



namespace Client.Platforms.Android
{
    public class NotificationHelper//отображает уведомления и позволяет тапать по ним
    {
        private readonly Context _context;
        private const string ChannelId = "penta_msg_channel";
        private static int _counterID = 0;//чтобы между экземплярами сохранялся
        private int smallIconID;
        private int largeIconID;

        public NotificationHelper(Context context)
        {
            _context = context;
            CreateNotificationChannel();

            smallIconID = context.Resources.GetIdentifier("hands", "drawable", context.PackageName);//объявляется в манифесте, живет в папке Images
            largeIconID = context.Resources.GetIdentifier("appicon", "mipmap", context.PackageName);
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, "PentaChannel", NotificationImportance.Default);
                var manager = (NotificationManager)_context.GetSystemService(Context.NotificationService);
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
            notificationManager.Notify(GenerateNextID(), CreateNotify(title,text));

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

        private Notification CreateNotify(string title, string text)
        {
            var intent = _context.PackageManager.GetLaunchIntentForPackage(_context.PackageName);
            intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
            var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(_context, ChannelId)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(smallIconID) // Иконка(типа системной) в левом верхнем углу экрана
                .SetLargeIcon(BitmapFactory.DecodeResource(_context.Resources, largeIconID))//иконка в уведомлении
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityHigh) // важное уведомление
                .SetCategory(NotificationCompat.CategoryMessage);

            return builder.Build();
        }

        private int GenerateNextID()
        {
            if (_counterID >= int.MaxValue)
                _counterID = 0;

            return _counterID++;
        }
    }
}
