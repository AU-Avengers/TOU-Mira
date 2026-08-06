using MiraAPI.GameModes;
using UnityEngine;

namespace TownOfUs.GameModes;

public class CultistMode : AbstractGameMode
{
    public override string Name => "Cultist";
    public override string Description => "Find converted impostors before they outnumber you.";
    public override Color Color => TownOfUsColors.ImpSoft;
}