using Minio;

namespace FileMicroservice.Api.Bootstrap;

public static class MinioBootstrap
{
    public static IServiceCollection AddMinioConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var minioOptions = configuration.GetRequiredSection(MinioOptions.Minio).Get<MinioOptions>()
                           ?? throw new NullReferenceException(nameof(MinioOptions));

        services.AddSingleton(new MinioClient()
            .WithEndpoint($"{minioOptions.Host}:{minioOptions.Port}")
            .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
            .Build());

        return services;
    }

    public class MinioOptions
    {
        public const string Minio = "Minio";

        public required string Host { get; init; }

        public required int Port { get; init; }

        public required string AccessKey { get; init; }

        public required string SecretKey { get; init; }
    }
}