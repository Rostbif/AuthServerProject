using IdentityServer4;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication.Google; // Add this line
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;
using System;
using Microsoft.Extensions.Configuration;

namespace IdentityServer
{
    public class Startup
    {
        public Startup()
        {
            DotEnv.Load();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var envVar = DotEnv.Read();
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(TestUsers.Users);

            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
            .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    var clientId = envVar["GOOGLE_CLIENT_ID"];
                    var clientSecret = envVar["GOOGLE_CLIENT_SECRET"];
                    // var x = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
                    // var y = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

                    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                    {
                        throw new Exception("Google Client ID or Client Secret is not set in environment variables.");
                    }

                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                })
                .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "interactive.confidential";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}