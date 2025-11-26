using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SeerGazeButton : TownOfUsRoleButton<SeerRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleSeerGaze", "Gaze");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Seer;
    public override int MaxUses => (int)OptionGroupSingleton<SeerOptions>.Instance.MaxCompares;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) &&
               OptionGroupSingleton<SeerOptions>.Instance.SalemSeer;
    }
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<SeerOptions>.Instance.SeerCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.SeerSprite;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => Role.GazeTarget != x && Role.IntuitTarget != x);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (Role.GazeTarget != null)
        {
            ++UsesLeft;
            SetUses(UsesLeft);
        }

        Role.GazeTarget = Target;

        CustomButtonSingleton<SeerIntuitButton>.Instance.ResetCooldownAndOrEffect();
        if (Role.GazeTarget != null && Role.IntuitTarget != null)
        {
            Role.SeerCompare(PlayerControl.LocalPlayer);
        }
    }
}