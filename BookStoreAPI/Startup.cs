using BookStoreAPI.Data;
using BookStoreAPI.Data.Mapper;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStoreAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    mySqlOptions => mySqlOptions.ServerVersion(new Version(8, 0, 18), ServerType.MySql));
            });

            services.AddScope<BooksService>();
            services.AddScope<PublishersService>();
            services.AddScope<AuthorsService>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JWT:Secret"])),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            AppDbInitializer.SeedRoles(app).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            CreateRoles(serviceProvider).Wait();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string roleName = "Admin";
            
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var adminRole = new IdentityRole(roleName);
                await roleManager.CreateAsync(adminRole);
            }


            var userPWD = Configuration["AppSettings:UserPassword"];
            var userName = Configuration["AppSettings:UserName"];
            
            var poweruser = new ApplicationUser
            {
                UserName = userName,
            };
            
            var _user = await userManager.FindByNameAsync(userName);

            if (_user == null)
            {
                var createPowerUser = await userManager.CreateAsync(poweruser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(poweruser, roleName);
                }
            }
        }
    }
}
