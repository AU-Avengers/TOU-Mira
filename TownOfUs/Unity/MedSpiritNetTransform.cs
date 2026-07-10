using UnityEngine;
using InnerNet;
using Hazel;

namespace TownOfUs.Modules.MedSpirit;

public class MedSpiritNetTransform : InnerNetObject
{
    public Rigidbody2D body;

    public override void ClearOrDecrementDirt()
    {
        // Not needed, but must be implemented
    }

    public override bool Serialize(MessageWriter writer, bool initialState)
    {
        // Not needed, but must be implemented
        return false;
    }

    public override void Deserialize(MessageReader reader, bool initialState)
    {
        // Not needed, but must be implemented
    }

    public override void HandleRpc(byte callId, MessageReader reader)
    {
        // Not needed, but must be implemented
    }
}
