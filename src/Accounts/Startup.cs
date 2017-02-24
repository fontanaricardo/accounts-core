namespace Accounts
{
    using System;
    using System.IO;
    using Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services;

    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddSingleton<ISeiService>(new SeiService(Configuration));

            string connection = string.Empty;

            if (_environment.IsDevelopment())
            {
                connection = Configuration["StorageConnectionString"];
            }
            else
            {
                connection = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
            }

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection), ServiceLifetime.Scoped);

            IdentityConfiguration(services);

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            SetLog(env, loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Manage}/{action=Index}/{id?}");
            });
        }

        private static void IdentityConfiguration(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;

                // Cookie settings
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);

                // User settings
                options.User.RequireUniqueEmail = true;
            });
        }

        /// <summary>
        /// Add console logs.
        /// Set the log folder only in production, to save log in files.
        /// </summary>
        /// <remarks>
        /// In production, if the folder "logs" not exists, it will be created.
        /// </remarks>
        /// <param name="env">Enviroment object definitions.</param>
        /// <param name="loggerFactory">Logger factory service.</param>
        private void SetLog(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsProduction())
            {
                string appPath = Environment.GetEnvironmentVariable("OptPath") ?? "/opt/accounts-core/";
                string logsPath = Path.Combine(appPath, "logs");
                bool exists = System.IO.Directory.Exists(logsPath);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(logsPath);
                }

                loggerFactory.AddFile(Path.Combine(logsPath, "{Date}.txt"));
            }
        }
    }
}
