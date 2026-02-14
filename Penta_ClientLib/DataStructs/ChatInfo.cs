namespace Penta_ClientLib.DataStructs
{
    public sealed class ChatInfo(
        int id,
        string name)
    {
        public  int ID { get; private set; } =id;
        public string ChatName { get; private set; } = name;
    }
}
