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

public sealed class AltruistSacrificeButton : TownOfUsRoleButton<AltruistRole, DeadBody>
{
    public override string Name => TouLocale.GetParsed("TouRoleAltruistRevive", "Revive");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Altruist;
    public override float Cooldown => Math.Clamp(MapCooldown, 0.001f, 120f);
    public override float EffectDuration => OptionGroupSingleton<AltruistOptions>.Instance.ReviveDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<AltruistOptions>.Instance.MaxRevives;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ReviveSprite;

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer?.GetNearestDeadBody(PlayerControl.LocalPlayer.MaxReportDistance / 4f);
    }

    public bool RevivedInRound { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && (ReviveType)OptionGroupSingleton<AltruistOptions>.Instance.ReviveMode.Value is ReviveType.Sacrifice;
    }

    protected override void OnClick()
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
            player.RpcCustomMurder(player);
        }
    }
}