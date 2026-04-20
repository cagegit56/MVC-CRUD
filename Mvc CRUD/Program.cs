using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataDbContext>( options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionStr")));
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IGetAllService, GetAllService>();
builder.Services.AddScoped<IPaginationService, PaginationService>();
builder.Services.AddScoped<IRateLimitViolationTracker, RateLimitViolationTracker>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));
 builder.Services.AddMediatR(cfg =>
  {
      cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
  });
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IUserInfoContextService, UserInfoContextService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var config = builder.Configuration.GetSection("Authentication:Keycloak");

    options.Authority = config["Authority"];
    options.ClientId = config["ClientId"];
    options.ClientSecret = config["ClientSecret"];
    options.ResponseType = config["ResponseType"]!;
    options.CallbackPath = config["CallbackPath"];
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = false;
    builder.Configuration.Bind("Authentication:Schemes:OpenIdConnect", options);
    options.SignedOutCallbackPath = "/signout-callback-oidc";

    options.NonceCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            var idTokenHint = context.HttpContext.GetTokenAsync("id_token").Result;
            if (!string.IsNullOrEmpty(idTokenHint))
            {
                context.ProtocolMessage.IdTokenHint = idTokenHint;
            }

            context.ProtocolMessage.PostLogoutRedirectUri = "https://localhost:7128/";
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true
    };

});


builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("CommentReplyPolicy", context =>
    {
        var user = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.FindFirst("preferred_username")?.Value 
                          ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(user, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 4,
            Window = TimeSpan.FromSeconds(15),
            SegmentsPerWindow = 6,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.OnRejected = async (context, token) =>
    {
        // var endpoint = httpContext.GetEndpoint();
        // var policyName = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
        var httpContext = context.HttpContext; 
        var user = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        var violation = context.HttpContext.RequestServices.GetRequiredService<IRateLimitViolationTracker>();
        var total = await violation.ExtendBlockAsync(user);
        httpContext.Response.Headers.RetryAfter = total.ToString();
        httpContext.Response.StatusCode = 429;
        await httpContext.Response.WriteAsync($"Error too many request, please retry after: {total}");
    };

});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHub<Mvc_CRUD.Services.SignalRHub>("/signalRHub");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var violation = context.RequestServices.GetRequiredService<IRateLimitViolationTracker>();
    var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var (blocked, retryAfter) = await violation.IsBlockedAsync(key);
    if (blocked)
    {
        var total = await violation.ExtendBlockAsync(key);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 429;
        context.Response.Headers["Retry-After"] = total.ToString();
        var acceptsHtml = context.Request.Headers["Accept"].ToString().Contains("text/html");
        if (acceptsHtml)
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync($"""
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Too Many Requests</title>
                    <meta http-equiv="refresh" content="{total}">
                </head>
                <body>
                    <h1>Too Many Requests</h1>
                    <p>You have been temporarily blocked. Try again after: <strong>{total} seconds</strong>.</p>
                    <p>This page will automatically refresh when your block expires.</p>
                    <p>for further assistance/help please visit: www.bepatientcalmdownrelax.com or contact : 076 987 6543</p>
                </body>
                </html>
            """);
        }
        else
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"error you have been temporarily blocked due to: Too many requests, please refresh after :{total}");
        }
        return;
    }
    await next();
});
app.UseRateLimiter();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
