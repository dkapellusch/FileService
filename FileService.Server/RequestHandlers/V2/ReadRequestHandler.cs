using System.Collections.Generic;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf;
using File = FileService.Contracts.File;

namespace FileService.Server.RequestHandlers.V2
{
    public class ReadRequestHandler
    {
        private readonly SerializedRocksDb<FileMetadata> _metadataStore;

        public ReadRequestHandler(SerializedRocksDb<FileMetadata> metadataStore) => _metadataStore = metadataStore;

        public async Task<File> HandleRequest(ReadFileRequest request)
        {
            var metadata = _metadataStore.Get(request.FileName);
            var file = await System.IO.File.ReadAllBytesAsync(metadata.PathOnDisk);
            return new File
            {
                Data = new FileData {Data = ByteString.CopyFrom(file)},
                CreatedTime = metadata.CreatedTime,
                FileName = metadata.FileName
            };
        }
    }
}