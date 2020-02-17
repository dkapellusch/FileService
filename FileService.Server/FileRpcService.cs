using System.Threading.Tasks;
using FileService.Contracts;
using FileService.Server.RequestHandlers.V2;
using Grpc.Core;

namespace FileService.Server
{
    public class FileRpcService : Contracts.FileService.FileServiceBase
    {
        private readonly StreamingWriteRequestHandler _streamingWriteRequestHandler;
        private readonly ReadRequestHandler _readRequestHandler;
        private readonly WriteRequestHandler _writeRequestHandler;
        private readonly StreamingReadRequestHandler _streamingReadRequestHandler;

        public FileRpcService
        (
            ReadRequestHandler readRequestHandler,
            WriteRequestHandler writeRequestHandler,
            StreamingWriteRequestHandler streamingWriteRequestHandler,
            StreamingReadRequestHandler streamingReadRequestHandler
        )
        {
            _streamingWriteRequestHandler = streamingWriteRequestHandler;
            _readRequestHandler = readRequestHandler;
            _writeRequestHandler = writeRequestHandler;
            _streamingReadRequestHandler = streamingReadRequestHandler;
        }

        public override async Task<File> GetFile(ReadFileRequest request, ServerCallContext context) =>
            await _readRequestHandler.HandleRequest(request);

        public override async Task<FileMetadata> WriteFile(File request, ServerCallContext context) =>
            await _writeRequestHandler.HandleRequest(request);

        public override async Task StreamFile(ReadFileRequest request, IServerStreamWriter<FileData> responseStream, ServerCallContext context)
        {
            await _streamingReadRequestHandler.HandleRequest(request, responseStream);
        }

        public override async Task<FileMetadata> WriteFileStream(IAsyncStreamReader<WriteFileRequest> requestStream, ServerCallContext context) =>
            await _streamingWriteRequestHandler.HandleRequest(requestStream, context.CancellationToken);
    }
}