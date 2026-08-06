using MiraAPI.GameModes;
using UnityEngine;

namespace TownOfUs.GameModes;

public class KillFrenzyMode : AbstractGameMode
{
    public override string Name => "Kill Frenzy";
    public override string Description => "Eliminate everyone else to be the winner.";
    public override Color Color => TownOfUsColors.Bloody;
}