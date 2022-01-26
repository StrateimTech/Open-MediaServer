using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Open_MediaServer.Media;

namespace Open_MediaServer
{
    public class Startup
    {
        private readonly MediaHandler MediaHandler = new();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Strateim Open Media Server", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Strateim Open Media Server"); });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", RequestDelegate);
                endpoints.MapGet("/404", RequestNotFound);
                endpoints.MapGet("/img/{file}", MediaHandler.HandleRequestImg);
                endpoints.MapGet("/video/{file}", MediaHandler.HandleRequestVideo);
                endpoints.MapGet("/other/{file}", MediaHandler.HandleRequestOther);
                endpoints.MapControllers();
            });
        }

        private async Task RequestDelegate(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("Hello...");
        }

        private async Task RequestNotFound(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Failed to find the content you were looking for sorry!");
        }
    }
}