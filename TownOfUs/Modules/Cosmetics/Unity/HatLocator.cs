using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TownOfUs.Modules.Cosmetics.Unity;

public class HatLocator : Il2CppSystem.Object
{

    public static string GetGuid(string hatId, string type)
    {
        return $"{hatId}/{type}";
    }

    public static void Initialize()
    {
        var instance = new HatLocator();
        var locator = new IResourceLocator(instance.Pointer);
        Addressables.AddResourceLocator(locator);
    }

    public HatLocator(IntPtr iPtr) : base(iPtr)
    {
    }
    
    public HatLocator() : base(ClassInjector.DerivedConstructorPointer<HatLocator>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public string LocatorId => GetType().FullName!;

    public static Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>
        Keys => CosmeticsLoader.Instance.EmptyKeys;

    private string ProviderId { get; } = typeof(HatProvider).FullName!;

    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type,
        out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;

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

        Debug($"Found cosmetic {realKey}, type {typeName}, il2cpp tyle {il2CPPType.NameOrDefault}");

        var location = new ResourceLocationBase(
            keyString,
            keyString,
            ProviderId,
            il2CPPType
        );

        var il2CPPList = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        il2CPPList.Add(location);
        // pointer magic cuz il2cpp interfaces are broken
        locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(il2CPPList.Pointer);

        return true;
    }
}