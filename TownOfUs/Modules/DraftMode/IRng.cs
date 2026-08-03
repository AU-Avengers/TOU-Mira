namespace TownOfUs.Modules.DraftMode
{
    public interface IRng
    {
        int NextInt(int maxExclusive);
        int NextInt(int minInclusive, int maxExclusive);
        double NextDouble();
    }

    public sealed class DeterministicRng : IRng
    {
        private uint _state;

        public DeterministicRng(uint seed)
        {
            _state = seed == 0u ? 0x9E3779B9u : seed;
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
    }
}
