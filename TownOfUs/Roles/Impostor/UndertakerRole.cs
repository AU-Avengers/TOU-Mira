using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class UndertakerRole(IntPtr cppPtr)
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

        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<UndertakerOptions>.Instance.UndertakerKill ||
                                                     (Player != null && Player.GetModifiers<BaseModifier>()
                                                         .Any(x => x is ICachedRole)) ||
                                                     (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<AltruistRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => TouLocale.Get(TouNames.Undertaker, "Undertaker");
    public string RoleDescription => "Drag Bodies And Hide Them";
    public string RoleLongDescription => "Drag bodies around to hide them from being reported";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanUseVent = OptionGroupSingleton<UndertakerOptions>.Instance.CanVent,
        Icon = TouRoleIcons.Undertaker
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is an Impostor Support role that can drag dead bodies around the map." +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Drag",
            "Drag a dead body, if allowed through settings you can also take it into a vent.",
            TouImpAssets.DragSprite),
        new("Drop",
            "Drop the dragged dead body, stopping it from being dragged any further.",
            TouImpAssets.DropSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.DragBody, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcStartDragging(PlayerControl playerControl, byte bodyId)
    {
        playerControl.GetModifierComponent()?.AddModifier(new DragModifier(bodyId));

        var touAbilityEvent =
            new TouAbilityEvent(AbilityType.UndertakerDrag, playerControl, Helpers.GetBodyById(bodyId));
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrop();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DropBody, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcStopDragging(PlayerControl playerControl, Vector2 dropLocation)
    {
        var dragMod = playerControl.GetModifier<DragModifier>()!;
        var dropPos = (Vector3)dropLocation;
        dropPos.z = dropPos.y / 1000f;
        dragMod.DeadBody!.transform.position = dropPos;

        var touAbilityEvent = new TouAbilityEvent(AbilityType.UndertakerDrop, playerControl, dragMod.DeadBody);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        playerControl.GetModifierComponent()?.RemoveModifier(dragMod);

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
        }
    }
}