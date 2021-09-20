using BookStoreAPI.Data;
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

        // This method gets called by the runtime. Use this method to add services to the container.
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

            //Add Service
            services.AddTransient<BooksService>();
            services.AddTransient<PublishersService>();
            services.AddTransient<AuthorsService>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            //Add Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            //Add JWT Bearer
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
                    //ValidateIssuer = true,
                    //ValidIssuer = Configuration["JWT:Issuer"],

                    //ValidateAudience = true,
                    //ValidAudience = Configuration["JWT:Audience"]
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            //   var serviceProvider = app.ApplicationServices.GetService<IServiceProvider>(); 
            //  CreateRoles(serviceProviderr).Wait();
            CreateRoles(serviceProvider).Wait();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            //adding customs roles : Question 1
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string roleName = "Admin";
            IdentityResult roleResult;

            //     foreach (var roleName in roleNames)
            //      {
            var roleExist = await RoleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                //create the roles and seed them to the database: Question 2
                roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
            }
            //      }

            //Here you could create a super user who will maintain the web app
            var poweruser = new ApplicationUser
            {
                UserName = Configuration["AppSettings:UserName"],
            };

            string userPWD = Configuration["AppSettings:UserPassword"];
            var _user = await UserManager.FindByNameAsync(Configuration["AppSettings:UserName"]);

            if (_user == null)
            {
                var createPowerUser = await UserManager.CreateAsync(poweruser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    //here we tie the new user to the role : Question 3
                    await UserManager.AddToRoleAsync(poweruser, "Admin");
                }
            }
            //return Ok();
        }
    }
}
