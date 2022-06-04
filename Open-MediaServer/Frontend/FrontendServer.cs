using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Open_MediaServer.Frontend;

public class FrontendServer
{
    public FrontendServer()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            ContentRootPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Frontend{Path.DirectorySeparatorChar}",
            WebRootPath = "Assets"
        });
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("OpenMedia",
                new OpenApiInfo
                {
                    Title = "StrateimTech Open-MediaServer",
                    Version = "v2",
                    Contact = new OpenApiContact
                    {
                        Url = new Uri("https://github.com/StrateimTech/Open-MediaServer")
                    }
                });
        });
        
        builder.WebHost.UseUrls($"http://*:{Program.ConfigManager.Config.FrontendPorts.http};https://*:{Program.ConfigManager.Config.FrontendPorts.https}");

        builder.Services.AddRazorPages(options =>
        {
            options.RootDirectory = "/Frontend/Pages";
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        
        if (Program.ConfigManager.Config.ShowSwaggerUi)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/OpenMedia/swagger.json", "StrateimTech Open-MediaServer");
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "Assets")),
            RequestPath = "/Frontend/Assets"
        });

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.Run();
    }
}