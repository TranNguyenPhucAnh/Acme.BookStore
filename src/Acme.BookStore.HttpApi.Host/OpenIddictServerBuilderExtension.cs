using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class OpenIddictServerBuilderExtension
{
    public static OpenIddictServerBuilder AddProductionEncryptionAndSigningCertificate(
        this OpenIddictServerBuilder builder,
        IConfiguration? configuration,
        string fileName,
        string passPhrase,
        X509KeyStorageFlags? flag = null)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {fileName}");
        }

        try
        {
            Console.WriteLine($"Attempting to load PFX file: {fileName}");
            var certificate = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(fileName, passPhrase, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(fileName, passPhrase);

            // builder.AddSigningCertificate(certificate);
            // var credentials = new SigningCredentials(new ECDsaSecurityKey(certificate.GetECDsaPrivateKey())
            // {
            //     KeyId = certificate.Thumbprint
            // }, SecurityAlgorithms.EcdsaSha256);

            Console.WriteLine("Adding signing credential");

            builder.AddSigningCredentials(
            new SigningCredentials(
                new ECDsaSecurityKey(certificate.GetECDsaPrivateKey()),
                SecurityAlgorithms.EcdsaSha256));

            Console.WriteLine("Signing credential added");

            Console.WriteLine("Adding encryption certificate");

            var encryptionCertPassword = configuration["OpenIddict:EncryptionCertificate:Password"]!;

            Console.WriteLine($"Attempting to load encyption file: enc.pfx");
            var enc = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile("enc.pfx", encryptionCertPassword, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile("enc.pfx", encryptionCertPassword);

            builder.AddEncryptionCertificate(enc);

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