using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Newtonsoft.Json;

namespace exportApi
{
    public interface IExportApiClient
    {
        Task<object> GetRandomActivity();
    }

    public class ExportApiClient : IExportApiClient
    {
        const string ExportDocumentStatus = "documents/95f71138eb424dc0a8c17da141496c93/export";

        private readonly HttpClient _client;
        private readonly string _remoteServiceBaseUrl;

        public ExportApiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:8192/api/");
            // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // httpClient. Add("Content-type", "application/json");
            _client = httpClient;
        }

        public async Task<object> GetRandomActivity()
        {
            var response = await _client.GetAsync(ExportDocumentStatus);

            return JsonConvert.DeserializeObject<object>(null);
        }

        public async Task<object> GetCatalogItems(int page, int take,
                                                   int? brand, int? type)
        {


            var responseString = await _client.GetStringAsync("uri");

            var catalog = JsonConvert.DeserializeObject<object>(responseString);
            return catalog;
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            // services.AddHttpClient< ExportApiClient>();
            services.AddHttpClient<IExportApiClient, ExportApiClient>()
                             //.SetHandlerLifetime(TimeSpan.FromSeconds(30)) // NOTE: 
                             ;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
