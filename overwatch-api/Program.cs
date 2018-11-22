using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace overwatch_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((ctx, log) => log.WriteTo.Console().WriteTo.File(
                    @"Logs\.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7));
    }
}
