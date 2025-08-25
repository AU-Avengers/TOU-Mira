using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class SpyModifier : TouGameModifier, IWikiDiscoverable, IColoredModifier
{
    public Color ModifierColor => new(0.8f, 0.64f, 0.8f, 1f);
    public override string ModifierName => TouLocale.Get(TouNames.Spy, "Spy");
    public override string IntroInfo => "You can also gain extra information on the Admin Table";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Spy;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;

    public string GetAdvancedDescription()
    {
        return
            $"The {ModifierName} gains extra information on the admin table. They now not only see how many people are in a room, but will also see who is in every room."
            + MiscUtils.AppendOptionsText(CustomRoleSingleton<SpyRole>.Instance.GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "Gain extra information on the Admin Table.";
    }

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
               Instance.Type != MapType.Fungle;
    }
}