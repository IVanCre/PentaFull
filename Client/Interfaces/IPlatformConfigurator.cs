using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Interfaces
{
    internal interface IPlatformConfigurator
    {
        /// <summary>
        /// Первичная настройка, регистрация и получение разрешений
        /// </summary>
        void FirstConfigurate();
    }
}
