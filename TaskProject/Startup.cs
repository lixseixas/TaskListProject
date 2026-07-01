using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Bl;

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

            // Add framework services.
            //services.AddDbContext<TaskContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //descomente para utilizar o localDB
            services.AddDbContext<TaskContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("LocalDbConnection")));

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddScoped<TasksDal>();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Added ILogger<Startup> to allow startup logging
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TaskContext context, ILogger<Startup> logger)
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            DbInitializer.Initialize(context);

            app.UseBrowserLink();

            //BackgroundJob.Enqueue<FileWatcher>(fw => fw.Watch());
        }
    }
}
