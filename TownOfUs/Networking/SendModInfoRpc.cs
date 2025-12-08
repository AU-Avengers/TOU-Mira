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

namespace TownOfUs.Networking;

[RegisterCustomRpc((uint)TownOfUsInternalRpc.SendClientModInfo)]
internal sealed class SendClientModInfoRpc(TownOfUsPlugin plugin, uint id)
    : PlayerCustomRpc<TownOfUsPlugin, Dictionary<byte, string>>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

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
        string[] blacklist = ["MalumMenu", "SickoMenu", "SigmaMenu", "MoveModPublic: 1.0.0-dev+b18db3c689edef84d1f433480a91ce4ae0154060", "MoveModPublic: 1.0.0-dev+94ede73cbba272aae2cd118cbc7a69d9df779e6b", "MoveMod", "Move Mod"];
        Error(
            $"{client.Data.PlayerName} is joining with the following mods:");
        foreach (var mod in list)
        {
            if (blacklist.Any(x => mod.Value.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                Error(
                    $"{mod.Value} (Cheat Mod?)");
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
                if (modDictionary.ContainsValue(mod.Value))
                {
                    continue;
                }
                newModDictionary.Add(mod.Value);
            }

            var cheatMods = newModDictionary.Where(mod => blacklist.Any(x => mod.Contains(x, StringComparison.OrdinalIgnoreCase))).ToList();
            
            if (cheatMods.Count > 0 && OptionGroupSingleton<HostSpecificOptions>.Instance.KickCheatMods)
            {
                var chatMessageBuilder = new StringBuilder();
                chatMessageBuilder.Append(TouLocale.GetParsed("AnticheatKickChatMessage").Replace("<player>", client.Data.PlayerName));
                foreach (var mod in cheatMods)
                {
                    chatMessageBuilder.Append(TownOfUsPlugin.Culture, $"\n<color=#FF0000>{mod}</color>");
                }
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, $"<color=#D53F42>{TouLocale.Get("AnticheatChatTitle")}</color>", chatMessageBuilder.ToString(), true, altColors:true);
                
                var playerInfo = GameData.Instance.GetPlayerById(client.PlayerId);
                if (playerInfo != null)
                {
                    AmongUsClient.Instance.KickPlayer(playerInfo.ClientId, false);
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
        }
    }
}