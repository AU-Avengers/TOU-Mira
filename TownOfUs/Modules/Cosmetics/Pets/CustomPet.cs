using UnityEngine;

namespace TownOfUs.Modules.Cosmetics.Pets;

public class CustomPet
{
    public CustomPet(
        string id,
        PetData petData,
        PetBehaviour petBehaviour,
        PreviewViewData previewData,
        GameObject obj
        )
    {
        Id = id;
        PetData = petData;
        PetBehaviour = petBehaviour;
        PreviewData = previewData;
        Object = obj;
    }

    public string Id { get; }
    public PetData PetData { get; }
    public PetBehaviour PetBehaviour { get; }
    public GameObject Object { get; }
    public PreviewViewData PreviewData { get; }
}