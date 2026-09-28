using System.Collections;
using System.Collections.Generic;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using MiraAPI.Networking;
using MiraAPI.Translation;

using Reactor.Networking.Attributes;

using Reactor.Utilities;

using Reactor.Utilities.Extensions;

using TownOfUs.Assets;
using TownOfUs.Buttons.Neutral.NeutralEvil;
using TownOfUs.Modules;
using TownOfUs.Utilities;

using TownOfUs.Options;

using TownOfUs.Roles.Neutral.NeutralEvil;

using TownOfUs.Networking;

using TownOfUs.Utilities;

using UnityEngine;



namespace TownOfUs.Patches;



/// <summary>
/// Shared state + RPC entry points for the Demolitionist sabotage.
/// </summary>
public static class DemolitionistSabotageState

{

    private const byte NoPlayer = byte.MaxValue;



    public static bool IsActive { get; private set; }

    public static byte DemolitionistId { get; private set; } = NoPlayer;

    public static Vector2 PlantedPosition { get; private set; }

    public static int PlantedConsoleKey { get; private set; }

    public static DemolitionistUtilityKind PlantedUtilityKind { get; private set; }

    public static string PlantedLocationName { get; private set; } = string.Empty;

    public static float TimeRemaining { get; private set; }

    public static int SuccessfulSabotages { get; private set; }

    public static int FlashPulseIndex { get; private set; }



    public static bool LocalPlantInProgress { get; set; }

    public static bool LocalDefuseInProgress { get; set; }

    public static readonly Color SecondaryColor = new Color32(0xF9, 0xA1, 0x23, 255);

    private static readonly Color DefuseFlashColor = Palette.AcceptedGreen;

    private static readonly Color ExplosionFlashColor = Palette.ImpostorRed;

    private static readonly List<(int ConsoleKey, DemolitionistUtilityKind Kind)> DisabledUtilities = new();

    private static DemolitionistSabotageTask? _localTask;

    private static ArrowBehaviour? _arrow;

    private static Coroutine? _flashRoutine;

    private static AudioSource? _sabotageSoundSource;

    private static bool _tickRunning;

    private static Material? _consoleOutlineMat;



    public static void RegisterDemolitionist(PlayerControl demolitionist)

    {

        DemolitionistId = demolitionist.PlayerId;

    }



    public static void ResetAll(bool clearProgress = true)

    {

        ClearActiveSabotage();

        if (clearProgress)
        {
            SuccessfulSabotages = 0;
        }

        DemolitionistId = NoPlayer;

        LocalPlantInProgress = false;

        LocalDefuseInProgress = false;

        FlashPulseIndex = 0;

        DisabledUtilities.Clear();

        DemolitionistUtilityConsoles.InvalidateCache();
        DemolitionistNumpad.Controller.ResetAll();

    }

    public static void StopForMeeting()
    {
        DemolitionistPlantButton.CancelPlantOrArmingForMeeting();

        if (!IsActive)
        {
            return;
        }

        if (DemolitionistNumpad.Controller.InProgress)
        {
            DemolitionistNumpad.Controller.CancelActive();
        }

        LocalPlantInProgress = false;
        LocalDefuseInProgress = false;

        ClearActiveSabotage();
        DemolitionistPlantButton.SyncAfterSabotageEnded(startCooldown: true);
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _) => StopForMeeting();

    public static bool IsUtilityDisabled(int consoleKey, DemolitionistUtilityKind kind)
    {
        foreach (var entry in DisabledUtilities)
        {
            if (entry.ConsoleKey == consoleKey && entry.Kind == kind)
            {
                return true;
            }
        }

        return false;
    }

    private static void RegisterDisabledUtility(int consoleKey, DemolitionistUtilityKind kind)
    {
        if (IsUtilityDisabled(consoleKey, kind))
        {
            return;
        }

        DisabledUtilities.Add((consoleKey, kind));
    }



    public static bool IsCriticalVanillaSabotageActive()
    {
        var system = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        return system is { AnyActive: true };
    }

    public static bool TryGetPlantedWorldPosition(out Vector3 worldPos)
    {
        return DemolitionistUtilityConsoles.TryGetWorldPosition(PlantedUtilityKind, PlantedPosition, out worldPos);
    }



    [MethodRpc((uint)TownOfUsRpc.DemolitionistPlantSabotage)]
    public static void RpcPlantSabotage(
        PlayerControl sender, byte demolitionistId, float x, float y, float duration, int consoleKey, byte utilityKind)
    {
        var resolvedDemolitionistId = sender != null ? sender.PlayerId : demolitionistId;
        PlantSabotage(resolvedDemolitionistId, new Vector2(x, y), duration, consoleKey,
            (DemolitionistUtilityKind)utilityKind);
    }

    [MethodRpc((uint)TownOfUsRpc.DemolitionistDefuseSabotage)]
    public static void RpcDefuseSabotage(PlayerControl sender, byte defuserId)
    {
        var resolvedDefuserId = sender != null ? sender.PlayerId : defuserId;
        DefuseSabotage(resolvedDefuserId);
    }



    [MethodRpc((uint)TownOfUsRpc.DemolitionistSabotageExpired)]

    public static void RpcSabotageExpired(PlayerControl sender)

    {

        ExpireSabotage();

    }



    private static void PlantSabotage(
        byte demolitionistId, Vector2 position, float duration, int consoleKey, DemolitionistUtilityKind utilityKind)

    {

        if (IsActive) ClearActiveSabotage();

        DemolitionistUtilityConsoles.InvalidateCache();

        IsActive = true;

        DemolitionistId = demolitionistId;

        PlantedPosition = position;

        PlantedConsoleKey = consoleKey;

        PlantedUtilityKind = utilityKind;

        PlantedLocationName = DemolitionistUtilityConsoles.GetDisplayName(utilityKind);

        TimeRemaining = duration;

        FlashPulseIndex = 0;

        DemolitionistUtilityConsoles.InvalidateCache();

        CreateArrowToTarget();

        StartSabotageAlarm();

        StartFlash();

        EnsureLocalSabotageTask();



        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        var notificationText = MiraLocaleManager
            .Get("TownOfUsMira.Role.Demolitionist.Notification.SabotageActive")
            .Replace("<location>", PlantedLocationName);

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{notificationText}</color></b>",

            Color.white,

            new Vector3(0f, 1f, -20f),

            spr: DivaniAssets.DemolitionistIcon.LoadAsset());

        EnsureTickRunning();

    }

    private static void DefuseSabotage(byte defuserId)

    {

        if (!IsActive) return;

        var location = PlantedLocationName;

        StopFlash();

        PlayResultFlash(DefuseFlashColor);

        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        var notificationText = MiraLocaleManager
            .Get("TownOfUsMira.Role.Demolitionist.Notification.SabotageDefused")
            .Replace("<location>", location);

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{notificationText}</color></b>",

            Color.white,

            new Vector3(0f, 1f, -20f),

            spr: DivaniAssets.DemolitionistIcon.LoadAsset());

        ClearActiveSabotage();
        DemolitionistPlantButton.SyncAfterSabotageEnded(startCooldown: true);
    }

    private static void ExpireSabotage()

    {

        if (!IsActive) return;

        var location = PlantedLocationName;

        var consoleKey = PlantedConsoleKey;

        var kind = PlantedUtilityKind;

        SuccessfulSabotages++;

        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;

        var remaining = Mathf.Max(0, needed - SuccessfulSabotages);

        StopFlash();

        PlayResultFlash(ExplosionFlashColor);

        PlayExplosionSound();

        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        var key = remaining > 0
            ? "TownOfUsMira.Role.Demolitionist.Notification.Exploded"
            : "TownOfUsMira.Role.Demolitionist.Notification.GoalReached";

        var notificationText = MiraLocaleManager
            .Get(key)
            .Replace("<location>", location)
            .Replace("<count>", SuccessfulSabotages.ToString(TownOfUsPlugin.Culture))
            .Replace("<needed>", needed.ToString(TownOfUsPlugin.Culture));

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{notificationText}</color></b>",

            Color.white,

            new Vector3(0f, 1f, -20f),

            spr: DivaniAssets.DemolitionistIcon.LoadAsset());

        if (OptionGroupSingleton<DemolitionistOptions>.Instance.DisableExplodedConsoles)

        {

            RegisterDisabledUtility(consoleKey, kind);

            EjectFromExplodedUtility(kind);

        }

        KillLocalIfDefusing();

        ClearActiveSabotage();
        DemolitionistPlantButton.SyncAfterSabotageEnded(startCooldown: true);
    }

    private static void KillLocalIfDefusing()
    {
        if (!OptionGroupSingleton<DemolitionistOptions>.Instance.ExplosionKillsDefusers)
        {
            return;
        }

        if (!DemolitionistDefuseButton.IsLocalDefusing)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead)
        {
            return;
        }

        if (DemolitionistNumpad.Controller.InProgress)
        {
            DemolitionistNumpad.Controller.CancelActive();
        }

        local.RpcSpecialMurder(
            local,
            MeetingCheck.OutsideMeeting,
            isIndirect: true,
            ignoreShield: false,
            didSucceed: true,
            resetKillTimer: false,
            createDeadBody: true,
            teleportMurderer: false,
            showKillAnim: false,
            playKillSound: true,
            causeOfDeath: "Demolitionist");
    }

    private static void EjectFromExplodedUtility(DemolitionistUtilityKind kind)
    {
        switch (kind)
        {
            case DemolitionistUtilityKind.Admin:
                var map = MapBehaviour.Instance;
                if (map != null && map.IsOpen)
                {
                    map.Close();
                }

                break;
            case DemolitionistUtilityKind.Cameras:
            case DemolitionistUtilityKind.Vitals:
            case DemolitionistUtilityKind.DoorLog:
                var mg = Minigame.Instance;
                if (mg == null)
                {
                    return;
                }

                var matches = kind switch
                {
                    DemolitionistUtilityKind.Cameras => mg.TryCast<SurveillanceMinigame>() != null
                                                        || mg.TryCast<PlanetSurveillanceMinigame>() != null,
                    DemolitionistUtilityKind.Vitals => mg.TryCast<VitalsMinigame>() != null,
                    DemolitionistUtilityKind.DoorLog => mg.TryCast<SecurityLogGame>() != null,
                    _ => false,
                };

                if (matches)
                {
                    mg.Close();
                }

                break;
        }
    }

    private static void ClearActiveSabotage()

    {
        IsActive = false;

        TimeRemaining = 0f;

        PlantedLocationName = string.Empty;

        PlantedConsoleKey = 0;

        PlantedUtilityKind = DemolitionistUtilityKind.None;



        RemoveLocalSabotageTask();

        StopSabotageAlarm();

        StopFlash();

        ClearConsoleOutline();



        if (_arrow != null && !_arrow.IsDestroyedOrNull())

        {

            _arrow.gameObject.Destroy();

            _arrow = null;

        }

    }



    public static void Tick()

    {

        if (!IsActive)

        {

            return;

        }



        TimeRemaining -= Time.deltaTime;



        if (TimeRemaining <= 0f)

        {

            TimeRemaining = 0f;

            var local = PlayerControl.LocalPlayer;

            if (local != null && local.PlayerId == DemolitionistId)

            {

                RpcSabotageExpired(local);

            }

            return;

        }



        UpdateArrow();

    }



    /// <summary>
    /// Same proximity rules as Plant: must be at the planted utility console.
    /// </summary>
    public static bool IsLocalPlayerAtPlantedConsole()

    {

        if (!IsActive) return false;



        var local = PlayerControl.LocalPlayer;

        if (local == null) return false;



        return DemolitionistUtilityConsoles.IsAtPlantedUtility(
            local, PlantedUtilityKind, PlantedPosition, PlantedConsoleKey);

    }

    public static bool IsLocalPlayerInPlantedConsoleUseRange()
    {
        if (!IsActive) return false;

        var local = PlayerControl.LocalPlayer;
        if (local == null) return false;

        return DemolitionistUtilityConsoles.IsLocalPlayerInPlantedConsoleUseRange(
            local, PlantedUtilityKind, PlantedPosition);
    }

    public static void UpdatePlantedConsoleOutline()
    {
        if (!IsActive)
        {
            ClearConsoleOutline();
            return;
        }

        if (!DemolitionistUtilityConsoles.TryGetPlantedConsoleRenderer(PlantedUtilityKind, PlantedPosition,
                out var rend)
            || rend == null)
        {
            return;
        }

        var mat = rend.material;
        if (mat == null)
        {
            return;
        }

        _consoleOutlineMat = mat;

        var color = DemolitionistRole.DemolitionistColor;
        var near = IsLocalPlayerAtPlantedConsole();

        if (mat.HasProperty("_Outline"))
        {
            mat.SetFloat("_Outline", near ? 1f : 0f);
        }

        if (mat.HasProperty("_OutlineColor"))
        {
            mat.SetColor("_OutlineColor", color);
        }

        if (mat.HasProperty("_AddColor"))
        {
            mat.SetColor("_AddColor", near ? color : Color.clear);
        }
    }

    private static void ClearConsoleOutline()
    {
        if (_consoleOutlineMat == null)
        {
            return;
        }

        if (_consoleOutlineMat.HasProperty("_Outline"))
        {
            _consoleOutlineMat.SetFloat("_Outline", 0f);
        }

        if (_consoleOutlineMat.HasProperty("_AddColor"))
        {
            _consoleOutlineMat.SetColor("_AddColor", Color.clear);
        }

        _consoleOutlineMat = null;
    }



    private static void EnsureTickRunning()

    {

        if (_tickRunning) return;

        _tickRunning = true;

        Coroutines.Start(CoTick());

    }



    private static IEnumerator CoTick()

    {

        while (IsActive)

        {

            Tick();

            yield return null;

        }

        _tickRunning = false;

    }



    private static void EnsureLocalSabotageTask()

    {

        var local = PlayerControl.LocalPlayer;

        if (local == null) return;



        RemoveLocalSabotageTask();



        var go = new GameObject("DemolitionistSabotageTask");

        go.transform.SetParent(local.transform);

        _localTask = go.AddComponent<DemolitionistSabotageTask>();

        _localTask.Owner = local;

        _localTask.Initialize();

        local.myTasks.Add(_localTask);

    }



    private static void RemoveLocalSabotageTask()

    {

        if (_localTask == null) return;

        _localTask.Complete();

        _localTask = null;

    }



    private static void CreateArrowToTarget()

    {

        var local = PlayerControl.LocalPlayer;

        if (local == null) return;



        if (_arrow != null && !_arrow.IsDestroyedOrNull())

        {

            _arrow.gameObject.Destroy();

            _arrow = null;

        }



        _arrow = MiscUtils.CreateArrow(local.transform, GetCurrentPulseColor());

        _arrow.target = GetArrowTarget();

        _arrow.Update();

        ApplyArrowColor();

    }



    private static void UpdateArrow()

    {

        if (_arrow == null || _arrow.IsDestroyedOrNull() || !IsActive) return;



        _arrow.target = GetArrowTarget();

        _arrow.Update();

        ApplyArrowColor();

    }



    public static Color GetCurrentPulseColor()
    {
        if (!LocalSettingsTabSingleton<TouLocalTabGameplay>.Instance.DemolitionistAlternatingColors.Value)
        {
            return DemolitionistRole.DemolitionistColor;
        }

        return (FlashPulseIndex & 1) == 0 ? DemolitionistRole.DemolitionistColor : SecondaryColor;
    }

    private static void ApplyArrowColor()

    {

        if (_arrow == null || _arrow.IsDestroyedOrNull() || _arrow.image == null)

        {

            return;

        }

        _arrow.image.color = GetCurrentPulseColor();

    }



    private static Vector3 GetArrowTarget()

    {

        if (TryGetPlantedWorldPosition(out var worldPos))

        {

            return worldPos;

        }



        var local = PlayerControl.LocalPlayer;

        var z = local != null ? local.transform.position.z : 0f;

        return new Vector3(PlantedPosition.x, PlantedPosition.y, z);

    }



    private static void StartSabotageAlarm()

    {

        StopSabotageAlarm();

        if (!SoundManager.Instance || !ShipStatus.Instance) return;



        try

        {

            var clip = ShipStatus.Instance.SabotageSound;

            if (clip == null) return;



            _sabotageSoundSource = SoundManager.Instance.PlaySound(clip, false, 1f);

            if (_sabotageSoundSource != null)

            {

                _sabotageSoundSource.loop = true;

                if (!_sabotageSoundSource.isPlaying)

                {

                    _sabotageSoundSource.Play();

                }

            }

        }

        catch

        {

            // SabotageSound field name can differ between AU versions.

        }

    }



    private static void StopSabotageAlarm()

    {

        if (_sabotageSoundSource != null)

        {

            _sabotageSoundSource.Stop();

            _sabotageSoundSource = null;

        }

    }



    private static void StartFlash()

    {

        StopFlash();

        if (!HudManager.Instance) return;

        FlashPulseIndex = 0;

        _flashRoutine = HudManager.Instance.StartCoroutine(CoAlternateFlash().WrapToIl2Cpp());

    }



    private static void StopFlash()

    {

        if (_flashRoutine != null && HudManager.Instance)

        {

            HudManager.Instance.StopCoroutine(_flashRoutine);

            _flashRoutine = null;

        }

        if (MiscUtils.FlashRenderer != null)
        {
            MiscUtils.FlashRenderer.enabled = false;
            MiscUtils.FlashRenderer.gameObject.SetActive(false);
        }

    }



    private static IEnumerator CoAlternateFlash()

    {

        while (IsActive)

        {

            var color = GetCurrentPulseColor();

            FlashPulseIndex++;

            yield return MiscUtils.CoFlash(color, waitfor: 0.5f, alpha: 0.35f);

        }

    }



    private static string? GetPlayerName(byte playerId)

    {

        foreach (var pc in PlayerControl.AllPlayerControls)

        {

            if (pc != null && pc.PlayerId == playerId && pc.Data != null)

            {

                return pc.Data.PlayerName;

            }

        }

        return null;

    }



    private static void PlayResultFlash(Color color)

    {

        if (!HudManager.Instance)

        {

            return;

        }

        Coroutines.Start(CoResultFlash(color));

    }

    private static IEnumerator CoResultFlash(Color color)

    {

        yield return MiscUtils.CoFlash(color, waitfor: 0.75f, alpha: 0.4f);

    }

    private static void PlayExplosionSound()

    {

        if (!SoundManager.Instance)

        {

            return;

        }

        try

        {

            var clip = DivaniAssets.DemolitionistExplosionSound.LoadAsset();

            if (clip != null)

            {

                SoundManager.Instance.PlaySound(clip, false, 1f);

            }

        }

        catch

        {

            // Optional SFX — ignore load failures.

        }

    }
}
