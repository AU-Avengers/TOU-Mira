using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class SpyModifier : TouGameModifier, IWikiDiscoverable, IColoredModifier, IButtonModifier
{
    public override ModifierUiConfiguration Configuration => new(
        new(0.8f, 0.64f, 0.8f, 1f),
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Spy.LoadAsset(),
            "TouMira.Role.Crewmate.Spy", 1.45f));
    public Color ModifierColor => new(0.8f, 0.64f, 0.8f, 1f);
    public override string IdPart => "Spy";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription")
               + MiscUtils.AppendOptionsText(CustomRoleSingleton<SpyRole>.Instance.GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Spy;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override void OnActivate()
    {
        base.OnActivate();

        if (!Player.AmOwner)
        {
            return;
        }

        CustomButtonSingleton<SpyAdminTableModifierButton>.Instance.AvailableCharge =
            OptionGroupSingleton<SpyOptions>.Instance.StartingCharge.Value;
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<SpyAdminTableModifierButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<SpyOptions>.Instance.RoundCharge.Value;
    }

    public static void OnTaskComplete()
    {
        CustomButtonSingleton<SpyAdminTableModifierButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<SpyOptions>.Instance.TaskCharge.Value;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not SpyRole && role.IsCrewmate() &&
               MiscUtils.GetCurrentMap != ExpandedMapNames.Fungle &&
               !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }
}