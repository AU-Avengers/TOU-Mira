using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraAPI.Translation;
using Reactor.Utilities.Extensions;
using TownOfUs.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Duelist;
using TownOfUs.Networking;
using TownOfUs.Roles.Neutral.NeutralOutlier;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral.NeutralOutlier;

public sealed class DuelStrikeButton : TownOfUsTargetButton<PlayerControl>, IKillButton, IDiseaseableButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Ability.Strike");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => DuelistRole.DuelistColor;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DuelStrikeButton;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override float Cooldown => 0f;
    public override float InitialCooldown => 0f;

    public override bool Enabled(RoleBehaviour? role)
    {
        var lp = PlayerControl.LocalPlayer;
        return lp != null && !lp.HasDied() && lp.HasModifier<DuelModifier>();
    }
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }
    public override PlayerControl? GetTarget()
    {
        var lp = PlayerControl.LocalPlayer;
        if (lp == null || !lp.TryGetModifier<DuelModifier>(out var mod))
        {
            return null;
        }

        var opponent = MiscUtils.PlayerById(mod.OpponentId);
        return opponent != null && IsTargetValid(opponent) ? opponent : null;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.HasDied())
        {
            return false;
        }

        var lp = PlayerControl.LocalPlayer;
        if (lp != null && lp.TryGetModifier<DuelModifier>(out var mod))
        {
            return target.PlayerId == mod.OpponentId &&
                   Vector2.Distance(lp.GetTruePosition(), target.GetTruePosition()) <= Distance;
        }
        return false;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        base.FixedUpdateHandler(playerControl);

        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }
        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.cosmetics.currentBodySprite.BodySprite.SetOutline(active ? DuelistRole.DuelistColor : null);
        }
    }

    public override bool CanUse()
    {
        var lp = PlayerControl.LocalPlayer;
        if (lp == null || lp.HasDied())
        {
            return false;
        }
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }
        if (!lp.CanMove)
        {
            return false;
        }

        if (DuelManager.HasStruck(lp.PlayerId) || DuelManager.IsResolved(lp.PlayerId))
        {
            return false;
        }
        return Target != null && Timer <= 0;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }
        OnClick();
        Timer = Cooldown;
    }

    protected override void OnClick()
    {
        var lp = PlayerControl.LocalPlayer;
        if (Target == null || lp == null)
        {
            return;
        }
        if (DuelManager.HasStruck(lp.PlayerId) || DuelManager.IsResolved(lp.PlayerId))
        {
            return;
        }
        DuelistRpc.RpcDuelStrike(lp, Target.PlayerId);
    }
}
