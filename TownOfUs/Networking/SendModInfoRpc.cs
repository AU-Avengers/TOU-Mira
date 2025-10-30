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
        Error(
            $"{client.Data.PlayerName} is joining with the following mods:");
        foreach (var mod in list)
        {
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
            foreach (var mod in list)
            {
                if (modDictionary.ContainsValue(mod.Value) || mod.Value.Contains("BepInEx"))
                {
                    continue;
                }
                newModDictionary.Add(mod.Value);
            }

            if (newModDictionary.Count > 0 && OptionGroupSingleton<HostSpecificOptions>.Instance.AntiCheatWarnings)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(TownOfUsPlugin.Culture, $"{client.Data.PlayerName} is joining with these different mods:");
                foreach (var mod in newModDictionary)
                {
                    stringBuilder.Append(TownOfUsPlugin.Culture, $"\n{mod}");
                }
                MiscUtils.AddFakeChat(client.Data, "<color=#D53F42>Anticheat System</color>", stringBuilder.ToString(), true, altColors:true);
            }
        }
    }
}