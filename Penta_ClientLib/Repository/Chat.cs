using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class Chat
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Indexed(Name = "Name")]
        public string Name { get; set; }

    }
}
