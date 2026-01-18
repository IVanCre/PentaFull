using Penta_ClientLib.Interfaces;


namespace Penta_ClientLib.Services
{

    internal class AccountManager(
        ISettingsHolder settings,
        IWebClient webClient):IAccountManager
    {
        private ISettingsHolder _settings=settings;
        private IWebClient _webClient=webClient;

        public Task<Tuple<bool, Exception>> DeleteAccount()
        {
            throw new NotImplementedException();
        }


        public async Task<Tuple<bool, Exception>> Registration(string login, string password)
        {
            try
            {
                int userID = await _webClient.TryRegisterAsync(login, password);
                if (userID > -1)
                {
                    _settings.SetValueByName("userLogin",login);
                    _settings.SetValueByName("userPassword", password);
                    _settings.SetValueByName("userID",userID);
                    return Tuple.Create<bool,Exception>(true, null);
                }
                else
                    return Tuple.Create<bool, Exception>(false, null);
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }
        public async Task<Tuple<bool,Exception>> Login(string login, string password)
        {
            bool result = false;
            if (!string.IsNullOrEmpty( await _settings.GetValueByName<string>("userID")))
            {
                result = await _webClient.TryLoginAsync(login, password);
                return Tuple.Create<bool, Exception>(result, null);
            }
            else
                return Tuple.Create(false, new Exception("Клиент не зарегистрирован"));

        }

    }
}
