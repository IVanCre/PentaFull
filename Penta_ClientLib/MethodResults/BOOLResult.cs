namespace Penta_ClientLib.MethodResults
{
    /// <summary>
    /// Хранит булевое значение и ошибку(если есть)
    /// </summary>
    /// <param name="value">значение</param>
    /// <param name="error"></param>
    public sealed class BOOLResult(
        bool value,
        Exception error)
    {
        public readonly Exception Error = error;
        public readonly bool Result = value;
    }
}
