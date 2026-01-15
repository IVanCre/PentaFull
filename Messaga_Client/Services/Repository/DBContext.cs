using SQLite;


namespace Messaga_Client.Services.Repository
{
    /// <summary>
    /// Аналог DBContext
    /// </summary>
    internal class DBContext
    {
        private const string DatabaseFilename = "MessagaClientDB.db3";
        private const SQLiteOpenFlags Flags =      
            SQLiteOpenFlags.ReadWrite | // open the database in read/write mode
            SQLiteOpenFlags.Create |    // create the database if it doesn't exist
            SQLiteOpenFlags.SharedCache;// enable multi-threaded database access
        private static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);


        private SQLiteAsyncConnection _db;
        public SQLiteAsyncConnection DB
        {
            get
            {
                if(_db==null)
                {
                    _db= new SQLiteAsyncConnection(DatabasePath, Flags);
                    DB.CreateTableAsync<SettingsEntity>();

                    DB.CreateTableAsync<UserEntity>();
                    DB.CreateTableAsync<MessageEntity>();
                    DB.CreateTableAsync<ChatInfoEntity>();
                }

                return _db;
            }
        }
    }
}
