using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class MinerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    [HideFromIl2Cpp] public List<Vent> Vents { get; set; } = [];

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not MinerRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<MinerOptions>.Instance.MinerKill ||
                                                     (Player != null && Player.GetModifiers<BaseModifier>()
                                                         .Any(x => x is ICachedRole)) ||
                                                     (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Miner, "Miner");
    public string RoleDescription => "From The Top, Make It Drop, That's A Vent";
    public string RoleLongDescription => "Place interconnected vents around the map";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Miner,
        OptionsScreenshot = TouImpAssets.MinerRoleBanner,
        IntroSound = TouAudio.MineSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (OptionGroupSingleton<MinerOptions>.Instance.MineVisibility is MineVisiblityOptions.AfterUse)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"Vents will only be visible once used");
        }

        return stringB;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Mine",
            "Place a vent where you are standing. These vents won't connect to already existing vents on the map but with each other.",
            TouImpAssets.MineSprite)
    ];

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is an Impostor Support role that can create vents." + MiscUtils.AppendOptionsText(GetType());
    }

    [MethodRpc((uint)TownOfUsRpc.PlaceVent, SendImmediately = true)]
    public static void RpcPlaceVent(PlayerControl player, int ventId, Vector2 position, float zAxis, bool immediate)
    {
        if (player.Data.Role is not MinerRole miner)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlaceVent - Invalid miner");
            return;
        }

        //Logger<TownOfUsPlugin>.Error("RpcPlaceVent");

        var ventPrefab = ShipStatus.Instance.AllVents[0];
        var vent = Instantiate(ventPrefab, ventPrefab.transform.parent);
        vent.name = $"MinerVent-{player.PlayerId}-{ventId}";

        Logger<TownOfUsPlugin>.Error($"RpcPlaceVent - vent: {vent.name} - {immediate}");

        if (!player.AmOwner && !immediate)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlaceVent - Hide Vent");
            vent.myRend.enabled = false;
        }

        vent.Id = ventId;
        vent.transform.position = new Vector3(position.x, position.y, zAxis);

        if (miner == null)
        {
            return;
        }

        if (miner.Vents.Count > 0)
        {
            var leftVent = miner.Vents[^1];
            vent.Left = leftVent;
            leftVent.Right = vent;
        }
        else
        {
            vent.Left = null;
        }

        vent.Right = null;
        vent.Center = null;

        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();

        miner.Vents.Add(vent);
        
        PlainShipRoom? plainShipRoom = null;

        var allRooms2 = ShipStatus.Instance.FastRooms;
        foreach (var plainShipRoom2 in allRooms2.Values)
        {
            if (plainShipRoom2.roomArea && plainShipRoom2.roomArea.OverlapPoint(vent.transform.position))
            {
                plainShipRoom = plainShipRoom2;
            }
        }
        
        var mapId = (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        if (TutorialManager.InstanceExists)
        {
            mapId = (MapNames)AmongUsClient.Instance.TutorialMapId;
        }

        if (mapId is MapNames.Polus && plainShipRoom?.RoomId is SystemTypes.Weapons)
        {
            vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                vent.gameObject.transform.position.y, -0.0209f);
        }

        if (ModCompatibility.SubLoaded)
        {
            vent.gameObject.layer = 12;
            vent.gameObject.AddSubmergedComponent("ElevatorMover"); // just in case elevator vent is not blocked
            if (vent.gameObject.transform.position.y > -7)
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                    vent.gameObject.transform.position.y, 0.03f);
            }
            else
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x,
                    vent.gameObject.transform.position.y, 0.0009f);
                vent.gameObject.transform.localPosition = new Vector3(vent.gameObject.transform.localPosition.x,
                    vent.gameObject.transform.localPosition.y, -0.003f);
            }
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.MinerPlaceVent, player, vent);
        MiraEventManager.InvokeEvent(touAbilityEvent);
        if (immediate)
        {
            var touAbilityEvent2 = new TouAbilityEvent(AbilityType.MinerRevealVent, player, vent);
            MiraEventManager.InvokeEvent(touAbilityEvent2);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.ShowVent, SendImmediately = true)]
    public static void RpcShowVent(PlayerControl player, int ventId)
    {
        if (player.Data.Role is not MinerRole miner)
        {
            Logger<TownOfUsPlugin>.Error("RpcShowVent - Invalid miner");
            return;
        }

        var vent = miner.Vents.FirstOrDefault(x => x.Id == ventId);

        if (vent != null)
        {
            vent.myRend.enabled = true;

            var touAbilityEvent = new TouAbilityEvent(AbilityType.MinerRevealVent, player, vent);
            MiraEventManager.InvokeEvent(touAbilityEvent);
        }
    }
}