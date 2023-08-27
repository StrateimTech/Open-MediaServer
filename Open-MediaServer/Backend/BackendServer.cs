﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Open_MediaServer.Analytics;

namespace Open_MediaServer.Backend;

public class BackendServer
{
    public BackendServer()
    {
        var builder = WebApplication.CreateBuilder();
        if (!Program.ConfigManager.Config.ShowConsoleProviders)
        {
            builder.Logging.ClearProviders();
        }

        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(Program.ConfigManager.Config.BackendPorts.http);
            options.ListenAnyIP(Program.ConfigManager.Config.BackendPorts.https, configure => configure.UseHttps());

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

        app.UseDeveloperExceptionPage();
        
        if (Program.ConfigManager.Config.AnalyticalApi != null)
        {
            app.UseAnalyticsMiddleware(Program.ConfigManager.Config.AnalyticalApi);
        }

        if (Program.ConfigManager.Config.ShowSwaggerUi)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/OpenMedia/swagger.json", "StrateimTech Open-MediaServer");
            });
        }

        if (Program.ConfigManager.Config.ForceHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}