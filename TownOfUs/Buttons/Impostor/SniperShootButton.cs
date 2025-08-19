using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class SniperShootButton : TownOfUsRoleButton<SniperRole, PlayerControl>, IKillButton
{
    public override string Name => "Snipe";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<SniperOptions>.Instance.ShootCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<SniperOptions>.Instance.AimDuration;
    public override int MaxUses => OptionGroupSingleton<SniperOptions>.Instance.MaxShots;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public bool Usable { get; set; } = true;

    public override bool CanUse()
    {
        return base.CanUse() && Usable && UsesLeft != 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        // quick flash feedback
        Coroutines.Start(MiscUtils.CoFlash(new Color(1f, 1f, 1f, 1f), 0.1f, 0.15f));

        if (Target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        if (Target.HasModifier<BaseShieldModifier>())
        {
            return;
        }

        // stop movement during aim window
        PlayerControl.LocalPlayer.NetTransform.Halt();

        SniperRole.RpcSniperShot(PlayerControl.LocalPlayer, Target);
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        // full vision (incl. through walls) for aim duration: switch to Ghost layer temporarily
        PlayerControl.LocalPlayer.gameObject.layer = LayerMask.NameToLayer("Ghost");

        OnClick();
        Button?.SetDisabled();
        EffectActive = true;
        Timer = EffectDuration;
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        // restore layer and start cooldown
        PlayerControl.LocalPlayer.gameObject.layer = LayerMask.NameToLayer("Players");
        Timer = Cooldown;
    }

    public override PlayerControl? GetTarget()
    {
        // click any alive player visible on the LOCAL main camera viewport (ignore LOS)
        if (MeetingHud.Instance)
        {
            return null;
        }

        var cam = Camera.main;
        var alive = Helpers.GetAlivePlayers();
        SetOutline(false);

        PlayerControl? best = null;
        float bestScore = float.MaxValue;
        foreach (var p in alive)
        {
            if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                continue;
            }

            if (cam == null)
            {
                continue;
            }

            var vp = cam.WorldToViewportPoint(p.GetTruePosition());
            if (!(vp.z > 0 && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f))
            {
                continue;
            }

            var score = (p.GetTruePosition() - PlayerControl.LocalPlayer.GetTruePosition()).sqrMagnitude;
            if (score < bestScore)
            {
                bestScore = score;
                best = p;
            }
        }

        Target = best;
        SetOutline(true);
        return Target;
    }
}

