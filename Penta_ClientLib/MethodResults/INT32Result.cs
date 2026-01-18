namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Хранит int32 значение и ошибку(если есть)
    /// </summary>
    /// <param name="value">значение</param>
    /// <param name="error"></param>
    public sealed class INT32Result(
        int value,
        Exception error)
    {
        public readonly Exception Error=error;
        public readonly int Result=value;
    }
}
