using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class Contact
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Indexed(Name = "UserName", Unique = true)]
        public string UserName { get; set; }
    }
}
