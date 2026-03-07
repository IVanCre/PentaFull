using MessageLib;


namespace Penta_Server.Interfaces
{
    public delegate void MesageSaved(Message savedMsg);
    /// <summary>
    /// Сохраняет Сообщения в БД через очередь
    /// </summary>
    public interface IMessageSaver
    {
        public event MesageSaved MessageSaved;//факт сохранения 

        public void Save(Message msg);
    }
}
