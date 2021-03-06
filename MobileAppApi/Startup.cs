// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class Startup
    {
        private bool _SignatureValidationEnabled;
        private const string Title = "Mobile App API";

        public Startup(IConfiguration configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private readonly IConfiguration _Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();

            services.AddControllers();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(_Configuration, "WorkFlow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                result.BeginTransaction();
                return result;
            });

            services.AddSingleton<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddSingleton<IGeanTekListValidationConfig, StandardGeanCommonWorkflowConfig>();
            services.AddSingleton<ITemporaryExposureKeyValidator, TemporaryExposureKeyValidator>();
            services.AddSingleton<ITemporaryExposureKeyValidatorConfig, TemporaryExposureKeyValidatorConfig>();

            services.AddScoped<HttpPostReleaseTeksCommand, HttpPostReleaseTeksCommand>();

            services.AddScoped<IReleaseTeksValidator, ReleaseTeksValidator>();
            services.AddScoped<ISignatureValidator, SignatureValidator>();

            _SignatureValidationEnabled = _Configuration.GetValue("ValidatePostKeysSignature", true);
            if (_SignatureValidationEnabled)
            {
                services.AddScoped<ISignatureValidator, SignatureValidator>();
            }
            else
            {
                services.AddScoped<ISignatureValidator, DoNotValidateSignatureValidator>();
            }

            services.AddScoped<HttpPostRegisterSecret, HttpPostRegisterSecret>();
            services.AddScoped<ISecretWriter, SecretWriter>();
            services.AddScoped<ITekWriter, TekWriter>();
            services.AddScoped<RandomNumberGenerator, RandomNumberGenerator>();
            services.AddScoped<IRandomNumberGenerator, RandomNumberGenerator>();

            services.AddScoped<HttpPostDecoyKeysCommand, HttpPostDecoyKeysCommand>();
            services.AddScoped<IDecoyKeysConfig, StandardDecoyKeysConfig>();

            services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo {Title = Title, Version = "v1"}); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            app.UseDeveloperExceptionPage();

            var logger = services.GetService<ILogger<Startup>>();
            if (_SignatureValidationEnabled)
            {
                logger.LogInformation("Signature validation of POST postkeys enabled : true");
            }
            else
            {
                logger.LogWarning("Signature validation of POST postkeys enabled : false");
            }


            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("v1/swagger.json", Title); });

            if (!env.IsDevelopment())
                app.UseHttpsRedirection(); //HTTPS redirection not mandatory for development purposes

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}