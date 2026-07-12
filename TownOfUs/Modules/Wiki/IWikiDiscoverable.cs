using MiraAPI.Modifiers;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public interface IWikiDiscoverable
{ public List<CustomButtonWikiDescription> Abilities => [];

    public string SecondTabName => TouLocale.Get("WikiAbilitiesTab", "Abilities"); public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.Normal;

    public uint FakeTypeId => ModifierManager.GetModifierTypeId(GetType()) ??
                              throw new InvalidOperationException("Modifier is not registered.");

    public string GetAdvancedDescription()
    {
        return MiscUtils.AppendOptionsText(GetType());
    }
}

public record struct CustomButtonWikiDescription(string Name, string Description, LoadableAsset<Sprite> Icon);