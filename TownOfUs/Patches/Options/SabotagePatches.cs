using HarmonyLib;
using Hazel;
using MiraAPI.GameOptions;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Options;

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.UpdateSystem))]
public static class ReactorPatch
{
    public static bool Prefix(ReactorSystemType __instance, PlayerControl player, MessageReader msgReader)
    {
        var flag = MiscUtils.GetCurrentMap switch
        {
            ActiveMap.Airship => false,
            ActiveMap.Submerged => false,
            ActiveMap.LevelImpostor => false,
            _ => true
        };

        if (!flag)
            return true;

        var b = msgReader.ReadByte();
        var num = (byte)(b & 3);

        if (b == 128 && !__instance.IsActive)
        {
            var seconds = MiscUtils.GetCurrentMap switch
            {
                ActiveMap.Skeld => OptionGroupSingleton<BetterSkeldOptions>.Instance.SaboCountdownReactor,
                ActiveMap.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.SaboCountdownReactor,
                ActiveMap.Polus => OptionGroupSingleton<BetterPolusOptions>.Instance.SaboCountdownReactor,
                ActiveMap.Fungle => OptionGroupSingleton<BetterFungleOptions>.Instance.SaboCountdownReactor,
                _ => __instance.ReactorDuration
            };
            if (seconds >= 15f && seconds <= 90f)
            {
                __instance.Countdown = seconds;
            }
            __instance.UserConsolePairs.Clear();
        }
        else if (b == 16)
            __instance.Countdown = 10000f;
        else if (b.HasAnyBit(64))
        {
            __instance.UserConsolePairs.Add(new(player.PlayerId, num));

            if (__instance.UserCount >= 2)
                __instance.Countdown = 10000f;
        }
        else if (b.HasAnyBit(32))
            __instance.UserConsolePairs.Remove(new(player.PlayerId, num));

        __instance.IsDirty = true;
        return false;
    }
}

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.UpdateSystem))]
public static class O2Patch
{
    public static bool Prefix(LifeSuppSystemType __instance, MessageReader msgReader)
    {
        var flag = MiscUtils.GetCurrentMap switch
        {
            ActiveMap.Skeld => true,
            ActiveMap.MiraHq => true,
            _ => false
        };

        if (!flag)
            return true;

        var b = msgReader.ReadByte();
        var num = b & 3;

        if (b == 128 && !__instance.IsActive)
        {
            var seconds = MiscUtils.GetCurrentMap switch
            {
                ActiveMap.Skeld => OptionGroupSingleton<BetterSkeldOptions>.Instance.SaboCountdownOxygen,
                ActiveMap.MiraHq => OptionGroupSingleton<BetterMiraHqOptions>.Instance.SaboCountdownOxygen,
                _ => __instance.LifeSuppDuration
            };
            if (seconds >= 15f && seconds <= 90f)
            {
                __instance.Countdown = seconds;
            }
            __instance.CompletedConsoles.Clear();
        }
        else if (b == 16)
            __instance.Countdown = 10000f;
        else if (b.HasAnyBit(64))
            __instance.CompletedConsoles.Add(num);

        __instance.IsDirty = true;
        return false;
    }
}

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.UpdateSystem))]
public static class HeliPatch
{
    public static bool Prefix(HeliSabotageSystem __instance, PlayerControl player, MessageReader msgReader)
    {
        if (MiscUtils.GetCurrentMap != ActiveMap.Airship)
            return true;

        var b = msgReader.ReadByte();
        var b2 = (byte)(b & 15);
        var tags = (HeliSabotageSystem.Tags)(b & 240);

        if (tags == HeliSabotageSystem.Tags.FixBit)
        {
            __instance.codeResetTimer = 10f;
            __instance.CompletedConsoles.Add(b2);
        }
        else if (tags == HeliSabotageSystem.Tags.DeactiveBit)
            __instance.ActiveConsoles.Remove(new(player.PlayerId, b2));
        else if (tags == HeliSabotageSystem.Tags.ActiveBit)
            __instance.ActiveConsoles.Add(new(player.PlayerId, b2));
        else if (tags == HeliSabotageSystem.Tags.DamageBit)
        {
            __instance.codeResetTimer = -1f;
            var seconds = OptionGroupSingleton<BetterAirshipOptions>.Instance.SaboCountdownReactor;
            if (seconds >= 15f && seconds <= 90f)
            {
                __instance.Countdown = seconds;
            }
            __instance.CompletedConsoles.Clear();
            __instance.ActiveConsoles.Clear();
        }

        __instance.IsDirty = true;
        return false;
    }
}

[HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.UpdateSystem))]
public static class MixUpPatch
{
    public static bool Prefix(MushroomMixupSabotageSystem __instance, PlayerControl player, MessageReader msgReader)
    {
        if (MiscUtils.GetCurrentMap != ActiveMap.Fungle)
            return true;
        
        MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)msgReader.ReadByte();
        if (operation == MushroomMixupSabotageSystem.Operation.TriggerSabotage)
        {
            __instance.Host_GenerateRandomOutfits();
            __instance.MushroomMixUp();
            __instance.currentState = MushroomMixupSabotageSystem.State.JustTriggered;
            var seconds =
                OptionGroupSingleton<BetterFungleOptions>.Instance.SaboCountdownMixUp;
            if (seconds >= 5f && seconds <= 60f)
            {
                __instance.currentSecondsUntilHeal = seconds;
            }
            __instance.IsDirty = true;
        }
        return false;
    }
}