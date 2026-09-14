using System.Diagnostics;

namespace TownOfUs.Modules.DraftMode
{
    public interface IRng
    {
        int NextInt(int maxExclusive);
        int NextInt(int minInclusive, int maxExclusive);
        double NextDouble();
        List<int> NextSpreadIndices(int count, int rangeExclusive);
    }

    public sealed class DeterministicRng : IRng
    {
        private uint _state;

        public DeterministicRng(uint seed)
        {
            _state = seed == 0u ? 0x9E3779B9u : seed;
        }
        public static DeterministicRng CreateRandomlySeeded()
        {
            unchecked
            {
                uint seed = (uint)Environment.TickCount;
                seed ^= (uint)Guid.NewGuid().GetHashCode();
                seed ^= (uint)DateTime.UtcNow.Ticks;
                seed ^= (uint)Stopwatch.GetTimestamp();
                return new DeterministicRng(seed);
            }
        }

        private uint NextState()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0) return 0;
            return (int)(NextState() % (uint)maxExclusive);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            return minInclusive + (int)(NextState() % (uint)(maxExclusive - minInclusive));
        }

        public double NextDouble() => (NextState() >> 8) / (double)(1u << 24);

        public List<int> NextSpreadIndices(int count, int rangeExclusive)
        {
            var result = new List<int>();
            if (count <= 0 || rangeExclusive <= 0) return result;
            count = Math.Min(count, rangeExclusive);

            double bucketSize = rangeExclusive / (double)count;
            for (int i = 0; i < count; i++)
            {
                int bucketStart = (int)(i * bucketSize);
                int bucketEnd = Math.Min(rangeExclusive, (int)((i + 1) * bucketSize));
                if (bucketEnd <= bucketStart) bucketEnd = bucketStart + 1;
                result.Add(NextInt(bucketStart, bucketEnd));
            }

            return result;
        }
    }
}