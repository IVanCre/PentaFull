using MessageLib;



namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Занимаетася обработкой входящих сообщений(от клиента к серверу)
    /// </summary>
    public interface IMessageProcessor
    {
        Task ProcessingMessage(Message msg);
    }
}
