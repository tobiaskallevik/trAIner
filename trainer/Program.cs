using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using trainer.Data;
using trainer.Models;
using trainer.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NotBanned", policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(context =>
                !context.User.IsInRole("Banned")));
});

/*Adds EmailSender as transient service and registers AuthMessageSenderOptions config instance*/
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("SendGridSettings"));

// Adds OpenAI API service
builder.Services.AddSingleton(new OpenAIAPI(""));

/*Adds external login services. The clientID and clientSecrets are saved in the secrets manager on the host machine. This is OK for development purposes, but it is a bit cumbersome when multiple people work on the same project.*/
/*We should look into something like azure key storage, both for making it more secure and for making the project easier to share.*/
services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientId");
        googleOptions.ClientSecret = builder.Configuration.GetSection("GoogleAuthSettings").GetValue<string>("ClientSecret");
    });

services.AddDistributedMemoryCache();

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adds the cleanup service, to log out users that have been banned
services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(0.5); // Check every 30 seconds
});


// Adds the cleanup service
builder.Services.AddHostedService<CleanupService>();

var app = builder.Build();


// Calls the ApplicationDbInitializer class to initialize the database
using (var dbServices = app.Services.CreateScope())
{
    var db = dbServices.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var um = dbServices.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rm = dbServices.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    ApplicationDbInitializer.Initialize(db, um, rm);
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "StartUp",
    pattern: "{controller=Landing}/{action=LandingPage}/{id?}");
app.MapRazorPages();

app.Run();