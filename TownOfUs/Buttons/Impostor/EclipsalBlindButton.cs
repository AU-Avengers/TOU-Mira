using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EclipsalBlindButton : TownOfUsRoleButton<EclipsalRole>, IAftermathableButton
{
    public override string Name => TouLocale.Get("TouRoleEclipsalBlind", "Blind");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<EclipsalOptions>.Instance.BlindCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<EclipsalOptions>.Instance.BlindDuration;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.BlindSprite;

    public void AftermathHandler()
    {
        ClickHandler();
    }

    protected override void OnClick()
    {
        OverrideName(TouLocale.Get("TouRoleEclipsalUnblinding", "Unblinding"));
        var blindRadius = OptionGroupSingleton<EclipsalOptions>.Instance.BlindRadius;
        var blindedPlayers =
            Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, blindRadius * ShipStatus.Instance.MaxLightRadius);

        foreach (var player in blindedPlayers.Where(x => !x.HasDied() && !x.IsImpostor()))
        {
            player.RpcAddModifier<EclipsalBlindModifier>(PlayerControl.LocalPlayer);
        }
        // PlayerControl.LocalPlayer.RpcAddModifier<EclipsalBlindModifier>(PlayerControl.LocalPlayer);
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.Get("TouRoleEclipsalBlind", "Blind"));
    }
}