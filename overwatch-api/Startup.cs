using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .AddOptions()
                .AddMemoryCache()
                .AddHttpClient();

            services
                .AddTransient<IStatsService, OwHerokuStatsService>()
                .AddTransient<IStatsService, OwApiStatsService>()
                .AddTransient<IStatsService, OwApiNetStatsService>()
                .AddSingleton<ThrottleLock<OwApiNetStatsService>>();

            services
                .Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"))
                .Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"))
                .AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>()
                .AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();


            services.AddSwaggerGen(x => x.SwaggerDoc("v1", new Info { Title = "Overwatch API", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIpRateLimiting()
               .UseSwagger()
               .UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "Overwatch API V1"))
               .UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod())
               .UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"))
               .UseMvc();
        }
    }
}
