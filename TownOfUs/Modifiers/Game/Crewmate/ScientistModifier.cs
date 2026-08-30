using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class ScientistModifier : TouGameModifier, IWikiDiscoverable, IButtonModifier
{
    public override ModifierUiConfiguration Configuration => new(
        new Color32(0, 199, 105, 255),
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Scientist.LoadAsset(),
            "AmongUs.Role.Scientist", 1.45f));
    public override string IdPart => "Scientist";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Scientist;
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

        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not ScientistRole &&
               !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }
}