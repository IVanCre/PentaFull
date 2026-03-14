using Android.Gms.Extensions;
using Firebase.Messaging;
using Penta_ClientLib.Interfaces;


namespace Client.Platforms.Android
{
    internal class DeviceTokenSender
    {
        private IWebClient _client;
        public DeviceTokenSender(IWebClient client) 
        {
            _client = client;
        }

        public async void SendTokenToServer()
        {
            try
            {
                var result = await FirebaseMessaging.Instance.GetToken();
                if (result != null)
                {
                    var token = result.ToString();
                    await _client.SendDeviceToken(token);
                }
            }
            catch (Exception ex)
            {
                // Если упало здесь, значит Firebase не инициализирован 
                // или нет Google Play Services на устройстве
                Console.WriteLine($"[FCM] Критическая ошибка: {ex.Message}");
            }
        }

    }

}
