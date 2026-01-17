using MessageClientLib.Interfaces;

namespace MessageClientLib.Services
{

    internal class AccountManager(
        ISettingsProvider settings,
        IWebClient webClient):IAccountManager
    {
        private ISettingsProvider _settings=settings;
        private IWebClient _webClient=webClient;

        public Task<BOOLEANResult> DeleteAccount()
        {
            throw new NotImplementedException();
        }


        public async Task<BOOLEANResult> Registration(string login, string password)
        {
            try
            {
                bool result = await _webClient.TryRegisterAsync(login, password);
                if (result)
                {
                    _settings.SetValueByName("userLogin",login);
                    _settings.SetValueByName("userPassword", password);
                }

                return new BOOLEANResult(result, null);
            }
            catch (Exception e)
            {
                return new BOOLEANResult(false, e);
            }
        }
        public async Task<BOOLEANResult> Login(string login, string password)
        {
            bool result = false;
            if (IsRegistred())
            {
                result = await _webClient.TryLoginAsync(login, password);
                return new BOOLEANResult(result, null);
            }
            else
                return new BOOLEANResult(false, new Exception("Клиент не зарегистрирован"));

        }

        private bool IsRegistred()
        {
            return !string.IsNullOrEmpty(_settings.GetValueByName<string>("userLogin"));
        }

        //public async Task<bool> TryRegister(string login, string password)
        //{
        //   bool result= await _webClient.TryRegisterAsync(login, password);
        //   if(result)
        //        result = await _settings.ChangeCredsAsync(login, password);

        //   return result;
        //}

        //public async Task<bool> TryLogin(string login,string password)
        //{
        //    bool result = false;
        //    if (IsRegistred())
        //    {
        //        result =await _webClient.TryLoginAsync(login, password);
        //    }
        //    return result;
        //}

        //public async Task<bool> TryDeleteAccount()
        //{
        //    bool result = false;
        //    if(IsRegistred())
        //        result = await _webClient.TryDeleteAccountAsync();

        //    return result;
        //}
    }
}
