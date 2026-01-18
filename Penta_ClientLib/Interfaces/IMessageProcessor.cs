
using MessageLib;


namespace Penta_ClientLib.Interfaces
{
    internal delegate void MessageRecieved(Message msg);

    internal interface IMessageProcessor
    {
        public event MessageRecieved RecievedMessage;
        Task<Tuple<bool, Exception>> SendMessage(Message mesage);
    }
}
