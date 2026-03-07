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

        public void Save(List<Message> messages);
        public void Save(Message msg);



        /// <summary>
        /// сохраняет одну копию для нескольких адресатов(для группы получателей)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="recieversID"></param>
        public void SaveOptimizedCopy(Message msg, int[] recieversID);

        /// <summary>
        /// Получает список получателей для этого сообщения на текущий момент
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task<int[]> GetRecieversID(Message msg);
    }
}
