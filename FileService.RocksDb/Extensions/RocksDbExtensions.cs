using System.Collections.Generic;
using System.Threading;
using FileService.RocksDb.RocksAbstractions;
using RocksDbSharp;

namespace FileService.RocksDb.Extensions
{
    public static class RocksDbExtensions
    {
        private static readonly object _createColumnFamilyLock = new object();

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

        public static ColumnFamilyHandle GetOrCreateColumnFamily(this RocksDatabase rocksDb, string columnFamilyName)
        {
            var columnFamily = TryGetColumnFamily(rocksDb, columnFamilyName);

            if (!(columnFamily is null)) return columnFamily;

            lock (_createColumnFamilyLock)
            {
                columnFamily = TryGetColumnFamily(rocksDb, columnFamilyName);
                if (!(columnFamily is null)) return columnFamily;

                var options = new DbOptions();

                rocksDb.RocksDb.CreateColumnFamily(options, columnFamilyName);

                return rocksDb.RocksDb.GetColumnFamily(columnFamilyName);
            }
        }

        private static ColumnFamilyHandle TryGetColumnFamily(RocksDatabase rocksDatabase, string columnFamilyName)
        {
            try
            {
                return rocksDatabase.RocksDb.GetColumnFamily(columnFamilyName);
            }
            catch
            {
                return null;
            }
        }
    }
}