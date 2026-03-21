using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Impostor;

public sealed class HypnotisedModifier(PlayerControl hypnotist) : BaseModifier
{
    public override string ModifierName => "Hypnotised";
    public override bool HideOnUi => true;
    public PlayerControl Hypnotist { get; } = hypnotist;

    public bool HysteriaActive { get; set; }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.HypnotistHypno, Hypnotist, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        UnHysteria();
        Player.MyPhysics.SetForcedBodyType(PlayerControl.LocalPlayer.BodyType);
    }

    public void Hysteria()
    {
        if (Player.HasDied())
        {
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.HypnotistHysteria, Hypnotist, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        if (!Player.AmOwner)
        {
            return;
        }

        if (HysteriaActive)
        {
            return;
        }

        // Message($"HypnotisedModifier.Hysteria - {Player.Data.PlayerName}");
        var players = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player).ToList();

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

        PlayerControl.LocalPlayer.MyPhysics.SetForcedBodyType(localBodyShape);

        foreach (var player in players)
        {
            player.MyPhysics.SetForcedBodyType(bodyShape);
            var hidden = Random.RandomRangeInt(0, 4);
            player.AddModifier<HypnotistHysteriaModifier>(bodyShape, hidden);
        }

        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You are under a Mass Hysteria!</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Hypnotist.LoadAsset());

            notif1.AdjustNotification();
        }

        HysteriaActive = true;
    }

    public void UnHysteria()
    {
        if (!HysteriaActive)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        // Message($"HypnotisedModifier.UnHysteria - {Player.Data.PlayerName}");
        ModifierUtils.GetActiveModifiers<HypnotistHysteriaModifier>().Do(x => x.Player.RemoveModifier(x));

        HysteriaActive = false;
    }
}