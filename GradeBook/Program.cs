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

                var maximglin = new Storage.Entities.User()
                {
                    Login = "maximglin",
                    Password = "1234",
                    IsAdmin = false
                };
                context.Users.Add(maximglin);

                var t = new Storage.Entities.Teacher() { Name = "Рожкова Оксана Александровна" };
                context.Teachers.Add(t);
                var t2 = new Storage.Entities.Teacher() { Name = "Гаврилов Андрей Геннадьевич" };
                context.Teachers.Add(t2);

                var g1 = new Storage.Entities.Group() { Name = "ИДБ-20-01" };
                var g2 = new Storage.Entities.Group() { Name = "ИДБ-20-02" };
                var g3 = new Storage.Entities.Group() { Name = "ИДБ-20-03" };
                context.Groups.AddRange(new[] { g1, g2, g3 });

                var bd = new Storage.Entities.Subject() { Name = "Базы данных" };
                bd.Groups.AddRange(new[] { g1, g2, g3 });
                context.Subjects.Add(bd);

                context.TeacherSubjectGroupRelations.Add(new Storage.Entities.TeacherSubjectGroup()
                {
                    Teacher = t,
                    Subject = bd,
                    Group = g1
                });
                context.TeacherSubjectGroupRelations.Add(new Storage.Entities.TeacherSubjectGroup()
                {
                    Teacher = t,
                    Subject = bd,
                    Group = g2
                });

                maximglin.Teachers.Add(t);


                var st1 = new Storage.Entities.Student()
                {
                    Name = "Глинкин М.О.",
                    Group = g1
                };
                var st2 = new Storage.Entities.Student()
                {
                    Name = "Дудар С.А.",
                    Group = g1
                };
                var st3 = new Storage.Entities.Student()
                {
                    Name = "Герасименко С.А.",
                    Group = g2
                };
                var st4 = new Storage.Entities.Student()
                {
                    Name = "Помазан Н.А.",
                    Group = g2
                };
                context.Students.AddRange(st1, st2, st3, st4);

                context.Grades.Add(new Storage.Entities.Grade()
                {
                    Student = st1,
                    Subject = bd,
                    Type = Storage.Entities.GradeType.M1,
                    State = Storage.Entities.GradeState.Set,
                    Value = 45
                });
                context.Grades.Add(new Storage.Entities.Grade()
                {
                    Student = st2,
                    Subject = bd,
                    Type = Storage.Entities.GradeType.M2,
                    Value = 48
                });
                context.Grades.Add(new Storage.Entities.Grade()
                {
                    Student = st3,
                    Subject = bd,
                    Type = Storage.Entities.GradeType.M1,
                    Value = 44
                });
                context.Grades.Add(new Storage.Entities.Grade()
                {
                    Student = st4,
                    Subject = bd,
                    Type = Storage.Entities.GradeType.M1,
                    State = Storage.Entities.GradeState.NeedsApproval,
                    Value = 42
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
                options.LoginPath = "/Home/Index";
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