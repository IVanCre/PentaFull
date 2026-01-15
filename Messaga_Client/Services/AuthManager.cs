using Messaga_Client.Interfaces;
using Messaga_Client.Services.DataProviders;


namespace Messaga_Client.Services
{

    internal class AuthManager(
        SettingsProvider settings,
        IWebClient webClient):IAuthManager
    {
        private SettingsProvider _settings=settings;
        private IWebClient _webClient=webClient;


        public bool IsRegistred()
        {
            return !string.IsNullOrEmpty(_settings.Login) && !string.IsNullOrEmpty(_settings.Password);
        }

        public async Task<bool> TryRegister(string login, string password)
        {
           bool result= await _webClient.TryRegisterAsync(login, password);
           if(result)
                result = await _settings.ChangeCredsAsync(login, password);

           return result;
        }

        public async Task<bool> TryLogin(string login,string password)
        {
            bool result = false;
            if (IsRegistred())
            {
                result =await _webClient.TryLoginAsync(login, password);
            }
            return result;
        }

        public async Task<bool> TryDeleteAccount()
        {
            bool result = false;
            if(IsRegistred())
                result = await _webClient.TryDeleteAccountAsync();

            return result;
        }
    }
}
