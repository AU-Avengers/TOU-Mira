using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MediumHiddenModifier : ConcealedModifier, IVisualAppearance
{
    public override float Duration => OptionGroupSingleton<MediumOptions>.Instance.MediateDurationReal + 1f;
    public override string ModifierName => "Hidden";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public override bool VisibleToOthers => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var playerColor = new Color(0.3f, 0f, 0.7f, 0.5f);

        return new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(), TownOfUsAppearances.Camouflage)
        {
            HatId = string.Empty,
            SkinId = string.Empty,
            VisorId = string.Empty,
            PlayerName = string.Empty,
            PetId = string.Empty,
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

        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
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
            Player.RawSetAppearance(new VisualAppearance(Player.GetDefaultAppearance(), TownOfUsAppearances.Camouflage)
            {
                ColorId = Player.Data.DefaultOutfit.ColorId,
                HatId = string.Empty,
                SkinId = string.Empty,
                VisorId = string.Empty,
                PlayerName = string.Empty,
                PetId = string.Empty,
                NameVisible = false,
                PlayerMaterialColor = Color.grey,
                Size = (OptionGroupSingleton<AdvancedSabotageOptions>.Instance.HidePlayerSizeInCamo) ? new Vector3(0.7f, 0.7f, 1f) : Player.GetAppearance().Size
            });
            Player.cosmetics.ToggleNameVisible(false);
        }

        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            MushroomMixUp(mushroom, Player);
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