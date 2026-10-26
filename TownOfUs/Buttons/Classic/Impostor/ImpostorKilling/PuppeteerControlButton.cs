using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class PuppeteerControlButton : TownOfUsRoleButton<PuppeteerRole>, IDiseaseableButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.PuppeteerControl", "Control");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<PuppeteerOptions>.Instance.ControlCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration =>
        OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
    public override int MaxUses => OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses;
    public int ExtraUses { get; set; }
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.ControlSprite;
    private static PuppeteerKillButton KillButton => CustomButtonSingleton<PuppeteerKillButton>.Instance;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        TimerPaused = false;
        if (PlayerControl.LocalPlayer?.Data?.Role is PuppeteerRole { Controlled.PlayerId: var controlledId } &&
            PuppeteerRole.ControlState.IsControlled(controlledId, out _) &&
            PuppeteerRole.ControlState.IsInInitialGrace(controlledId))
        {
            TimerPaused = true;
        }

        base.FixedUpdateHandler(playerControl);
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, true));
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not PuppeteerRole { Controlled: var controlled })
        {
            return false;
        }

        if (controlled != null)
        {
            if (controlled.Data == null ||
                controlled.HasDied() ||
                controlled.Data.Disconnected ||
                !PuppeteerRole.ControlState.IsControlled(controlled.PlayerId, out _))
            {
                PuppeteerRole.RpcPuppeteerEndControl(PlayerControl.LocalPlayer, controlled, controlled.transform.position);
                return false;
            }
            return base.CanUse();
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (!PlayerControl.LocalPlayer.CanMove ||
            PlayerControl.LocalPlayer.HasModifier<DisabledModifier>(x => !x.CanUseAbilities))
        {
            return false;
        }

        return PlayerControl.LocalPlayer.moveable &&
               (!EffectActive && (!LimitedUses || UsesLeft > 0) || EffectActive);
    }

    public override bool CanClick()
    {
        return CanUse() && !EffectActive && Role.Controlled == null && Timer <= 0;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
    }

    protected override void OnClick()
    {
        Info("Checking control button");
        if (PlayerControl.LocalPlayer.Data?.Role is not PuppeteerRole { Controlled: var controlled })
        {
            return;
        }
        Info("Role is valid");
        if (controlled == null)
        {
            Info("No player is being controlled, opening menu.");
            var playerMenu = CustomPlayerMenu.Create();
            playerMenu.PhoneUI.GetChild(0).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            playerMenu.PhoneUI.GetChild(1).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

            playerMenu.Begin(
                plr => !plr.HasDied() && !plr.AmOwner &&
                       !plr.IsInTargetingAnimState() &&
                       !plr.HasModifierOfType<IUncontrollable>() &&
                       (plr.HasModifier<DisabledModifier>(mod => mod.CanBeInteractedWith && mod.IsConsideredAlive) ||
                        !plr.HasModifier<DisabledModifier>()),
                plr =>
                {
                    playerMenu.ForceClose();
                    if (plr == null)
                    {
                        return;
                    }

                    if (plr.IsInTargetingAnimState())
                    {
                        var notificationText = MiraLocaleManager.Get(
                                "TownOfUsMira.Role.PuppeteerTargetInAnimation",
                                "<player> is currently in an animation (ladder/zipline/platform/vent), please wait.")
                            .Replace("<player>", plr.CachedPlayerData.PlayerName);

                        var notif = Helpers.CreateAndShowNotification(
                            $"<b>{notificationText}</b>",
                            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Puppeteer);
                        notif.Text.SetOutlineThickness(0.35f);
                        return;
                    }

                    PuppeteerRole.RpcPuppeteerControl(PlayerControl.LocalPlayer, plr);
                    EffectActive = true;
                    Timer = EffectDuration;
                    KillButton.ToggleControlText(true);
                    if (LimitedUses)
                    {
                        UsesLeft--;
                        Button?.SetUsesRemaining(UsesLeft);
                    }
                });
        }
    }

    public override void OnEffectEnd()
    {
        KillButton.ToggleControlText(false);
    }
}