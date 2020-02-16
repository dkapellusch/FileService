using FileService.RocksDb.RocksAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.RocksDb.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddRocksDb(this IServiceCollection services, string pathToDb) =>
            services.AddSingleton(new RocksDatabase(pathToDb))
                .AddSingleton<ByteChunkStore>();
    }
}