using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Events.TouEvents;
using TownOfUs.GameModes;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Roles.TownOfPolus.Crewmate;

public sealed class PolusEngineerRole(IntPtr cppPtr) : PolusBaseCrewRole(cppPtr), IWikiDiscoverable
{
    public override bool IsAffectedByComms => false;
    public override string LocaleKey => "Engineer";
    public override string RoleName => TouLocale.Get($"TownOfPolusRole{LocaleKey}");
    public override string RoleDescription => TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}IntroBlurb");
    public override string RoleLongDescription => TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TownOfPolusRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public override Color RoleColor => TownOfUsColors.PolusEngineer;

    public override CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        AssociatedGameMode = typeof(TownOfPolusMode),
        GhostRole = (RoleTypes)RoleId.Get<PolusGhostCrewRole>(),
        FreeplayFolder = "Town of Polus",
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(PolusGgAssets.IconEngineer.LoadAsset(), "TownOfPolus.Role.Crewmate.Engineer", 1.45f),
        Icon = PolusGgAssets.IconEngineer
    };

    public static void EngineerFix(PlayerControl engineer)
    {
        switch ((ExpandedMapNames)GameOptionsManager.Instance.currentGameOptions.MapId)
        {
            case ExpandedMapNames.Skeld or ExpandedMapNames.Dleks:
                var comms1 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (comms1.IsActive)
                {
                    FixComms();
                }

                var reactor1 = ShipStatus.Instance.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (reactor1.IsActive)
                {
                    FixReactor(SystemTypes.Reactor);
                }

                var oxygen1 = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
                if (oxygen1.IsActive)
                {
                    FixOxygen();
                }

                var lights1 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights1.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                break;
            case ExpandedMapNames.MiraHq:
                var comms2 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HqHudSystemType>();
                if (comms2.IsActive)
                {
                    FixMiraComms();
                }

                var reactor2 = ShipStatus.Instance.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (reactor2.IsActive)
                {
                    FixReactor(SystemTypes.Reactor);
                }

                var oxygen2 = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
                if (oxygen2.IsActive)
                {
                    FixOxygen();
                }

                var lights2 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights2.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                break;
            case ExpandedMapNames.Polus:
                var comms3 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (comms3.IsActive)
                {
                    FixComms();
                }

                var seismic = ShipStatus.Instance.Systems[SystemTypes.Laboratory].Cast<ReactorSystemType>();
                if (seismic.IsActive)
                {
                    FixReactor(SystemTypes.Laboratory);
                }

                var lights3 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights3.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                break;
            case ExpandedMapNames.Airship:
                var comms4 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (comms4.IsActive)
                {
                    FixComms();
                }

                var reactor = ShipStatus.Instance.Systems[SystemTypes.HeliSabotage].Cast<HeliSabotageSystem>();
                if (reactor.IsActive)
                {
                    FixAirshipReactor();
                }

                var lights4 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights4.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                break;
            case ExpandedMapNames.Fungle:
                var reactor7 = ShipStatus.Instance.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (reactor7.IsActive)
                {
                    FixReactor(SystemTypes.Reactor);
                }

                var comms7 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HqHudSystemType>();
                if (comms7.IsActive)
                {
                    FixMiraComms();
                }

                var mushroom = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage]
                    .Cast<MushroomMixupSabotageSystem>();
                if (mushroom.IsActive)
                {
                    RpcFix(engineer, 1);
                }

                break;
            case ExpandedMapNames.Submerged:
                var reactor5 = ShipStatus.Instance.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (reactor5.IsActive)
                {
                    FixReactor(SystemTypes.Reactor);
                }

                var lights5 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights5.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                var comms5 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (comms5.IsActive)
                {
                    FixComms();
                }

                foreach (var i in PlayerControl.LocalPlayer.myTasks)
                {
                    if (i.TaskType == ModCompatibility.RetrieveOxygenMask)
                    {
                        RpcFix(engineer, 2);
                    }
                }

                break;
            case ExpandedMapNames.LevelImpostor:
                var comms6 = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                if (comms6.IsActive)
                {
                    FixComms();
                }

                var reactor6 = ShipStatus.Instance.Systems[SystemTypes.Reactor].Cast<ReactorSystemType>();
                if (reactor6.IsActive)
                {
                    FixReactor(SystemTypes.Reactor);
                }

                var oxygen6 = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
                if (oxygen6.IsActive)
                {
                    FixOxygen();
                }

                var lights6 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (lights6.IsActive)
                {
                    RpcFix(engineer, 0);
                }

                if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Laboratory, out var seismic1) &&
                    seismic1.Cast<IActivatable>().IsActive)
                {
                    FixReactor(SystemTypes.Laboratory);
                }

                break;
        }
    }

    private static void FixComms()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 0);
    }

    private static void FixMiraComms()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
    }

    private static void FixAirshipReactor()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0);
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
    }

    private static void FixReactor(SystemTypes system)
    {
        ShipStatus.Instance.RpcUpdateSystem(system, 16);
    }

    private static void FixOxygen()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
    }

    [MethodRpc((uint)TownOfUsRpc.TopEngineerFix)]
    private static void RpcFix(PlayerControl engineer, byte type)
    {
        if (engineer.Data.Role is not PolusEngineerRole)
        {
            Error("Invalid engineer");
            return;
        }

        if (type == 0)
        {
            var lights = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            lights.ActualSwitches = lights.ExpectedSwitches;
        }
        else if (type == 1)
        {
            var mushroom = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage]
                .Cast<MushroomMixupSabotageSystem>();
            mushroom.currentSecondsUntilHeal = 0.1f;
        }
        else if (type == 2)
        {
            ModCompatibility.RepairSubOxygen();
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.EngineerFix, engineer);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}