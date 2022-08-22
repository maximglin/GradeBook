using GradeBook.Models;
using GradeBook.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace GradeBook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new GradeBookContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Users.Add(new Storage.Entities.User()
                {
                    Login = "admin",
                    Password = "a1234",
                    IsAdmin = true
                });
                context.Users.Add(new Storage.Entities.User()
                {
                    Login = "maximglin",
                    Password = "1234",
                    IsAdmin = false
                });
                context.SaveChanges();
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<IGradeBookContext, GradeBookContext>();
            

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/Home/Index";
                options.LogoutPath = "/Home/Index";
            });

            builder.Services.AddSingleton<IAuthorizationHandler, AdminHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.AddRequirements(new Models.AdminRequirement());
                });
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}