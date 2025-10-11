using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public interface IWikiDiscoverable
{
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public string SecondTabName => TouLocale.Get("WikiAbilitiesTab", "Abilities");
    public bool IsHiddenFromList => false;

    public uint FakeTypeId => ModifierManager.GetModifierTypeId(GetType()) ??
                              throw new InvalidOperationException("Modifier is not registered.");

    public string GetAdvancedDescription()
    {
        return MiscUtils.AppendOptionsText(GetType());
    }
}

public record struct CustomButtonWikiDescription(string name, string description, LoadableAsset<Sprite> icon);