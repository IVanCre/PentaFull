using MessageLib;

namespace Messaga_Client.Interfaces
{
    internal interface IWebClient
    {
        Task<bool> TryRegisterAsync(string userName, string pass);
        Task<bool> TryLoginAsync(string userName, string password);
        Task<bool> TryDeleteAccountAsync();


        Task<bool> ConnectToMessageHub();
        void DisconnectFromMessageHub();
        Task<bool> TrySendMessageToHub(Message message);
        Func<Message, Task> MessageRecieveAsync { get; set; }//обработка входящих сообщений с помощью внешней функции
    }
}
