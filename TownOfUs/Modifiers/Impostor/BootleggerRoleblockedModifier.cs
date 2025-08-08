using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class BootleggerRoleblockedModifier : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Roleblocked";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Barkeeper;
    public override bool Unique => false;
    public override bool CanUseAbilities => false;
    public override bool CanReport => false;
    public override bool AutoStart => true;
    public override float Duration => OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        if (OptionGroupSingleton<BootleggerOptions>.Instance.Hangover)
        {
            appearance.Speed *= -1;
            appearance.PetId = string.Empty;
        }

        return appearance;
    }
    public override string GetDescription()
    {
        return $"Someone gave you a drink, you are roleblocked!";
    }
    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }
    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
            $"<b>You are no longer roleblocked.</color></b>", Color.white,
            spr: TouRoleIcons.Barkeeper.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        Player.ResetAppearance(fullReset: true);

        if (!OptionGroupSingleton<BootleggerOptions>.Instance.Hangover) Player.AddModifier<BootleggerHangoverModifier>();
    }

}