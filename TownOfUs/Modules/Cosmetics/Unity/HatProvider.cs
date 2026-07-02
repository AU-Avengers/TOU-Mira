using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace TownOfUs.Modules.Cosmetics.Unity;

public class HatProvider : ResourceProviderBase
{
    
    public static void Initialize()
    {
        var instance = new HatProvider();
        Addressables.ResourceManager.ResourceProviders.Insert(0, instance);
    }

    public override bool CanProvide(Type t, IResourceLocation location)
    {
        return location.InternalId.StartsWith("toum.", StringComparison.InvariantCulture);
    }

    public override Type GetDefaultType(IResourceLocation location)
    {
        return location.ResourceType;
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        string internalId = provideHandle.Location.InternalId;
        Debug($"Processing {internalId}");

        if (!internalId.StartsWith("toum", StringComparison.InvariantCulture))
        {
            Error($"{internalId} is not a TOU Mira cosmetic");
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Exception("Not a TOU Mira cosmetic"));
            return;
        }

        var idAndType = internalId.Split("/");
        if (idAndType.Length != 2) 
        {
            Error($"Invalid identifier: {idAndType}");
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Exception("Invalid TOU Mira ID"));
            return;
        }

        var id = idAndType[0];
        var type = idAndType[1];

        if (CosmeticsLoader.Instance.ProvideCosmetic(provideHandle, id, type, out var exception))
        {
            Debug($"Successfully provided cosmetic {id} of type {type}");
        }
        else
        {
            Error($"Failed to provide cosmetic {id} of type {type}:\n{exception}");
            provideHandle.Complete<UnityEngine.Object>(null!, false, 
                new Exception(exception!.ToString()));
        }
    }

    public override void Release(IResourceLocation location, System.Object obj)
    {
        Warning("I don't know how to release cosmetic yet");
    }
}