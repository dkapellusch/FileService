using System;
using System.IO;
using System.Linq;
using RocksDbSharp;

namespace FileService.RocksDb.RocksAbstractions
{
    public static class RocksDbFactory
    {
        internal static RocksDbSharp.RocksDb GetDatabase(string databasePath, SliceTransform prefixTransform)
        {
            var columnFamilies = new ColumnFamilies();

            if (File.Exists(databasePath) || Directory.Exists(databasePath))
                foreach (var columnFamily in RocksDbSharp.RocksDb.ListColumnFamilies(new DbOptions(), databasePath).ToHashSet())
                    columnFamilies.Add(columnFamily,
                        new ColumnFamilyOptions()
                            .SetPrefixExtractor(prefixTransform)
                            .SetCompression(Compression.Snappy)
                            .SetBlockBasedTableFactory(new BlockBasedTableOptions()
                                .SetBlockCache(Cache.CreateLru(ulong.MaxValue))
                                .SetFilterPolicy(BloomFilterPolicy.Create(1024))
                                .SetWholeKeyFiltering(true)
                                .SetIndexType(BlockBasedTableIndexType.TwoLevelIndex)));

            var options = Native.Instance.rocksdb_options_create();
            Native.Instance.rocksdb_options_increase_parallelism(options, Environment.ProcessorCount);
            Native.Instance.rocksdb_options_optimize_level_style_compaction(options, 0);
            Native.Instance.rocksdb_options_set_create_if_missing(options, true);
            Native.Instance.rocksdb_cache_create_lru(UIntPtr.Add(new UIntPtr(), 1024_000));

            return RocksDbSharp.RocksDb.Open(new DbOptions()
                    .SetCreateIfMissing()
                    .SetCreateMissingColumnFamilies()
                    .IncreaseParallelism(Environment.ProcessorCount),
                databasePath,
                columnFamilies);
        }
    }
}