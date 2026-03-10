using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class AmGroupAdminEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed(Name = "ChatID")]
        public int ChatID { get; set; }
    }
}
