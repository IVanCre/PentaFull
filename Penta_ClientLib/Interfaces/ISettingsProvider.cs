

namespace Penta_ClientLib.Interfaces
{
    public interface ISettingsProvider
    {
        T GetValueByName<T>(string paramName);
        void SetValueByName<T>(string paramName, T value );
    }
}
