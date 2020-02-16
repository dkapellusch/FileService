using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileService.Contracts;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Utils;

namespace FileService.Test
{
    public static class Helpers
    {
        private const int MaxChunkSize = 10_000;

        public static async Task UploadFile(this IClientStreamWriter<WriteFileRequest> streamWriter, string localFileName, string remoteFileName)
        {
            await using var fileStream = new FileStream(localFileName, FileMode.Open, FileAccess.Read);

            await streamWriter.WriteAllAsync(ChunkFile(fileStream)
                .Select(chunk => new WriteFileRequest
                {
                    FileName = remoteFileName,
                    Chunk = new FileData
                    {
                        Data = ByteString.CopyFrom(chunk)
                    }
                }));
        }

        public static async Task DownloadFile(this IAsyncStreamReader<FileData> fileStream, string fileName)
        {
            await using var outputStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await fileStream.ForEachAsync(async m =>
            {
                var charArray = m.Data.ToByteArray();
                await outputStream.WriteAsync(charArray, 0, charArray.Length);
            });
        }

        static IEnumerable<byte[]> ChunkFile(this FileStream stream)
        {
            var chunk = new byte[MaxChunkSize];
            while (true)
            {
                var index = 0;
                while (index < chunk.Length)
                {
                    var bytesRead = stream.Read(chunk, index, chunk.Length - index);
                    if (bytesRead == 0) break;

                    index += bytesRead;
                }

                if (index != 0) yield return chunk;

                if (index != chunk.Length) yield break;
            }
        }
    }
}