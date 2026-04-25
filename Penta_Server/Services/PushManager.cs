using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Penta_Server.Interfaces;


namespace Penta_Server.Services
{
    public class PushManager:IPushManager
    {
        private IDeviceTokenRepository _deviceTknHolder;
        private ILogWriter _logger;
        private string _firebaseSDKFile = Path.Combine(AppContext.BaseDirectory,"penta-client-1928-firebase-adminsdk-fbsvc-6fb6a671e1.json");

        public PushManager(
            IDeviceTokenRepository deviceTknHolder,
            ILogWriter logger)
        {
            _deviceTknHolder = deviceTknHolder;
            _logger = logger;

            if (FirebaseApp.DefaultInstance == null)
            {
                if (File.Exists(_firebaseSDKFile))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(_firebaseSDKFile)
                    });
                }
                else
                {
                    _logger.SaveError("FirebaseSDKFile not found");
                }
            }
        }


        // Метод отправки "тихого" сообщения (Data Message)
        public async Task SendPushToUserDevices(int toUserID, int fromUserID)
        {
            await SendPushToUserDevices(toUserID, fromUserID, $"Новое сообщение.Нажмите для просмотра. {DateTime.Now.ToString("HH:mm:ss")}");
        }

        public async Task SendPushToUserDevices(int toUserID, int fromUserID,  string msg)
        {
            var findedDevices = await _deviceTknHolder.GetTokenDeviceByID(toUserID);
            if (findedDevices != null && findedDevices.Count > 0)
            {
                _logger?.SaveInfo($"Пересылаем клиенту id={toUserID} пуш-уведомление ");
                foreach (var deviceToken in findedDevices)//веерная рассылка на все известные устройства
                {
                    var message = new Message()
                    {
                        Token = deviceToken,
                        Data = new Dictionary<string, string>()                // Поля Data — это то, что ваш Worker обработает в фоне
                        {
                            { "fromUserID", fromUserID.ToString()},
                            { "message", msg },
                            { "timestamp_msec",DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()}//когда сообщение отправлено с сервера
                        },
                        Android = new AndroidConfig()                // Важно для Android: высокий приоритет, чтобы "разбудить" устройство
                        {
                            Priority = Priority.High,
                        }
                    };

                    try
                    {
                        await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    }
                    catch(Exception e)
                    {
                        _logger.SaveError($"Ошибка при отправке Push: {e.Message}");
                    }
                }
            }
        }
    }
}
