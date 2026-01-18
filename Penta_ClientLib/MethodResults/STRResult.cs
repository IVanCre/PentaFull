namespace Penta_ClientLib.Interfaces
{
    /// <summary>
    /// Хранит string значение и ошибку(если есть)
    /// </summary>
    /// <param name="value">значение</param>
    /// <param name="error"></param>
    public sealed class STRResult(
        string value,
        Exception error)
    {
        public readonly Exception Error = error;
        public readonly string Result = value;
    }
}
