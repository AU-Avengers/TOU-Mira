using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Cosmetics.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.Cosmetics.Pets;

public class PetLoader : IBaseLoader
{
    public Dictionary<string, CustomPet> CustomPets { get; } = [];

    public void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, customPet) in CustomPets)
        {
            try
            {
                refData.pets.Add(customPet.PetData);
                Info($"Added {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load pet {id} with exception:\n{e.ToString()}");
            }
        }
    }

    public void LoadCosmetics(string directory)
    {
        /*foreach (var pet in _petsToLoad)
        {
            var data = pet.Value;
            LoadPetPrefab(pet.Key, data.Name, data.StoreName, data.MatchPlayerColor);
        }*/
    }

    public bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType)
    {
        il2CPPType = null;
        if (!CustomPets.ContainsKey(id))
        {
            return false;
        }

        il2CPPType = type == ReferenceType.PetViewData ? Il2CppType.Of<PetBehaviour>() : null;
        return il2CPPType != null;
    }

    private static List<string> _checkedPets = new();

    public bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomPets.TryGetValue(id, out var pet))
        {
            return false;
        }

        switch (type)
        {
            case ReferenceType.Preview:
                Debug($"Found pet preview for {id}");
                handle.Complete(pet.PreviewData, true, null);
                return true;
            case ReferenceType.PetViewData:
                Debug($"Found pet view data for {id}");
                // For some reason, this works??!?!?!?!
                if (_checkedPets.Contains(id))
                {
                    handle.Complete(pet.Obj, true, null);
                }
                else
                {
                    _checkedPets.Add(id);
                    handle.Complete(pet.PetBehaviour, true, null);
                }
                return true;
            case ReferenceType.GameObject:
                Debug($"Found pet object for {id}");
                handle.Complete(pet.Obj, true, null);
                return true;
            default:
                Error("Unknown pet type");
                return false;
        }
    }

    private static GameObject PetHolder;

    private static Dictionary<string, StringNames> _storeNames = new();/*
    private static Dictionary<GameObject, PetMetadata> _petsToLoad = new();

    public static void AddPetPrefab(GameObject petPrefab, string name, string storeName, bool matchPlayerColor)
    {
        var data = new PetMetadata();
        data.Name = name;
        data.StoreName = storeName;
        data.MatchPlayerColor = matchPlayerColor;
        _petsToLoad.Add(petPrefab, data);
    }*/
    public void LoadPetPrefab(GameObject petPrefab, string name, string storeName, bool matchPlayerColor = false)
    {
        if (!PetHolder)
        {
            PetHolder = new GameObject("PetHolder");
            PetHolder.DontUnload().DontDestroy();
            PetHolder.gameObject.SetActive(false);
        }
        var fullId = Names.Normalize(name, "pet", storeName);

        var newPet = Object.Instantiate(petPrefab, PetHolder.transform);
        newPet.DontUnload().DontDestroy();
        var petViewData = newPet.GetComponent<PetBehaviour>();
        petViewData.DontUnload().DontDestroy();
        var sprite = newPet.GetComponent<SpriteRenderer>().sprite;
        sprite.DontUnload().DontDestroy();

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = name;
        previewData.PreviewSprite = sprite;

        var petData = ScriptableObject.CreateInstance<PetData>();
        petData.name = name;
        if (_storeNames.TryGetValue(storeName, out var stringName))
        {
            petData.StoreName = stringName;
        }
        else
        {
            var newStringName = CustomStringName.CreateAndRegister(storeName);
            petData.StoreName = newStringName;
            _storeNames.Add(storeName, newStringName);
        }
        petData.Free = true;
        petData.ProductId = fullId;
        petData.PreviewCrewmateColor = matchPlayerColor;
        petData.PetPrefabRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.PetViewData));
        petData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        petViewData.data = petData;
        var customHat = new CustomPet(fullId, petData, petViewData, previewData, newPet);
        CustomPets.Add(fullId, customHat);

        petData.PetPrefabRef.LoadAsset<PetBehaviour>();
        petData.PreviewData.LoadAsset<PreviewViewData>();
    }
}