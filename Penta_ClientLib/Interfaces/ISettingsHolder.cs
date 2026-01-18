

namespace Penta_ClientLib.Interfaces
{
    public interface ISettingsHolder
    {
        Task<T> GetValueByName<T>(string paramName);
        void SetValueByName<T>(string paramName, T value );
    }
}
