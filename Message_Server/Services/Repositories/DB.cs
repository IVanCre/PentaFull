using System.Collections.Generic;
using Message_Server.Services.Repositories.Models;
using Microsoft.EntityFrameworkCore;



namespace Message_Server.Services.Repositories
{

    public class DB : DbContext
    {
        private string _connString;


        public DbSet<MessageEntity> Messages => Set<MessageEntity>();
        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<TokenEntity> Tokens => Set<TokenEntity>();
        public DbSet<GroupEntity> Groups => Set<GroupEntity>();


        public DB(string connString)
        {
            _connString = connString;
        }
        public DB() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connString);
        }
    }
}
