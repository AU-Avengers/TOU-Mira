using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Rpc;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Networking;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class DisperseButton : TownOfUsButton
{
    public override string Name => TouLocale.Get("TouModifierDisperserDisperse", "Disperse");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f + MapCooldown;
    public override int MaxUses => 1;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.DisperseSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<DisperserModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterVentSprite.LoadAsset();
    }

    protected override void OnClick()
    {
        var coords = DisperserModifier.GenerateDisperseCoordinates();

        Rpc<DisperseRpc>.Instance.Send(PlayerControl.LocalPlayer, coords);
    }
}