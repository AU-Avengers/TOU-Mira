using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class SpyAdminTableModifierButton : TownOfUsButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.Admin, "Admin");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Spy;
    public override float Cooldown => OptionGroupSingleton<SpyOptions>.Instance.DisplayCooldown.Value + MapCooldown;
    public float AvailableCharge { get; set; } = OptionGroupSingleton<SpyOptions>.Instance.StartingCharge.Value;
    public bool usingPortable { get; set; }

    public override float EffectDuration
    {
        get
        {
            if (OptionGroupSingleton<SpyOptions>.Instance.DisplayDuration == 0)
            {
                return AvailableCharge;
            }

            return AvailableCharge < OptionGroupSingleton<SpyOptions>.Instance.DisplayDuration.Value
                ? AvailableCharge
                : OptionGroupSingleton<SpyOptions>.Instance.DisplayDuration.Value;
        }
    }

    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.AdminSprite;

    private void RefreshAbilityButton()
    {
        if (AvailableCharge > 0f && !PlayerControl.LocalPlayer.AreCommsAffected())
        {
            Button?.SetEnabled();
            return;
        }

        Button?.SetDisabled();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        if (usingPortable && !MapBehaviour.Instance.gameObject.activeSelf)
        {
            RefreshAbilityButton();
            ResetCooldownAndOrEffect();
            usingPortable = false;
            return;
        }

        if (usingPortable)
        {
            AvailableCharge -= Time.deltaTime;
            if (AvailableCharge <= 0f)
            {
                MapBehaviour.Instance.Close();
                RefreshAbilityButton();
                ResetCooldownAndOrEffect();
                usingPortable = false;
                return;
            }
        }
        else
        {
            RefreshAbilityButton();
        }

        Button?.usesRemainingText.gameObject.SetActive(true);
        Button?.usesRemainingSprite.gameObject.SetActive(true);
        Button!.usesRemainingText.text = (int)AvailableCharge + "%";
        if (!usingPortable && EffectActive)
        {
            ResetCooldownAndOrEffect();
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<SpyModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead &&
               OptionGroupSingleton<SpyOptions>.Instance.HasPortableAdmin is PortableAdmin.Modifier
                   or PortableAdmin.Both;
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return Timer <= 0 && !EffectActive && AvailableCharge > 0f;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        AvailableCharge = OptionGroupSingleton<SpyOptions>.Instance.StartingCharge.Value;
        Button!.transform.localPosition =
            new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, -150f);
    }

    protected override void OnClick()
    {
        if (!OptionGroupSingleton<SpyOptions>.Instance.MoveWithMenu)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
        }

        usingPortable = true;
        ToggleMapVisible(OptionGroupSingleton<SpyOptions>.Instance.MoveWithMenu);
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (usingPortable)
        {
            MapBehaviour.Instance.Close();
            RefreshAbilityButton();
            usingPortable = false;
        }
    }

    public override void ClickHandler()
    {
        if (!CanUse() || Minigame.Instance != null)
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    private static void ToggleMapVisible(bool canMove = false)
    {
        if (MapBehaviour.Instance && MapBehaviour.Instance.gameObject.activeSelf)
        {
            MapBehaviour.Instance.Close();
            return;
        }

        if (!ShipStatus.Instance)
        {
            return;
        }

        HudManager.Instance.InitMap();
        if (!PlayerControl.LocalPlayer.CanMove && !MeetingHud.Instance)
        {
            return;
        }

        var opts = GameManager.Instance.GetMapOptions();
        var portableAdmin = MapBehaviour.Instance;

        portableAdmin.GenericShow();
        portableAdmin.countOverlay.gameObject.SetActive(true);
        portableAdmin.countOverlay.SetOptions(opts.ShowLivePlayerPosition, opts.IncludeDeadBodies);
        portableAdmin.countOverlayAllowsMovement = canMove;
        portableAdmin.taskOverlay.Hide();
        portableAdmin.HerePoint.enabled = !opts.ShowLivePlayerPosition;
        portableAdmin.TrackedHerePoint.gameObject.SetActive(false);
        if (portableAdmin.HerePoint.enabled)
        {
            PlayerControl.LocalPlayer.SetPlayerMaterialColors(portableAdmin.HerePoint);
        }
    }
}