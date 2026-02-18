using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Hazel;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using TownOfUs.Options;
using TownOfUs.Utilities;
using ModCompatibility = TownOfUs.Modules.ModCompatibility;

namespace TownOfUs.Networking;

[RegisterCustomRpc((uint)TownOfUsInternalRpc.SendClientModInfo)]
internal sealed class SendClientModInfoRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, Dictionary<byte, string>>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
    public static bool RequireCrowded => ModCompatibility.CrowdedLoaded && OptionGroupSingleton<HostSpecificOptions>.Instance.RequireCrowded.Value;
    public static bool RequireAleLudu => ModCompatibility.AleLuduLoaded && OptionGroupSingleton<HostSpecificOptions>.Instance.RequireAleLudu.Value;
    public static bool RequireSubmerged => ModCompatibility.SubLoaded && OptionGroupSingleton<HostSpecificOptions>.Instance.RequireSubmerged.Value;

    public override void Write(MessageWriter writer, Dictionary<byte, string>? data)
    {
        if (data == null)
        {
            writer.Write((byte)0);
            return;
        }

        writer.Write((byte)data.Count);
        foreach (var kvp in data)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }

    public override Dictionary<byte, string> Read(MessageReader reader)
    {
        var count = reader.ReadByte();
        var data = new Dictionary<byte, string>(count);
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadByte();
            var value = reader.ReadString();
            data[key] = value;
        }

        return data;
    }

    public override void Handle(PlayerControl innerNetObject, Dictionary<byte, string>? data)
    {
        if (data == null || data.Count == 0)
        {
            return;
        }

        ReceiveClientModInfo(innerNetObject, data);
    }

    internal static void ReceiveClientModInfo(PlayerControl client, Dictionary<byte, string> list)
    {
        // Added the original Move Mod to blacklist due to it having (unintended) cheat functionalities (player can still move themselves and zoom out in the game)
        string[] blacklist =
        [
            "MalumMenu", "SickoMenu", "SigmaMenu", "MoveMod", "Move Mod", "Get All Lobbies", "AUSUMMARY - Game Logger: 1.0.0", "AUSUMMARY - Game Logger: 1.1.0", "Mod Menu"
        ];
        string[] whitelist =
        [
            "AuthFix", "GraphicsPlus", "Submerged", "LevelImposter", "VanillaEnhancements", "StringUtils",
            "ModExplorer", "Reactor", "Mini.RegionInstall", "TOU Mira Legacy", "GameNotifier", "Localize Us!",
            "AUSUMMARY - ", "BetterAmongUs", "CrowdedMod", "AleLuduMod"
        ];
        Error(
            $"{client.Data.PlayerName} is joining with the following mods:");
        foreach (var mod in list)
        {
            if (blacklist.Any(x => mod.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                Error(
                    $"{mod.Value} (Cheat Mod? / Incompatible Mod?)");
                continue;
            }
            else if (whitelist.Any(x => mod.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                Info(
                    $"{mod.Value} (Known Mod)");
                continue;
            }
            Warning(
                $"{mod.Value}");
        }

        if (!client.AmOwner && PlayerControl.LocalPlayer.IsHost() && HudManager.InstanceExists)
        {
            var mods = IL2CPPChainloader.Instance.Plugins;
            var modDictionary = new Dictionary<byte, string>();
            modDictionary.Add(0, $"BepInEx " + Paths.BepInExVersion.WithoutBuild());
            byte modByte = 1;
            foreach (var mod in mods)
            {
                modDictionary.Add(modByte, $"{mod.Value.Metadata.Name}: {mod.Value.Metadata.Version}");
                modByte++;
            }
            var newModDictionary = new List<string>();
            var bepChecked = false;
            foreach (var mod in list)
            {
                if (mod.Value.Contains("BepInEx") && !bepChecked)
                {
                    bepChecked = true;
                    continue;
                }
                if (modDictionary.ContainsValue(mod.Value) || whitelist.Any(x => mod.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                newModDictionary.Add(mod.Value);
            }

            var kickPlayer = false;

            var cheatMods = newModDictionary.Where(mod => blacklist.Any(x => mod.Contains(x, StringComparison.OrdinalIgnoreCase))).ToList();
            var playerInfo = GameData.Instance.GetPlayerById(client.PlayerId);
            
            if (cheatMods.Count > 0 && OptionGroupSingleton<HostSpecificOptions>.Instance.KickCheatMods)
            {
                var chatMessageBuilder = new StringBuilder();
                chatMessageBuilder.Append(TouLocale.GetParsed("AnticheatKickChatMessage").Replace("<player>", client.Data.PlayerName));
                foreach (var mod in cheatMods)
                {
                    chatMessageBuilder.Append(TownOfUsPlugin.Culture, $"\n<color=#FF0000>{mod}</color>");
                }
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, $"<color=#D53F42>{TouLocale.Get("AnticheatChatTitle")}</color>", chatMessageBuilder.ToString(), true, altColors:true);
                
                if (playerInfo != null)
                {
                    AmongUsClient.Instance.KickPlayer(playerInfo.ClientId, false);
                    kickPlayer = true;
                }
            }
            else if (newModDictionary.Count > 0 && OptionGroupSingleton<HostSpecificOptions>.Instance.AntiCheatWarnings)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed("AnticheatMessage").Replace("<player>", client.Data.PlayerName)}");
                foreach (var mod in newModDictionary)
                {
                    if (blacklist.Any(x => mod.Contains(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        stringBuilder.Append(TownOfUsPlugin.Culture, $"\n<color=#FF0000>{mod}</color>");
                        continue;
                    }
                    stringBuilder.Append(TownOfUsPlugin.Culture, $"\n{mod}");
                }
                MiscUtils.AddFakeChat(client.Data, $"<color=#D53F42>{TouLocale.Get("AnticheatChatTitle")}</color>", stringBuilder.ToString(), true, altColors:true);
            }
            if (playerInfo != null && !kickPlayer)
            {
                if (!RequireCrowded && !RequireAleLudu && !RequireSubmerged)
                {
                    return;
                }

                var requiredMods = new List<string>();
                if (RequireCrowded)
                {
                    requiredMods.Add("CrowdedMod");
                }
                if (RequireAleLudu)
                {
                    requiredMods.Add("AleLuduMod");
                }
                if (RequireSubmerged)
                {
                    requiredMods.Add("Submerged");
                }

                var reqModDictionary = new List<string>();
                foreach (var mod in list)
                {
                    if (!requiredMods.Any(x => mod.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    reqModDictionary.Add(mod.Value);
                }

                if (reqModDictionary.Count == requiredMods.Count)
                {
                    return;
                }
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed("AnticheatKickMissingMessage").Replace("<player>", client.Data.PlayerName)}");
                foreach (var mod in requiredMods)
                {
                    if (!reqModDictionary.Any(x => mod.Contains(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        stringBuilder.Append(TownOfUsPlugin.Culture, $"\n<color=#FF0000>{mod}</color>");
                        continue;
                    }
                    stringBuilder.Append(TownOfUsPlugin.Culture, $"\n{mod}");
                }
                MiscUtils.AddFakeChat(client.Data, $"<color=#D53F42>{TouLocale.Get("SystemChatTitle")}</color>", stringBuilder.ToString(), true, altColors:true);
                kickPlayer = true;
                AmongUsClient.Instance.KickPlayer(playerInfo.ClientId, false);
            }

            if (kickPlayer)
            {
                Error($"Player was kicked!");
            }
        }
    }
}