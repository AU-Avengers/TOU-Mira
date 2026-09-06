using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class JesterVentButton : TownOfUsVentRoleButton<JesterRole>, ILegacyCapable
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VentLabel, "Vent");
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => TownOfUsColors.Jester;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<JesterOptions>.Instance.VentCooldown.Value + MapCooldown, 0.001f, 120f);

    public override float EffectDuration => OptionGroupSingleton<JesterOptions>.Instance.VentDuration.Value;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyVanillaAssets.VentSprite : TouNeutAssets.JesterVentSprite;
    public override bool ShouldPauseInVent => false;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<JesterOptions>.Instance.CanVent.Value;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = !PlayerControl.LocalPlayer.inVent ? 0.001f : Cooldown;
        }
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            if (Target != null)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
            }
        }
        else if (Timer != 0)
        {
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
    }
}
