using System.Threading.Tasks;
using FileService.Contracts;
using FileService.Server.RequestHandlers;
using Grpc.Core;

namespace FileService.Server
{
    public class FileRpcService : Contracts.FileService.FileServiceBase
    {
        private readonly StreamingWriteRequestHandler _streamingWriteRequestHandler;
        private readonly ReadRequestHandler _readRequestHandler;
        private readonly WriteRequestHandler _writeRequestHandler;
        private readonly StreamFileRequestHandler _streamingReadRequestHandler;

        public FileRpcService
        (
            ReadRequestHandler readRequestHandler,
            WriteRequestHandler writeRequestHandler,
            StreamingWriteRequestHandler streamingWriteRequestHandler,
            StreamFileRequestHandler streamingReadRequestHandler
        )
        {
            _streamingWriteRequestHandler = streamingWriteRequestHandler;
            _readRequestHandler = readRequestHandler;
            _writeRequestHandler = writeRequestHandler;
            _streamingReadRequestHandler = streamingReadRequestHandler;
        }

        public override async Task<File> GetFile(ReadFileRequest request, ServerCallContext context) =>
            await _readRequestHandler.HandleRequest(request);

        public override async Task<Ack> WriteFile(File request, ServerCallContext context) =>
            await _writeRequestHandler.HandleRequest(request);

        public override async Task StreamFile(ReadFileRequest request, IServerStreamWriter<FileData> responseStream, ServerCallContext context)
        {
            foreach (var fileChunk in _streamingReadRequestHandler.HandleRequest(request))
                await responseStream.WriteAsync(fileChunk);
        }

        public override async Task<Ack> WriteFileStream(IAsyncStreamReader<WriteFileRequest> requestStream, ServerCallContext context) =>
            await _streamingWriteRequestHandler.HandleRequest(requestStream, context.CancellationToken);
    }
}