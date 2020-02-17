using System;
using System.Collections.Generic;
using System.IO;

namespace FileService.Contracts
{
    public static class FileHelper
    {
        private const int MaxChunkSize = 10_000;
        private const string OutputPath = "FileStore";
        private const int PartitionCount = 10;

        public static string GetOutputPath(string fileName)
        {
            var partition = GetPartition(fileName);
            var folder = Directory.CreateDirectory($"{OutputPath}/{partition}");
            return Path.Combine(folder.FullName, fileName);
        }

        public static IEnumerable<byte[]> ChunkFile(this FileStream stream)
        {
            var chunk = new byte[MaxChunkSize];
            while (true)
            {
                var index = 0;
                while (index < chunk.Length)
                {
                    var bytesRead = stream.Read(chunk, index, chunk.Length - index);
                    if (bytesRead == 0) break;

                    index += bytesRead;
                }

                if (index != 0) yield return chunk;

                if (index != chunk.Length) yield break;
            }
        }

        private static int GetPartition(string key) => string.IsNullOrWhiteSpace(key)
            ? 0
            : (int) (Math.Abs(key.Murmur3Hash()) % PartitionCount);
    }
}