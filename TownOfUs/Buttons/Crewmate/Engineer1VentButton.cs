using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class EngineerVentButton : TownOfUsRoleButton<EngineerTouRole, Vent>
{
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VentLabel, "Vent");
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => TownOfUsColors.Engineer;

    public override float Cooldown =>
        OptionGroupSingleton<EngineerOptions>.Instance.VentCooldown + 0.001f + MapCooldown;

    public override float EffectDuration => OptionGroupSingleton<EngineerOptions>.Instance.VentDuration;
    public override int MaxUses => (int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.EngiVentSprite;
    public override bool ShouldPauseInVent => false;
    public int ExtraUses { get; set; }

    public override Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 4, Filter);
        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 3, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 2, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);
        }

        if (vent != null && PlayerControl.LocalPlayer.CanUseVent(vent))
        {
            return vent;
        }

        return null;
    }

    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            Target?.SetOutline(false, false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !PlayerControl.LocalPlayer.inVent && Target != null) ||
                PlayerControl.LocalPlayer.inVent) && (MaxUses == 0 || UsesLeft > 0);
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
            // Logger<TownOfUsPlugin>.Error($"Effect is No Longer Active");
            // Logger<TownOfUsPlugin>.Error($"Cooldown is active");
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
            // Logger<TownOfUsPlugin>.Error($"Effect is Now Active");
        }
        else
        {
            Timer = !PlayerControl.LocalPlayer.inVent ? 0.001f : Cooldown;
            // Logger<TownOfUsPlugin>.Error($"Cooldown is active");
        }
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            // Logger<TownOfUsPlugin>.Error($"Entering Vent");
            if (Target != null)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
            }
            // else Logger<TownOfUsPlugin>.Error($"Vent is null...");
        }
        else if (Timer != 0)
        {
            // Logger<TownOfUsPlugin>.Error($"Leaving Vent");
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
        if (PlayerControl.LocalPlayer.inVent)
        {
            // Logger<TownOfUsPlugin>.Error($"Left Vent");
            Vent.currentVent.SetButtons(false);
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            UsesLeft--;
            if (MaxUses != 0)
            {
                Button?.SetUsesRemaining(UsesLeft);
            }
        }
    }
}