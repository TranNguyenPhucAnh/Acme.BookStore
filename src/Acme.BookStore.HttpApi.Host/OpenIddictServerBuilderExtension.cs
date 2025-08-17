using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class OpenIddictServerBuilderExtension
{
    public static OpenIddictServerBuilder AddProductionEncryptionAndSigningCertificate(
        this OpenIddictServerBuilder builder,
        IConfiguration configuration,
        X509KeyStorageFlags? flag = null)
    {
        var signPath = configuration["OpenIddict:SigningCertificate:Path"];
        
        if (!File.Exists(signPath))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {signPath}");
        }

        try
        {
            Console.WriteLine($"Attempting to load Signing Certificate: {signPath}");

            var signPass = configuration["OpenIddict:SigningCertificate:Password"];
            var signCert = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass);

            builder.AddSigningCertificate(signCert);

            Console.WriteLine("Signing certificate added");

            Console.WriteLine("Adding encryption certificate");

            var encPath = configuration["OpenIddict:EncryptionCertificate:Path"];

            if (!File.Exists(encPath))
            {
                throw new FileNotFoundException($"Encryption Certificate couldn't found: {encPath}");
            }
            var encPass = configuration["OpenIddict:EncryptionCertificate:Password"]!;

            Console.WriteLine($"Attempting to load encyption certificate: {encPath}");

            var encCert = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(encPath, encPass, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(encPath, encPass);

            builder.AddEncryptionCertificate(encCert);

            Console.WriteLine("Encryption certificate added");

            return builder;
            }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddProductionEncryptionAndSigningCertificate: {ex}");
            throw;
        }
    }
}