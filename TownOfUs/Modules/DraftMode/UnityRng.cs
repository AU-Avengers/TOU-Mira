using Random = UnityEngine.Random;

namespace TownOfUs.Modules.DraftMode
{
    public sealed class UnityRng : IRng
    {
        private readonly Dictionary<string, List<int>> _bags = new();

        public int NextInt(int maxExclusive) =>
            maxExclusive <= 0 ? 0 : Random.Range(0, maxExclusive);

        public int NextInt(int minInclusive, int maxExclusive) =>
            maxExclusive <= minInclusive ? minInclusive : Random.Range(minInclusive, maxExclusive);

        public double NextDouble() => Random.value;

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
                    int j = Random.Range(0, i + 1);
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
