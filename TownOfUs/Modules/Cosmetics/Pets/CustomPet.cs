using UnityEngine;

namespace TownOfUs.Modules.Cosmetics.Pets;

public record CustomPet(
    string Id,
    PetData PetData,
    PetBehaviour PetBehaviour,
    PreviewViewData PreviewData,
    GameObject Obj
);