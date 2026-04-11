using Android.Gms.Extensions;
using Firebase.Messaging;
using Penta_ClientLib.Interfaces;


namespace Client.Platforms.Android.PushServices
{
    internal class DeviceTokenSender
    {
        private IWebClient _client;
        private string _storedToken=string.Empty;

        public DeviceTokenSender(IWebClient client)
        {
            _client = client;
        }

        public async void UpdateToken()//тут мы его получили и сохранили в кэш
        {
            try
            {
                var result = await FirebaseMessaging.Instance.GetToken();
                if (result != null)
                    _storedToken = result.ToString();
            }
            catch (Exception ex) 
            {
                _storedToken = string.Empty;
            }
        }
        public async void SendTokenToServer()//а вот тут уже отсылаем токен, которрый получили
        {
            if (!string.IsNullOrEmpty(_storedToken))
                await _client.SendDeviceToken(_storedToken);

            _storedToken = string.Empty;
        }

    }

}
