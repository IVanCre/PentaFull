using MessageLib;

namespace Penta_ClientLib.Interfaces
{
    internal interface IWebClient
    {
        Task<int> TryRegisterAsync(string userName, string pass);
        Task<bool> TryLoginAsync(string userName, string password);
        Task<bool> TryDeleteAccountAsync();


        Task<bool> ConnectToMessageHub();
        void DisconnectFromMessageHub();


        Task<bool> SendMessage(Message message);

        public event MessageRecieved RecievedMessage;//обработка входящих сообщений с помощью внешней функции
    }
}
