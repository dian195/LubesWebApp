using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using WebApp.Models;
using WebApp.Repository;
using WebApp.Repository.Interfaces;
using WebApp.Repository.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

//Add Session
string configTimeOut = config.GetValue<string>("StringApplication:IdleTimeOut");
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToInt32(configTimeOut.Trim() == "" ? "10" : configTimeOut.Trim()));
});

builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
});


//configurasi database
string? connectionString = null;
connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Sesuaikan dengan nama koneksi Anda di appsettings.json
builder.Services.AddDbContext<AppDB>(options =>
options.UseMySql(connectionString, new MariaDbServerVersion(new Version(8, 0, 33))));

//Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(option =>
       {
           option.Cookie.Name = "LubesApp";
           option.LoginPath = new PathString("/Auth/SignIn");
           option.AccessDeniedPath = new PathString("/Admin/AccessDenied");
           //option.ExpireTimeSpan = TimeSpan.FromMinutes(Convert.ToInt32(configTimeOut.Trim() == "" ? "10" : configTimeOut.Trim()));
           option.ExpireTimeSpan = TimeSpan.FromDays(Convert.ToInt32(configTimeOut.Trim() == "" ? "10" : configTimeOut.Trim()));
           option.Cookie.MaxAge = option.ExpireTimeSpan; // optional
           option.SlidingExpiration = true;
       });



#region Implement Interface

builder.Services.AddScoped<IAPIQRScanServices, APIQRScanServices>();
builder.Services.AddScoped<ILogin, LoginServices>();
builder.Services.AddScoped<IExport, ExportServices>();


#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
