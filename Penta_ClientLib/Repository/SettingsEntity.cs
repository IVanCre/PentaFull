using SQLite;


namespace Penta_ClientLib.Repository
{
    internal class SettingsEntity
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [Indexed(Name = "Name", Unique = true)]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
