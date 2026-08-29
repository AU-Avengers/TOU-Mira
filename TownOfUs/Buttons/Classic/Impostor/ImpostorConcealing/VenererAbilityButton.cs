using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class VenererAbilityButton : TownOfUsRoleButton<VenererRole>, IAftermathableButton, ILegacyCapable
{
    private VenererAbility _queuedAbility = VenererAbility.None;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyImpAssets.NoAbilitySprite : TouImpAssets.NoAbilitySprite;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<VenererOptions>.Instance.AbilityCooldown + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;

    public override bool ZeroIsInfinite { get; set; } = true;

    public VenererAbility ActiveAbility { get; private set; } = VenererAbility.None;

    public void UpdateAbility(VenererAbility ability)
    {
        if (ability == VenererAbility.None)
        {
            ActiveAbility = VenererAbility.None;
            _queuedAbility = VenererAbility.None;

            SetActive(false, Role);
        }

        if (ActiveAbility == VenererAbility.Freeze)
        {
            return;
        }

        if (ability != VenererAbility.None && Role)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You have unlocked the {ability} ability for getting a kill. {(EffectActive ? "You must wait until your current ability is over." : string.Empty)}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Venerer.LoadAsset());

            notif1.AdjustNotification();
        }

        if (EffectActive)
        {
            _queuedAbility = ability;
        }
        else
        {
            UpdateButton(ability);
        }
    }

    private void UpdateButton(VenererAbility ability)
    {
        ActiveAbility = ability;

        if (EffectActive)
        {
            ResetCooldownAndOrEffect();
        }

        switch (ActiveAbility)
        {
            case VenererAbility.Camouflage:
                SetAbility("Camouflage", LegacyAssets.IsLegacy ? LegacyImpAssets.CamouflageSprite.LoadAsset() : TouImpAssets.CamouflageSprite.LoadAsset());
                break;
            case VenererAbility.Sprint:
                SetAbility("Sprint", LegacyAssets.IsLegacy ? LegacyImpAssets.SprintSprite.LoadAsset() : TouImpAssets.SprintSprite.LoadAsset());
                break;
            case VenererAbility.Freeze:
                SetAbility("Freeze", LegacyAssets.IsLegacy ? LegacyImpAssets.FreezeSprite.LoadAsset() : TouImpAssets.FreezeSprite.LoadAsset());
                break;
        }

        SetActive(true, PlayerControl.LocalPlayer.Data.Role);
    }

    private void SetAbility(string name, Sprite sprite)
    {
        OverrideName(MiraLocaleManager.Get($"TouRoleVenerer{name}", name));
        OverrideSprite(sprite);
    }

    private void OverrideAbilityName(bool active)
    {
        var name = ActiveAbility switch
        {
            VenererAbility.Camouflage => active ? "Uncamouflage" : "Camouflage",
            VenererAbility.Sprint => active ? "Unsprint" : "Sprint",
            VenererAbility.Freeze => active ? "Unfreeze" : "Freeze",
            _ => string.Empty
        };

        if (name.Length > 0)
        {
            OverrideName(MiraLocaleManager.Get($"TouRoleVenerer{name}", name));
        }
    }

    public override void OnEffectEnd()
    {
        var selfMods = PlayerControl.LocalPlayer.GetModifierComponent()?.ActiveModifiers
            .Where(mod => mod is IVenererModifier && mod is not VenererFreezeModifier).ToList();

        if (selfMods != null)
        {
            foreach (var mod in selfMods)
            {
                PlayerControl.LocalPlayer.RpcRemoveModifier(mod.UniqueId);
            }
        }

        var freezes = ModifierUtils.GetActiveModifiers<VenererFreezeModifier>(x => x.Venerer == PlayerControl.LocalPlayer).ToList();
        foreach (var freeze in freezes)
        {
            freeze.Player.RpcRemoveModifier(freeze.UniqueId);
        }

        OverrideAbilityName(false);
        UpdateButton(_queuedAbility != VenererAbility.None ? _queuedAbility : ActiveAbility);
        _queuedAbility = VenererAbility.None;
    }

    public override void ClickHandler()
    {
        if (ActiveAbility == VenererAbility.None || !CanUse())
        {
            return;
        }

        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
            Button?.SetDisabled();
            OnEffectEnd();
            return;
        }

        OnClick();
        Button?.SetDisabled();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
            OverrideAbilityName(true);
        }
        else
        {
            Timer = Cooldown;
        }
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return (Timer <= 0 && !EffectActive) || (EffectActive && Timer <= EffectDuration - 2f);
    }

    public void AftermathHandler()
    {
        ClickHandler();
    }

    protected override void OnClick()
    {
        switch (ActiveAbility)
        {
            case VenererAbility.Camouflage:
                PlayerControl.LocalPlayer.RpcAddModifier<VenererCamouflageModifier>();
                break;
            case VenererAbility.Sprint:
                PlayerControl.LocalPlayer.RpcAddModifier<VenererCamouflageModifier>();
                PlayerControl.LocalPlayer.RpcAddModifier<VenererSprintModifier>();
                break;
            case VenererAbility.Freeze:
                PlayerControl.LocalPlayer.RpcAddModifier<VenererCamouflageModifier>();
                PlayerControl.LocalPlayer.RpcAddModifier<VenererSprintModifier>();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.IsDead || player.Data.Disconnected || player.AmOwner)
                    {
                        continue;
                    }

                    player.RpcAddModifier<VenererFreezeModifier>(PlayerControl.LocalPlayer);
                }

                break;
        }
    }
}