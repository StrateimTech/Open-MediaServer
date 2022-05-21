using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

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

        app.Run();
    }
}