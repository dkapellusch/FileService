using System;
using RocksDbSharp;

namespace FileService.RocksDb.RocksAbstractions
{
    public sealed class RocksDatabase : IDisposable
    {
        public RocksDatabase(string path, ulong maxKeyLength = 500)
        {
            Path = path;
            RocksDb = RocksDbFactory.GetDatabase(path, SliceTransform.CreateFixedPrefix(maxKeyLength));
            MaxKeyLength = maxKeyLength;
        }

        public ulong MaxKeyLength { get; }

        public string Path { get; }

        public RocksDbSharp.RocksDb RocksDb { get; }

        public void Dispose()
        {
            RocksDb?.Dispose();
        }
    }
}