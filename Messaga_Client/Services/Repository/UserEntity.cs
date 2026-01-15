using SQLite;


namespace Messaga_Client.Services.Repository
{
    internal class UserEntity
    {
        [PrimaryKey, AutoIncrement]
        [Indexed(Name = "ID",Unique =true)]
        public int ID { get; set; }

        [Indexed(Name="Name")]
        public string Name { get; set; }
    }
}
