using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EscapistMarkButton : TownOfUsRoleButton<EscapistRole>, IAftermathableButton
{
    public override string Name => TouLocale.Get("TouRoleEscapistMark", "Mark");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.MarkSprite;

    public void AftermathHandler()
    {
        ClickHandler();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is { MarkedLocation: null };
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role is { MarkedLocation: null };
    }

    protected override void OnClick()
    {
        EscapistRole.RpcMarkLocation(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.transform.position);

        // TouAudio.PlaySound(TouAudio.EscapistMarkSound);
        CustomButtonSingleton<EscapistRecallButton>.Instance.SetActive(true, Role);
        CustomButtonSingleton<EscapistRecallButton>.Instance.ResetCooldownAndOrEffect();
        SetActive(false, Role);
    }
}