using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
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

        // Logger<TownOfUsPlugin>.Message($"HypnotisedModifier.Hysteria - {Player.Data.PlayerName}");
        var players = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player).ToList();
        
        var bodyType = Random.RandomRangeInt(0, 10);
        var bodyShape = PlayerBodyTypes.Normal;
            
        if (bodyType == 1)
        {
            bodyShape = PlayerBodyTypes.Horse;
        }
        else if (bodyType == 2)
        {
            bodyShape = PlayerBodyTypes.LongSeeker;
        }
        else if (bodyType == 3)
        {
            bodyShape = PlayerBodyTypes.Long;
        }
        else if (bodyType == 4)
        {
            bodyShape = PlayerBodyTypes.Seeker;
        }

        Player.MyPhysics.SetForcedBodyType(bodyShape);
        
        foreach (var player in players)
        {
            player.MyPhysics.SetForcedBodyType(bodyShape);
            var hidden = Random.RandomRangeInt(0, 4);
            if (hidden == 0)
            {
                var morph = new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);
                
                player.RawSetAppearance(morph);
                if (bodyShape is PlayerBodyTypes.Seeker)
                {
                    var seeker = new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph)
                    {
                        HatId = string.Empty,
                        SkinId = string.Empty,
                        VisorId = string.Empty,
                        PlayerName = string.Empty,
                        PetId = string.Empty
                    };

                    player.RawSetAppearance(seeker);
                }
            }
            else if (hidden == 1)
            {
                player.SetCamouflage();
            }
            else
            {
                var swoop = new VisualAppearance(player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
                {
                    HatId = string.Empty,
                    SkinId = string.Empty,
                    VisorId = string.Empty,
                    PlayerName = string.Empty,
                    PetId = string.Empty,
                    RendererColor = new Color(0f, 0f, 0f, 0.1f),
                    NameColor = Color.clear,
                    ColorBlindTextColor = Color.clear
                };

                player.RawSetAppearance(swoop);
            }

            player?.cosmetics.ToggleNameVisible(false);
        }

        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You are under a Mass Hysteria!</color></b>", Color.white,
                spr: TouRoleIcons.Hypnotist.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
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

        // Logger<TownOfUsPlugin>.Message($"HypnotisedModifier.UnHysteria - {Player.Data.PlayerName}");
        foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied()))
        {
            player.MyPhysics.SetForcedBodyType(PlayerControl.LocalPlayer.BodyType);
            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                continue;
            }
            player.RawSetAppearance(player.GetDefaultModifiedAppearance());
            player.cosmetics.ToggleNameVisible(true);
        }

        HysteriaActive = false;
    }
}