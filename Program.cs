using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RedditApi.Data;
using RedditApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var appId = builder.Configuration.GetSection("ConnectionTokens:appId").Value ?? throw new InvalidOperationException("Connection tokens 'appId' not found");
var appSecret = builder.Configuration.GetSection("ConnectionTokens:appSecret").Value ?? throw new InvalidOperationException("Connection tokens 'appSecret' not found");
var accessToken = builder.Configuration.GetSection("ConnectionTokens:accessToken").Value;
var refreshToken = builder.Configuration.GetSection("ConenctionTokens:refreshToken").Value;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddAuthentication().AddReddit(opt =>
{
    opt.ClientId = appId ?? "sNpJAjtXH9Q6oTRHnKp4cg";
    opt.ClientSecret = appSecret ?? "C7guhpLuZvtkYf999UsQL_eGMK8wxQ";
    opt.CallbackPath = "/auth/reddit/callback";
    opt.Scope.Add("identity");
    opt.Scope.Add("edit");
    opt.Scope.Add("flair");
    opt.Scope.Add("history");
    opt.Scope.Add("modconfig");
    opt.Scope.Add("modflair");
    opt.Scope.Add("modlog");
    opt.Scope.Add("modposts");
    opt.Scope.Add("modwiki");
    opt.Scope.Add("mysubreddits");
    opt.Scope.Add("privatemessages");
    opt.Scope.Add("read");
    opt.Scope.Add("report");
    opt.Scope.Add("save");
    opt.Scope.Add("submit");
    opt.Scope.Add("subscribe");
    opt.Scope.Add("vote");
    opt.Scope.Add("wikiedit");
    opt.Scope.Add("wikiread");
});

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddTransient<RefreshTokenService>();
builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();
//builder.Services.AddTransient<RefreshTokenService>(s => new RefreshTokenService("localhost", 7163, builder.Configuration, s.GetService<ILogger<RefreshTokenService>>()));
builder.Services.AddTransient<RefreshTokenService>(provider => new RefreshTokenService("localhost", 7163));


var app = builder.Build();

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

app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseWhen(context => !String.IsNullOrEmpty(context.Request.Query["state"]), appBuilder =>
{
    appBuilder.UseMiddleware<RefreshTokenService>();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();