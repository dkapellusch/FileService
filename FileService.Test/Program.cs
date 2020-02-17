using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileService.Contracts;
using Grpc.Core;

namespace FileService.Test
{
    static class Program
    {
        static async Task Main()
        {
            Directory.CreateDirectory("./outputFiles");
            var localFilePath = "TestImage.png";
            var fileInfo = new FileInfo(localFilePath);
            var client = new Contracts.FileService.FileServiceClient(new Channel("localhost:9001", ChannelCredentials.Insecure));
            var count = 10_000;
            var jobs = Enumerable.Range(0, count).ToArray();
            var stopwatch = Stopwatch.StartNew();

            await Task.WhenAll(jobs
                .Select(async i =>
                    await client.WriteFileStream().RequestStream.UploadFile(localFilePath, $"{fileInfo.Name}{i}")
                ));
            stopwatch.Stop();
            Console.WriteLine($"Uploaded {count} files of size {fileInfo.Length / 1000} kb in {stopwatch.ElapsedMilliseconds}ms");

            stopwatch.Restart();
            await Task.WhenAll(jobs
                .Select(async i =>
                {
                    var fileResponse = client.StreamFile(new ReadFileRequest {FileName = $"{fileInfo.Name}{i}"});
                    await fileResponse.ResponseStream.DownloadFile($"./outputFiles/{fileInfo.Name}{i}{fileInfo.Extension}");
                }));
            stopwatch.Stop();
            Console.WriteLine($"Downloaded {count} files of size {fileInfo.Length / 1000} kb in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}