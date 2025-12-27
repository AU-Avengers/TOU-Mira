using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraAPI.Hud;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class PuppeteerControlButton : TownOfUsKillRoleButton<PuppeteerRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    private string _controlName = "Control";
    private string _killName = "Control Kill";
    public override string Name => _controlName;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    private bool _hasKilled;
    public override bool IsEffectCancellable() => true;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<PuppeteerOptions>.Instance.ControlCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration =>
        OptionGroupSingleton<PuppeteerOptions>.Instance.ControlDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses.Value;
    public int ExtraUses { get; set; }
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.ControlSprite;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _controlName = TouLocale.GetParsed("TouRolePuppeteerControl", "Control");
        _killName = TouLocale.GetParsed("TouRolePuppeteerKIll", "Control Kill");
        OverrideName(_controlName);
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PuppeteerRole;
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

        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return PlayerControl.LocalPlayer.moveable &&
               ((EffectActive && !_hasKilled) || (!EffectActive && (!LimitedUses || UsesLeft > 0))) &&
               (Target != null && !_hasKilled || pr.Controlled == null);
    }

    public override bool CanClick()
    {
        return ((Timer <= 0 && !EffectActive) || EffectActive && !_hasKilled) && CanUse();
    }

    public override PlayerControl? GetTarget()
    {
        if (PlayerControl.LocalPlayer.Data?.Role is not PuppeteerRole pr)
        {
            return null;
        }

        if (pr.Controlled != null)
        {
            return pr.Controlled.GetClosestLivingPlayer(
                true,
                Distance,
                predicate: plr =>
                    plr != null &&
                    plr != PlayerControl.LocalPlayer &&
                    !plr.HasDied() &&
                    !plr.IsImpostorAligned());
        }

        return null;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
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

        if (!_hasKilled &&
            Target != null &&
            pr.Controlled != null &&
            pr.Controlled.Data != null &&
            !pr.Controlled.HasDied() &&
            !pr.Controlled.Data.Disconnected &&
            PuppeteerControlState.IsControlled(pr.Controlled.PlayerId, out _))
        {
            Info("Target is not null, killing");
            PlayerControl.LocalPlayer.RpcFramedMurder(
                Target,
                pr.Controlled,
                causeOfDeath: "PuppetControl");
            _hasKilled = true;
            return;
        }

        if (pr.Controlled == null)
        {
            _hasKilled = false;
            Info("No player is being controlled, opening menu.");
            var playerMenu = CustomPlayerMenu.Create();
            playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
            playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

            playerMenu.Begin(
                plr => !plr.HasDied() && plr.PlayerId != PlayerControl.LocalPlayer.PlayerId && ((plr.TryGetModifier<DisabledModifier>(out var mod) && mod.CanBeInteractedWith && mod.IsConsideredAlive) || 
                    !plr.HasModifier<DisabledModifier>()),
                plr =>
                {
                    if (plr == null)
                    {
                        return;
                    }
                    playerMenu.ForceClose();
                    PuppeteerRole.RpcPuppeteerControl(PlayerControl.LocalPlayer, plr);
                    EffectActive = true;
                    Timer = EffectDuration;
                    OverrideName(_killName);
                    OverrideSprite(TouAssets.KillSprite.LoadAsset());
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
        OverrideName(_controlName);
        _hasKilled = false;
        OverrideSprite(TouImpAssets.ControlSprite.LoadAsset());
    }
}