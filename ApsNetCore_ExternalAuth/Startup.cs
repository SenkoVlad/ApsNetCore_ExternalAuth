using ApsNetCore_ExternalAuth.Infrastructure.Data;
using ApsNetCore_ExternalAuth.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ApsNetCore_ExternalAuth
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
            services.AddDbContext<AppDbContext>(option =>
            {
                option.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddIdentity<User, Role>()
                    .AddEntityFrameworkStores<AppDbContext>();
            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        //   options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                    })
                    .AddGoogle(options =>
                    {
                        options.ClientId = Configuration["Authentication:Google:ClientId"];
                        options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    })
                    .AddCookie(option =>
                    {
                        option.Cookie.IsEssential = true;
                        option.LoginPath = "/Home/ExternalLogin/";
                    });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Home/ExternalLogin/";
            });
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
