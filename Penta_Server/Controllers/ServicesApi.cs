
using Microsoft.AspNetCore.Mvc;
using Penta_Server.Interfaces;

namespace Penta_Server.Controllers
{

    [Route("ServicesApi")]
    [ApiController]
    public class ServicesApi(IDataSourcesApi servProvider) : ControllerBase
    {
        private readonly IDataSourcesApi _servProvider= servProvider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns>
        /// Tuple<int,string>
        /// int - id группы, куда прприходят сообщения
        /// string - токен доступа для этого сервиса
        /// </returns>
        [HttpGet("RegistService")]
        public async Task<Tuple<int,string>> RegistNewService(string serviceName)=> await _servProvider.RegistNewService(serviceName);

        [HttpPost("AddUserToNotify")]
        public async Task<bool> AddUserToNotify(string connectID, string serviceToken)=>await _servProvider.AddUserToNotify(connectID, serviceToken);


        [HttpPost("SendMessage")]
        public void SendMessage(string message, string serviceToken)
        {
            _=_servProvider.SendMessage(message, serviceToken);
        }


        [HttpDelete("DeleteService")]
        public async Task<bool> DeleteService(string serviceToken)=> await _servProvider.DeleteService(serviceToken);
    }
}
