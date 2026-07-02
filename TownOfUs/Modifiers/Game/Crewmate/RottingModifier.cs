using MiraAPI.GameOptions;
using System.Collections;
using HarmonyLib;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
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

    public static IEnumerator StartRotting(PlayerControl player, PlayerControl? killer = null)
    {
        var rotting = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId);
        if (rotting == null)
        {
            yield break;
        }

        /*if (OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind)
        {
            // Fire event for Time Lord system (this will also call RecordBodyCleaned internally)
            var bodyPlayer = MiscUtils.PlayerById(player.PlayerId);
            if (bodyPlayer != null)
            {
                TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordBodyCleaned(player, rotting, rotting.transform.position, 
                    TimeLordBodyManager.CleanedBodySource.Rotting);
            }
            AmongUsClient.Instance.StartCoroutine(TimeLordBodyManager.CoHideBodyForTimeLord(rotting));
        }
        else
        {
            AmongUsClient.Instance.StartCoroutine(rotting.CoClean());
        }*/
        CrimeSceneComponent.ClearCrimeScene(rotting);
        AmongUsClient.Instance.StartCoroutine(CoSetUpRot(rotting, player, killer == null ? player : killer));
    }

    public static IEnumerator CoSetUpRot(DeadBody body, PlayerControl target, PlayerControl killer)
    {
        yield return new WaitForEndOfFrame();
        ViperDeadBody deadBody = (Object.Instantiate(GameManager.Instance.deadBodyPrefab[1]) as ViperDeadBody)!;
        deadBody.enabled = false;
        deadBody.ParentId = target.PlayerId;
        deadBody.bodyRenderers.Do(x => target.SetPlayerMaterialColors(x));
        target.SetPlayerMaterialColors(deadBody.bloodSplatter);
        deadBody.transform.position = body.transform.position;
        body.ClearBody();
        deadBody.SetupViperInfo(OptionGroupSingleton<RottingOptions>.Instance.RotDelay, killer, target);
        deadBody.enabled = true;
    }
}