using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class OpenIddictServerBuilderExtension
{
    public static OpenIddictServerBuilder AddProductionEncryptionAndSigningCertificate(
        this OpenIddictServerBuilder builder,
        Tuple<string, string> awsSecrets,
        IConfiguration configuration,
        X509KeyStorageFlags? flag = null)
    {
        try
        {
            Console.WriteLine("Adding encryption certificate");

            var encPath = configuration["OpenIddict:EncryptionCertificate:Path"]!;

            byte[] decodedEnc = Convert.FromBase64String(awsSecrets.Item1);
            File.WriteAllBytes(encPath, decodedEnc);

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

            Console.WriteLine("Adding signing certificate");

            var signPath = configuration["OpenIddict:SigningCertificate:Path"]!;

            byte[] decodedSign = Convert.FromBase64String(awsSecrets.Item2);
            File.WriteAllBytes(signPath, decodedSign);

            if (!File.Exists(signPath))
            {
                throw new FileNotFoundException($"Signing Certificate couldn't found: {signPath}");
            }

            Console.WriteLine($"Attempting to load Signing Certificate: {signPath}");

            var signPass = configuration["OpenIddict:SigningCertificate:Password"];
            
            var signCert = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass);

            builder.AddSigningCertificate(signCert);

            Console.WriteLine("Signing certificate added");

            return builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddProductionEncryptionAndSigningCertificate: {ex}");
            throw;
        }
    }
}