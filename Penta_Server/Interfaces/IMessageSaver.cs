using MessageLib;


namespace Penta_Server.Interfaces
{
    public delegate void MesageSaved(Message savedMsg);
    /// <summary>
    /// Сохраняет Сообщения в БД через очередь
    /// </summary>
    public interface IMessageSaver
    {
        public void Save(List<Message> messages);
        public void Save(Message msg);
        public event MesageSaved MessageSaved;//сообщение сохранено
    }
}
