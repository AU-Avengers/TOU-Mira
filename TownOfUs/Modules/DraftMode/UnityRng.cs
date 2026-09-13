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