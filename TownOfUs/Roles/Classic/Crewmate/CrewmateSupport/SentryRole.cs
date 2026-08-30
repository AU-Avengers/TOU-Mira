using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using System.Text;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Roles;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SentryRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string IdPart => "Sentry";
    public DoomableType DoomHintType => DoomableType.Insight;

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            var abilities = new List<CustomButtonWikiDescription>
            {
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PlaceCamera", "Deploy"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PlaceCamera.WikiDescription"),
                    TouCrewAssets.DeployCamSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PortableCamera", "View"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}PortableCamera.WikiDescription"),
                    TouAssets.CameraSprite)
            };

            return abilities;
        }
    }

    public Color RoleColor => TownOfUsColors.Sentry;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Sentry.LoadAsset(), "TouMira.Role.Crewmate.Sentry", 1.45f),
        Icon = TouRoleIcons.Sentry,
        OptionsScreenshot = TouBanners.SentryRoleBanner,
        IntroSound = TouAudio.SentryIntro,
    };

    [HideFromIl2Cpp] public static List<KeyValuePair<SurvCamera, int>> Cameras { get; set; } = [];

    [HideFromIl2Cpp] public List<Vector2> FutureCameras { get; set; } = [];

    [HideFromIl2Cpp] public bool PortableCamsUnlockedNotified { get; set; }

    [HideFromIl2Cpp] public static HashSet<byte> PortableCamsUsers { get; } = [];

    [HideFromIl2Cpp]
    public static bool AnyPortableCamsInUse => PortableCamsUsers.Count > 0;

    public bool CompletedAllTasks
    {
        get
        {
            GetTaskCounts(Player, out var completed, out var total);
            return total > 0 && completed == total;
        }
    }

    public void LobbyStart()
    {
        Clear();
    }


    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var opts = OptionGroupSingleton<SentryOptions>.Instance;
        var mapId = MiscUtils.GetCurrentMap;
        var mapWithoutCameras = SentryCameraUtilities.IsMapWithoutCameras(mapId);
        var portableImmediate = opts.PortableCamerasMode is SentryPortableCamerasMode.Immediately ||
                                (opts.PortableCamerasMode is SentryPortableCamerasMode.OnMapsWithoutCameras && mapWithoutCameras);
        var portableLine = portableImmediate
            ? MiraLocaleManager.Get("TownOfUsMira.Role.SentryPortableAvailableImmediately", "Portable Cameras are available immediately.")
            : MiraLocaleManager.Get("TownOfUsMira.Role.SentryPortableAvailableAfterTasks", "Portable Cameras are available after tasks.");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<size=60%><color=#BFBFBF>{portableLine}</color></size>");

        var deployedVis = OptionGroupSingleton<SentryOptions>.Instance.DeployedCamerasVisibility;
        if (deployedVis is SentryDeployedCamerasVisibility.AfterMeeting)
        {
            var canMove = OptionGroupSingleton<SentryOptions>.Instance.CanMoveWhilePlacingCameras.Value;
            if (!canMove)
            {
                stringB.AppendLine($"<b><color=#FFAA00>Stand still to deploy sentry-only cameras</color></b>");
            }
            stringB.AppendLine($"<size=60%><color=#BFBFBF>Cameras become public after the next meeting.</color></size>");
        }
        if (Cameras.Count > 0 || FutureCameras.Count > 0)
        {
            var options = OptionGroupSingleton<SentryOptions>.Instance;
            var maxCameras = (int)options.MaxCamerasPlaced;
            var currentCount = Cameras.Count;
            var camerasHeader = maxCameras > 0
                ? MiraLocaleManager.Get("TownOfUsMira.Role.SentryCamerasHeader", "Cameras") + $" ({currentCount}/{maxCameras})"
                : MiraLocaleManager.Get("TownOfUsMira.Role.SentryCamerasHeader", "Cameras");
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{camerasHeader}</b>");

            if (Cameras.Count > 0)
            {
                var idx = 1;
                var duration = (int)OptionGroupSingleton<SentryOptions>.Instance.CameraRoundsLast;
                foreach (var cameraPair in Cameras)
                {
                    var cam = cameraPair.Key;
                    if (cam == null)
                    {
                        continue;
                    }

                    var sr = cam.gameObject?.GetComponent<SpriteRenderer>();
                    var isPending = (sr != null && sr.color.a < 0.99f);

                    var remainingText = duration == 0 ? string.Empty : $" ({duration} rounds)";
                    var cameraPos = cam.transform != null ? cam.transform.position : Vector3.zero;
                    var roomName = cam.NewName != StringNames.None
                        ? TranslationController.Instance.GetString(cam.NewName)
                        : MiscUtils.GetRoomName(cameraPos);
                    if (string.IsNullOrWhiteSpace(roomName))
                    {
                        roomName = $"({cameraPos.x:0.0}, {cameraPos.y:0.0})";
                    }

                    var status = isPending
                        ? $" <size=60%><color=#BFBFBF>{MiraLocaleManager.Get("TownOfUsMira.Role.SentrySentryOnly", "(Sentry-only)")}</color></size>"
                        : string.Empty;
                    stringB.AppendLine(TownOfUsPlugin.Culture, 
                        $"• <b>Cam {idx}</b>: {roomName}{remainingText}{status}");
                    idx++;
                }
            }

            if (FutureCameras.Count > 0)
            {
                foreach (var pos in FutureCameras)
                {
                    var room = MiscUtils.GetRoomName(pos);
                    if (string.IsNullOrWhiteSpace(room))
                    {
                        room = $"({pos.x:0.0}, {pos.y:0.0})";
                    }
                    var placingText = MiraLocaleManager.Get("TownOfUsMira.Role.SentryPlacing", "(Placing...)");
                    stringB.AppendLine(TownOfUsPlugin.Culture, 
                        $"• <color=#BFBFBF>{room} <size=60%>{placingText}</size></color>");
                }
            }
        }

        if (CompletedAllTasks)
        {
            var unlockedText = MiraLocaleManager.Get("TownOfUsMira.Role.SentryPortableCameraUnlocked", "Portable Cameras Unlocked!");
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b><color=#00FF00>{unlockedText}</color></b>");
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (TutorialManager.InstanceExists)
        {
            Clear();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        FutureCameras.Clear();
    }

    public void Clear()
    {
        if (Cameras.Count > 0)
        {
            foreach (var cameraPair in Cameras.Select(x => x.Key))
            {
                if (cameraPair == null)
                {
                    continue;
                }

                Destroy(cameraPair.gameObject);
            }
        }

        FutureCameras.Clear();
        Cameras.Clear();
    }

    public static void ClearAll()
    {
        SentryCameraUtilities.ClearAllCameras();
    }

    public static void ClearPortableCamsUsers()
    {
        PortableCamsUsers.Clear();
    }

    [MethodRpc((uint)TownOfUsRpc.SentryPortableCamsInUse)]
    public static void RpcSentryPortableCamsInUse(PlayerControl player, bool inUse)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        try
        {
            if (player == null) return;
            if (inUse)
            {
                PortableCamsUsers.Add(player.PlayerId);
            }
            else
            {
                PortableCamsUsers.Remove(player.PlayerId);
            }

            SentryCameraPortablePatch.ApplyPortableBlinkState();
        }
        catch
        {
            // ignored
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SentryPlaceCamera)]
    public static void RpcPlaceCamera(PlayerControl player, Vector2 position)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not SentryRole sentry)
        {
            return;
        }

        sentry.FutureCameras.Add(position);
    }

    [MethodRpc((uint)TownOfUsRpc.SentryRevealCamera)]
    public static void RpcRevealCamera(PlayerControl player, Vector2 position, float zAxis)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not SentryRole sentry)
        {
            return;
        }

        if (!sentry.FutureCameras.Contains(position))
        {
            return;
        }

        sentry.FutureCameras.Remove(position);

        var newPos = new Vector3(position.x, position.y, zAxis);
        var camera = SentryCameraUtilities.CreateCameraAtPosition(newPos, player);
        if (camera == null)
        {
            return;
        }

        var rounds = (int)OptionGroupSingleton<SentryOptions>.Instance.CameraRoundsLast;
        Cameras.Add(new(camera, rounds));

        SentryCameraUtilities.EnsureSubmergedUsesPolusSurveillanceIfSentryCamsExist();

        SentryCameraPortablePatch.ApplyPortableBlinkState();
    }

    private static void GetTaskCounts(PlayerControl player, out int completed, out int total)
    {
        completed = 0;
        total = 0;

        if (player == null || player.Data == null)
        {
            return;
        }

        if (player.myTasks != null && player.myTasks.Count > 0)
        {
            var tasks = player.myTasks.ToArray().Where(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
            foreach (var t in tasks)
            {
                total++;
                var taskInfo = player.Data.FindTaskById(t.Id);
                var isComplete = taskInfo != null ? taskInfo.Complete : t.IsComplete;
                if (isComplete)
                {
                    completed++;
                }
            }

            return;
        }

        foreach (var info in player.Data.Tasks)
        {
            total++;
            if (info.Complete)
            {
                completed++;
            }
        }
    }
}