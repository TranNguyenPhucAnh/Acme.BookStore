using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                Console.WriteLine($"Signature Algorithm:{certificate.SignatureAlgorithm.FriendlyName}");
                Console.WriteLine($"Friendly Name:      {certificate.FriendlyName}");
                //Console.WriteLine($"Public Key:         {certificate.PublicKey.Key.ToXmlString(false)}");
                var ecdsa = certificate.GetECDsaPublicKey();
                if (ecdsa != null)
                {
                    var parameters = ecdsa.ExportParameters(false);
                    Console.WriteLine("Public Key:");
                    Console.WriteLine($"  Curve: {parameters.Curve.Oid.FriendlyName}");
                    Console.WriteLine($"  Q.X:   {Convert.ToBase64String(parameters.Q.X)}");
                    Console.WriteLine($"  Q.Y:   {Convert.ToBase64String(parameters.Q.Y)}");
                    Console.WriteLine($"Public Key Algorithm: ECDSA ({parameters.Curve.Oid.Value})");
                }
                else
                {
                    Console.WriteLine("Public Key: (not ECDSA or unavailable)");
                    Console.WriteLine($"Public Key Algorithm: {certificate.PublicKey.Oid.FriendlyName} ({certificate.PublicKey.Oid.Value})");
                }
                var hasPrivateKey = certificate.HasPrivateKey;
                Console.WriteLine($"Has Private Key: {hasPrivateKey}");

                try
                {
                    var privateKey = certificate.GetECDsaPrivateKey();
                    if (privateKey == null)
                    {
                        Console.WriteLine("Private key not available via GetECDsaPrivateKey()");
                    }
                    else
                    {
                        Console.WriteLine("Successfully loaded ECDsa private key");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get private key: {ex.Message}");
                }

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