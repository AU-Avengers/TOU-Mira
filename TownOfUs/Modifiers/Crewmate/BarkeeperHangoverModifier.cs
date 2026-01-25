using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class BarkeeperHangoverModifier : TimedModifier
{
    public override string ModifierName => "Hangover";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Bootlegger;
    public override bool Unique => false;
    public override float Duration => (int)OptionGroupSingleton<BarkeeperOptions>.Instance.HangoverDuration;
    public override bool AutoStart => true;

    public override string GetDescription()
    {
        return $"You are having a hangover, you are temporarily immune to being roleblocked.";
    }

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                    $"<b>You are now hungover!</color></b>", Color.white,
                    spr: TouRoleIcons.Bootlegger.LoadAsset());

            notif.Text.SetOutlineThickness(0.35f);
            notif.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
    }
    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
            $"<b>You are no longer hungover.</color></b>", Color.white,
            spr: TouRoleIcons.Bootlegger.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
    }
}