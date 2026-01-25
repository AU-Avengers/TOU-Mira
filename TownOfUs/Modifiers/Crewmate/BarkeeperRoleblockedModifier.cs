using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class BarkeeperRoleblockedModifier : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Roleblocked";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Barkeeper;
    public override bool Unique => false;
    public override bool CanUseAbilities => false;
    public override bool CanReport => false;
    public override float Duration => OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDuration;
    public override bool AutoStart => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        if (OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover)
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

        if (!OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover) Player.AddModifier<BarkeeperHangoverModifier>();
    }

}