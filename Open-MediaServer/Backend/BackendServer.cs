using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Open_MediaServer.Backend;

public class BackendServer
{
    public BackendServer()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseKestrel(options =>
        {
            int? fileUploadMax = Program.ConfigManager.Config.FileNetworkUploadMax;
            if (fileUploadMax != null)
            {
                fileUploadMax *= 100000000;
            }
            options.Limits.MaxRequestBodySize = fileUploadMax;
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
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "StrateimTech Open-MediaServer");
            });
        }
        
        app.UseHttpsRedirection();
        
        app.UseAuthorization();
        
        // app.MapControllers();
        
        app.Run();
    }
}