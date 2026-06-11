using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class EngineerVentButton : TownOfUsRoleButton<EngineerTouRole, Vent>, ILegacyCapable
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VentLabel, "Vent");
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => TownOfUsColors.Engineer;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<EngineerOptions>.Instance.VentCooldown + MapCooldown, 0.001f, 120f);

    public override float EffectDuration => OptionGroupSingleton<EngineerOptions>.Instance.VentDuration;
    public override int MaxUses => (int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyVanillaAssets.VentSprite : TouCrewAssets.EngiVentSprite;
    public override bool ShouldPauseInVent => false;
    public int ExtraUses { get; set; }

    public override Vent? GetTarget()
    {
        return HudManager.Instance.ImpostorVentButton.currentTarget;
    }

    public override bool IsEffectCancellable()
    {
        return PlayerControl.LocalPlayer.inVent;
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
            (EffectActive ||
             Timer <= 0 && !PlayerControl.LocalPlayer.inVent);
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
            // Error($"Effect is No Longer Active");
            // Error($"Cooldown is active");
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
            // Error($"Effect is Now Active");
        }
        else
        {
            Timer = !PlayerControl.LocalPlayer.inVent ? 0.001f : Cooldown;
            // Error($"Cooldown is active");
        }
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            // Error($"Entering Vent");
            if (Target != null)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
            }
            // else Error($"Vent is null...");
        }
        else if (Timer != 0)
        {
            // Error($"Leaving Vent");
            OnEffectEnd();
            if (!HasEffect)
            {
                EffectActive = false;
                Timer = Cooldown;
            }
        }
    }

    public override void OnEffectEnd()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            return;
        }

        // Error($"Left Vent");
        _ = Vent.currentVent.CanUse(PlayerControl.LocalPlayer.Data, out _, out var couldUse);
        Vent.currentVent.SetButtons(false);

        Vent toExit = Vent.currentVent;

        if (!couldUse)
        {
            Error($"Current vent cannot be exited, finding alternate route.");
            Vent? newVent = null;
            foreach (var closeVent in Vent.currentVent.NearbyVents)
            {
                if (newVent != null)
                {
                    break;
                }
                var @event = new PlayerCanUseEvent(closeVent.Cast<IUsable>());
                MiraEventManager.InvokeEvent(@event);

                if (!@event.IsCancelled)
                {
                    newVent = closeVent;
                }
            }

            if (newVent != null)
            {
                toExit = newVent;
            }
        }

        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(toExit.Id);

        UsesLeft--;
        if (LimitedUses)
        {
            Button?.SetUsesRemaining(UsesLeft);
        }
    }
}