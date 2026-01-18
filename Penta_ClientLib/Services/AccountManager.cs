using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;

namespace Penta_ClientLib.Services
{

    internal class AccountManager(
        ISettingsProvider settings,
        IWebClient webClient):IAccountManager
    {
        private ISettingsProvider _settings=settings;
        private IWebClient _webClient=webClient;

        public Task<BOOLResult> DeleteAccount()
        {
            throw new NotImplementedException();
        }


        public async Task<BOOLResult> Registration(string login, string password)
        {
            try
            {
                int userID = await _webClient.TryRegisterAsync(login, password);
                if (userID > -1)
                {
                    _settings.SetValueByName("userLogin",login);
                    _settings.SetValueByName("userPassword", password);
                    _settings.SetValueByName("userID",userID);
                    return new BOOLResult(true, null);
                }
                else
                    return new BOOLResult(false, null);
            }
            catch (Exception e)
            {
                return new BOOLResult(false, e);
            }
        }
        public async Task<BOOLResult> Login(string login, string password)
        {
            bool result = false;
            if (IsRegistred())
            {
                result = await _webClient.TryLoginAsync(login, password);
                return new BOOLResult(result, null);
            }
            else
                return new BOOLResult(false, new Exception("Клиент не зарегистрирован"));

        }

        private bool IsRegistred()
        {
            return !string.IsNullOrEmpty(_settings.GetValueByName<string>("userID"));
        }
    }
}
