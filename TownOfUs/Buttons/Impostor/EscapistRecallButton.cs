using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EscapistRecallButton : TownOfUsRoleButton<EscapistRole>, IAftermathableButton
{
    public override string Name => TouLocale.Get("TouRoleEscapistRecall", "Recall");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<EscapistOptions>.Instance.RecallCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<EscapistOptions>.Instance.MaxEscapes;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.RecallSprite;

    public void AftermathHandler()
    {
        ClickHandler();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is not { MarkedLocation: null };
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role is not { MarkedLocation: null };
    }

    protected override void OnClick()
    {
        if (Role.MarkedLocation != null)
        {
            Coroutines.Start(CoRecall(Role.MarkedLocation.Value));
        }
        // TouAudio.PlaySound(TouAudio.EscapistRecallSound);
    }

    private static IEnumerator CoRecall(Vector2 location)
    {
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0.6f, 0.1f, 0.2f, 1f), 11f / 24f);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(location);

        if (ModCompatibility.IsSubmerged())
        {
            ModCompatibility.ChangeFloor(PlayerControl.LocalPlayer.GetTruePosition().y > -7);
            ModCompatibility.CheckOutOfBoundsElevator(PlayerControl.LocalPlayer);
        }

        EscapistRole.RpcRecall(PlayerControl.LocalPlayer);
        yield return HudManager.Instance.CoFadeFullScreen(new Color(0.6f, 0.1f, 0.2f, 1f), Color.clear);
    }
}