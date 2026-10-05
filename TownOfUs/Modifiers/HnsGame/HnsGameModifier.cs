using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameModes;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame;

[MiraIgnore]
public abstract class HnsGameModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.HideAndSeek.Modifier.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.HideAndSeek.Modifier.{IdPart}.IntroBlurb");

    public override bool HideFromGuessing => true;

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.HideAndSeek.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.HideAndSeek.Modifier.{IdPart}.WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }
    public List<CustomButtonWikiDescription> Abilities { get; } = [];
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.HideAndSeek;
    public override bool CanSpawnOnCurrentMode() => CustomGameModeManager.IsHideNSeek() || GameManager.Instance.IsHideAndSeek();
    public override Color FreeplayFileColor => new Color32(0, 0, 0, 255);

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true, x => x.PreventsOtherModifiers) && role is not SpectatorRole;
    }
}