using MessageLib;

namespace MessageClientLib.Interfaces
{
    internal interface IMessageProcessor
    {
        public Func<Message, Task> MessageRecieveAsync { get; set; }
        Task<BOOLEANResult> SendMessage(Message mesage);
    }
}
