using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TimeLordRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IRewindImmune
{
    public override bool IsAffectedByComms => false;
    public bool IgnoredByRewind => false;
    public bool IgnoredByRecording => false;
    public DoomableType DoomHintType => DoomableType.Perception;

    public string IdPart => "TimeLord";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}", "Time Lord");

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rewind", "Rewind"),
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rewind.WikiDescription"),
            TouCrewAssets.RewindSprite)
    ];

    public Color RoleColor => TownOfUsColors.TimeLord;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.TimeLord.LoadAsset(), "TouMira.Role.Crewmate.TimeLord", 1.45f),
        Icon = TouRoleIcons.TimeLord,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        MaxRoleCount = 1,
        IntroSound = TouAudio.TimeLordIntroSound
    };

    [MethodRpc((uint)TownOfUsRpc.TimeLordRewind)]
    public static void RpcStartRewind(PlayerControl timeLord, float duration)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(timeLord);
            return;
        }
        var isTimeLordRole = timeLord.Data?.Role is TimeLordRole;
        var hasTestModifier = timeLord.HasModifier<TestTimeLordModifier>();

        if (!isTimeLordRole && !hasTestModifier)
        {
            Error("RpcStartRewind - Invalid Time Lord");
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (PlayerControl.LocalPlayer)
        {
            try
            {
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.TimeLord.ToTextColor()}{MiraLocaleManager.Get("TownOfUsMira.Role.TimeLordRewindNotif", "Time is being rewound!")}</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.TimeLord.LoadAsset());
                notif.AdjustNotification();
                notif.alphaTimer = duration + 1f;
            }
            catch
            {
               // ignored
            }
        }
        var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 1f, 15f);

        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost &&
(RewindRevive)OptionGroupSingleton<TimeLordOptions>.Instance.ReviveOnRewind.Value != RewindRevive.Disabled)
        {
            var now = DateTime.UtcNow;
            var cutoff = now - TimeSpan.FromSeconds(history);
            var schedule = GameHistory.KilledPlayers
                .Where(x => x.KillTime >= cutoff)
                .Select(x =>
                {
                    var killAge = (float)(now - x.KillTime).TotalSeconds;
                    var triggerAt = Mathf.Clamp(duration * (killAge / history), 0f, duration);
                    return (x.VictimId, KillAgeSeconds: triggerAt);
                })
                .GroupBy(x => x.VictimId)
                .Select(g => g.OrderBy(v => v.KillAgeSeconds).First())
                .ToList();

            TimeLordRewindSystem.ConfigureHostRevives(schedule);
        }
        else
        {
            TimeLordRewindSystem.ConfigureHostRevives(null);
        }

        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost &&
OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind)
        {
            TimeLordRewindSystem.ConfigureHostTaskUndosFromHistory(duration, history);
        }
        else
        {
            TimeLordRewindSystem.ConfigureHostTaskUndos(null);
        }

        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            foreach (var drag in ModifierUtils.GetActiveModifiers<DragModifier>().ToList())
            {
                if (!drag.Player || drag.DeadBody == null)
                {
                    continue;
                }

                UndertakerRole.RpcStopDragging(drag.Player, drag.DeadBody.transform.position);
            }
        }

        TimeLordRewindSystem.StartRewind(timeLord.PlayerId, duration);

        var touAbilityEvent = new TouAbilityEvent(AbilityType.TimeLordRewind, timeLord);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    [MethodRpc((uint)TownOfUsRpc.TimeLordRewindRevive)]
    public static void RpcRewindRevive(PlayerControl revived)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(revived);
            return;
        }
        if (!revived)
        {
            return;
        }

        TimeLordRewindSystem.ReviveFromRewind(revived);
    }

    [MethodRpc((uint)TownOfUsRpc.TimeLordUndoTask)]
    public static void RpcUndoTask(PlayerControl sender, byte targetPlayerId, uint taskId)
    {
        if (sender == null)
        {
            return;
        }
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(sender);
            return;
        }

        TimeLordRewindSystem.UndoTask(targetPlayerId, taskId);
    }

    [MethodRpc((uint)TownOfUsRpc.TimeLordSetDeadBodyPos, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSetDeadBodyPos(PlayerControl sender, byte bodyId, Vector2 position)
    {
        if (sender == null)
        {
            return;
        }
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(sender);
            return;
        }

        var body = Helpers.GetBodyById(bodyId);
        if (body == null)
        {
            return;
        }

        var p = (Vector3)position;
        p.z = p.y / 1000f;
        body.transform.position = p;
    }
}