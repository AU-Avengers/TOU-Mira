using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TimeLordRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Perception;

    public string LocaleKey => "TimeLord";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}", "Time Lord");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Rewind", "Rewind"),
            TouLocale.GetParsed($"TouRole{LocaleKey}RewindWikiDescription"),
            TouCrewAssets.RewindSprite)
    ];

    public Color RoleColor => TownOfUsColors.TimeLord;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.TimeLord,
        MaxRoleCount = 1,
        IntroSound = TouAudio.TimeLordIntroSound
    };

    [MethodRpc((uint)TownOfUsRpc.TimeLordRewind)]
    public static void RpcStartRewind(PlayerControl timeLord)
    {
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

        if (PlayerControl.LocalPlayer != null)
        {
            try
            {
                var notif = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.TimeLord.ToTextColor()}{TouLocale.GetParsed("TouRoleTimeLordRewindNotif", "Time is being rewound!")}</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.TimeLord.LoadAsset());
                notif.AdjustNotification();
            }
            catch
            {
               // ignored
            }
        }

        const float duration = 3.5f; var history = Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindHistorySeconds, 0.25f, 120f);

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost &&
OptionGroupSingleton<TimeLordOptions>.Instance.ReviveOnRewind)
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

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost &&
OptionGroupSingleton<TimeLordOptions>.Instance.UndoTasksOnRewind)
        {
            TimeLordRewindSystem.ConfigureHostTaskUndosFromHistory(duration, history);
        }
        else
        {
            TimeLordRewindSystem.ConfigureHostTaskUndos(null);
        }

        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            foreach (var drag in ModifierUtils.GetActiveModifiers<DragModifier>().ToList())
            {
                if (drag?.Player == null || drag.DeadBody == null)
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

        TimeLordRewindSystem.UndoTask(targetPlayerId, taskId);
    }

    [MethodRpc((uint)TownOfUsRpc.TimeLordSetDeadBodyPos, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSetDeadBodyPos(PlayerControl sender, byte bodyId, Vector2 position)
    {
        if (sender == null)
        {
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