using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Acme.BookStore;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateBootstrapLogger();

        try
        {
            var awsSecrets = await GetAWSSecrets();
            Environment.SetEnvironmentVariable("ENC_PFX_B64", awsSecrets.Item1);
            Environment.SetEnvironmentVariable("SIGN_PFX_B64", awsSecrets.Item2);

            Log.Information("Starting Acme.BookStore.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
#if DEBUG
                        .MinimumLevel.Debug()
#else
                        .MinimumLevel.Information()
#endif
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Async(c => c.File("Logs/logs.txt"))
                        .WriteTo.Async(c => c.Console())
                        .WriteTo.Async(c => c.AbpStudio(services));
                });
            await builder.AddApplicationAsync<BookStoreHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    private static async Task<Tuple<string, string>> GetAWSSecrets()
    {
        string encyptionSecretName = "enc.pfx.b64";
        string signingSecretName = "sign.pfx.b64";
        string region = "ap-southeast-1";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest requestEnc = new GetSecretValueRequest
        {
            SecretId = encyptionSecretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueRequest requestSign = new GetSecretValueRequest
        {
            SecretId = signingSecretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse responseEnc;
        GetSecretValueResponse responseSign;

        try
        {
            responseEnc = await client.GetSecretValueAsync(requestEnc);
            responseSign = await client.GetSecretValueAsync(requestSign);
        }
        catch (Exception e)
        {
            throw e;
        }

        string secretEnc = responseEnc.SecretString;
        string secretSign = responseSign.SecretString;

        Console.WriteLine("AWS Secret retrieved enc.pfx.b64: " + secretEnc);
        Console.WriteLine("AWS Secret retrieved sign.pfx.b64: " + secretSign);

        return new Tuple<string, string>(secretEnc, secretSign);
    }
}
