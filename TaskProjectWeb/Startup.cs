using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskProject.Services;

namespace TaskProject
{
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
            services.AddControllersWithViews();

            services.AddRazorPages().AddRazorRuntimeCompilation();

            // Authentication using cookies (we sign-in after verifying credentials, but JWT is created in DAL for interop)
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/UserLogin";
                    options.Cookie.Name = "TaskProjectAuth";
                });

            services.AddAuthorization();

            // Add IHttpContextAccessor for accessing user claims in DelegatingHandler
            services.AddHttpContextAccessor();
            
            // Register JwtAuthHeaderHandler as transient for HttpClient pipeline
            services.AddTransient<JwtAuthHeaderHandler>();

            // Add framework services.
            services.AddMvc();

            // Register HttpClient for calling TaskReportApi
            // Register typed HttpClient to call TaskReportApi
            services.AddHttpClient<ITaskReportApiClient, TaskReportApiClient>(client =>
            {
                var baseUrl = Configuration.GetValue<string>("TaskReportApi:Url") ?? "https://localhost:44322/";
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

            services.Configure<RabbitMqOptions>(Configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddSingleton<IWeeklyTaskReportPublisher, RabbitMqWeeklyTaskReportPublisher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Added ILogger<Startup> to allow startup logging
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Application starting up");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // <--- must come before UseAuthorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseBrowserLink();
        }
    }
}
