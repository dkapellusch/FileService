using System.Threading;

namespace FileService.RocksDb
{
    internal class RocksOffsetProvider
    {
        private long _length;

        public RocksOffsetProvider(int startingPoint) => Length = startingPoint;

        internal long Length
        {
            get => _length;
            private set => _length = value;
        }

        internal string GetOffset() => PadOffset(Interlocked.Increment(ref _length));

        private static string PadOffset(long offset) => offset.ToString().PadLeft(10, '-').PadRight(20, '-');
    }
}