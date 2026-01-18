using Penta_ClientLib.Interfaces;
using System.Text;


namespace Penta_ClientLib.Services
{

    internal class ContactManager(
        ISettingsHolder settings,
        IContactHolder contactHolder) : IContactManager
    {
        private ISettingsHolder _settings = settings;
        private IContactHolder _contactHolder= contactHolder;


        public async Task<Tuple<bool, Exception>> AddNewUserContact(string userName, string userContactID)
        {
            try
            {
                int userID = GetContactID(userContactID);
                var result = await _contactHolder.AddContactAsync(userName, userID);
                return Tuple.Create<bool,Exception>(result, null);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, ex);
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
        public async Task<Tuple<List<string>, Exception>> GetAllContacts()
        {
            try
            {
                var list = await _contactHolder.GetAllContacts();
                return Tuple.Create<List<string>, Exception>(list, null);
            }
            catch (Exception e)
            {
                return Tuple.Create< List<string>,Exception >(null, e);
            }
        }
        public async Task<Tuple<bool, Exception>> DeleteUserContact(string userName)
        {
            try
            {
                var result = await _contactHolder.DeleteContact(userName);
                return Tuple.Create<bool,Exception>(result, null);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, ex);
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
        public async Task<Tuple<string, Exception>> GetMyContactString()
        {
            var userID = await _settings.GetValueByName<int>("userID");
            var masked = (3_000_000_000 - userID).ToString();
            StringBuilder str = new(masked);
            str.Insert(0, "8");
            str.Insert(1, '-');
            str.Insert(5, '-');
            str.Insert(9, '-');
            str.Insert(12, '-');// формат типа телефона 8-218-729-22-21 ))))))

            return Tuple.Create<string,Exception>(str.ToString(), null);
        }


    }
}
