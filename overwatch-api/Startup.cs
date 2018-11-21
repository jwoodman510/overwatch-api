using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using overwatch_api.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace overwatch_api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()));

            services
                .AddCors()
                .AddMemoryCache()
                .AddHttpClient()
                .AddLogging(x => x.AddConsole());

            services
                .AddTransient<IStatsService, OwApiStatsService>()
                .AddTransient<IStatsService, OwApiNetStatsService>();

            services.AddSwaggerGen(x => x.SwaggerDoc("v1", new Info { Title = "Overwatch API", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger()
               .UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "Overwatch API V1"))
               .UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod())
               .UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"))
               .UseMvc();
        }
    }
}
