using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class AltruistSacrificeButton : TownOfUsRoleButton<AltruistRole, DeadBody>
{
    public override string Name => TouLocale.GetParsed("TouRoleAltruistRevive", "Revive");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Altruist;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override float EffectDuration => OptionGroupSingleton<AltruistOptions>.Instance.ReviveDuration.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override int MaxUses => OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value
        ? 0
        : (int)OptionGroupSingleton<AltruistOptions>.Instance.MaxRevives;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ReviveSprite;
    public override bool UsableInDeath => true;

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer?.GetNearestDeadBody(PlayerControl.LocalPlayer.MaxReportDistance / 4f);
    }

    public bool RevivedInRound { get; set; }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        var killOnStart = OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value;
        var shouldShowWhenDead = killOnStart && EffectActive;

        if (shouldShowWhenDead)
        {
            Button?.ToggleVisible(true);
            UpdateUsesCounterVisibility();
            return;
        }

        Button?.ToggleVisible(visible && Enabled(role) && !role.Player.HasDied());
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        var mode = (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value;
        if (mode is not ReviveType.Sacrifice)
        {
            return false;
        }

        if (role is AltruistRole)
        {
            return true;
        }

        if (!EffectActive || !OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value)
        {
            return false;
        }

        return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.GetRoleWhenAlive() is AltruistRole;
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }

        if (RevivedInRound)
        {
            return false;
        }

        return base.CanUse() && Target != null;
    }

    public override void ClickHandler()
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        base.ClickHandler();
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        UpdateUsesCounterVisibility();
    }

    private void UpdateUsesCounterVisibility()
    {
        if (Button == null) return;
        
        if (OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value)
        {
            Button.usesRemainingText.gameObject.SetActive(false);
            Button.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            Button.usesRemainingText.gameObject.SetActive(true);
            Button.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    public override void SetUses(int amount)
    {
        base.SetUses(amount);
        UpdateUsesCounterVisibility();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var killOnStart = OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value;

        var player = MiscUtils.PlayerById(Target.ParentId);
        if (player != null)
        {
            if (player.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie)
            {
                var other = player.GetModifier<LoverModifier>()!.GetOtherLover;
                AltruistRole.RpcRevive(PlayerControl.LocalPlayer, other()!);
            }

            AltruistRole.RpcRevive(PlayerControl.LocalPlayer, player);
        }

        if (killOnStart)
        {
            Coroutines.Start(AltruistReviveButton.CoKillOnStart(PlayerControl.LocalPlayer));
        }

        OverrideName(TouLocale.Get("TouRoleAltruistReviving", "Reviving"));
    }

    public override void OnEffectEnd()
    {
        RevivedInRound = true;
        OverrideName(TouLocale.Get("TouRoleAltruistRevive", "Revive"));
        Coroutines.Start(CoSacrifite(PlayerControl.LocalPlayer));
    }

    public static IEnumerator CoSacrifite(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        if (MeetingHud.Instance == null && ExileController.Instance == null && !player.HasDied())
        {
            player.RpcCustomMurder(player, MeetingCheck.OutsideMeeting, showKillAnim: false, createDeadBody: false);
        }
    }
}