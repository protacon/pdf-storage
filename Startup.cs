﻿using System;
using System.IO;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Pdf.Storage.Data;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.PdfMerge;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Util;
using Protacon.NetCore.WebApi.ApiKeyAuth;
using Protacon.NetCore.WebApi.Util.ModelValidation;
using Swashbuckle.AspNetCore.Swagger;
using Hangfire.PostgreSql;
using Amazon.S3;
using Pdf.Storage.Config;
using Microsoft.OpenApi.Models;

namespace Pdf.Storage
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddApiKeyAuth(options =>
                {
                    if(Configuration.GetChildren().All(x => x.Key != "ApiAuthentication"))
                        throw new InvalidOperationException($"Expected 'ApiAuthentication' section.");

                    var keys = Configuration.GetSection("ApiAuthentication:Keys")
                        .AsEnumerable()
                        .Where(x => x.Value != null)
                        .Select(x => x.Value);

                    options.ValidApiKeys = keys;
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));

            services.AddSwaggerGen(c =>
            {
                var basePath = System.AppContext.BaseDirectory;

                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Pdf.Storage",
                        Version = "v1",
                        Description = File.ReadAllText(Path.Combine(basePath, "ApiDescription.md"))
                    });

                c.AddSecurityDefinition("ApiKey", ApiKey.OpenApiSecurityScheme);
                c.AddSecurityRequirement(ApiKey.OpenApiSecurityRequirement("ApiKey"));
            });

            services.Configure<AppSettings>(Configuration);

            if (bool.Parse(Configuration["Mock:Db"] ?? "false"))
            {
                var dbId = Guid.NewGuid().ToString();

                services.AddDbContext<PdfDataContext>(opt => opt.UseInMemoryDatabase(dbId));

                services.AddHangfire(config => config.UseMemoryStorage());
            }
            else
            {
                services.AddDbContext<PdfDataContext>(opt =>
                    opt.UseNpgsql(Configuration["ConnectionString"]));

                services.AddHangfire(config =>
                    config
                        .UseFilter(new PreserveOriginalQueueAttribute())
                        .UsePostgreSqlStorage(Configuration["ConnectionString"] ?? throw new InvalidOperationException("Missing: ConnectionString")));
            }

            services.AddTransient<IPdfConvert, PdfConvert>();
            services.AddTransient<IPdfQueue, PdfQueue>();
            services.AddTransient<IErrorPages, ErrorPages>();
            services.AddTransient<IPdfMerger, PdfMerger>();
            services.AddTransient<Uris>();
            services.AddTransient<IHangfireQueue, HangfireQueue>();

            if (bool.Parse(Configuration["Mock:Mq"] ?? "false"))
            {
                services.AddTransient<IMqMessages, MqMessagesNullObject>();
            }
            else
            {
                services.AddTransient<IMqMessages, MqMessages>();
            }

            switch(Configuration["PdfStorageType"] ?? throw new InvalidOperationException("PdfStorageType missing."))
            {
                case "awsS3":
                    services.Configure<AwsS3Config>(Configuration.GetSection("AwsS3"));
                    services.AddSingleton<IStorage, AwsS3Storage>();
                    break;
                case "googleBucket":
                    services.AddTransient<IStorage, GoogleCloudPdfStorage>();
                    break;
                case "inMemory":
                    services.AddSingleton<IStorage, InMemoryPdfStorage>();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid configuration: PdfStorageType ({Configuration["PdfStorageType"]})");
            }

            services.Configure<ApiKeyAuthenticationOptions>(Configuration.GetSection("ApiAuthentication"));
            services.Configure<MqConfig>(Configuration.GetSection("Mq"));
            services.AddTransient<CleanUpCronJob>();
        }

        public void Configure(IApplicationBuilder app, IHangfireQueue hangfireQueue)
        {
            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pdf.Storage");
                c.RoutePrefix = "doc";
            });

            var options = new BackgroundJobServerOptions
            {
                Queues = HangfireConstants.GetQueues().ToArray(),
                WorkerCount = 4,
            };

            hangfireQueue.ScheduleRecurring<CleanUpCronJob>("clearObsoletePdfSourceDataRows", job => job.Execute(), Cron.Hourly());

            switch(GetAppRole())
            {
                case "api":
                    app.UseMvc();
                    app.UseHangfireDashboard();
                    break;
                case "worker":
                    app.UseHangfireServer(options);
                    app.UseHangfireDashboard();
                    break;
                default:
                    app.UseMvc();
                    app.UseHangfireServer(options);
                    app.UseHangfireDashboard();
                    break;
            }
        }

        private string GetAppRole()
        {
            return Configuration["AppRole"] ?? "standalone";
        }
    }
}
