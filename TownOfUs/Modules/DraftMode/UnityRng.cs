using Random = UnityEngine.Random;

namespace TownOfUs.Modules.DraftMode
{
    public sealed class UnityRng : IRng
    {
        public int NextInt(int maxExclusive) =>
            maxExclusive <= 0 ? 0 : Random.Range(0, maxExclusive);

        public int NextInt(int minInclusive, int maxExclusive) =>
            maxExclusive <= minInclusive ? minInclusive : Random.Range(minInclusive, maxExclusive);

        public double NextDouble() => Random.value;
    }
}
