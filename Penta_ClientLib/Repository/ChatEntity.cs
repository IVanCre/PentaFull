using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class ChatEntity
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Indexed(Name = "Name")]
        public string Name { get; set; }

    }
}
