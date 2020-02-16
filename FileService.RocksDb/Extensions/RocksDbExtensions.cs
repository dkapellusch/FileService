using System.Collections.Generic;
using System.Threading;
using RocksDbSharp;

namespace FileService.RocksDb.Extensions
{
    public static class RocksDbExtensions
    {
        public static IEnumerable<(byte[] key, byte[] value)> GetEnumerable(this Iterator iterator, CancellationToken token = default)
        {
            using var rocksIterator = iterator;

            while (rocksIterator.Valid() && !token.IsCancellationRequested)
            {
                var key = rocksIterator.Key();
                var value = rocksIterator.Value();

                yield return (key, value);

                rocksIterator.Next();
            }
        }
    }
}