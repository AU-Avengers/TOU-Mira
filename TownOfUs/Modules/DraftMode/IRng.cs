using System.Diagnostics;

namespace TownOfUs.Modules.DraftMode
{
    public interface IRng
    {
        int NextInt(int maxExclusive);
        int NextInt(int minInclusive, int maxExclusive);
        double NextDouble();
        int NextShuffledInt(string bagKey, int maxExclusive);
        List<int> NextSpreadIndices(int count, int rangeExclusive);
        void ResetBags();
    }

    public sealed class DeterministicRng : IRng
    {
        private uint _state;
        private readonly Dictionary<string, List<int>> _bags = new();

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

        public int NextShuffledInt(string bagKey, int maxExclusive)
        {
            if (maxExclusive <= 0) return 0;
            if (maxExclusive == 1) return 0;

            var key = $"{bagKey}_{maxExclusive}";
            if (!_bags.TryGetValue(key, out var bag) || bag.Count == 0)
            {
                bag = new List<int>(maxExclusive);
                for (int i = 0; i < maxExclusive; i++) bag.Add(i);
                for (int i = bag.Count - 1; i > 0; i--)
                {
                    int j = NextInt(i + 1);
                    (bag[i], bag[j]) = (bag[j], bag[i]);
                }
                _bags[key] = bag;
            }

            int result = bag[^1];
            bag.RemoveAt(bag.Count - 1);
            return result;
        }

        public void ResetBags() => _bags.Clear();
    }
}