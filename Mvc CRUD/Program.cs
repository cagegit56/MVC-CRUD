using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataDbContext>( options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionStr")));
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IGetAllService, GetAllService>();
builder.Services.AddScoped<IPaginationService, PaginationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();


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
    options.ResponseType = config["ResponseType"];
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
