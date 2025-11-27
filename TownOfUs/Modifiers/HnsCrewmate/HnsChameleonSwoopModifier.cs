using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.HnsCrewmate;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options;
using TownOfUs.Options.Roles.HnsCrewmate;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.HnsCrewmate;

public sealed class HnsChameleonSwoopModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => OptionGroupSingleton<HnsChameleonOptions>.Instance.SwoopDuration;
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var playerColor = (PlayerControl.LocalPlayer.IsCrewmate() || (PlayerControl.LocalPlayer.DiedOtherRound() &&
                                                                      OptionGroupSingleton<GeneralOptions>
                                                                          .Instance.TheDeadKnow))
            ? new Color(0f, 0f, 0f, 0.1f)
            : Color.clear;

        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
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
        if (Player.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.SwooperActivateSound);
        }

        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);

        var button = CustomButtonSingleton<ChameleonSwoopButton>.Instance;
        button.OverrideSprite(TouCrewAssets.CrewUnswoopSprite.LoadAsset());
        button.OverrideName(TouLocale.GetParsed("HnsRoleChameleonUnswoop", "Unswoop"));

        var touAbilityEvent = new TouAbilityEvent(AbilityType.SwooperSwoop, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
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

        if (Player.AmOwner)
        {
            var button = CustomButtonSingleton<ChameleonSwoopButton>.Instance;
            button.OverrideSprite(TouCrewAssets.CrewSwoopSprite.LoadAsset());
            button.OverrideName(TouLocale.GetParsed("HnsRoleChameleonSwoop", "Swoop"));
            if (MeetingHud.Instance == null)
            {
                TouAudio.PlaySound(TouAudio.SwooperDeactivateSound);
            }
        }

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.cosmetics.ToggleNameVisible(false);
        }

        var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            MushroomMixUp(mushroom, Player);
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.SwooperUnswoop, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
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