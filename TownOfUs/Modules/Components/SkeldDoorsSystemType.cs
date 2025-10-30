using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using TownOfUs.Interfaces.BaseGame;

namespace TownOfUs.Modules.Components;
// This is a reimplementation of AutoDoorsSystemType for vanilla maps, as Impostor servers assume they're unchanged.

[RegisterInIl2Cpp(typeof(ISystemType), typeof(IActivatable), typeof(RunTimer), typeof(IDoorSystem))]
public class SkeldDoorsSystemType(nint cppPtr) : Il2CppSystem.Object(cppPtr), BaseGame.ISystemType, BaseGame.IActivatable, BaseGame.IRunTimer, BaseGame.IDoorSystem
{
    public const byte SystemId = 151;
    public const SystemTypes SystemType = (SystemTypes)SystemId;
    public static AutoOpenDoor[] GetAutoDoors() => ShipStatus.Instance.AllDoors.OfType<AutoOpenDoor>().ToArray();
    public SkeldDoorsSystemType() : this(ClassInjector.DerivedConstructorPointer<SkeldDoorsSystemType>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    
	public bool IsActive
	{
		get
		{
			return GetAutoDoors().Any(x => !x.IsOpen);
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
		for (int i = 0; i < GetAutoDoors().Length; i++)
		{
			if (GetAutoDoors()[i].DoUpdate(deltaTime))
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
			for (int i = 0; i < GetAutoDoors().Length; i++)
			{
                GetAutoDoors()[i].Serialize(writer);
			}
			return;
		}
		writer.WritePacked(dirtyBits);
		for (int j = 0; j < GetAutoDoors().Length; j++)
		{
			if ((dirtyBits & 1U << j) != 0U)
			{
                GetAutoDoors()[j].Serialize(writer);
			}
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < GetAutoDoors().Length; i++)
			{
                GetAutoDoors()[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < GetAutoDoors().Length; j++)
		{
			if ((num & 1U << j) != 0U)
			{
                GetAutoDoors()[j].Deserialize(reader);
			}
		}
	}

	public void SetDoor(AutoOpenDoor door, bool open)
	{
		door.SetDoorway(open);
		dirtyBits |= 1U << Array.IndexOf(GetAutoDoors(), door);
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		for (int i = 0; i < GetAutoDoors().Length; i++)
		{
			var openableDoor = GetAutoDoors()[i];
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
		for (int i = 0; i < GetAutoDoors().Length; i++)
		{
			var autoDoor = GetAutoDoors()[i];
			if (autoDoor.Room == system)
			{
				return autoDoor.CooldownTimer / 30f;
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
