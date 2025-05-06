using CoinVista.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC & Razor
builder.Services.AddControllersWithViews();

// CoinGecko HTTP Client
builder.Services.AddHttpClient<CoinGeckoService>();

// Services
builder.Services.AddSingleton<InvestmentService>();  // In-memory investment store
builder.Services.AddSingleton<UserService>();        // JSON-based user persistence
builder.Services.AddSingleton<CoinHistoryCacheService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
