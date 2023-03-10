using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;

namespace Backend;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddCookiePolicy(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.ReturnUrlParameter = "returnUrl";

                options.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = context =>
                    {
                        context.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        // Since NGINX proxies requests over to ASP (and SSL offloads at NGINX), we use this to understand more about the target user.
        // TODO: Why do we only forward Proto? How does the IP get through for DevSite?
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto });

        app.UseCookiePolicy(); // Enforces the cookie policy set up in ConfigureServices.

        if (!env.IsDevelopment())
        {
            app.UseDefaultFiles(); // Rewrites default file paths (eg. / to /index.html). Doesn't actually serve any files. So it needs to be before UseStaticFiles.
            app.UseStaticFiles(); // Serves static files in wwwroot folder.
        }

        app.UseRouting(); // Used by UseAuthentication and UseEndpoints.
        app.UseAuthentication(); // Authentication is to allow users to identify who they are.
        app.UseAuthorization(); // Authorization determines what capabilities users have. Enables usage of the [Authorize] attribute.

        // Map MVC controllers.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Handle paths unknown at the end of the pipeline, (eg. /dashboard/subscriptions) created by react-router by rewriting them to /index.html.
        // It has a built-in UseStaticFiles call.
        app.UseSpa(config =>
        {
            if (env.IsDevelopment())
                config.UseProxyToSpaDevelopmentServer("http://localhost:3001");
        });
    }
}