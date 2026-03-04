using MessageLib;
using Penta_ClientLib.DataStructs;

namespace Penta_ClientLib.Interfaces
{
    public delegate void MessageRecieved(Message msg);
    public delegate void MessageSended(long messageID);

    public interface IWebClient :IDisposable
    {
        Task<int> TryRegisterAsync(string userName, string pass);
        Task<bool> TryDeleteAccountAsync();
        Task<bool> SendDeviceToken(string tokenDevice);

        /// <summary>
        /// Имя файла с самым свежим билдом клиентом, относительно указанной версии текущего
        /// </summary>
        /// <param name="currentVersion">major.minor.patch</param>
        /// <param name="type">тип клиента</param>
        /// <returns></returns>
        Task<string> GetNewestClientFilaName(string currentVersion, ClientType type);
        Task<HttpContent> LoadClientFileAsync(string fileName, ClientType type);


        Task<bool> ConnectToMessageHub();
        Task<bool> SendMessage(Message message);
        public event MessageRecieved RecievedMessage;//обработка входящих сообщений с помощью внешней функции
        public event MessageSended MessageSended;//факт успешной отправки сообщения на сервер
        public event ConnectionStateChanged ConnectionStateChanged;//состояние подключение к серваку
    }
}
