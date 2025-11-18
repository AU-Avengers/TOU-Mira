using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Freeplay;

public sealed class SelfKillButton : TownOfUsButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 3;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               TutorialManager.InstanceExists &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    protected override void OnClick()
    {
        // Nothing happens
    }

    public override void OnEffectEnd()
    {
        if (!PlayerControl.LocalPlayer.HasDied())
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
        }
    }
}