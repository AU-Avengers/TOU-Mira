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
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class UndertakerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not UndertakerRole || Player.HasDied() || !Player.AmOwner ||
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
    public string IdPart => "Undertaker";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Undertaker.LoadAsset(), "TouMira.Role.Impostor.Undertaker", 1.45f),
        UseVanillaKillButton = true,
        CanUseVent = OptionGroupSingleton<UndertakerOptions>.Instance.CanVent,
        Icon = TouRoleIcons.Undertaker,
        OptionsScreenshot = TouBanners.UndertakerRoleBanner,
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Drag", "Drag"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Drag.WikiDescription"),
                    TouImpAssets.DragSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Drop", "Drop"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Drop.WikiDescription"),
                    TouImpAssets.DropSprite)
            ];
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DragBody, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDragging(PlayerControl playerControl, byte bodyId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(playerControl);
            return;
        }
        if (ModifierUtils.GetActiveModifiers<DragModifier>().Any(x => x.BodyId == bodyId))
        {
            return;
        }
        playerControl.GetModifierComponent()?.AddModifier(new DragModifier(bodyId));

        var touAbilityEvent =
            new TouAbilityEvent(AbilityType.UndertakerDrag, playerControl, Helpers.GetBodyById(bodyId));
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrop();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DropBody, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStopDragging(PlayerControl playerControl, Vector2 dropLocation)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(playerControl);
            return;
        }
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