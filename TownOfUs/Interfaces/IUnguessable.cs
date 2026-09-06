namespace TownOfUs.Extensions;

public interface IUnguessable : IUnguessableBasic
{
    RoleBehaviour AppearAs { get; }
}
public interface IUnguessableBasic
{
    // basically, does the player die when the appearance role is guessed (so yes for traitor, no for pestilence)
    bool IsGuessable { get; }
}