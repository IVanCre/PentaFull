using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class UserInChatEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed(Name = "UserID", Unique = true)]
        public int UserId { get; set; }

        public int ChatId { get; set; }
    }
}
