using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class ScientistModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Scientist";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Scientist;
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

        CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge =
            OptionGroupSingleton<ScientistOptions>.Instance.StartingCharge;
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<ScientistOptions>.Instance.RoundCharge;
    }

    public static void OnTaskComplete()
    {
        CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<ScientistOptions>.Instance.TaskCharge;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (role is TransporterRole && !OptionGroupSingleton<TransporterOptions>.Instance.CanUseVitals)
        {
            return false;
        }

        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not ScientistRole
               && !role.Player.GetModifierComponent().HasModifier<SatelliteModifier>(true)
               && !role.Player.GetModifierComponent().HasModifier<ButtonBarryModifier>(true);
    }
}