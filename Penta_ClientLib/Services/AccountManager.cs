using MessageLib;
using Penta_ClientLib.Interfaces;


namespace Penta_ClientLib.Services
{

    internal class AccountManager(
        ISettingsProvider settings,
        IWebClient webClient):IAccountManager
    {
        private ISettingsProvider _settings=settings;
        private IWebClient _webClient=webClient;

        public async Task<Tuple<bool, Exception>> DeleteAccount()
        {
            try
            {
                var userID = await _settings.GetUserID();
                var result = await _webClient.SendMessage(MessageFactory.DeleteAccountRequest(userID));

                return Tuple.Create<bool, Exception>(result, null);
            }
            catch(Exception e)
            {
                return Tuple.Create(false, e);
            }
        }

        public async Task<Tuple<bool, Exception>> Registration(string login, string password)
        {
            try
            {
                int userID = await _webClient.TryRegisterAsync(login, password);
                if (userID > -1)
                {
                    await _settings.SetUserLogin(login);
                    await _settings.SetUserPassword(password);
                    await _settings.SetUserID(userID);
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

    }
}
