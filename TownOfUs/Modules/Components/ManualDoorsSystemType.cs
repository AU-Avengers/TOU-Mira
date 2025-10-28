using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using TownOfUs.Interfaces.BaseGame;
using UnityEngine;

namespace TownOfUs.Modules.Components;
// This is a reimplementation of DoorsSystemType for vanilla maps (mainly just skeld), as Impostor servers assume they're unchanged.

[RegisterInIl2Cpp(typeof(ISystemType), typeof(IActivatable), typeof(RunTimer), typeof(IDoorSystem))]
public class ManualDoorsSystemType(nint cppPtr) : Il2CppSystem.Object(cppPtr), BaseGame.ISystemType, BaseGame.IActivatable, BaseGame.RunTimer, BaseGame.IDoorSystem
{
    public const byte SystemId = 152;
    public const SystemTypes SystemType = (SystemTypes)SystemId;

	public bool IsActive
	{
		get
		{
			return false;
		}
	}

    public ManualDoorsSystemType() : this(ClassInjector.DerivedConstructorPointer<ManualDoorsSystemType>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

	public bool IsDirty { get; private set; }

	public void Deteriorate(float deltaTime)
	{
		for (int i = 0; i < SystemTypeHelpers.AllTypes.Length; i++)
		{
			SystemTypes key = SystemTypeHelpers.AllTypes[i];
			float num;
			if (timers.TryGetValue(key, out num))
			{
				timers[key] = Mathf.Clamp(num - deltaTime, 0f, 30f);
			}
		}
		if (initialCooldown > 0f)
		{
			initialCooldown -= deltaTime;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte b = msgReader.ReadByte();
		int id = (b & 31);
		int num = (b & 192);
		if (num == 64)
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors.First((OpenableDoor d) => d.Id == id);
			if (openableDoor == null)
			{
				Debug.LogWarning(string.Format(TownOfUsPlugin.Culture, "Couldn't find door {0}", id));
			}
			else
			{
				openableDoor.SetDoorway(true);
			}
		}
		IsDirty = true;
	}

	public void MarkClean()
	{
		IsDirty = false;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)timers.Count);
		foreach (KeyValuePair<SystemTypes, float> keyValuePair in timers)
		{
			writer.Write((byte)keyValuePair.Key);
			writer.Write(keyValuePair.Value);
		}
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			ShipStatus.Instance.AllDoors[i].Serialize(writer);
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int num = (int)reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			SystemTypes key = (SystemTypes)reader.ReadByte();
			float value = reader.ReadSingle();
			timers[key] = value;
		}
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			ShipStatus.Instance.AllDoors[j].Deserialize(reader);
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		timers[room] = 30f;
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			OpenableDoor openableDoor = ShipStatus.Instance.AllDoors[i];
			if (openableDoor.Room == room)
			{
				openableDoor.SetDoorway(false);
			}
		}
		IsDirty = true;
	}

	public float GetTimer(SystemTypes room)
	{
		if (initialCooldown > 0f)
		{
			return initialCooldown / 10f;
		}
		float num;
		if (timers.TryGetValue(room, out num))
		{
			return num / 30f;
		}
		return 0f;
	}

	public void SetInitialSabotageCooldown()
	{
		initialCooldown = 10f;
	}

	public const byte CloseDoors = 128;

	public const byte OpenDoor = 64;

	private const byte ActionMask = 192;

	private const byte IdMask = 31;

	private readonly Dictionary<SystemTypes, float> timers = new Dictionary<SystemTypes, float>();

	private float initialCooldown;
}
