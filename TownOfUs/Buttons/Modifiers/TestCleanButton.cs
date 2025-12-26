using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.TimeLord;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class TestCleanButton : TownOfUsTargetButton<DeadBody>
{
    public override string Name => TouLocale.GetParsed("TouRoleJanitorClean", "Clean");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 10f;
    public override float EffectDuration => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.CleanButtonSprite;

    public DeadBody? CleaningBody { get; set; }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        CleaningBody = Target;
        OverrideName(TouLocale.Get("TouRoleJanitorCleaning", "Cleaning"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.GetParsed("TouRoleJanitorClean", "Clean"));
        if (CleaningBody == Target && CleaningBody != null)
        {
            // Directly call the clean logic without role check (for testing modifier)
            CleanBodyDirectly(CleaningBody);
            TouAudio.PlaySound(TouAudio.JanitorCleanSound);
        }

        CleaningBody = null;
    }

    private static void CleanBodyDirectly(DeadBody body)
    {
        if (body == null)
        {
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.JanitorClean, PlayerControl.LocalPlayer, body);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (OptionGroupSingleton<TimeLordOptions>.Instance.UncleanBodiesOnRewind)
        {
            var bodyPlayer = MiscUtils.PlayerById(body.ParentId);
            if (bodyPlayer != null)
            {
                TownOfUs.Events.Crewmate.TimeLordEventHandlers.RecordBodyCleaned(PlayerControl.LocalPlayer, body, body.transform.position, 
                    TimeLordBodyManager.CleanedBodySource.Janitor);
            }
            Coroutines.Start(TimeLordBodyManager.CoHideBodyForTimeLord(body));
        }
        else
        {
            Coroutines.Start(body.CoClean());
        }
        Coroutines.Start(CrimeSceneComponent.CoClean(body));
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<TestCleanModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.bodyRenderers.Do(x => x.SetOutline(active ? TextOutlineColor : null));
        }
    }
}

