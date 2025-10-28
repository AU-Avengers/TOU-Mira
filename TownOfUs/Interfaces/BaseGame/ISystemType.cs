using Hazel;

namespace TownOfUs.Interfaces.BaseGame;

// ReSharper disable once InconsistentNaming
public sealed partial class BaseGame
{
    public interface ISystemType
    {
        bool IsDirty { get; }

        void Deteriorate(float deltaTime);

        void UpdateSystem(PlayerControl player, MessageReader msgReader);

        void MarkClean();

        void Serialize(MessageWriter writer, bool initialState);

        void Deserialize(MessageReader reader, bool initialState);
    }
}