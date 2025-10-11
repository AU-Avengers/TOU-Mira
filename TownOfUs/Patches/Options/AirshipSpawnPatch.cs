using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Options;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class AirshipSpawnPatch
{
    private static List<StringNames> RemovedSpawns = [];

    [HarmonyPrefix]
    public static void Prefix(SpawnInMinigame __instance)
    {
        if (OptionGroupSingleton<AirshipOptions>.Instance.SpawnMode == AirshipOptions.SpawnModes.HostChoosesOne)
        {
            var location = __instance.Locations.FirstOrDefault(x => x.Name == EnumToType());

            if (location != null)
            {
                __instance.Locations = new([location, location, location]);
            }
        }
        else if (OptionGroupSingleton<AirshipOptions>.Instance.SpawnMode == AirshipOptions.SpawnModes.SameSpawns)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                List<StringNames> picks =
                [
                    StringNames.MainHall, StringNames.Kitchen, StringNames.CargoBay, StringNames.Engine,
                    StringNames.Brig, StringNames.Records
                ];
                picks.Shuffle();

                RemoveSpawns(PlayerControl.LocalPlayer, picks[0], picks[1], picks[2]);
            }

            __instance.Locations = new([.. __instance.Locations.Where(x => !RemovedSpawns.Contains(x.Name))]);
        }
    }

    static StringNames EnumToType()
    {
        return OptionGroupSingleton<AirshipOptions>.Instance.SingleLocation.Value switch
        {
            0 => StringNames.MainHall,
            1 => StringNames.Kitchen,
            2 => StringNames.CargoBay,
            3 => StringNames.Engine,
            4 => StringNames.Brig,
            5 => StringNames.Records,
            _ => StringNames.MainHall
        };
    }

    [MethodRpc((uint)TownOfUsRpc.RemoveSpawns)]
    public static void RemoveSpawns(PlayerControl player, StringNames location, StringNames location2,
        StringNames location3)
    {
        RemovedSpawns = [location, location2, location3];
    }
}