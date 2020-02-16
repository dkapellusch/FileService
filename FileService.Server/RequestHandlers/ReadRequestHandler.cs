using System.Linq;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf;

namespace FileService.Server.RequestHandlers
{
    public class ReadRequestHandler
    {
        private readonly ByteChunkStore _fileDb;

        public ReadRequestHandler(ByteChunkStore fileDb) => _fileDb = fileDb;

        public Task<File> HandleRequest(ReadFileRequest request)
        {
            var bytes = _fileDb.Read(request.FileName).SelectMany(b => b).ToArray();
            var byteString = ByteString.CopyFrom(bytes);
            return Task.FromResult(new File {FileName = request.FileName, Data = new FileData {Data = byteString}});
        }
    }
}