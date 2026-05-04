using System.Diagnostics.CodeAnalysis;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace TownOfUs.Modules.Cosmetics;

public abstract class BaseLoader
{
    public abstract void InstallCosmetics(ReferenceData refData);

    public abstract void LoadCosmetics(string directory);

    public abstract bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType);

    public abstract bool ProvideCosmetic(ProvideHandle handle, string id, string type);
}