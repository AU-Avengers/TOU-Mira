using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace TownOfUs.Buttons;

public sealed class FakeVentButton : CustomActionButton
{
    public override string Name => " ";
    public override Color TextOutlineColor => Color.clear;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouAssets.BlankSprite;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public bool Show { get; set; } = true;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        var pb = Button?.GetComponent<PassiveButton>();
        if (pb != null)
        {
            pb.OnClick = new Button.ButtonClickedEvent();
        }

        Button!.TryCast<AbilityButton>()!
            .commsDown
            .GetComponent<SpriteRenderer>()
            .sprite = Sprite.LoadAsset();

        SetButtonLocation(ButtonLocation.BottomLeft);
        SetButtonLocation(ButtonLocation.BottomRight);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && role != null &&
               LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.OffsetButtonsToggle.Value && Show &&
               HudManager.InstanceExists && !MeetingHud.Instance &&
               !role.IsImpostor && (!role.CanVent || (role is ICustomRole customRole && !customRole.Configuration.CanUseVent));
    }

    protected override void OnClick()
    {
    }
}