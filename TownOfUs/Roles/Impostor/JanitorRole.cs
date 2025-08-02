using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class JanitorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not JanitorRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<JanitorOptions>.Instance.JanitorKill ||
                                                     (Player != null && Player.GetModifiers<BaseModifier>()
                                                         .Any(x => x is ICachedRole)) ||
                                                     (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<DetectiveRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => TouLocale.Get(TouNames.Janitor, "Janitor");
    public string RoleDescription => "Sanitize The Ship";

    public string RoleLongDescription => "Clean bodies to hide kills" +
                                         (OptionGroupSingleton<JanitorOptions>.Instance.CleanDelay == 0
                                             ? string.Empty
                                             : "\n<b>You must stay next to the body while cleaning.</b>");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Janitor,
        IntroSound = TouAudio.JanitorCleanSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is an Impostor Support role that can clean dead bodies." +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Clean",
            "Clean a dead body, making it disapear and making it unreportable.",
            TouImpAssets.CleanButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.CleanBody, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcCleanBody(PlayerControl player, byte bodyId)
    {
        if (player.Data.Role is not JanitorRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCleanBody - Invalid Janitor");
            return;
        }

        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            var touAbilityEvent = new TouAbilityEvent(AbilityType.JanitorClean, player, body);
            MiraEventManager.InvokeEvent(touAbilityEvent);

            Coroutines.Start(body.CoClean());
            Coroutines.Start(CrimeSceneComponent.CoClean(body));
        }
    }
}