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
    public override string ModifierName => TouLocale.Get(TouNames.Rotting, "Rotting");
    public override string IntroInfo => "Your body will also rot away upon death.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Rotting;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public string GetAdvancedDescription()
    {
        return
            $"After {OptionGroupSingleton<RottingOptions>.Instance.RotDelay} second(s), your body will rot away, preventing you from being reported";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return $"Your body will rot away after {OptionGroupSingleton<RottingOptions>.Instance.RotDelay} second(s).";
    }

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