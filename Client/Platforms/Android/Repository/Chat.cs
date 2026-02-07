using SQLite;


namespace Client.Platforms.Android.Repository
{
    internal class Chat
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Indexed(Name = "Name")]
        public string Name { get; set; }

    }
}
