using Messaga_Client.Services.Repository;


namespace Messaga_Client.Services.DataProviders
{
    internal class SettingsProvider
    {
        private DBContext _dbContext;

        public string Login { get; private set; }
        public string Password { get; private set; }
        public int MaxMessageHoldInDays { get; private set; } = 30;


        public SettingsProvider(DBContext context)
        {
            _dbContext = context;

            var finded = GetClientSettingsAsync().Result;
            Login = finded.Login;
            Password = finded.Password;
            MaxMessageHoldInDays = finded.MaxMessageHoldInDays;

        }

        private async Task<SettingsEntity> GetClientSettingsAsync()
        {
            var finded = await _dbContext.DB.Table<SettingsEntity>().ToListAsync();
            return finded[0];
        }

        public async Task<bool> ChangeCredsAsync(string login, string password)
        {
            if (string.IsNullOrEmpty(Login) && string.IsNullOrEmpty(password))
            {
                var finded = await GetClientSettingsAsync();
                finded.Login = login;
                finded.Password = password;

                var updated = await _dbContext.DB.UpdateAsync(finded);
                return updated == 1;
            }
            else
                return false;
        }
    }
}
