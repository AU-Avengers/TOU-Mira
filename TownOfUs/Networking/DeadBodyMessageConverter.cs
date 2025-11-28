using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace TownOfUs.Networking;

[MessageConverter]
public class DeadBodyMessageConverter : MessageConverter<DeadBody?>
{
    public override DeadBody? Read(MessageReader reader, Type objectType)
    {
        var target_byte = reader.ReadByte();
        if (target_byte == 255)
        {
            return null;
        }

        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target_byte);
        return body;
    }

    public override void Write(MessageWriter writer, DeadBody? value)
    {
        writer.Write(value != null ? value.ParentId : (byte)255);
    }
}