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

            FileExtensionContentTypeProvider provider = new();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
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

        public void LoadFiles(string path)
        {
            if (!Directory.Exists(path) || path.Contains("bin") || path.Contains("obj") || path.Contains("neo-dev-portal") || path.Contains("wwwroot")) return;
            Directory.GetFiles(path).ToList().ForEach(p => { 
                if (Path.GetExtension(p) == ".md") {
                    Sources.Pages.Add(new Page() { Lines = File.ReadAllLines(p), Link = p.Replace("\\", "/").Replace("//", "/").TrimStart('.').Replace(".md", ".html") });
                }
            });
            Directory.GetDirectories(path).ToList().ForEach(p => LoadFiles(p));
        }
    }
}
