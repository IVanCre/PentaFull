using MessageLib;
using Microsoft.AspNetCore.SignalR;


namespace Message_Server.Interfaces
{
    public interface IMessageProcessor
    {
        Task ProcessingMessage(Message msg, IHubCallerClients connectedClients);
    }
}
