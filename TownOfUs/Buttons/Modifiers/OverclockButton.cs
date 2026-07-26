using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Options.Modifiers.Assailant;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class OverclockButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("TouModifierOverclockerOverclock", "Overclock");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Overclocker;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<OverclockerOptions>.Instance.OverclockCooldown.Value + MapCooldown, 2.5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<OverclockerOptions>.Instance.OverclockDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<OverclockerOptions>.Instance.OverclockUses.Value;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.RewindSprite;

    public override bool UsableFirstRound => OptionGroupSingleton<OverclockerOptions>.Instance.OverclockRoundOne.Value;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer &&
               PlayerControl.LocalPlayer.HasModifier<OverclockerModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override bool CanUse()
    {
        return base.CanUse() && PlayerControl.LocalPlayer.TryGetModifier<OverclockerModifier>(out var modifier) && modifier.CurrentState < ChargeState.UnderclockedBegin || EffectActive;
    }

    public override bool CanClick()
    {
        return base.CanClick() && PlayerControl.LocalPlayer.TryGetModifier<OverclockerModifier>(out var modifier) && modifier.CurrentState is ChargeState.Normal;
    }

    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.TryGetModifier<OverclockerModifier>(out var modifier))
        {
            return;
        }

        modifier.CurrentState = ChargeState.Overclocked;
        OverrideSprite(TouCrewAssets.RewindingSprite.LoadAsset());
        OverrideName(TouLocale.GetParsed("TouModifierOverclockerOverclocked", "Overclocked"));

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouModifierOverclockerOverclockNotif").Replace("<multi>", OptionGroupSingleton<OverclockerOptions>.Instance.OverclockMultiplier.Value.ToString(TownOfUsPlugin.Culture))}</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Overclocker.LoadAsset());
        notif1.AdjustNotification();
    }

    public bool ShowedFeedback;
    public override void OnEffectEnd()
    {
        if (!PlayerControl.LocalPlayer.TryGetModifier<OverclockerModifier>(out var modifier))
        {
            return;
        }
        modifier.CurrentState = ChargeState.UnderclockedBegin;
        OverrideName(TouLocale.GetParsed("TouModifierOverclockerUnderclocked", "Underclocked"));
        OverrideSprite(TouCrewAssets.RewindSprite.LoadAsset());
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        ShowedFeedback = true;
        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TouLocale.GetParsed("TouModifierOverclockerUnderclockNotif").Replace("<multi>", OptionGroupSingleton<OverclockerOptions>.Instance.UnderclockMultiplier.Value.ToString(TownOfUsPlugin.Culture))}</b>", Color.white,
            new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Overclocker.LoadAsset());
        notif1.AdjustNotification();
    }
}