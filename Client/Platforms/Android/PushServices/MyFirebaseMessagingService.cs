using AndroidAppLib = Android.App;
using Firebase.Messaging;
using Android.Runtime;
using Penta_ClientLib.Interfaces;


namespace Client.Platforms.Android.PushServices
{
    //сервис, который в фоне(вне приложения) обрабатывает входящие сообщения от Firebase
    [Register("com.companyname.penta.MyFirebaseMessagingService")]
    [AndroidAppLib.Service(Exported = false)]
    [AndroidAppLib.IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        private IClientFacade _clientFacade;
        private INotifyHelper _notifier;

        public MyFirebaseMessagingService()
        {
            _clientFacade= App.Services.GetRequiredService<IClientFacade>();
            _notifier = App.Services.GetRequiredService<INotifyHelper>();
        }


        // Срабатывает при получении сообщения
        public override async void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            // Извлекаем данные из Data-пакета
            if (message.Data.Count > 0)
            {
                int fromID = int.Parse(message.Data["fromUserID"]);
                string text = message.Data["message"];
                long timestampMilisec = long.Parse(message.Data["timestamp_msec"]);//время отправки пуша

                var timestampOfLastConnect = await _clientFacade.GetTimestampOfLastConnectToServer();
//значит юзер подключался к серваку раньше, чем был отправлен пуш
//и следовательно, не видел это новое сообщение, которое сгенерировало этот пуш
                if (timestampOfLastConnect < timestampMilisec)
                {
                    string senderContactName = await _clientFacade.FindUserPseudonimeByID(fromID);
                    _notifier.UpdateNotification(fromID,senderContactName, text);//отолбражаем уведомление
                }
                //else -значит юзер зашел раньше, чем получил пуш и следовательно уже сам увидел новое сообщение
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
