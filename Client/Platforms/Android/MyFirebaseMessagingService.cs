using AndroidAppLib= Android.App;
using Firebase.Messaging;
using Android.Runtime;


namespace Client.Platforms.Android
{
    //сервис, который в фоне обрабатывает входящие сообщения от Firebase
    [Register("com.companyname.penta.MyFirebaseMessagingService")]
    [AndroidAppLib.Service(Exported = false)]
    [AndroidAppLib.IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        // Срабатывает при получении сообщения
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            // Извлекаем данные из Data-пакета
            if (message.Data.Count > 0)
            {
                string title = message.Data["title"];
                string text = message.Data["message"];

                ProcessInBackground(title,text);
            }
        }
        private void ProcessInBackground(string title, string text)
        {
            var _notifier = new NotificationHelper(AndroidAppLib.Application.Context);
            _notifier.ShowNotification(title,text);
        }

        // Срабатывает при обновлении токена (нужно отправить на ваш сервер)
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            // Отправьте этот token на свой бэкенд, чтобы знать, куда слать пуши
            Preferences.Set("fcm_token", token);
            Console.WriteLine("Обновился токен устройства");
        }
    }
}
