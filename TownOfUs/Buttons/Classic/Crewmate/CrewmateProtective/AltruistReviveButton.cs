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

public sealed class AltruistReviveButton : TownOfUsRoleButton<AltruistRole>
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
        if (mode is ReviveType.Sacrifice)
        {
            return false;
        }

        if (role is AltruistRole)
        {
            return true;
        }

        // If we sacrificed at the start, we become a ghost role.
        // Keep the button alive while the effect is running so the animation can continue as a ghost.
        if (!EffectActive || !OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value)
        {
            return false;
        }

        return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.GetRoleWhenAlive() is AltruistRole;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterBodySprite.LoadAsset();
        UpdateUsesCounterVisibility();
    }

    private void UpdateUsesCounterVisibility()
    {
        if (Button == null) return;
        
        // Hide uses counter if KillOnStartRevive is enabled
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

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return false;
        }
        // When sacrificed at the start, the button should remain visible for the animation,
        // but should NOT be clickable while dead.
        if (PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }

        if (RevivedInRound)
        {
            return false;
        }

        var bodiesInRange = Helpers.GetNearestDeadBodies(
            PlayerControl.LocalPlayer.transform.position,
            OptionGroupSingleton<AltruistOptions>.Instance.ReviveRange.Value * ShipStatus.Instance.MaxLightRadius,
            Helpers.CreateFilter(Constants.NotShipMask));

        return base.CanUse() && bodiesInRange.Count > 0;
    }

    public override void ClickHandler()
    {
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        base.ClickHandler();
    }

    protected override void OnClick()
    {
        var killOnStart = OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value;

        var bodiesInRange = Helpers.GetNearestDeadBodies(
            PlayerControl.LocalPlayer.transform.position,
            OptionGroupSingleton<AltruistOptions>.Instance.ReviveRange.Value * ShipStatus.Instance.MaxLightRadius,
            Helpers.CreateFilter(Constants.NotShipMask));

        var playersToRevive = bodiesInRange.Select(x => x.ParentId).ToList();

        foreach (var playerId in playersToRevive)
        {
            var player = MiscUtils.PlayerById(playerId);
            if (player != null)
            {
                if (player.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie)
                {
                    var other = player.GetModifier<LoverModifier>()!.GetOtherLover;
                    if (!playersToRevive.Contains(other()!.PlayerId) && other()!.Data.IsDead)
                    {
                        AltruistRole.RpcRevive(PlayerControl.LocalPlayer, other()!);
                    }
                }

                AltruistRole.RpcRevive(PlayerControl.LocalPlayer, player);
            }
        }

        // Kill altruist right after starting the revive (small delay ensures the revive RPCs are dispatched first)
        if (killOnStart)
        {
            Coroutines.Start(CoKillOnStart(PlayerControl.LocalPlayer));
        }

        OverrideName(TouLocale.Get("TouRoleAltruistReviving", "Reviving"));
    }

    public static IEnumerator CoKillOnStart(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        if (MeetingHud.Instance == null && ExileController.Instance == null && !player.HasDied())
        {
            player.RpcCustomMurder(player, MeetingCheck.OutsideMeeting, showKillAnim: false, createDeadBody: true);
        }
    }

    public override void OnEffectEnd()
    {
        RevivedInRound = true;
        OverrideName(TouLocale.Get("TouRoleAltruistRevive", "Revive"));
        if ((ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value is ReviveType.GroupSacrifice)
        {
            Coroutines.Start(CoSacrifite(PlayerControl.LocalPlayer));
        }
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