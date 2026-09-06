using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Buttons.Classic;

public sealed class GhostwalkerVentButton : TownOfUsVentButton, ILegacyCapable
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VentLabel, "Vent");
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => TownOfUsColors.Ghostwalker;

    public override float Cooldown =>
        0.001f;
    public override float InitialCooldown =>
        0.001f;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyVanillaAssets.VentSprite : TouAssets.GhostwalkerVentSprite;
    public override bool ShouldPauseInVent => false;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is IGhostRole { GhostActive: true } && PlayerControl.LocalPlayer.inVent;
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            return;
        }

        Vent.currentVent.SetButtons(false);
        Vent toExit = Vent.currentVent;
        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(toExit.Id);
    }
}
