using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Modifiers;

public sealed class SecurityButton : TownOfUsButton
{
    public Minigame? securityMinigame;

    public override string Name =>
        TranslationController.Instance.GetStringWithDefault(StringNames.Security, "Security");

    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override Color TextOutlineColor => TownOfUsColors.Operative;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<OperativeOptions>.Instance.DisplayCooldown + MapCooldown, 0.001f, 120f);
    public float AvailableCharge { get; set; } = OptionGroupSingleton<OperativeOptions>.Instance.StartingCharge;

    public override float EffectDuration
    {
        get
        {
            if (OptionGroupSingleton<OperativeOptions>.Instance.DisplayDuration == 0)
            {
                return AvailableCharge;
            }

            return AvailableCharge < OptionGroupSingleton<OperativeOptions>.Instance.DisplayDuration
                ? AvailableCharge
                : OptionGroupSingleton<OperativeOptions>.Instance.DisplayDuration;
        }
    }

    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.CameraSprite;
    public bool canMoveWithMinigame { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<OperativeModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        // this is so you can see it through cams
        Button!.transform.localPosition =
            new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, -150f);
        AvailableCharge = OptionGroupSingleton<OperativeOptions>.Instance.StartingCharge;
        if (KeybindIcon != null)
        {
            KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
        }
    }

    private void RefreshAbilityButton()
    {
        if (AvailableCharge > 0f && !PlayerControl.LocalPlayer.AreCommsAffected())
        {
            DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
            return;
        }

        DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        if (securityMinigame != null)
        {
            AvailableCharge -= Time.deltaTime;
            if (AvailableCharge <= 0f)
            {
                securityMinigame.Close();
                RefreshAbilityButton();
                ResetCooldownAndOrEffect();
                canMoveWithMinigame = false;
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
        if (securityMinigame == null && EffectActive)
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

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
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
        // Warning($"Checking Base Conditions");
        var mapId = (ExpandedMapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        if (TutorialManager.InstanceExists)
        {
            mapId = (ExpandedMapNames)AmongUsClient.Instance.TutorialMapId;
        }

        canMoveWithMinigame = true;
        var basicCams = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x =>
            x.gameObject.name.Contains("Surv_Panel") || x.name.Contains("Cam") ||
            x.name.Contains("BinocularsSecurityConsole"));
        if (mapId is ExpandedMapNames.Airship)
        {
            // Warning($"Checking Airship Conditions");
            basicCams = Object.FindObjectsOfType<SystemConsole>()
                .FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
            PlayerControl.LocalPlayer.NetTransform.Halt();
            canMoveWithMinigame = false;
        }
        else if (mapId is ExpandedMapNames.Skeld or ExpandedMapNames.Dleks)
        {
            // Warning($"Checking Skeld Conditions");
            basicCams = Object.FindObjectsOfType<SystemConsole>()
                .FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
            PlayerControl.LocalPlayer.NetTransform.Halt();
            canMoveWithMinigame = false;
        }
        else if (mapId is ExpandedMapNames.MiraHq)
        {
            // Warning($"Checking Mira HQ Conditions");
            basicCams = Object.FindObjectsOfType<SystemConsole>()
                .FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
            if (!OptionGroupSingleton<OperativeOptions>.Instance.MoveOnMira)
            {
                PlayerControl.LocalPlayer.NetTransform.Halt();
                canMoveWithMinigame = false;
            }
        }
        else if (mapId is ExpandedMapNames.Fungle)
        {
            // Warning($"Checking Fungle Conditions");
            PlayerControl.LocalPlayer.NetTransform.Halt();
            canMoveWithMinigame = false;
        }
        else if (mapId is ExpandedMapNames.Submerged)
        {
            // Warning($"Checking Submerged Conditions");
            basicCams = Object.FindObjectsOfType<SystemConsole>()
                .FirstOrDefault(x => x.gameObject.name.Contains("SecurityConsole"));
            PlayerControl.LocalPlayer.NetTransform.Halt();
            canMoveWithMinigame = false;
        }

        if (basicCams == null)
        {
            Error($"No Camera System Found!");
            return;
        }

        securityMinigame = Object.Instantiate(basicCams.MinigamePrefab, Camera.main.transform, false);
        securityMinigame.transform.SetParent(Camera.main.transform, false);
        securityMinigame.transform.localPosition = new Vector3(0f, 0f, -50f);
        securityMinigame.Begin(null);
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();
        canMoveWithMinigame = false;

        if (securityMinigame != null)
        {
            securityMinigame.Close();
            RefreshAbilityButton();
            securityMinigame = null;
        }
    }
}