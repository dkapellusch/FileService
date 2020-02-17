using System;
using System.IO;
using System.Threading.Tasks;
using FileService.RocksDb.Extensions;
using FileService.Server.RequestHandlers.V2;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Server
{
    internal static class Program
    {
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
            .AddEnvironmentVariables()
            .Build();

        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

        private static IWebHostBuilder CreateHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .UseConfiguration(Configuration)
            .ConfigureKestrel(kestrel => kestrel.ListenAnyIP(Configuration.GetValue<int>("port"), o => o.Protocols = HttpProtocols.Http2))
            .UseKestrel()
            .ConfigureServices((hostContext, services) => services
                .AddRocksDb("./files.db")
                .AddRequestHandlers()
                .AddServices()
                .AddGrpc()
            )
            .Configure(b => b
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder
                    .MapGrpcService<FileRpcService>()
                )
            );

        private static IServiceCollection AddRequestHandlers(this IServiceCollection services) => services
            .AddTransient<ReadRequestHandler>()
            .AddTransient<WriteRequestHandler>()
            .AddTransient<StreamingReadRequestHandler>()
            .AddTransient<StreamingWriteRequestHandler>();

        private static IServiceCollection AddServices(this IServiceCollection services) => services
            .AddScoped<FileRpcService>();
    }
}