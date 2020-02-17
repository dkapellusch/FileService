using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FileService.Contracts.Serialization;
using FileService.RocksDb.Extensions;
using RocksDbSharp;

namespace FileService.RocksDb.RocksAbstractions
{
    public class SerializedRocksDb<T>
    {
        public SerializedRocksDb(RocksDatabase rocksDatabase, ISerializer<T> valueSerializer)
        {
            RocksDatabase = rocksDatabase;
            ValueSerializer = valueSerializer;
        }

        private RocksDatabase RocksDatabase { get; }

        internal RocksDbSharp.RocksDb RocksDb => RocksDatabase.RocksDb;

        internal ISerializer<T> ValueSerializer { get; }

        internal ColumnFamilyHandle GetColumnFamily(string name = null) => RocksDatabase.GetOrCreateColumnFamily($"{typeof(T).Name}/{name}");

        public T Get(string key, ColumnFamilyHandle columnFamilyHandle = null, ReadOptions readOptions = null) =>
            Get(Encoding.UTF8.GetBytes(key), columnFamilyHandle, readOptions);

        public T Get(byte[] key, ColumnFamilyHandle columnFamilyHandle = null, ReadOptions readOptions = null)
        {
            columnFamilyHandle ??= RocksDb.GetDefaultColumnFamily();
            readOptions ??= new ReadOptions();

            var result = RocksDb.Get(key, columnFamilyHandle, readOptions);
            return ValueSerializer.Deserialize(result);
        }

        public void Put(string key, T value, ColumnFamilyHandle columnFamilyHandle = null, WriteOptions writeOptions = null) =>
            Put(Encoding.UTF8.GetBytes(key), value, columnFamilyHandle, writeOptions);

        public void Put(byte[] key, T value, ColumnFamilyHandle columnFamilyHandle = null, WriteOptions writeOptions = null)
        {
            columnFamilyHandle ??= RocksDb.GetDefaultColumnFamily();
            writeOptions ??= new WriteOptions();

            RocksDb.Put(key, ValueSerializer.Serialize(value), columnFamilyHandle, writeOptions);
        }

        public bool PutBatch(params (string key, T value, ColumnFamilyHandle columnFamilyHandle)[] values)
        {
            var batch = new WriteBatch();
            batch.SetSavePoint();

            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var (key, value, columnFamily) = values[i];
                    batch.Put(Encoding.UTF8.GetBytes(key), ValueSerializer.Serialize(value), columnFamily);
                }

                RocksDb.Write(batch);
                return true;
            }
            catch (Exception e)
            {
                batch.RollbackToSavePoint();
                Console.WriteLine(e);
                return false;
            }
        }

        public void Remove(string key, ColumnFamilyHandle columnFamilyHandle = null, WriteOptions writeOptions = null) =>
            Remove(Encoding.UTF8.GetBytes(key), columnFamilyHandle, writeOptions);

        public void Remove(byte[] key, ColumnFamilyHandle columnFamilyHandle = null, WriteOptions writeOptions = null)
        {
            columnFamilyHandle ??= RocksDb.GetDefaultColumnFamily();

            writeOptions ??= new WriteOptions();

            RocksDb.Remove(key, columnFamilyHandle, writeOptions);
        }

        public IEnumerable<(string key, T value)> IteratorFromStart
        (ColumnFamilyHandle columnFamilyHandle = null,
         ReadOptions readOptions = null,
         CancellationToken token = default
        )
        {
            columnFamilyHandle ??= RocksDb.GetDefaultColumnFamily();
            readOptions ??= new ReadOptions();

            var iterator = RocksDb.NewIterator(columnFamilyHandle, readOptions);

            return iterator
                .SeekToFirst()
                .GetEnumerable(token)
                .Select(kv =>
                {
                    var key = Encoding.UTF8.GetString(kv.key);
                    var value = ValueSerializer.Deserialize(kv.value);
                    return (key, value);
                });
        }

        public IEnumerable<(string key, T value)> IteratorFromKey
        (string key,
         ColumnFamilyHandle columnFamilyHandle = null,
         ReadOptions readOptions = null,
         CancellationToken token = default)
        {
            using var snapShot = RocksDb.CreateSnapshot();
            columnFamilyHandle ??= RocksDb.GetDefaultColumnFamily();
            readOptions ??= new ReadOptions().SetSnapshot(snapShot).SetTotalOrderSeek(true);

            var iterator = RocksDb.NewIterator(columnFamilyHandle, readOptions);
            return iterator
                .Seek(key)
                .GetEnumerable(token)
                .Select(kv =>
                {
                    var deserializedKey = Encoding.UTF8.GetString(kv.key);
                    var value = ValueSerializer.Deserialize(kv.value);

                    return (deserializedKey, value);
                });
        }
    }
}