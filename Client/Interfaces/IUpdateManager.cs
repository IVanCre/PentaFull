using Penta_ClientLib.DataStructs;

namespace Client.Interfaces
{
    internal interface IUpdateManager
    {
        /// <summary>
        /// Запускает автоматическое автообновление клиента
        /// </summary>
        /// <param name="clientType"></param>
        /// <returns></returns>
        Task<Exception> TryUpdateClientAsync(ClientType clientType);
    }
}
