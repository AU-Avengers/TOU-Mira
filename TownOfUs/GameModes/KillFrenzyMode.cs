using MiraAPI.GameModes;
using UnityEngine;

namespace TownOfUs.GameModes;

public class KillFrenzyMode : AbstractGameMode
{
    public override string Name => "Kill Frenzy";
    public override string Description => "Last killer standing wins.";
    public override Color Color => TownOfUsColors.HaunterRevealed;
}
