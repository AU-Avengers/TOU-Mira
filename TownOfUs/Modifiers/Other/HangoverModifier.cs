using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Other;

public sealed class HangoverModifier(float duration, bool startTimer) : TimedModifier
{
    public override string ModifierName => TouLocale.Get("TouModifierHangover");
    public override bool HideOnUi => false;
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Barkeeper;
    public override bool Unique => false;
    public override float Duration => duration;
    public override bool AutoStart => startTimer;

    public override string GetDescription()
    {
        return TouLocale.Get("TouModifierHangoverDescription");
    }

    public override void OnActivate()
    {
        if (Player.AmOwner && AutoStart)
        {
            var notif = Helpers.CreateAndShowNotification(
                    $"<b>{TouLocale.Get("TouModifierHangoverStartNotification")}</color></b>", Color.white,
                    spr: TouRoleIcons.Barkeeper.LoadAsset());

            notif.Text.SetOutlineThickness(0.35f);
            notif.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner && !Player.HasDied())
        {
            var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.Get("TouModifierHangoverEndNotification")}</color></b>", Color.white,
            spr: TouRoleIcons.Barkeeper.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
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