

namespace Message_Server.StaticUtilits
{
    public sealed class RoleNames
    {
        public const string Admin = "Administrator";
        public const string User = "User";
    }

    public static class UserRoleCreator
    {
        /// <summary>
        /// Создает Роль на основе связки ник-пароль.
        /// Для роли админа существуют специальные критерии!
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns> 
        /// <exception cref="NotImplementedException"></exception>
        public static string GenerateRole(string username, string password)
        {
            if (username.ToLower() == "admin_1991" && PasswordManager.IsPasswordStrong(password))
                return RoleNames.Admin;
            else
                return RoleNames.User;
        }
    }
}
