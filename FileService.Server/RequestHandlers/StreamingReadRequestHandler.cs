using System.Collections.Generic;
using System.Linq;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Google.Protobuf;

namespace FileService.Server.RequestHandlers
{
    public class StreamFileRequestHandler
    {
        private readonly ByteChunkStore _fileDb;

        public StreamFileRequestHandler(ByteChunkStore fileDb) => _fileDb = fileDb;

        public IEnumerable<FileData> HandleRequest(ReadFileRequest request) =>
            _fileDb.Read(request.FileName).Select(m => new FileData {Data = ByteString.CopyFrom(m)});
    }
}