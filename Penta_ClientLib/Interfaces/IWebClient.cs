using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    public delegate void MessageRecieved(Message msg);
    public interface IWebClient :IDisposable
    {
        Task<int> TryRegisterAsync(string userName, string pass);
        Task<bool> TryDeleteAccountAsync();
        Task<bool> SendDeviceToken(string tokenDevice);

        Task<bool> ConnectToMessageHub();

        Task<bool> SendMessage(Message message);

        public event MessageRecieved RecievedMessage;//обработка входящих сообщений с помощью внешней функции
        public event ConnectionStateChanged ConnectionStateChanged;//состояние подключение к серваку
    }
}
