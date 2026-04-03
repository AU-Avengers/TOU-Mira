using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class HypnotistHysteriaModifier(PlayerBodyTypes bodyType, int appearanceType) : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Hypnotist Hysteria";
    public override bool AutoStart => false;
    public bool VisualPriority => true;
    public override bool VisibleToOthers => true;
    public PlayerBodyTypes NewBodyType => bodyType;
    public int AppearanceType => appearanceType;

    public VisualAppearance GetVisualAppearance()
    {
        if (AppearanceType == 0)
        {
            var morph = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph)
            {
                Size = new Vector3(0.7f, 0.7f, 1f),
                PetId = "pet_EmptyPet",
                PlayerName = string.Empty
            };

            if (NewBodyType is PlayerBodyTypes.Seeker)
            {
                return new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph)
                {
                    HatId = "hat_NoHat",
                    SkinId = "skin_None",
                    VisorId = "visor_EmptyVisor",
                    PlayerName = string.Empty,
                    PetId = "pet_EmptyPet",
                    Size = new Vector3(0.7f, 0.7f, 1f)
                };
            }

            return morph;
        }

        if (AppearanceType == 1)
        {
            return new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(), TownOfUsAppearances.Camouflage)
            {
                ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
                NameVisible = false,
                PlayerMaterialColor = Color.grey,
                Size = new Vector3(0.7f, 0.7f, 1f)
            };
        }

        var swoop = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = new Color(0f, 0f, 0f, 0.1f),
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear,
            Size = new Vector3(0.7f, 0.7f, 1f)
        };

        return swoop;
    }

    public override void OnActivate()
    {
        Player.MyPhysics.SetForcedBodyType(NewBodyType);
        Player.RawSetAppearance(this);
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
        Player.MyPhysics.SetForcedBodyType(PlayerBodyTypes.Normal);

        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            MushroomMixUp(mushroom, Player);
        }
        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            return;
        }

        Player.RawSetAppearance(Player.GetDefaultModifiedAppearance());
        Player.cosmetics.ToggleNameVisible(true);
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