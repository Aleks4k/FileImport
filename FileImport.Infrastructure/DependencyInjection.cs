using dotenv.net;
using FileImport.Application.Common.Contracts;
using FileImport.Application.Files.Contracts;
using FileImport.Application.Users.Contracts;
using FileImport.Domain.Data;
using FileImport.Infrastructure.Repository;
using FileImport.Infrastructure.Services;
using FileImport.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace FileImport.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostBuilder host)
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            host.UseSerilog();
            services.AddMemoryCache();
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING_FILEIMPORT");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)
            );
            services.Configure<FileStorageOptions>(options =>
            {
                options.RootPath = Environment.GetEnvironmentVariable("FILESTORAGE_ROOTPATH")!;
                options.RootPathChecked = Environment.GetEnvironmentVariable("FILESTORAGE_ROOTPATH_CHECKED")!;
            });
            var googleSettings = new ValidationSettings() { Audience = new[] { Environment.GetEnvironmentVariable("GoogleSettings_ClientId") } };
            services.AddSingleton(googleSettings);
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            var jwtSettings = new JwtSettings()
            {
                AccessTokenKey = Environment.GetEnvironmentVariable("JWT_AccessTokenKey")!,
                Issuer = Environment.GetEnvironmentVariable("JWT_Issuer")!,
                Audience = Environment.GetEnvironmentVariable("JWT_Audience")!,
                AccessTokenTTL = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_AccessTokenTTL"))
            };
            services.AddSingleton(jwtSettings);
            services.AddScoped<IJwtService, JwtService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessTokenKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            services.AddAuthorization();
            services.AddSingleton<IConcurrentDictionaryRepository, ConcurrentDictionaryRepository>();
            services.AddScoped<IUser, UserRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IFileArchiveRepository, FileArchiveRepository>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IDownloadRepository, DownloadRepository>();
            return services;
        }
    }
}
