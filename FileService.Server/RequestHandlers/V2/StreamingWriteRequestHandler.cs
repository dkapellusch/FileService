using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Utils;

namespace FileService.Server.RequestHandlers.V2
{
    public class StreamingWriteRequestHandler
    {
        private readonly SerializedRocksDb<FileMetadata> _metaDataStore;

        public StreamingWriteRequestHandler(SerializedRocksDb<FileMetadata> metaDataStore) => _metaDataStore = metaDataStore;

        public async Task<FileMetadata> HandleRequest(IAsyncStreamReader<WriteFileRequest> requestStream, CancellationToken token)
        {
            FileMetadata metadata = null;
            WriteFileRequest request = null;
            FileStream fileStream = null;

            try
            {
                await requestStream
                    .ForEachAsync(async chunk =>
                    {
                        if (request is null)
                        {
                            request = chunk;
                            var extension = request.FileName.Split(".").Skip(1).LastOrDefault();
                            var outputPath = FileHelper.GetOutputPath(request.FileName);

                            metadata = new FileMetadata
                            {
                                FileName = request.FileName,
                                Extension = extension ?? string.Empty,
                                CreatedTime = DateTime.UtcNow.ToTimestamp(),
                                ServerName = Environment.MachineName,
                                SizeInBytes = request.Chunk.CalculateSize(),
                                PathOnDisk = outputPath
                            };

                            fileStream = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
                        }

                        if (!token.IsCancellationRequested && fileStream != null)
                            await fileStream.WriteAsync(chunk.Chunk.Data.ToByteArray(), token);
                    });
            }
            finally
            {
                fileStream?.Dispose();
            }

            _metaDataStore.Put(metadata.FileName, metadata);
            return metadata;
        }
    }
}