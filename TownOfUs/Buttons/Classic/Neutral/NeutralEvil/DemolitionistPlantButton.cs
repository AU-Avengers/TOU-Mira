using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using MiraAPI.Translation;
using TownOfUs.Assets;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral.NeutralEvil;
using TownOfUs.Utilities;
using TownOfUs.Buttons;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral.NeutralEvil;

public class DemolitionistPlantButton : TownOfUsButton
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Ability.Plant");
    public override float Cooldown => OptionGroupSingleton<DemolitionistOptions>.Instance.PlantCooldown.Value;

    public override float EffectDuration => _arming
        ? OptionGroupSingleton<DemolitionistOptions>.Instance.PlantToSabotageDelay.Value
        : OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle
            ? OptionGroupSingleton<DemolitionistOptions>.Instance.PlantTime.Value
            : 0f;
    public override int MaxUses => 0;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DemolitionistPlantButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight; 
    public override Color TextOutlineColor => DemolitionistRole.DemolitionistColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public static DemolitionistPlantButton? Instance { get; set; }

    private Vector2 _capturedPosition;
    private int _capturedConsoleKey;
    private DemolitionistUtilityKind _capturedKind;
    private bool _isPlanting;
    private bool _arming;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is DemolitionistRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (MeetingHud.Instance || ExileController.Instance) return false;

        if (DemolitionistSabotageState.IsCriticalVanillaSabotageActive()) return false;
        if (DemolitionistSabotageState.IsActive) return false;
        if (DemolitionistNumpad.Controller.InProgress) return false;
        if (_isPlanting || EffectActive) return false;
        if (!DemolitionistUtilityConsoles.TryGetClosest(player, out _, out _, forDemolitionistPlant: true)) return false;

        return base.CanUse();
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

    public static void CancelPlantOrArmingForMeeting()
    {
        var plant = Instance;
        if (plant == null)
        {
            return;
        }

        if (!plant._isPlanting && !plant._arming)
        {
            return;
        }

        plant.AbortPlant();
    }

    public static void SyncAfterSabotageEnded(bool startCooldown)
    {
        var plant = Instance;
        if (plant == null)
        {
            return;
        }

        plant._isPlanting = false;
        plant._arming = false;
        plant.EffectActive = false;
        plant.Button?.OverrideText(plant.Name.ToUpperInvariant());
        if (startCooldown)
        {
            plant.Timer = plant.Cooldown;
        }

        if (plant.Button == null)
        {
            return;
        }

        if (plant.CanUse())
        {
            plant.Button.SetEnabled();
        }
        else if (plant.Timer > 0f)
        {
            plant.Button.SetDisabled();
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        if (!DemolitionistUtilityConsoles.TryGetClosest(player, out var consolePosition, out var kind, forDemolitionistPlant: true)
            || kind == DemolitionistUtilityKind.None)
        {
            return;
        }

        if (DemolitionistSabotageState.IsActive || DemolitionistSabotageState.IsCriticalVanillaSabotageActive()) return;

        _capturedPosition = consolePosition;
        _capturedKind = kind;
        _capturedConsoleKey = DemolitionistUtilityConsoles.GetStableId(kind, consolePosition);

        if (!OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle)
        {
            Coroutines.Start(PlantNumpadCoroutine(player));
            return;
        }

        Coroutines.Start(PlantTimedCoroutine(player));
    }

    private IEnumerator PlantTimedCoroutine(PlayerControl player)
    {
        _isPlanting = true;
        var plantTime = EffectDuration;
        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        EffectActive = true;
        Timer = plantTime;
        Button?.OverrideText(MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Button.Planting"));

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Notification.Planting")}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DemolitionistIcon.LoadAsset());

        var elapsed = 0f;
        while (elapsed < plantTime)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                AbortPlant();
                yield break;
            }

            if (!DemolitionistUtilityConsoles.TryGetClosest(player, out var currentPos, out var currentKind, forDemolitionistPlant: true)
                || currentKind != _capturedKind
                || DemolitionistUtilityConsoles.GetStableId(currentKind, currentPos) != _capturedConsoleKey)
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>{MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Notification.PlantTooFar")}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.DemolitionistIcon.LoadAsset());
                AbortPlant();
                yield break;
            }

            if (DemolitionistSabotageState.IsCriticalVanillaSabotageActive() || DemolitionistSabotageState.IsActive)
            {
                AbortPlant();
                yield break;
            }

            if (MeetingHud.Instance != null || ExileController.Instance != null)
            {
                AbortPlant();
                yield break;
            }

            elapsed += Time.deltaTime;
            Timer = plantTime - elapsed;
            if (Button != null)
            {
                Button.SetFillUp(Timer, plantTime);
            }

            yield return null;
        }

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            AbortPlant();
            yield break;
        }

        StartArming();
    }

    private IEnumerator PlantNumpadCoroutine(PlayerControl player)
    {
        _isPlanting = true;

        if (!DemolitionistNumpad.Controller.OpenPlant(player, _capturedPosition, _capturedConsoleKey, _capturedKind))
        {
            AbortPlant();
            yield break;
        }

        // Do not call TryGetClosest while minigame is open — vanilla Use/couldUse often false during KeypadGame,
        // so first frame would abort even though player is still at the utility.
        while (DemolitionistNumpad.Controller.InProgress)
        {
            if (player == null || player.Data == null || player.Data.IsDead
                || DemolitionistSabotageState.IsCriticalVanillaSabotageActive())
            {
                AbortPlant();
                yield break;
            }

            yield return null;
        }

        _isPlanting = false;

        // Numpad closed: if the code was correct, begin arming; otherwise reset the button.
        if (DemolitionistNumpad.Controller.ConsumePlantSuccess())
        {
            StartArming();
        }
        else
        {
            EffectActive = false;
            Timer = 0f;
        }
    }

    private void StartArming()
    {
        _isPlanting = false;
        _arming = true;

        var delay = OptionGroupSingleton<DemolitionistOptions>.Instance.PlantToSabotageDelay.Value;
        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);
        var armingText = MiraLocaleManager
            .Get("TownOfUsMira.Role.Demolitionist.Notification.Arming")
            .Replace("<seconds>", delay.ToString("0", TownOfUsPlugin.Culture));

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{armingText}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DemolitionistIcon.LoadAsset());

        Button?.OverrideText(
            MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Button.Arming"));


        if (delay > 0f)
        {
            EffectActive = true;
            Timer = delay;
            Coroutines.Start(ArmingWatchCoroutine());
        }
        else
        {
            FireSabotage();
        }
    }

    private IEnumerator ArmingWatchCoroutine()
    {
        while (_arming)
        {
            if (DemolitionistSabotageState.IsCriticalVanillaSabotageActive())
            {
                var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>{MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Notification.CancelledBySabotage")}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.DemolitionistIcon.LoadAsset());
                AbortPlant();
                yield break;
            }

            yield return null;
        }
    }

    public override void OnEffectEnd()
    {
        if (!_arming)
        {
            return;
        }

        FireSabotage();
    }

    private void FireSabotage()
    {
        _arming = false;
        EffectActive = false;
        Button?.OverrideText(Name.ToUpperInvariant());

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            SyncAfterSabotageEnded(startCooldown: false);
            return;
        }

        var duration = OptionGroupSingleton<DemolitionistOptions>.Instance.SabotageDuration;
        DemolitionistSabotageState.RpcPlantSabotage(
            player,
            player.PlayerId,
            _capturedPosition.x,
            _capturedPosition.y,
            duration,
            _capturedConsoleKey,
            (byte)_capturedKind);

        Timer = 0f;
    }

    private void AbortPlant()
    {
        if (DemolitionistNumpad.Controller.InProgress)
        {
            DemolitionistNumpad.Controller.CancelActive();
        }

        EffectActive = false;
        Timer = 0f;
        _isPlanting = false;
        _arming = false;
        Button?.OverrideText(Name.ToUpperInvariant());
    }
}
