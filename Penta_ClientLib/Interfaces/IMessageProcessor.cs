using Penta_ClientLib.MethodResults;
using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal delegate void MessageRecieved(Message msg);

    internal interface IMessageProcessor
    {
        public event MessageRecieved RecievedMessage;
        Task<BOOLResult> SendMessage(Message mesage);
    }
}
