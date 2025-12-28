using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
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
               TutorialManager.InstanceExists &&
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