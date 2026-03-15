using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class UserInChatEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed(Name = "UserID")]
        public int UserId { get; set; }

        [Indexed(Name = "ChatId")]
        public int ChatId { get; set; }
    }
}
