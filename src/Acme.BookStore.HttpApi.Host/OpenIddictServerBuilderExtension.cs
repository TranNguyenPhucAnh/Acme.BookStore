using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

public static class OpenIddictServerBuilderExtension
{
    public static OpenIddictServerBuilder AddProductionEncryptionAndSigningCertificate(this OpenIddictServerBuilder builder, string fileName, string passPhrase, X509KeyStorageFlags? flag = null)
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

                Console.WriteLine("PFX file loaded successfully");

                Console.WriteLine("===== Certificate Information =====");
                Console.WriteLine($"Subject:            {certificate.Subject}");
                Console.WriteLine($"Issuer:             {certificate.Issuer}");
                Console.WriteLine($"Thumbprint:         {certificate.Thumbprint}");
                Console.WriteLine($"Serial Number:      {certificate.SerialNumber}");
                Console.WriteLine($"Not Before:         {certificate.NotBefore}");
                Console.WriteLine($"Not After:          {certificate.NotAfter}");
                Console.WriteLine($"Has Private Key:    {certificate.HasPrivateKey}");
                Console.WriteLine($"Friendly Name:      {certificate.FriendlyName}");
                Console.WriteLine($"Signature Algorithm:{certificate.SignatureAlgorithm.FriendlyName}");
                Console.WriteLine($"Public Key:         {certificate.PublicKey.Key.ToXmlString(false)}");
                Console.WriteLine("===================================");

                Console.WriteLine("Adding signing certificate");
                builder.AddSigningCertificate(certificate);
                Console.WriteLine("Signing certificate added");

                Console.WriteLine("Adding encryption certificate");
                builder.AddEncryptionCertificate(certificate);
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