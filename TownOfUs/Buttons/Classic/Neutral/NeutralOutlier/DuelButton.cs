using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral.NeutralOutlier;
using TownOfUs.Modules.Duelist;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral.NeutralOutlier;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral.NeutralOutlier;

public sealed class DuelButton : TownOfUsRoleButton<DuelistRole>, IDiseaseableButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.Duelist.Ability.Duel");
    public override float Cooldown => OptionGroupSingleton<DuelistOptions>.Instance.DuelCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DuelistDuelButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => DuelistRole.DuelistColor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is DuelistRole { VictoryPending: false };
    }

    private static bool IsValidTarget(PlayerControl? plr, PlayerControl me) =>
        plr != null && plr.Data != null && !plr.Data.Disconnected && !plr.HasDied()
        && plr.PlayerId != me.PlayerId && !plr.HasModifier<DuelModifier>()
        && !plr.HasModifier<FirstDeadShield>()
        && plr.PlayerId != me.GetModifier<LoverModifier>()?.OtherLover?.PlayerId;

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }
        if (player.HasModifier<DuelModifier>())
        {
            return false;
        }
        if (Role != null && Role.VictoryPending)
        {
            return false;
        }
        if (!base.CanUse())
        {
            return false;
        }
        return HasAnyTarget(player) && Timer <= 0;
    }
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }
    private static bool HasAnyTarget(PlayerControl me)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (IsValidTarget(p, me))
            {
                return true;
            }
        }
        return false;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.HasModifier<GlitchHackedModifier>())
        {
            return;
        }
        if (Minigame.Instance != null)
        {
            return;
        }

        OpenTargetMenu(player);
    }

    protected override void OnClick()
    {
    }

    private void OpenTargetMenu(PlayerControl player)
    {
        var menu = CustomPlayerMenu.Create();
        menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            player.cosmetics.currentBodySprite.BodySprite.material;
        menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            player.cosmetics.currentBodySprite.BodySprite.material;

        menu.Begin(
            plr => IsValidTarget(plr, player),
            plr =>
            {
                menu.ForceClose();

                if (plr == null || !IsValidTarget(plr, player))
                {
                    return;
                }

                if (!DuelManager.TryGetDuelDestinations(player, plr, out var duelistDest, out var targetDest))
                {
                    return;
                }

                DuelistRpc.RpcStartDuel(player, plr.PlayerId, duelistDest, targetDest,
                    player.transform.position, plr.transform.position);
            });
    }
}
