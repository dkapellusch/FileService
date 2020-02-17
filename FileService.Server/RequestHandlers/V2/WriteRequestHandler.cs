using System;
using System.Linq;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf.WellKnownTypes;
using File = System.IO.File;

namespace FileService.Server.RequestHandlers.V2
{
    public class WriteRequestHandler
    {
        private readonly SerializedRocksDb<FileMetadata> _metaDataStore;

        public WriteRequestHandler(SerializedRocksDb<FileMetadata> metaDataStore) => _metaDataStore = metaDataStore;

        public async Task<FileMetadata> HandleRequest(Contracts.File request)
        {
            var extension = request.FileName.Split(".").Skip(1).LastOrDefault();
            var outputPath = FileHelper.GetOutputPath(request.FileName);

            var metadata = new FileMetadata
            {
                FileName = request.FileName,
                Extension = extension ?? string.Empty,
                CreatedTime = DateTime.UtcNow.ToTimestamp(),
                ServerName = Environment.MachineName,
                SizeInBytes = request.Data.CalculateSize(),
                PathOnDisk = outputPath
            };

            await File.WriteAllBytesAsync(metadata.PathOnDisk, request.Data.Data.ToByteArray());
            _metaDataStore.Put(metadata.FileName, metadata);
            return metadata;
        }
    }
}