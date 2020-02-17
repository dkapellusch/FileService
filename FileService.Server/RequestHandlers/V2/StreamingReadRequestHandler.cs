using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Utils;

namespace FileService.Server.RequestHandlers.V2
{
    public class StreamingReadRequestHandler
    {
        private readonly SerializedRocksDb<FileMetadata> _metadataStore;

        public StreamingReadRequestHandler(SerializedRocksDb<FileMetadata> metadataStore) => _metadataStore = metadataStore;

        public async Task HandleRequest(ReadFileRequest request, IServerStreamWriter<FileData> responseStream)
        {
            var metadata = _metadataStore.Get(request.FileName);
            await using var fileStream = new FileStream(metadata.PathOnDisk, FileMode.Open, FileAccess.Read);

            await responseStream.WriteAllAsync(fileStream.ChunkFile()
                .Select(chunk => new FileData {Data = ByteString.CopyFrom(chunk)})
            );
        }
    }
}