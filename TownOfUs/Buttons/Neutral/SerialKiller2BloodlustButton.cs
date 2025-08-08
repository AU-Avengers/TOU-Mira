using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SerialKiller2BloodlustButton : TownOfUsRoleButton<SerialKillerRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => "Kill";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SerialKiller;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.SerialKillerKillSprite;
    public override float Cooldown => 0.001f;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("SerialKiller Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        PlayerControl.LocalPlayer.RemoveModifier<SerialKillerBloodlustModifier>();


    }

    public override bool CanUse()
    {
        return base.CanUse() && PlayerControl.LocalPlayer.HasModifier<SerialKillerBloodlustModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}