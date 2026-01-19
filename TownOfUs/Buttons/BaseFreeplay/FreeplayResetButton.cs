using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Rpc;
using TownOfUs.Networking;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Buttons.BaseFreeplay;

public sealed class FreeplayResetButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("FreeplayRestartButton", "Reset Game");
    public override Color TextOutlineColor => new Color32(165, 231, 89, 255);
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.FreeplayResetSprite;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               (TutorialManager.InstanceExists || MultiplayerFreeplayMode.Enabled) &&
               !FreeplayButtonsVisibility.Hidden;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (MultiplayerFreeplayMode.Enabled)
        {
            Rpc<MultiplayerFreeplayRequestRpc>.Instance.Send(
                PlayerControl.LocalPlayer,
                new MultiplayerFreeplayRequest(MultiplayerFreeplayAction.Reset, 0, 0, 0));
            return;
        }

        HudManager.Instance.ShowPopUp(TouLocale.GetParsed("FreeplayRestartPopup"));
        ShipStatus.Instance.Begin();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReviveEveryoneFreeplay();
        }

        // Restore Freeplay state (roles/modifiers/etc.) back to the original baseline.
        FreeplayDebugState.RestoreBaseline();
    }
}