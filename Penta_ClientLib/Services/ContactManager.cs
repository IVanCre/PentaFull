using Penta_ClientLib.Interfaces;
using Penta_ClientLib.MethodResults;
using System.Text;


namespace Penta_ClientLib.Services
{
    public interface IContactHolder
    {
        Task<bool> AddContactAsync(string userName, int userID);
        Task<int> GetContactIDAsync(string userName);
        Task<bool> DeleteContactAsync(string userName);
    }

    internal class ContactManager(
        ISettingsProvider settings,
        IContactHolder contactHolder) : IContactManager
    {
        private ISettingsProvider _settings = settings;
        private IContactHolder _contactHolder= contactHolder;


        public async Task<BOOLResult> AddNewUserContact(string userName, string userContactID)
        {
            try
            {
                int userID = GetContactID(userContactID);
                var result = await _contactHolder.AddContactAsync(userName, userID);
                return new BOOLResult(result, null);
            }
            catch (Exception ex)
            {
                return new BOOLResult(false, ex);
            }
        }
        public async Task<bool> AutoAddNewUserContact(string userName, int userID)
        {
            try
            {
                return await _contactHolder.AddContactAsync(userName, userID);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<int> GetUserID(string userName)
        {
            try
            {
                return await _contactHolder.GetContactIDAsync(userName);
            }
            catch(Exception e)
            {
                return -1;
            }
        }

        public async Task<BOOLResult> DeleteUserContact(string userName)
        {
            try
            {
                var result = await _contactHolder.DeleteContactAsync(userName);
                return new BOOLResult(result, null);
            }
            catch (Exception ex)
            {
                return new BOOLResult(false, ex);
            }
        } 





        private int GetContactID(string contactString)// формат типа телефона 8-218-729-22-21
        {
            StringBuilder str = new(contactString);
            str.Replace("-", "");//вырезаем тире
            str.Remove(0, 1);//вырезаем бесполезную восьмерку

            var masked = long.Parse(str.ToString());
            return Convert.ToInt32(3_000_000_000 - masked);
        }
        public async Task<STRResult> GetMyContactString()
        {
            var userID = _settings.GetValueByName<int>("userID");
            var masked = (3_000_000_000 - userID).ToString();
            StringBuilder str = new(masked);
            str.Insert(0, "8");
            str.Insert(1, '-');
            str.Insert(5, '-');
            str.Insert(9, '-');
            str.Insert(12, '-');// формат типа телефона 8-218-729-22-21 ))))))

            return new STRResult(str.ToString(), null);
        }


    }
}
