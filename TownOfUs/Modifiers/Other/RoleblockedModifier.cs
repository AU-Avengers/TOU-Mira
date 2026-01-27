using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Other;

public sealed class RoleblockedModifier(bool invertControls, bool hangover, float blockDuration, float hangoverDuration) : DisabledModifier, IVisualAppearance
{
    public override string ModifierName => "Roleblocked";
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Barkeeper;
    public override bool Unique => false;
    public override bool CanUseAbilities => false;
    public override bool CanReport => false;
    public override float Duration => blockDuration;
    public float HangoverDuration => hangoverDuration;
    public bool InvertControls => invertControls;
    public bool Hangover => hangover;
    public override bool AutoStart => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        if (InvertControls)
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
        if (!Player.HasDied())
        {
            if (Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You are no longer roleblocked.</color></b>", Color.white,
                    spr: TouRoleIcons.Barkeeper.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
            if (Hangover)
            {
                var autoStart = MeetingHud.Instance == null && ExileController.Instance == null;
                Player.AddModifier<HangoverModifier>(HangoverDuration, autoStart);
            }
        }

        Player.ResetAppearance(fullReset: true);
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }
    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        Player.RemoveModifier(this);
    }
}