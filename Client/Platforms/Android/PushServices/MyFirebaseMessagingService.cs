using AndroidAppLib = Android.App;
using Firebase.Messaging;
using Android.Runtime;
using Penta_ClientLib.Interfaces;


namespace Client.Platforms.Android.PushServices
{
    //сервис, который в фоне обрабатывает входящие сообщения от Firebase
    [Register("com.companyname.penta.MyFirebaseMessagingService")]
    [AndroidAppLib.Service(Exported = false)]
    [AndroidAppLib.IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        private IClientFacade _clientFacade;

        public MyFirebaseMessagingService()
        {
            _clientFacade= App.Services.GetRequiredService<IClientFacade>();
        }


        // Срабатывает при получении сообщения
        public override async void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            // Извлекаем данные из Data-пакета
            if (message.Data.Count > 0)
            {
                string title = message.Data["title"];
                string text = message.Data["message"];

                int userID;
                if (int.TryParse(title, out userID))//пробуем подставить имя отправителя из контактов
                    title = await _clientFacade.FindUserPseudonimeByID(userID);
                else
                    title = "Неизвестный отправитель";


                var _notifier = new NotificationHelper(AndroidAppLib.Application.Context);
                _notifier.ShowNotification(title, text);//отолбражаем уведомление
            }
        }

        
        public override void OnNewToken(string token)//когда экосистема firebase сама обновляет у себя токен и присылает его нам
        {
            base.OnNewToken(token);

            var _tokenSender =App.Services.GetRequiredService<DeviceTokenSender>();
            if (_tokenSender != null)
                _tokenSender.UpdateToken();
        }
    }
}
