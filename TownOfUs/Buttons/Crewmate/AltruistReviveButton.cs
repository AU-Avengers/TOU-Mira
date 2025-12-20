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

public sealed class AltruistReviveButton : TownOfUsRoleButton<AltruistRole, DeadBody>
{
    public override string Name => TouLocale.GetParsed("TouRoleAltruistRevive", "Revive");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Altruist;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override float EffectDuration => OptionGroupSingleton<AltruistOptions>.Instance.ReviveDuration.Value;
    public override int MaxUses => OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value 
        ? 0 
        : (int)OptionGroupSingleton<AltruistOptions>.Instance.MaxRevives;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ReviveSprite;
    public override bool UsableInDeath => true;

    public bool RevivedInRound { get; set; }

    public override DeadBody? GetTarget()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return null;
        }

        var mode = (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value;
        if (mode is ReviveType.Sacrifice)
        {
            return PlayerControl.LocalPlayer.GetNearestDeadBody(PlayerControl.LocalPlayer.MaxReportDistance / 4f);
        }

        var reviveRange =
            OptionGroupSingleton<AltruistOptions>.Instance.ReviveRange.Value * ShipStatus.Instance.MaxLightRadius;
        return PlayerControl.LocalPlayer.GetNearestDeadBody(reviveRange);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
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

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        
        if (OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value)
        {
            Button?.usesRemainingText.gameObject.SetActive(false);
            Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        var killOnStart = OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value;
        var shouldShowWhenDead = killOnStart && EffectActive;
        
        Button?.ToggleVisible(visible && Enabled(role) && (!role.Player.HasDied() || shouldShowWhenDead));
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

        var mode = (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value;
        if (mode is ReviveType.Sacrifice)
        {
            // Base targeted-button logic (plus our dead/round guards above).
            return base.CanUse() && Target != null;
        }

        var bodiesInRange = Helpers.GetNearestDeadBodies(
            PlayerControl.LocalPlayer!.transform.position,
            OptionGroupSingleton<AltruistOptions>.Instance.ReviveRange.Value * ShipStatus.Instance.MaxLightRadius,
            Helpers.CreateFilter(Constants.NotShipMask));

        return base.CanUse() && bodiesInRange.Count > 0;
    }

    protected override void OnClick()
    {
        var mode = (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value;

        if (mode is ReviveType.Sacrifice)
        {
            if (Target == null)
            {
                return;
            }

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
        }
        else
        {
            var bodiesInRange = Helpers.GetNearestDeadBodies(
                PlayerControl.LocalPlayer!.transform.position,
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
        }

        OverrideName(TouLocale.Get("TouRoleAltruistReviving", "Reviving"));
    }

    public override void OnEffectEnd()
    {
        RevivedInRound = true;
        OverrideName(TouLocale.Get("TouRoleAltruistRevive", "Revive"));

        var mode = (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value;
        if ((mode is ReviveType.Sacrifice or ReviveType.GroupSacrifice) &&
            !OptionGroupSingleton<AltruistOptions>.Instance.KillOnStartRevive.Value)
        {
            Coroutines.Start(CoSacrifite(PlayerControl.LocalPlayer));
        }
    }

    public static IEnumerator CoSacrifite(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        if (MeetingHud.Instance == null && ExileController.Instance == null && !player.HasDied())
        {
            player.RpcCustomMurder(player, showKillAnim: false, createDeadBody: false);
        }
    }
}