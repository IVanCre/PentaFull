

namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Реализация в библиотеке отсутствует. Сделай сам и внедри через DI
    /// </summary>
    public interface ISettingsHolder
    {
        Task<T> GetValueByName<T>(string paramName);
        void SetValueByName<T>(string paramName, T value );
    }
}
