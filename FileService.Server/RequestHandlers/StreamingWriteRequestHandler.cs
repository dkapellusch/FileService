using System.Threading;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using Grpc.Core;
using Grpc.Core.Utils;

namespace FileService.Server.RequestHandlers
{
    public class StreamingWriteRequestHandler
    {
        private readonly ByteChunkStore _fileDb;

        public StreamingWriteRequestHandler(ByteChunkStore fileDb) => _fileDb = fileDb;

        public async Task<Ack> HandleRequest(IAsyncStreamReader<WriteFileRequest> requestStream, CancellationToken token)
        {
            WriteFileRequest request = null;
            await requestStream
                .ForEachAsync(chunk =>
                {
                    request ??= chunk;
                    if (!token.IsCancellationRequested)
                        _fileDb.WriteChunk(chunk.FileName, chunk.Chunk.Data.ToByteArray());
                    return Task.CompletedTask;
                });


            _fileDb.Complete(request?.FileName);
            return new Ack();
        }
    }
}