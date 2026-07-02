using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TownOfUs.Modules.Cosmetics.Unity;

public class HatLocator : UnityEngine.Object, IResourceLocator
{

    public static string GetGuid(string hatId, string type)
    {
        return $"{hatId}/{type}";
    }

    public static void Initialize()
    {
        var instance = new HatLocator();
        Addressables.AddResourceLocator(instance);
    }

    public string LocatorId => GetType().FullName!;

    public IEnumerable<object>
        Keys => CosmeticsLoader.Instance.EmptyKeys;

    private string ProviderId { get; } = typeof(HatProvider).FullName!;

    public bool Locate(object key, Type type,
        out IList<IResourceLocation> locations)
    {
        locations = new List<IResourceLocation>();

        if (key.ToString() is not { } keyString)
        {
            return false;
        }

        if (!keyString.StartsWith("toum.", StringComparison.InvariantCulture))
        {
            return false;
        }

        var split = keyString.Split('/');
        if (split.Length != 2)
        {
            Error($"Invalid format: {keyString}");
            return false;
        }

        var realKey = split[0];
        var typeName = split[1];

        if (!CosmeticsLoader.Instance.LocateCosmetic(realKey, typeName, out var il2CPPType))
        {
            Error($"{realKey} not found in custom cosmetics.");
            return false;
        }

        Debug($"Found cosmetic {realKey}, type {typeName}, il2cpp tyle {il2CPPType.FullName}");

        var location = new ResourceLocationBase(
            keyString,
            keyString,
            ProviderId,
            il2CPPType
        );

        locations.Add(location);

        return true;
    }
}