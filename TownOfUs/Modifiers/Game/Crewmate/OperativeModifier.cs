using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class OperativeModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Operative";
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

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Operative;
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

        CustomButtonSingleton<SecurityButton>.Instance.AvailableCharge =
            OptionGroupSingleton<OperativeOptions>.Instance.StartingCharge;
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<SecurityButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<OperativeOptions>.Instance.RoundCharge;
    }

    public static void OnTaskComplete()
    {
        CustomButtonSingleton<SecurityButton>.Instance.AvailableCharge +=
            OptionGroupSingleton<OperativeOptions>.Instance.TaskCharge;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() &&
               !role.Player.GetModifierComponent().HasModifier<SatelliteModifier>(true) &&
               !role.Player.GetModifierComponent().HasModifier<ButtonBarryModifier>(true);
    }
}