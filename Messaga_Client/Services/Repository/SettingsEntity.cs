using SQLite;


namespace Messaga_Client.Services.Repository
{
    internal class SettingsEntity
    {
        [PrimaryKey, AutoIncrement]
        [Indexed(Name = "ID", Unique = true)]
        public int ID { get; set;}
        public string Login { get; set; }
        public string Password { get; set; }

        public int MaxMessageHoldInDays { get; set; }
    }
}
