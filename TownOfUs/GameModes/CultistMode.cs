using MiraAPI.GameModes;
using UnityEngine;

namespace TownOfUs.GameModes;

public class CultistMode : AbstractGameMode
{
    public override bool HideMode => !TownOfUsPlugin.IsDevBuild;
    public override string Name => "Cultist";
    public override string Description => "Find converted impostors before they outnumber you.";
    public override Color Color => TownOfUsColors.ImpSoft;
    public override bool ShowNormalGameSettings => false;
    public override bool ShowNormalRoleSettings => false;
}