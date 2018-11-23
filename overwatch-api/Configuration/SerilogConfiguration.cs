using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace overwatch_api
{
    public static class SerilogConfiguration
    {
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder builder)
        {
            return builder.UseSerilog((ctx, log) => Configure(ctx, log));
        }

        private static void Configure(WebHostBuilderContext ctx, LoggerConfiguration log)
        {
            if(ctx.HostingEnvironment.IsDevelopment())
            {
                log
                    .WriteTo.Console()
                    .WriteTo.File(@"Logs\.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);
            }
            else
            {
                log.WriteTo.ApplicationInsightsEvents(ctx.Configuration["ApplicationInsights:InstrumentationKey"]);
            }
        }
    }
}
