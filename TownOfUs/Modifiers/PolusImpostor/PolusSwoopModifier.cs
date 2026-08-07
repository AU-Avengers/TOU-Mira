using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options;
using TownOfUs.Options.Roles.PolusImpostor;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.PolusImpostor;

public sealed class PolusSwoopModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => OptionGroupSingleton<PolusSwooperOptions>.Instance.SwoopDuration;
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override bool VisibleToOthers => false;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var playerColor = (PlayerControl.LocalPlayer.IsImpostorAligned() || (PlayerControl.LocalPlayer.DiedOtherRound() &&
                                                                      OptionGroupSingleton<GeneralOptions>
                                                                          .Instance.TheDeadKnow))
            ? new Color(0f, 0f, 0f, 0.1f)
            : Color.clear;

        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = playerColor,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear
        };
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
        {
            Player.RawSetAppearance(this);
            Player.cosmetics.ToggleNameVisible(false);
        }
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance();
        Player.cosmetics.ToggleNameVisible(true);

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }

        if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
        {
            MushroomMixUp(VanillaSystemCheckPatches.ShroomSabotageSystem, Player);
        }
    }

    public static void MushroomMixUp(MushroomMixupSabotageSystem instance, PlayerControl player)
    {
        if (player != null && !player.Data.IsDead && instance.currentMixups.ContainsKey(player.PlayerId))
        {
            var condensedOutfit = instance.currentMixups[player.PlayerId];
            var playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
            playerOutfit.NamePlateId = player.Data.DefaultOutfit.NamePlateId;

            player.MixUpOutfit(playerOutfit);
        }
    }
}