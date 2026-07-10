using UnityEngine;

namespace TownOfUs.Modules.Cosmetics.Pets;

public class CustomPet(
    string id,
    PetData petData,
    PetBehaviour petBehaviour,
    PreviewViewData previewData,
    GameObject obj
        )
{
    public string Id { get; } = id;
    public PetData PetData { get; } = petData;
    public PetBehaviour PetBehaviour { get; } = petBehaviour;
    public GameObject Obj { get; } = obj;
    public PreviewViewData PreviewData { get; } = previewData;
}