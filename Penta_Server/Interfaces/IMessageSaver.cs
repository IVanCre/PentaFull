using MessageLib;

namespace Penta_Server.Interfaces
{
    /// <summary>
    /// Сохраняет Сообщения в БД
    /// </summary>
    public interface IMessageSaver
    {
        public void Save(List<Message> messages);
        public void Save(Message msg);
    }
}
