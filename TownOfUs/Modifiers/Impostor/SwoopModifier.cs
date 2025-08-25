using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Impostor;

public sealed class SwoopModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => OptionGroupSingleton<SwooperOptions>.Instance.SwoopDuration;
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var playerColor = (PlayerControl.LocalPlayer.IsImpostor() || (PlayerControl.LocalPlayer.DiedOtherRound() && OptionGroupSingleton<GeneralOptions>
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

        var button = CustomButtonSingleton<SwooperSwoopButton>.Instance;
        button.OverrideSprite(TouImpAssets.UnswoopSprite.LoadAsset());
        button.OverrideName("Unswoop");

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

        var button = CustomButtonSingleton<SwooperSwoopButton>.Instance;
        button.OverrideSprite(TouImpAssets.SwoopSprite.LoadAsset());
        button.OverrideName("Swoop");

        if (Player.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.SwooperDeactivateSound);
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