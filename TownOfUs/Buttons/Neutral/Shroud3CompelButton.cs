using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ShroudCompelButton : TownOfUsRoleButton<ShroudRole>
{
    public override string Name => "Compel";
    public override string Keybind => "tou.ActionCustom";
    public override Color TextOutlineColor => TownOfUsColors.Shroud;
    public override float Cooldown => OptionGroupSingleton<ShroudOptions>.Instance.CompelCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.CompelSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    protected override void OnClick()
    {
        var distance = PlayerControl.LocalPlayer.Data.Role.GetAbilityDistance();
        var enshrouded = Role.EnshroudedTarget;
        if (enshrouded == null) return;

        var victim = enshrouded.GetClosestLivingPlayer(true, distance);
        if (victim == null) return;

        // PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());

        ShroudRole.RpcCompel(enshrouded, victim);
        
        Role.EnshroudedTarget = null;
    }
    public override bool CanUse()
    {
        var distance = PlayerControl.LocalPlayer.Data.Role.GetAbilityDistance();
        var enshrouded = Role.EnshroudedTarget;

        if (enshrouded == null || enshrouded.Data == null || enshrouded.Data.IsDead)
            return false;

        var victim = enshrouded.GetClosestLivingPlayer(true, distance);
        if (victim == null)
            return false;

        return base.CanUse();
    }



}
