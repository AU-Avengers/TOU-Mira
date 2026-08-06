using Hazel;
using InnerNet;
using Reactor.Utilities.Attributes;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace TownOfUs.Modules.MedSpirit;

[RegisterInIl2Cpp]
public sealed class MedSpiritNetTransform(nint cppPtr) : InnerNetObject(cppPtr)
{
    private bool isPaused;
    private Rigidbody2D body;
    private readonly Queue<Vector2> sendQueue = new();
    private readonly Queue<Vector2> incomingPosQueue = new();
    private float rubberbandModifier = 1f;
    private float idealSpeed;
    private ushort lastSequenceId;
    private Vector2 lastPosition;
    private Vector2 lastPosSent;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        lastPosSent = transform.position;
        incomingPosQueue.Enqueue(transform.position);
    }
    
    public override void ClearOrDecrementDirt()
    {
        if (DirtyBits > 0U)
        {
            DirtyBits -= 1U;
        }
    }

    public void OnEnable()
    {
        SetDirtyBit(3U);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void Halt()
    {
        var minSid = (ushort)(lastSequenceId + 1);
        SnapTo(transform.position, minSid);
    }

    public void RpcSnapTo(Vector2 position)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            SnapTo(position, (ushort)(lastSequenceId + 1));
        }
        var num = (ushort)(lastSequenceId + 2);
        var messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 21, SendOption.Reliable);
        NetHelpers.WriteVector2(position, messageWriter);
        messageWriter.Write(num);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public void SnapTo(Vector2 position)
    {
        var minSid = (ushort)(lastSequenceId + 2);
        SnapTo(position, minSid);
    }

    public void ClearPositionQueues()
    {
        if (AmOwner)
        {
            sendQueue.Clear();
            return;
        }
        incomingPosQueue.Clear();
    }

    private void SnapTo(Vector2 position, ushort minSid)
    {
        if (!NetHelpers.SidGreaterThan(minSid, lastSequenceId))
        {
            return;
        }

        ClearPositionQueues();
        lastSequenceId = minSid;

        body.position = position;
        transform.position = position;
        body.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (isPaused)
        {
            return;
        }

        if (AmOwner)
        {
            if (HasMoved())
            {
                sendQueue.Enqueue(body.position);
                SetDirtyBit(2U);
            }
        }
        else
        {
            if (incomingPosQueue.Count < 1)
            {
                return;
            }
            SkipExcessiveFrames();
            SetMovementSmoothingModifier();
            MoveTowardNextPoint();
        }
    }

    private bool HasMoved()
    {
        var num = body ? Vector2.Distance(body.position, lastPosSent) : Vector2.Distance(transform.position, lastPosSent);
        return num > 0.0001f;
    }

    public override void HandleRpc(byte callId, MessageReader reader)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        if (callId == 21)
        {
            var position = NetHelpers.ReadVector2(reader);
            var minSid = reader.ReadUInt16();
            SnapTo(position, minSid);
        }
    }

    public override bool Serialize(MessageWriter writer, bool initialState)
    {
        if (isPaused)
        {
            return false;
        }
        if (initialState)
        {
            writer.Write(lastSequenceId);
            NetHelpers.WriteVector2(body.position, writer);
            return true;
        }
        if (!isActiveAndEnabled)
        {
            ClearDirtyBits();
            return false;
        }
        if (sendQueue.Count == 0)
        {
            return false;
        }
        lastSequenceId += 1;
        writer.Write(lastSequenceId);
        var num = (ushort)sendQueue.Count;
        writer.WritePacked(num);
        foreach (var vector in sendQueue)
        {
            NetHelpers.WriteVector2(vector, writer);
            lastPosSent = vector;
        }
        sendQueue.Clear();
        lastSequenceId += (ushort)(num - 1);
        DirtyBits -= 1U;
        return true;
    }

    public override void Deserialize(MessageReader reader, bool initialState)
    {
        if (isPaused)
        {
            return;
        }
        if (initialState)
        {
            lastSequenceId = reader.ReadUInt16();
            transform.position = NetHelpers.ReadVector2(reader);
            incomingPosQueue.Clear();
            incomingPosQueue.Enqueue(transform.position);
            return;
        }
        if (AmOwner)
        {
            return;
        }
        var num = reader.ReadUInt16();
        var num2 = reader.ReadPackedInt32();
        for (var i = 0; i < num2; i++)
        {
            var num3 = (ushort)(num + i);
            var vector2 = NetHelpers.ReadVector2(reader);
            if (NetHelpers.SidGreaterThan(num3, lastSequenceId))
            {
                lastSequenceId = num3;
                incomingPosQueue.Enqueue(vector2);
            }
        }
    }

    private void MoveTowardNextPoint()
    {
        var vector = incomingPosQueue.Peek();
        var position = body.position;
        var vector2 = vector - position;
        if (ShouldExtendCurrentFrame(vector, position))
        {
            var vector3 = vector2.normalized * (idealSpeed * rubberbandModifier);
            vector3 = Vector2.ClampMagnitude(vector3, 10f);
            body.velocity = vector3;
            lastPosition = body.position;
            return;
        }
        if (incomingPosQueue.Count <= 1)
        {
            if (Vector2.Distance(body.position, vector) > 0.01f)
            {
                body.position = vector;
                body.velocity = Vector2.zero;
                lastPosition = body.position;
            }
            return;
        }
        var vector4 = incomingPosQueue.Dequeue();
        if (Vector2.Distance(body.position, vector4) > 0.05f)
        {
            body.position = vector4;
        }
        vector = incomingPosQueue.Peek();
        position = body.position;
        vector2 = vector - position;
        idealSpeed = vector2.magnitude / Time.fixedDeltaTime;
        var vector5 = vector2.normalized * (idealSpeed * rubberbandModifier);
        vector5 = Vector2.ClampMagnitude(vector5, 10f);
        body.velocity = vector5;
        lastPosition = body.position;
    }

    private void SetMovementSmoothingModifier()
    {
        var num = incomingPosQueue.Count <= 5 ? 0.5f : 0.995f;
        rubberbandModifier = Mathf.Lerp(rubberbandModifier, num, Time.fixedDeltaTime * 3f);
    }

    private bool ShouldExtendCurrentFrame(Vector2 nextPos, Vector2 currentPos)
    {
        return !DidPassPosition(nextPos, lastPosition, currentPos) && incomingPosQueue.Count <= 5;
    }

    private static bool DidPassPosition(Vector2 nextPos, Vector2 lastPos, Vector2 currentPos)
    {
        var num = Vector2.Distance(lastPos, currentPos);
        var num2 = Vector2.Distance(currentPos, nextPos);
        var num3 = Vector2.Distance(lastPos, nextPos);
        return Mathf.Abs(num - (num3 + num2)) < 0.003f;
    }

    private void SkipExcessiveFrames()
    {
        if (incomingPosQueue.Count < 12)
        {
            return;
        }
        if (body)
        {
            body.position = incomingPosQueue.Peek();
            MoveTowardNextPoint();
            if (incomingPosQueue.Count >= 14)
            {
                body.position = incomingPosQueue.Peek();
                MoveTowardNextPoint();
            }
        }
        else
        {
            transform.position = incomingPosQueue.Dequeue();
        }
    }
}