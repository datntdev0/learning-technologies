
using datntdev.OpenIddict.IdentityServer.Data;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace datntdev.OpenIddict.IdentityServer.Services
{
    public class HostedServiceDbSeeder(IServiceProvider services): IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync(ct);

            // Seed the database with default openiddict configuration
            var openIdManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            if (await openIdManager.FindByClientIdAsync("public-application", ct) is null)
            {
                var application = new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Application.Public",
                    ClientId = "Application.Public",
                    ClientType = OpenIddictConstants.ClientTypes.Public,
                    ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                    RedirectUris = {
                        new Uri("https://oauth.pstmn.io/v1/callback"), // Postman OAuth callback URL
                    },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                    }
                };
                await openIdManager.CreateAsync(application, ct);
            }
            if (await openIdManager.FindByClientIdAsync("secret-application", ct) is null)
            {
                var application = new OpenIddictApplicationDescriptor
                {
                    DisplayName = "Application.Confidential",
                    ClientId = "Application.Confidential",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    ClientType = OpenIddictConstants.ClientTypes.Confidential,
                    ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    }
                };
                await openIdManager.CreateAsync(application, ct);
            }

            // Seed the database with default admin user
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if (await userManager.FindByNameAsync("admin") is null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@email.com",
                    Email = "admin@email.com",
                };
                await userManager.CreateAsync(user, "123Qwe!@#");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
