using Microsoft.AspNetCore.Authentication.Cookies;

namespace EvidenceServisnichZakazek;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Register";
                options.ExpireTimeSpan = TimeSpan.FromDays(7); // save for 7 days

            });

        builder.Services.AddScoped<EvidenceServisnichZakazek.Repositories.IUserRepository, EvidenceServisnichZakazek.Repositories.UserRepository>();
        
        var app = builder.Build();
        
        string? connectionString = app.Configuration.GetConnectionString("DefaultConnection");
        
        EvidenceServisnichZakazek.Data.DatabaseInitializer.Initialize(connectionString);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}