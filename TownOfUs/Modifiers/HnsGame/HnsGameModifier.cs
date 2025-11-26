using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame;

[MiraIgnore]
public abstract class HnsGameModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get($"HnsModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"HnsModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"HnsModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"HnsModifier{LocaleKey}WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }
    public List<CustomButtonWikiDescription> Abilities { get; } = [];
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek;
    public override bool CanSpawnOnCurrentMode() => GameManager.Instance.IsHideAndSeek();
    public override Color FreeplayFileColor => new Color32(0, 0, 0, 255);

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true) && !role.TryCast<SpectatorRole>();
    }
}