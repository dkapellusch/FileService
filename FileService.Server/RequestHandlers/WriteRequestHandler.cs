using System;
using System.Threading.Tasks;
using FileService.Contracts;
using FileService.RocksDb.RocksAbstractions;
using NPoco.Extensions;

namespace FileService.Server.RequestHandlers
{
    public class WriteRequestHandler
    {
        private readonly ByteChunkStore _fileDb;

        public WriteRequestHandler(ByteChunkStore fileDb) => _fileDb = fileDb;

        public Task<Ack> HandleRequest(File request)
        {
            var bytesToWrite = request.Data.Data.ToByteArray();
            foreach (var bytes in bytesToWrite.Chunkify(bytesToWrite.Length / Math.Min(bytesToWrite.Length, 1000)))
                _fileDb.WriteChunk(request.FileName, bytes);

            return Task.FromResult(new Ack());
        }
    }
}