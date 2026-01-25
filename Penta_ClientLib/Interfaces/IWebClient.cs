using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    internal delegate void MessageRecieved(Message msg);
    internal interface IWebClient :IDisposable
    {
        Task<int> TryRegisterAsync(string userName, string pass);
        Task<bool> TryLoginAsync(string userName, string password);
        Task<bool> TryDeleteAccountAsync();


        Task<bool> SendMessage(Message message);

        public event MessageRecieved RecievedMessage;//обработка входящих сообщений с помощью внешней функции
    }
}
