using SQLite;


namespace Client.Platforms.Android.Repository
{
    internal class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [Indexed(Name = "Name", Unique = true)]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
