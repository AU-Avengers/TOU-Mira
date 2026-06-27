using Hazel;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules.Components;
// This is a reimplementation of AutoDoorsSystemType for vanilla maps, as Impostor servers assume they're unchanged.

public class SkeldDoorsSystemType : Object, ISystemType, IActivatable, RunTimer, IDoorSystem
{
    public const byte SystemId = 151;
    public const SystemTypes SystemType = (SystemTypes)SystemId;
    
	public bool IsActive
	{
		get
		{
			return ShipStatus.Instance.AllDoors.Any(b => !b.IsOpen);
		}
	}

	public bool IsDirty
	{
		get
		{
			return dirtyBits > 0U;
		}
	}

	public void Deteriorate(float deltaTime)
	{
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			if (ShipStatus.Instance.AllDoors[i].DoUpdate(deltaTime))
			{
				dirtyBits |= 1U << i;
			}
		}
		if (initialCooldown > 0f)
		{
			initialCooldown -= deltaTime;
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
        // This is not implemented on AutoDoorsSystemType
	}

	public void MarkClean()
	{
		dirtyBits = 0U;
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
                ShipStatus.Instance.AllDoors[i].Serialize(writer);
			}
			return;
		}
		writer.WritePacked(dirtyBits);
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((dirtyBits & 1U << j) != 0U)
			{
                ShipStatus.Instance.AllDoors[j].Serialize(writer);
			}
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
			{
                ShipStatus.Instance.AllDoors[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < ShipStatus.Instance.AllDoors.Length; j++)
		{
			if ((num & 1U << j) != 0U)
			{
                ShipStatus.Instance.AllDoors[j].Deserialize(reader);
			}
		}
	}

	public void SetDoor(AutoOpenDoor door, bool open)
	{
		door.SetDoorway(open);
		dirtyBits |= 1U << Array.IndexOf(ShipStatus.Instance.AllDoors, door);
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
        Info($"Skeld Doors on this map: {ShipStatus.Instance.AllDoors.Length}");
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
			var openableDoor = ShipStatus.Instance.AllDoors[i];
			if (openableDoor.Room == room)
			{
				openableDoor.SetDoorway(false);
				dirtyBits |= 1U << i;
			}
		}
	}

	public float GetTimer(SystemTypes system)
	{
		if (initialCooldown > 0f)
		{
			return initialCooldown / 10f;
		}
		for (int i = 0; i < ShipStatus.Instance.AllDoors.Length; i++)
		{
            OpenableDoor openableDoor = ShipStatus.Instance.AllDoors[i];
            if (openableDoor.Room == system)
            {
                var auto = openableDoor as AutoOpenDoor;
                return auto!.CooldownTimer / 30f;
            }
		}
		return 0f;
	}

	public void SetInitialSabotageCooldown()
	{
		initialCooldown = 10f;
	}

	private uint dirtyBits;

	private float initialCooldown;
}
