using MessageLib;
using Penta_Server.Interfaces;
using System.Text;


namespace Penta_Server.Services
{
    public class DataSourcesApi(
        IDataSourceRepository dataRepo,
        IGroupChatRepository groupRepo,
        IUserRepository userRepo,
        IMessageProcessor msgProc) : IDataSourcesApi
    {
        private readonly IDataSourceRepository _dataSourceRepository = dataRepo;
        private readonly IGroupChatRepository _groupRepo = groupRepo;
        private IUserRepository _userRepo = userRepo;
        private IMessageProcessor _msgProc=msgProc;

        public async Task<Tuple<int, string>> RegistNewService(string serviceName)
        {
            Tuple<int, string> result= Tuple.Create(-1, string.Empty);
            if(serviceName.Length>0)
            {
                var finded =await _dataSourceRepository.FindByName(serviceName);
                if(finded==null)
                {
                    int id =await _groupRepo.CreatGroupAsync(-1,serviceName);
                    var addedItem =await _dataSourceRepository.AddNewDataSource(serviceName,id);
                    if (addedItem != null)
                        result = Tuple.Create(addedItem.ID, addedItem.AccessToken);
                }
            }

            return result;
        }

        public async Task<bool> AddUserToNotify(string connectID, string serviceToken)
        {
            bool result = false;
            if(serviceToken!=string.Empty)
            {
                var finded = await _dataSourceRepository.FindByToken(serviceToken);
                if(finded!=null)
                {
                    int userID = ExtractUserID(connectID);
                    if (userID > 0)
                    {
                        var findedUser = _userRepo.FindUserNameByIDAsync(userID);
                        result= await _groupRepo.AddUserToGroupAsync(userID, finded.GroupID);
                    }
                }
            }
            return result ;
        }

        private int ExtractUserID(string userContactID)//дубликат метода есть в класе ContactConverter в Penta_ClientLib
        {
            try
            {
                if (!string.IsNullOrEmpty(userContactID))
                {
                    if (userContactID.Length == 15 &&
                         userContactID[0] == '8' &&
                         userContactID[1] == '-' &&
                         userContactID[5] == '-' &&
                         userContactID[9] == '-' &&
                         userContactID[12] == '-')
                    {
                        uint _mask = 3_305_078_396;
                        StringBuilder str = new(userContactID);
                        str.Replace("-", "");//вырезаем тире
                        str.Remove(0, 1);//вырезаем бесполезную восьмерку

                        var masked = long.Parse(str.ToString());
                        return Convert.ToInt32(_mask - masked);
                    }
                    else
                        return -1;
                }
            }
            catch (Exception e) { }//если мы тут -все плохо

            return -1;
        }


        public async Task<bool> DeleteService(string serviceToken)
        {
            if (serviceToken != string.Empty)
            {
                var finded =await _dataSourceRepository.FindByToken(serviceToken);
                if (finded != null)
                {
                    var result =await _dataSourceRepository.DeleteDataSource(serviceToken);
                    result &=await _groupRepo.DeleteGroup(-1,finded.GroupID);
                    return result;
                }
            }

            return false;
        }


        public async Task<bool> SendMessage(string message, string serviceToken)
        {
            if (serviceToken != string.Empty)
            {
                var finded = await _dataSourceRepository.FindByToken(serviceToken);
                if (finded != null)
                {
                    _msgProc.ProcessingMessage(
                        MessageFactory.CreateUserToGroupChat(
                            -1,//потому что это не фактический юзер, а внешний сервис
                            finded.GroupID,
                            MessageType.Text,
                            MessageUtils.TextToBytes(message),
                            Guid.NewGuid()));

                    return true;
                }
            }
            return false;
        }
    }
}
