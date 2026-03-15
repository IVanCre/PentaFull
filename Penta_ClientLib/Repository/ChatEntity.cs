using Penta_ClientLib.DataStructs;
using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class ChatEntity
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Indexed(Name = "Name", Unique = true)]
        public string Name { get; set; }
        public ChatType ChatType { get; set; }
        public bool HaveUnreaded { get; set; }

    }
}
