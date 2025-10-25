using Hazel;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.Roles;
using Reactor.Utilities.Attributes;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp(typeof(ISystemType), typeof(IActivatable))]
public sealed class HexBombSabotageSystem(nint cppPtr) : Il2CppSystem.Object(cppPtr)
{
    public const byte SabotageId = 150;
    public readonly float duration;

    public bool IsActive => (TimeRemaining > 0 || Stage == HexBombStage.Finished);
    public static bool InMeeting => MeetingHud.Instance != null || ExileController.Instance != null;
    public bool IsDirty { get; private set; }
    public float TimeRemaining { get; private set; }
    public HexBombStage Stage { get; private set; }
    public static bool BombFinished { get; internal set; }

    private float _dirtyTimer;
    public HexBombSabotageSystem(float duration) : this(ClassInjector.DerivedConstructorPointer<HexBombSabotageSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);
        Instance = this;
        this.duration = duration;
    }

    public static HexBombSabotageSystem Instance { get; private set; }
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

        if (InMeeting)
        {
            return;
        }

        if (!PlayerTask.PlayerHasTaskOfType<HexBombSabotageTask>(PlayerControl.LocalPlayer))
        {
            PlayerControl.LocalPlayer.AddSystemTask((SystemTypes)SabotageId);
        }

        SpellslingerRole.SabotageTriggered = true;

        if (!InMeeting)
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
            if (Stage == HexBombStage.Initiate)
            {
                Stage = HexBombStage.Countdown;
                TimeRemaining = duration;
                BombFinished = false;
                IsDirty = true;
            }
            else if (Stage == HexBombStage.Countdown)
            {
                Stage = HexBombStage.Finished;
                var spellslinger = CustomRoleUtils.GetActiveRolesOfType<SpellslingerRole>().FirstOrDefault();
                if (spellslinger != null)
                {
                    foreach (var player in PlayerControl.AllPlayerControls.ToArray()
                                 .Where(x => !x.HasDied() && !x.IsImpostor()))
                    {
                        DeathHandlerModifier.UpdateDeathHandler(player, TouLocale.Get("DiedToSpellslingerHexBomb"), DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue,
                            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", spellslinger.Player.Data.PlayerName),
                            lockInfo: DeathHandlerOverride.SetTrue);
                    }
                }
                TimeRemaining = 7f;
                BombFinished = false;
                IsDirty = true;
            }
            else if (Stage == HexBombStage.SpellslingerDead)
            {
                IsDirty = true;
                Stage = HexBombStage.None;

                BombFinished = false;
            }
            else if (Stage == HexBombStage.Finished)
            {
                IsDirty = true;
                if (TutorialManager.InstanceExists)
                {
                    TimeRemaining = 7f;
                    Stage = HexBombStage.SpellslingerDead;
                }
                BombFinished = true;
            }
        }
        else if (Stage == HexBombStage.Countdown && !CustomRoleUtils.GetActiveRolesOfType<SpellslingerRole>().Any())
        {
            Stage = HexBombStage.SpellslingerDead;
            TimeRemaining = 3f;
            BombFinished = false;
            IsDirty = true;
        }
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        if (msgReader.ReadByte() != 1) return;
        Stage = HexBombStage.Initiate;
        TimeRemaining = 4f;
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
    Initiate,
    None,
    Countdown,
    Finished,
    SpellslingerDead,
}
