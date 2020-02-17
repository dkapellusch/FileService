using System;
using System.Text;
using System.Threading;
using Murmur;

namespace FileService.Contracts
{
    public static class HashingExtensions
    {
        private static readonly ThreadLocal<Murmur128> _hasher = new ThreadLocal<Murmur128>(() => MurmurHash.Create128());

        public static long Murmur3Hash(this byte[] bytes) => BitConverter.ToInt64(_hasher.Value.ComputeHash(bytes));

        public static long Murmur3Hash(this string inputString) => Encoding.UTF8.GetBytes(inputString).Murmur3Hash();
    }
}