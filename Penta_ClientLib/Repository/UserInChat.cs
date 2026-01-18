using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class UserInChat
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed(Name = "UserID", Unique = true)]
        public int ContactId { get; set; }

        public int ChatId { get; set; }
    }
}
