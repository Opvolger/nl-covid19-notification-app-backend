// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment currentEnvironment, IConfiguration configuration)
        {
            _CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IHostingEnvironment _CurrentEnvironment;
        private readonly IConfiguration _Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ContentDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddScoped<HttpGetManifestBinaryContentCommand, HttpGetManifestBinaryContentCommand>();
            services.AddScoped<DynamicManifestReader, DynamicManifestReader>();
            services.AddScoped<ManifestBuilder, ManifestBuilder>();
            services.AddScoped<GetActiveExposureKeySetsListCommand, GetActiveExposureKeySetsListCommand>();
            services.AddScoped<GetLatestContentCommand<RiskCalculationContentEntity>, GetLatestContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<GetLatestContentCommand<AppConfigContentEntity>, GetLatestContentCommand<AppConfigContentEntity>>();

            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddSingleton<IPublishingId, StandardPublishingIdFormatter>();

            services.AddSingleton<IUtcDateTimeProvider>(new StandardUtcDateTimeProvider());
            services.AddSingleton<IPublishingId>(new StandardPublishingIdFormatter());
            services.AddSingleton<IGaenContentConfig>(new GaenContentConfig(_Configuration));

            //TODO change to use same algorithm with a test cert and remove FakeContentSigner.
            services.AddSingleton<IContentSigner, CmsSigner>();
            if (_CurrentEnvironment.IsProduction() || _CurrentEnvironment.IsStaging())
            {
                services.AddSingleton<ICertificateProvider, X509CertificateProvider>();
                services.AddSingleton<IThumbprintConfig>(x => new CertificateProviderConfig(x.GetService<IConfiguration>(), "ExposureKeySets:Signing:NL"));
            }
            else
            {
                services.AddSingleton<ICertificateProvider, ResourceCertificateProvider3>();
                services.AddSingleton<ICertificateLocationConfig>(x => new StandardCertificateLocationConfig(x.GetService<IConfiguration>(), "Certificates:NL"));
            }

            services.AddScoped<HttpGetCdnContentCommand<AppConfigContentEntity>, HttpGetCdnContentCommand<AppConfigContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<RiskCalculationContentEntity>, HttpGetCdnContentCommand<RiskCalculationContentEntity>>();
            services.AddScoped<HttpGetCdnContentCommand<ExposureKeySetContentEntity>, HttpGetCdnContentCommand<ExposureKeySetContentEntity>>();

            services.AddScoped<IReader<ManifestEntity>, SafeBinaryContentDbReader<ManifestEntity>>();
            services.AddScoped<IReader<AppConfigContentEntity>, SafeBinaryContentDbReader<AppConfigContentEntity>>();
            services.AddScoped<IReader<RiskCalculationContentEntity>, SafeBinaryContentDbReader<RiskCalculationContentEntity>>();
            services.AddScoped<IReader<ExposureKeySetContentEntity>, SafeBinaryContentDbReader<ExposureKeySetContentEntity>>();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CDN Backer",
                    Version = "v1",
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("/swagger/v1/swagger.json", "CDN Backer"); });


            if (env.IsDevelopment() || env.IsEnvironment("Test"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}