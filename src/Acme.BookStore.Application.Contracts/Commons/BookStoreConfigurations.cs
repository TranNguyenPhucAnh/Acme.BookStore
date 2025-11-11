using Microsoft.Extensions.Configuration;

namespace Acme.BookStore
{
    public static class BookStoreConfigurations
    {
        private const string AwsCloudFrontDomainKey = "Aws:CloudFrontDomain";
        private const string AwsS3BucketUploadDocumentKey = "Aws:S3BucketUploadDocument";
        private const string AwsAccessKeyKey = "Aws:AccessKey";
        private const string AwsSecretKeyKey = "Aws:SecretKey";

        private static IConfiguration _configuration;

        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static string _awsCloudfrontDomain;
        public static string AwsCloudFrontDomain
        {
            get
            {
                if (string.IsNullOrEmpty(_awsCloudfrontDomain))
                {
                    _awsCloudfrontDomain = _configuration.GetValue<string>(AwsCloudFrontDomainKey);
                }
                return _awsCloudfrontDomain;
            }
        }

        private static string _s3BucketUploadDocument;
        public static string S3BucketUploadDocument
        {
            get
            {
                if (string.IsNullOrEmpty(_s3BucketUploadDocument))
                {
                    _s3BucketUploadDocument = _configuration.GetValue<string>(AwsS3BucketUploadDocumentKey);
                }
                return _s3BucketUploadDocument;
            }
        }

        private static string _awsAccessKey;
        public static string AwsAccessKey
        {
            get
            {
                if (string.IsNullOrEmpty(_awsAccessKey))
                {
                    _awsAccessKey = _configuration.GetValue<string>(AwsAccessKeyKey);
                }
                return _awsAccessKey;
            }
        }

        private static string _awsSecretKey;
        public static string AwsSecretKey
        {
            get
            {
                if (string.IsNullOrEmpty(_awsSecretKey))
                {
                    _awsSecretKey = _configuration.GetValue<string>(AwsSecretKeyKey);
                }
                return _awsSecretKey;
            }
        }
    }
}