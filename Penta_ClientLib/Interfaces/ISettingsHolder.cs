namespace Penta_ClientLib.Interfaces
{
    internal interface ISettingsHolder
    {
        Task<T> GetValueByName<T>(string paramName);
        Task SetValueByName<T>(string paramName, T value);
    }
}
