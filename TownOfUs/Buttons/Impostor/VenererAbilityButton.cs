using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class VenererAbilityButton : TownOfUsRoleButton<VenererRole>, IAftermathableButton
{
    private VenererAbility _queuedAbility = VenererAbility.None;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.NoAbilitySprite;
    public override float Cooldown => OptionGroupSingleton<VenererOptions>.Instance.AbilityCooldown;
    public override float EffectDuration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;

    public VenererAbility ActiveAbility { get; private set; } = VenererAbility.None;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && ActiveAbility != VenererAbility.None;
    }

    public void UpdateAbility(VenererAbility ability)
    {
        if (ability == VenererAbility.None)
        {
            ActiveAbility = VenererAbility.None;
            _queuedAbility = VenererAbility.None;

            SetActive(false, PlayerControl.LocalPlayer.Data.Role);
        }

        if (ActiveAbility == VenererAbility.Freeze)
        {
            return;
        }

        if (ability != VenererAbility.None && PlayerControl.LocalPlayer.Data.Role is VenererRole)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You have unlocked the {ability.ToString()} ability for getting a kill. {(EffectActive ? "You must wait until your current ability is over." : string.Empty)}</color></b>",
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
                SetAbility("Camouflage", TouImpAssets.CamouflageSprite.LoadAsset());
                break;
            case VenererAbility.Sprint:
                SetAbility("Sprint", TouImpAssets.SprintSprite.LoadAsset());
                break;
            case VenererAbility.Freeze:
                SetAbility("Freeze", TouImpAssets.FreezeSprite.LoadAsset());
                break;
        }

        SetActive(true, PlayerControl.LocalPlayer.Data.Role);
    }

    private void SetAbility(string name, Sprite sprite)
    {
        OverrideName(TouLocale.Get($"TouRoleVenener{name}", name));
        OverrideSprite(sprite);
    }

    public override void OnEffectEnd()
    {
        var mod = PlayerControl.LocalPlayer.GetModifierComponent()?.ActiveModifiers
            .FirstOrDefault(mod => mod is IVenererModifier);

        if (mod != null)
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier(mod.UniqueId);
        }

        if (_queuedAbility == VenererAbility.None)
        {
            return;
        }

        UpdateButton(_queuedAbility);
        _queuedAbility = VenererAbility.None;
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