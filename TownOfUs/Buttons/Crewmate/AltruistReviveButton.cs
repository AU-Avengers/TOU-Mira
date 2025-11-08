using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
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
    public override int MaxUses => (int)OptionGroupSingleton<AltruistOptions>.Instance.MaxRevives;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ReviveSprite;

    public bool RevivedInRound { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value is not ReviveType.Sacrifice;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterBodySprite.LoadAsset();
    }

    public override bool CanUse()
    {
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

    protected override void OnClick()
    {
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

        OverrideName(TouLocale.Get("TouRoleAltruistReviving", "Reviving"));
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
            player.RpcCustomMurder(player);
        }
    }
}