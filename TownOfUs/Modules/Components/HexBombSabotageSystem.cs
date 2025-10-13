using Hazel;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Utilities.Attributes;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp(typeof(ISystemType), typeof(IActivatable))]
public sealed class HexBombSabotageSystem(nint cppPtr) : Il2CppSystem.Object(cppPtr)
{
    public const byte SabotageId = 68;

    public bool IsActive => TimeRemaining > 0 || Stage == HexBombStage.Warning || Stage == HexBombStage.Finished;
    public bool IsDirty { get; private set; }
    public float TimeRemaining { get; private set; }
    public HexBombStage Stage { get; private set; }
    public static bool BombFinished { get; internal set; }

    private float _dirtyTimer;
    public HexBombSabotageSystem() : this(ClassInjector.DerivedConstructorPointer<HexBombSabotageSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public void Deteriorate(float deltaTime)
    {
        if (!IsActive)
        {
            if (Stage != HexBombStage.None)
            {
                Stage = HexBombStage.None;
                IsDirty = true;
                BombFinished = false;
            }

            return;
        }

        if (!PlayerTask.PlayerHasTaskOfType<HexBombSabotageTask>(PlayerControl.LocalPlayer))
        {
            PlayerControl.LocalPlayer.AddSystemTask((SystemTypes)SabotageId);
        }

        if (MeetingHud.Instance == null && ExileController.Instance == null)
        {
            TimeRemaining -= deltaTime;
            _dirtyTimer += deltaTime;
        }

        if (_dirtyTimer > 2f)
        {
            _dirtyTimer = 0f;
            IsDirty = true;
        }

        if (TimeRemaining <= 0)
        {
            if (Stage == HexBombStage.Warning)
            {
                Stage = HexBombStage.Countdown;
                TimeRemaining = OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDuration;
                BombFinished = false;
                IsDirty = true;
            }
            else if (Stage == HexBombStage.Countdown)
            {
                Stage = HexBombStage.Finished;
                TimeRemaining = 3f;
                BombFinished = false;
                IsDirty = true;
            }
            else if (Stage == HexBombStage.Finished)
            {
                IsDirty = true;
                if (TutorialManager.InstanceExists)
                {
                    Stage = HexBombStage.None;
                }
                else
                {
                    var spellslinger = CustomRoleUtils.GetActiveRolesOfType<SpellslingerRole>().FirstOrDefault();
                    if (spellslinger != null)
                    {
                        BombFinished = true;
                    }
                }
            }
        }
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        if (msgReader.ReadByte() != 1) return;
        Stage = HexBombStage.Warning;
        TimeRemaining = OptionGroupSingleton<SpellslingerOptions>.Instance.HexBombDelay;
        IsDirty = true;
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        TimeRemaining = reader.ReadSingle();
        Stage = (HexBombStage)reader.ReadByte();
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write(TimeRemaining);
        writer.Write((byte)Stage);
        IsDirty = initialState;
    }

    public void MarkClean()
	{
		IsDirty = false;
	}
}

public enum HexBombStage
{
    None,
    Warning,
    Countdown,
    Finished,
}
