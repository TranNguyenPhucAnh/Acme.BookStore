using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public static class OpenIddictServerBuilderExtension
{
    public static OpenIddictServerBuilder AddProductionEncryptionAndSigningCertificate(
        this OpenIddictServerBuilder builder,
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

            //Console.WriteLine("Adding encryption certificate");

            //actually, encrytion certificate/credential is optional, only add it if the client requires token encryption
            // if (!File.Exists("/app/certs/enc.pfx"))
            // {
            //     throw new FileNotFoundException($"Encryption Certificate couldn't found: {"/app/certs/enc.pfx"}");
            // }
            // var encryptionCertPassword = configuration["OpenIddict:EncryptionCertificate:Password"]!;

            // Console.WriteLine($"Attempting to load encyption file: /app/certs/enc.pfx");

            // var enc = flag != null
            //     ? X509CertificateLoader.LoadPkcs12FromFile("/app/certs/enc.pfx", encryptionCertPassword, flag.Value)
            //     : X509CertificateLoader.LoadPkcs12FromFile("/app/certs/enc.pfx", encryptionCertPassword);

            // builder.AddEncryptionCertificate(enc);

            // Console.WriteLine("Encryption certificate added");

            return builder;
            
            }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddProductionEncryptionAndSigningCertificate: {ex}");
            throw;
        }
    }
}