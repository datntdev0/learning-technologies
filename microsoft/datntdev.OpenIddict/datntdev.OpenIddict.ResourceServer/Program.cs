using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace datntdev.OpenIddict.ResourceServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            ConfigureOpenIddict(builder.Services);
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureOpenIddict(IServiceCollection services)
        {
            // Configure the ASP.NET Core authentication stack to use OpenIddict as the default scheme
            var defaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            services.AddAuthentication(o => o.DefaultScheme = defaultScheme);

            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    options.SetIssuer("https://localhost:7206");
                    var encryptKey = new SymmetricSecurityKey(
                       Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="));

                    // Register signing and encryption credentials
                    options.AddEncryptionKey(encryptKey);

                    // Register HttpClient that allow this resources server to call to identity server
                    options.UseSystemNetHttp();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });
        }
    }
}
