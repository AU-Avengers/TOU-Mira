using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Neutral.NeutralOutlier;

public sealed class DuelReturnInvisModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => 5f;
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    private bool CanSee()
    {
        var observer = PlayerControl.LocalPlayer;
        return Player.AmOwner || (observer != null && TownOfUs.Modules.GameHistory.IsFullyDead(observer));
    }

    private void HideSkinLayer()
    {
        if (Player == null || Player.cosmetics == null || Player.cosmetics.skin == null ||
            Player.cosmetics.skin.layer == null)
        {
            return;
        }

        var alpha = CanSee() ? 0.1f : 0f;
        Player.cosmetics.skin.layer.color = Player.cosmetics.skin.layer.color.SetAlpha(alpha);
    }

    public VisualAppearance GetVisualAppearance()
    {
        var playerColor = CanSee() ? new Color(0f, 0f, 0f, 0.1f) : Color.clear;

        var app = new VisualAppearance(Player.GetDefaultAppearance(), TownOfUsAppearances.PlayerNameOnly)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PetId = "pet_EmptyPet",
            PlayerName = string.Empty,
            NameVisible = false,
            RendererColor = playerColor,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear,
        };
        return app;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);
        HideSkinLayer();
        if (!Player.HasModifier<DuelReturnDisabledModifier>())
        {
            Player.AddModifier<DuelReturnDisabledModifier>();
        }
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

        HideSkinLayer();
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance();
        Player.cosmetics.ToggleNameVisible(true);
        if (Player.HasModifier<DuelReturnDisabledModifier>())
        {
            Player.RemoveModifier<DuelReturnDisabledModifier>();
        }
    }
}

public sealed class DuelReturnDisabledModifier : DisabledModifier
{
    public override string ModifierName => "Swooped";
    public override float Duration => 5f;
    public override bool AutoStart => true;
    public override bool RemoveOnComplete => false;
    public override bool CanUseAbilities => false;
}
