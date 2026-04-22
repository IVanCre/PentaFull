namespace Penta_Server.Interfaces
{
    public enum ClientType
    {
        Android=0,
        Windows
    }

    public interface IClientFileObserver
    {
        /// <summary>
        /// Ищет имя файла самого свежего билда для конкретной платформы относительно указанной версии.
        /// Для актуальной версии вернет ничего
        /// </summary>
        /// <param name="oldClientVersion">текущая версия клиента</param>
        /// <param name="type">тип клиента</param>
        /// <returns></returns>
        Task<string> GetNewClientVersionFileNameAsync(string oldClientVersion, ClientType type);
        
        /// <summary>
        /// Ищет имя самого свежего билда для конкретной платформы
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<string> GetNewClientVersionFileNameAsync(ClientType type);

        /// <summary>
        /// Считывает конкретный файл для конкретной платформы
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<FileStream> ReadFileAsync(string fileName, ClientType type);
    }
}
