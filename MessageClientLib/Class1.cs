using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace MessageClientLib
{
    public static class  Dependecies 
    {

        public static IServiceCollection AddClientDependencies(this IServiceCollection services)
        {
            //тут добавляем в контейнер наши классы

            return services;
        }
    }
}
