using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;

using System.Reflection;
using TRuDI.Backend.HanAdapter;
using Microsoft.Extensions.Logging;
using WebSocketManager;
using System;
using TRuDI.Backend.MessageHandlers;

namespace TRuDI.Backend
{
    using TRuDI.Backend.HanAdapter;
    using TRuDI.Backend.MessageHandlers;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddWebSocketManager();
            services.AddSingleton<ApplicationState>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                foreach(var hanAdapterInfo in HanAdapterRepository.AvailableAdapters)
                {
                    options.FileProviders.Add(new EmbeddedFileProvider(hanAdapterInfo.Assembly, hanAdapterInfo.BaseNamespace));
                }
            });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseWebSockets();

            app.MapWebSocketManager("/notifications", serviceProvider.GetService<NotificationsMessageHandler>());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=OperatingModeSelection}/{action=Index}/{id?}");

                routes.MapRoute("resources", "{*path}", new { controller = "Ressources", action = "Get", path = string.Empty });
            });
        }
    }
}