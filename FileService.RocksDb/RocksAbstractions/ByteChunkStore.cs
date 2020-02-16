using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FileService.RocksDb.Extensions;

namespace FileService.RocksDb.RocksAbstractions
{
    public class ByteChunkStore
    {
        private readonly ConcurrentDictionary<string, RocksOffsetProvider> _offsetProviders = new ConcurrentDictionary<string, RocksOffsetProvider>();
        private readonly RocksDatabase _rocksDatabase;

        public ByteChunkStore(RocksDatabase rocksDatabase) => _rocksDatabase = rocksDatabase;

        public void WriteChunk(string key, byte[] chunk)
        {
            var offsetProvider = _offsetProviders.GetOrAdd(key, _ => new RocksOffsetProvider(0));
            var serializedKey = Encoding.UTF8.GetBytes($"{key}/{offsetProvider.GetOffset()}");
            _rocksDatabase.RocksDb.Put(serializedKey, chunk);
        }

        public IEnumerable<byte[]> Read(string key)
        {
            var iterator = _rocksDatabase.RocksDb.NewIterator();
            var tokenSource = new CancellationTokenSource();
            return iterator.Seek(key)
                .GetEnumerable(tokenSource.Token)
                .Select(kv =>
                {
                    var deserializedKey = Encoding.UTF8.GetString(kv.key).Split("/")[0];
                    if (!deserializedKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        tokenSource.Cancel();
                        return null;
                    }

                    return kv.value;
                })
                .Where(v => !(v is null));
        }

        public void Complete(string key) => _offsetProviders.TryRemove(key, out _);
    }
}