using datntdev.OpenIddict.IdentityServer.Components;
using datntdev.OpenIddict.IdentityServer.Components.Account;
using datntdev.OpenIddict.IdentityServer.Data;
using datntdev.OpenIddict.IdentityServer.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace datntdev.OpenIddict.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddControllers();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDatabase"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            ConfigureOpenIddict(builder.Services);

            builder.Services.AddHostedService<HostedServiceDbSeeder>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }

        private static void ConfigureOpenIddict(IServiceCollection services)
        {
            // Configure the ASP.NET Core authentication stack to use OpenIddict as the default scheme
            var defaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            services.AddAuthentication(o => o.DefaultScheme = defaultScheme);

            services.ConfigureDbContext<ApplicationDbContext>(o => o.UseOpenIddict());
            services.AddOpenIddict()
                // Configure OpenIddict to use the EF Core stores/models
                .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
                .AddServer(options =>
                {
                    // Enable the authorization and token endpoints
                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token");

                    // Allow standard flows
                    options
                        .AllowAuthorizationCodeFlow()
                        .AllowClientCredentialsFlow();

                    var encryptKey = new SymmetricSecurityKey(
                        Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="));

                    // Register the encryption credentials. This sample uses a symmetric
                    // encryption key that is shared between the server and the API project.
                    //
                    // Note: in a real world application, this encryption key should be
                    // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
                    options
                        .AddEncryptionKey(encryptKey)
                        .AddDevelopmentSigningCertificate()
                        // You won't able to debug access token from jwt.io
                        // Because it is encrypted, disable to see payload
                        .DisableAccessTokenEncryption();

                    // Register ASP.NET Core host and configure options
                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance
                    options.UseLocalServer();

                    options.UseAspNetCore();
                });
        }
    }
}
