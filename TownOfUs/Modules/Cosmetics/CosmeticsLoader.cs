using System.Globalization;
using TownOfUs.Modules.Cosmetics.Pets;
using TownOfUs.Modules.Cosmetics.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.Cosmetics;

public class CosmeticsLoader
{
    private static CosmeticsLoader? _cosmeticsLoader;
    public static CosmeticsLoader Instance => _cosmeticsLoader ??= new CosmeticsLoader();

    private readonly List<UnityEngine.Object> _emptyKeys = new();

    public IEnumerable<object> EmptyKeys { get; }

    private CosmeticReleaseGroup CosmeticGroup { get; }

    // ID -> Name
    private Dictionary<string, string> CustomGroups { get; }

    // used to prevent groups with empty # of certain elements showing in inventory
    public Group PetGroups { get; }
    private readonly PetLoader _petLoader = new();

    private CosmeticsLoader()
    {
        // EmptyKeys = new IEnumerable<object>();
        CosmeticGroup = ScriptableObject.CreateInstance<CosmeticReleaseGroup>();
        CosmeticGroup.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        CustomGroups = [];
        CustomGroups.Add("default", "Custom Cosmetics");

        PetGroups = new Group(CustomGroups);
    }

    public void LoadCosmetics()
    {
        Info("Loading pets...");
        _petLoader.LoadPetPrefab(TouAssets.TortelliniPet.LoadAsset(), "Tortellini", "Atony");
        _petLoader.LoadPetPrefab(TouAssets.BlackMinipostorPet.LoadAsset(), "Black Minipostor", "Atony");

        Info("Setting up cosmetic group...");
        foreach (var id in _petLoader.CustomPets.Keys)
        {
            var group = Names.GetGroup(id);
            PetGroups.AddGroup(group);
            CosmeticGroup.ids.Add(id);
        }
    }

    public void InstallCosmetics(ReferenceData referenceData)
    {
        Info("Installing pets");
        _petLoader.InstallCosmetics(referenceData);

        Info("Installing cosmetic group...");
        var newGroups = referenceData.Groups.releaseGroups.ToList();
        newGroups.Add(CosmeticGroup);
        referenceData.Groups.releaseGroups = newGroups.ToArray();
    }

    public bool LocateCosmetic(
        string id,
        string type,
        out Type il2CPPType
    )
    {
        il2CPPType = null;
        try
        {
            il2CPPType = type switch
            {
                ReferenceType.Preview => typeof(PreviewViewData),
                _ => null!
            };

            return il2CPPType != null
                   || _petLoader.LocateCosmetic(id, type, out il2CPPType);
        }
        catch (Exception e)
        {
            Error($"Unexpected error while locating cosmetic {id}:\n{e}");
            return false;
        }
    }

    public bool ProvideCosmetic(
        ProvideHandle provideHandle,
        string id,
        string type,
        out Exception? exception
        )
    {
        exception = null;
        try
        {
            var result = 
                _petLoader.ProvideCosmetic(provideHandle, id, type);

            return result ? true : throw new InvalidOperationException($"No cosmetic found for {id} and type {type}");
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public bool TryGetPet(string id, out CustomPet? hat)
    {
        return _petLoader.CustomPets.TryGetValue(id, out hat);
    }
}