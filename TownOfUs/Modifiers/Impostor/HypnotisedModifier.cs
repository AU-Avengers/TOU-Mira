using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Impostor;

public sealed class HypnotisedModifier(PlayerControl hypnotist) : BaseModifier
{
    public override string ModifierName => "Hypnotised";
    public override bool HideOnUi => true;
    public PlayerControl Hypnotist { get; } = hypnotist;

    public bool HysteriaActive { get; set; }
    public bool HysteriaNotificationShown { get; set; }

    public override void OnDeath(DeathReason reason)
    {
        RemoveSelf();
    }

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.HypnotistHypno, Hypnotist, Player);
        touAbilityEvent.InvokeEvent();
    }

    public override void OnDeactivate()
    {
        UnHysteria();
        Player.MyPhysics.SetForcedBodyType(PlayerControl.LocalPlayer.BodyType);
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();

        if (Player.AmOwner)
        {
            UnHysteria();
        }
    }

    public void Hysteria()
    {
        if (Player.HasDied())
        {
            return;
        }

        if (Hypnotist == null || Hypnotist.HasDied() || Hypnotist.Data.Disconnected || Hypnotist.Data.Role is not HypnotistRole)
        {
            RemoveSelf();
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.HypnotistHysteria, Hypnotist, Player);
        touAbilityEvent.InvokeEvent();
        if (!Player.AmOwner)
        {
            return;
        }

        if (HysteriaActive)
        {
            return;
        }

        var players = PlayerControl.AllPlayerControls.Where(x => !x.HasDied() && x != Player);

        var bodyType = Random.RandomRangeInt(0, 10);
        var bodyShape = PlayerBodyTypes.Normal;
        var localBodyShape = PlayerBodyTypes.Normal;

        if (bodyType == 1)
        {
            bodyShape = PlayerBodyTypes.Horse;
            localBodyShape = PlayerBodyTypes.Horse;
        }
        else if (bodyType == 2)
        {
            bodyShape = PlayerBodyTypes.LongSeeker;
            localBodyShape = PlayerBodyTypes.Long;
        }
        else if (bodyType == 3)
        {
            bodyShape = PlayerBodyTypes.Long;
            localBodyShape = PlayerBodyTypes.Long;
        }
        else if (bodyType == 4)
        {
            bodyShape = PlayerBodyTypes.Seeker;
        }
        else if (bodyType == 5)
        {
            localBodyShape = PlayerBodyTypes.Classic;
            bodyShape = PlayerBodyTypes.Classic;
        }

        PlayerControl.LocalPlayer.MyPhysics.SetForcedBodyType(localBodyShape);

        foreach (var player in players)
        {
            if (player.HasModifier<HypnotistHysteriaModifier>())
            {
                player.RemoveModifier<HypnotistHysteriaModifier>();
            }

            player.MyPhysics.SetForcedBodyType(bodyShape);
            var hidden = Random.RandomRangeInt(0, 4);
            player.AddModifier<HypnotistHysteriaModifier>(bodyShape, hidden);
        }

        if (Player.AmOwner && !HysteriaNotificationShown)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Modifier.MassHysteriaNotification")}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hypnotist);

            notif1.AdjustNotification();
            HysteriaNotificationShown = true;
        }

        HysteriaActive = true;
    }

    public void UnHysteria()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        ModifierUtils.GetActiveModifiers<HypnotistHysteriaModifier>().Do(x => x.Player.RemoveModifier(x));
        PlayerControl.LocalPlayer.MyPhysics.SetForcedBodyType(PlayerControl.LocalPlayer.BodyType);

        HysteriaActive = false;
    }

    public IEnumerator CoUnHysteria()
    {
        UnHysteria();
        yield break;
    }
}
