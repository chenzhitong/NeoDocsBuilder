using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SearchEngine
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MessageSenderOptions>(Configuration);
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMiddleware<RedirectHtmlMiddleware>();

            FileExtensionContentTypeProvider provider = new();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
            });
            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == 404)
                {
                    context.HttpContext.Response.Redirect("/");
                }
            });
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            using Timer t = new(18000000);
            t.Elapsed += Elapsed;
            t.Start();
            Elapsed(null, null);
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            Sources.Pages.Clear();
            LoadFiles(".");
        }

        static string[] blockList = ["bin", "debug", "release", "obj", "neo-dev-portal", "wwwroot"];

        public void LoadFiles(string path)
        {
            if (!Directory.Exists(path) || blockList.Any(p => path.Contains(p, System.StringComparison.OrdinalIgnoreCase))) return;
            Directory.GetFiles(path).ToList().ForEach(p => { 
                if (Path.GetExtension(p) == ".md") {
                    Sources.Pages.Add(new Page() { Lines = File.ReadAllLines(p), Link = p.Replace("\\", "/").Replace("//", "/").TrimStart('.').Replace(".md", ".html") });
                }
            });
            Directory.GetDirectories(path).ToList().ForEach(LoadFiles);
        }
    }
}
