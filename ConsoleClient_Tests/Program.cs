using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Penta_ClientLib;
using Penta_ClientLib.Interfaces;


namespace ConsoleClient_Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddClientDependencies();

            var host = builder.Build();
            host.RunAsync();

            AppLogic.WorkLoop(host.Services);//тут уже наша логика
        }
    }
}
