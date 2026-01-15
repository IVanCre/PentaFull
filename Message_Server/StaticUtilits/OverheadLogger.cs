namespace Message_Server.Utilits
{
    /// <summary>
    /// Использовать в местах, куда нельзя прокинуть зависимостью, но очень надо
    /// </summary>
    public static class OverheadLogger
    {
        public static void LogError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"SYSTEM ERROR: {DateTime.Now} {text}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
