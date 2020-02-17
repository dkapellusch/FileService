using FileService.Contracts.Serialization;
using FileService.RocksDb.RocksAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.RocksDb.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddRocksDb(this IServiceCollection services, string pathToDb) =>
            services.AddSingleton(new RocksDatabase(pathToDb))
                .AddSingleton<ByteChunkStore>()
                .AddSingleton(typeof(SerializedRocksDb<>))
                .AddSingleton<ISerializer, MsgPackSerializer>()
                .AddSingleton(typeof(ISerializer<>), typeof(ProtoSerializer<>));
    }
}