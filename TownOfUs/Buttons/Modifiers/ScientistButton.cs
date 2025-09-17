using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Modifiers;

public sealed class ScientistButton : TownOfUsButton
{
    public VitalsMinigame? vitals;
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VitalsAbility, "Vitals");
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Scientist;
    public override float Cooldown => OptionGroupSingleton<ScientistOptions>.Instance.DisplayCooldown + MapCooldown;
    public float AvailableCharge { get; set; } = OptionGroupSingleton<ScientistOptions>.Instance.StartingCharge;

    public override float EffectDuration
    {
        get
        {
            if (OptionGroupSingleton<ScientistOptions>.Instance.DisplayDuration == 0)
            {
                return AvailableCharge;
            }

            return AvailableCharge < OptionGroupSingleton<ScientistOptions>.Instance.DisplayDuration
                ? AvailableCharge
                : OptionGroupSingleton<ScientistOptions>.Instance.DisplayDuration;
        }
    }

    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.VitalsSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<ScientistModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        AvailableCharge = OptionGroupSingleton<ScientistOptions>.Instance.StartingCharge;
    }

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

        if (vitals != null)
        {
            AvailableCharge -= Time.deltaTime;
            vitals.BatteryText.text = $"{(int)AvailableCharge}";
            if (AvailableCharge <= 0f)
            {
                vitals.Close();
                RefreshAbilityButton();
                ResetCooldownAndOrEffect();
                vitals = null;
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
        if (vitals == null && EffectActive)
        {
            ResetCooldownAndOrEffect();
        }
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

    protected override void OnClick()
    {
        if (!OptionGroupSingleton<ScientistOptions>.Instance.MoveWithMenu)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
        }

        vitals = Object.Instantiate<VitalsMinigame>(RoleManager.Instance.GetRole(RoleTypes.Scientist)
            .Cast<ScientistRole>().VitalsPrefab);
        vitals.transform.SetParent(Camera.main.transform, false);
        vitals.transform.localPosition = new Vector3(0f, 0f, -50f);
        vitals.BatteryText.gameObject.SetActive(true);
        vitals.Begin(null);
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (vitals != null)
        {
            vitals.Close();
            RefreshAbilityButton();
            vitals = null;
        }
    }
}