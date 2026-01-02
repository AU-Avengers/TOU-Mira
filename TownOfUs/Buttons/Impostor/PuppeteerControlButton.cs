using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class PuppeteerControlButton : TownOfUsRoleButton<PuppeteerRole>, IDiseaseableButton
{
    public override string Name => TouLocale.GetParsed("TouRolePuppeteerControl", "Control");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<PuppeteerOptions>.Instance.ControlCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration =>
        OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses.Value;
    public int ExtraUses { get; set; }
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.ControlSprite;
    private static PuppeteerKillButton KillButton => CustomButtonSingleton<PuppeteerKillButton>.Instance;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        // Freeze effect/cooldown timers during the initial control sync grace window.
        // This prevents "burning" duration/cooldown while the victim is intentionally immobilized.
        TimerPaused = false;
        if (PlayerControl.LocalPlayer?.Data?.Role is PuppeteerRole pr &&
            pr.Controlled != null &&
            PuppeteerControlState.IsControlled(pr.Controlled.PlayerId, out _) &&
            PuppeteerControlState.IsInInitialGrace(pr.Controlled.PlayerId))
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
        if (PlayerControl.LocalPlayer.Data?.Role is not PuppeteerRole pr)
        {
            return false;
        }

        if (pr.Controlled != null)
        {
            if (pr.Controlled.Data == null ||
                pr.Controlled.HasDied() ||
                pr.Controlled.Data.Disconnected ||
                !PuppeteerControlState.IsControlled(pr.Controlled.PlayerId, out _))
            {
                PuppeteerRole.RpcPuppeteerEndControl(PlayerControl.LocalPlayer, pr.Controlled);
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
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
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
        if (PlayerControl.LocalPlayer.Data?.Role is not PuppeteerRole pr)
        {
            return;
        }
        Info("Role is valid");
        if (pr.Controlled == null)
        {
            Info("No player is being controlled, opening menu.");
            var playerMenu = CustomPlayerMenu.Create();
            playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

            playerMenu.Begin(
                plr => !plr.HasDied() && plr.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
                       !plr.GetModifiers<BaseModifier>().Any(x => x is IUncontrollable) &&
                       ((plr.TryGetModifier<DisabledModifier>(out var mod) && mod.CanBeInteractedWith &&
                         mod.IsConsideredAlive) ||
                        !plr.HasModifier<DisabledModifier>()),
                plr =>
                {
                    playerMenu.ForceClose();
                    if (plr == null)
                    {
                        return;
                    }

                    if (plr.onLadder || plr.inMovingPlat)
                    {
                        var notif = Helpers.CreateAndShowNotification(
                            $"<b>{plr.CachedPlayerData.PlayerName} is currently on a ladder or platform, please wait.</b>",
                            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Puppeteer.LoadAsset());
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