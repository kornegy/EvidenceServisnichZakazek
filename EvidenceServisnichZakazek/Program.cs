using EvidenceServisnichZakazek.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EvidenceServisnichZakazek;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        
        var jwtKey = builder.Configuration["JwtKey"];

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, //musime li kontrolovat stranku
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidateAudience = true, //musime li kontrolovat desktop appku
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true, // kontrola podpisu!
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Register";
                options.ExpireTimeSpan = TimeSpan.FromDays(7); // uklada na 7 dnu

            });

        builder.Services.AddScoped<EvidenceServisnichZakazek.Repositories.IUserRepository, EvidenceServisnichZakazek.Repositories.UserRepository>();
        builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        
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