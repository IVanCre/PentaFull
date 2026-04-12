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
        private string _firebaseSDKFile = "penta-client-1928-firebase-adminsdk-fbsvc-6fb6a671e1.json";

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
                        Credential = GoogleCredential.FromFile("penta-client-1928-firebase-adminsdk-fbsvc-6fb6a671e1.json")
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
            await SendPushToUserDevices(toUserID, fromUserID.ToString(), $"Нажмите, чтобы просмотреть. {DateTime.Now.ToString("HH:mm:ss")}");
        }

        public async Task SendPushToUserDevices(int userID, string title, string msg)
        {
            var findedDevices = await _deviceTknHolder.GetTokenDeviceByID(userID);
            if (findedDevices != null && findedDevices.Count > 0)
            {
                _logger?.SaveForDEBUG($"Пересылаем клиенту id={userID} пуш-уведомление ");
                foreach (var deviceToken in findedDevices)//веерная рассылка на все известные устройства
                {
                    var message = new Message()
                    {
                        Token = deviceToken,
                        Data = new Dictionary<string, string>()                // Поля Data — это то, что ваш Worker обработает в фоне
                        {
                            { "title",title },
                            { "message", msg }
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
                        _logger.SaveError(e.Message);
                    }
                }
            }
        }
    }
}
