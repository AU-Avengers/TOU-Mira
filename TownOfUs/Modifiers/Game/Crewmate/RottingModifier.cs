using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class RottingModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Rotting";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription").Replace("<rotDelay>",
            $"{OptionGroupSingleton<RottingOptions>.Instance.RotDelay}");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription").Replace("<rotDelay>",
            $"{OptionGroupSingleton<RottingOptions>.Instance.RotDelay}");
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Rotting;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.RottingAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public static IEnumerator StartRotting(PlayerControl player)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<RottingOptions>.Instance.RotDelay);
        var rotting = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId);
        if (rotting == null)
        {
            yield break;
        }

        Coroutines.Start(rotting.CoClean());
        Coroutines.Start(CrimeSceneComponent.CoClean(rotting));
    }
}