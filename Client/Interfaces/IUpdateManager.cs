using Penta_ClientLib.DataStructs;

namespace Client.Interfaces
{
    internal interface IUpdateManager
    {
        Task TryUpdateClientAsync(ClientType clientType);
    }
}
