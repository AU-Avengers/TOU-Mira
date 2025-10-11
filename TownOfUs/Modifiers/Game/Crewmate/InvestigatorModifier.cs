using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class InvestigatorModifier : TouGameModifier, IWikiDiscoverable, IColoredModifier
{
    public Color ModifierColor => new(0f, 0.7f, 0.7f, 1f);
    public override string LocaleKey => "Investigator";
    public override string ModifierName => TouLocale.Get($"TouRole{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription")
               + MiscUtils.AppendOptionsText(CustomRoleSingleton<InvestigatorRole>.Instance.GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Investigator;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];


    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not InvestigatorRole;
    }

    public override void OnActivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        Helpers.GetAlivePlayers().Where(plr => !plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().AddModifier<FootstepsModifier>());
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        PlayerControl.AllPlayerControls.ToArray().Where(plr => plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().RemoveModifier<FootstepsModifier>());
    }

    public override void OnDeath(DeathReason reason)
    {
        if (!Player.AmOwner)
        {
            return;
        }

        PlayerControl.AllPlayerControls.ToArray().Where(plr => plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().RemoveModifier<FootstepsModifier>());
    }
}