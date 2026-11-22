using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medical.PL.Options;
using Medical.PL.Middlewares;
using Medical.DAL.Context;
using Medical.DAL.Identity;
using Medical.DAL.Repository;
using Medical.DAL.Repositories;
using Medical.BLL.Services.Interfaces;
using Medical.BLL.Services.Implementations;
using Medical.BLL.Services;
using Medical.BLL.Infrastructure.Cache.Implementations;
using Medical.BLL.Infrastructure.Lock.Implementations;
using Medical.BLL.Infrastructure.Lock.Interfaces;
using Medical.BLL.Infrastructure.Cache.Interfaces;
using Medical.BLL.Infrastructure.RateLimit.Interfaces;
using Medical.BLL.Infrastructure.RateLimit.Implementations;
using static Medical.BLL.Infrastructure.Lock.Implementations.RedisLockProvider;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.KnownProxies.Add(IPAddress.Parse("172.30.0.11"));
    options.KnownProxies.Add(IPAddress.Parse("172.30.0.12"));
});

// Create Redis Cluster Connection
var redisSection = builder.Configuration.GetSection("Redis");

var endpoints = redisSection.GetSection("Endpoints").Get<string[]>();

if (endpoints is null || endpoints.Length == 0)
{
    throw new InvalidOperationException(
        "No Redis Cluster endpoints configured."
    );
}

var redisOptions = new ConfigurationOptions
{
    AbortOnConnectFail = redisSection.GetValue<bool>("AbortOnConnectFail", false),
    ConnectRetry = redisSection.GetValue<int>("ConnectRetry", 5),
    ConnectTimeout = redisSection.GetValue<int>("ConnectTimeout", 5000),
    KeepAlive = redisSection.GetValue<int>("KeepAlive", 60)
};

foreach (var endpoint in endpoints)
{
    redisOptions.EndPoints.Add(endpoint);
}

// Connect to Redis Cluster
var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisOptions);

// Register the same multiplexer as a Singleton.
// Factory registration allows DI to dispose it on application shutdown.
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => multiplexer);

// Register IDatabase from the same multiplexer.
builder.Services.AddSingleton<IDatabase>(
    sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
);

// Configure Data Protection Keys
builder.Services
    .AddDataProtection()
    .SetApplicationName("Medical")
    .PersistKeysToStackExchangeRedis(
        multiplexer,
        "Medical:DataProtection:Keys"
    );

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._#";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options => {
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


builder.Services.AddScoped<IdentitySeeder>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<ILicenseTransferRequestRepository, LicenseTransferRequestRepository>();

builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.AddScoped<IAccountServices, AccountServices>();
builder.Services.AddScoped<ILicenseServices, LicenseServices>();
builder.Services.AddScoped<IUserProfileServices, UserProfileServices>();
builder.Services.AddScoped<IUserRequestServices, UserRequestServices>();
builder.Services.AddScoped<IAdminDashboradServices, AdminDashboradServices>();

// Distributed Cache
builder.Services.AddScoped<IDistributedCache, RedisCache>();

// Distributed Locking
builder.Services.AddSingleton<ILockCore, RedisLockCore>();
builder.Services.AddSingleton<ILockWaiter, RedisLockWaiter>();
builder.Services.AddScoped<ILockProvider, RedisLockProvider>();
builder.Services.AddScoped(_ => new LockOwner(Guid.NewGuid().ToString("N")));

// Distributed Rate Limiter
builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimit")
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    IdentitySeeder seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    await seeder.SeedAsync();
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
