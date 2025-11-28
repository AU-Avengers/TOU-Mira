using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace TownOfUs.Networking;

[MessageConverter]
public class ByteStringDictionaryMessageConverter : MessageConverter<Dictionary<byte, string>?>
{
    public override Dictionary<byte, string>? Read(MessageReader reader, Type objectType)
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

    public override void Write(MessageWriter writer, Dictionary<byte, string>? value)
    {
        if (value == null)
        {
            writer.Write((byte)0);
            return;
        }

        writer.Write((byte)value.Count);
        foreach (var kvp in value)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }
}