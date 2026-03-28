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

        /// <summary>
        /// Сохранение сообщения, с указанием для скольких копий оптимизировать хранение фактических данных 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="dataCopyCount">для скольких копий предназначенны данные(byte[] Data)</param>
        /// <param name="sharedDataMarker">объединяет сообщения, которые имеют одинаковые Data</param>
        public void Save(Message msg,long sharedDataMarker, int dataCopyCount=1);
    }
}
