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
        IConfiguration configuration,
        X509KeyStorageFlags? flag = null)
    {
        var signPath = configuration["AuthServer:CertificatePath"];
        
        if (!File.Exists(signPath))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {signPath}");
        }

        try
        {
            Console.WriteLine($"Attempting to load PFX file: {signPath}");

            var signPass = configuration["AuthServer:CertificatePassPhrase"];
            var certificate = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(signPath, signPass);

            // builder.AddSigningCertificate(certificate); signing certificate ECDSA algorithm is not supported by OpenIddict
            // If you want to use ECDSA, you need to use ECDsaSecurityKey instead of X509Certificate2, therefore credential
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

            var encPath = configuration["OpenIddict:EncryptionCertificate:Path"];

            if (!File.Exists(encPath))
            {
                throw new FileNotFoundException($"Encryption Certificate couldn't found: {encPath}");
            }
            var encryptionCertPassword = configuration["OpenIddict:EncryptionCertificate:Password"]!;

            Console.WriteLine($"Attempting to load encyption file: {encPath}");

            var enc = flag != null
                ? X509CertificateLoader.LoadPkcs12FromFile(encPath, encryptionCertPassword, flag.Value)
                : X509CertificateLoader.LoadPkcs12FromFile(encPath, encryptionCertPassword);

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