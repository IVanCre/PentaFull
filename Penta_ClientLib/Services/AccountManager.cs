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

        public async Task<Tuple<bool, Exception>> RegistrationAsync(string login, string password)
        {
            try
            {
                var result = await _webClient.TryEnterAsync(login, password,true);
                if(result.Item2==null)
                { 
                    await _settings.SetUserLogin(login);
                    await _settings.SetUserPassword(password);
                    await _settings.SetUserID(result.Item1);
                    return Tuple.Create<bool,Exception>(true, null);
                }
                else
                    return Tuple.Create<bool, Exception>(false, result.Item2);//возвращаем серверную ошибку-ответ
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }
        public async Task<Tuple<bool, Exception>> LoginAsync(string login, string password)
        {
            try
            {
                var result = await _webClient.TryEnterAsync(login, password,false);
                if (result.Item2 == null)
                {
                    await _settings.SetUserLogin(login);
                    await _settings.SetUserPassword(password);
                    await _settings.SetUserID(result.Item1);
                    return Tuple.Create<bool, Exception>(true, null);
                }
                else
                    return Tuple.Create<bool, Exception>(false, result.Item2);//возвращаем серверную ошибку-ответ
            }
            catch (Exception e)
            {
                return Tuple.Create(false, e);
            }
        }

    }
}
