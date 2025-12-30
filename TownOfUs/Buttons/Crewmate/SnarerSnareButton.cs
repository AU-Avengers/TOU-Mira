using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SnarerSnareButton : TownOfUsRoleButton<SnarerRole, Vent>
{
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);

    public override string Name => TouLocale.GetParsed("TouRoleSnarerSnare", "Snare");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Snarer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<SnarerOptions>.Instance.SnareCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<SnarerOptions>.Instance.MaxSnares;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.TrapSprite;

    public override bool IsTargetValid(Vent? target)
    {
        return base.IsTargetValid(target) && target != null && !VentSnareSystem.IsSnared(target.Id);
    }

    public override Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 4, Filter) ??
                   PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 3, Filter) ??
                   PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 2, Filter) ??
                   PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);

        if (vent != null && PlayerControl.LocalPlayer.CanUseVent(vent))
        {
            return vent;
        }

        return null;
    }

    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            Target?.SetOutline(false, false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return base.CanUse() && Timer <= 0 && Target != null && UsesLeft > 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        SnarerRole.RpcSnarerPlaceSnare(PlayerControl.LocalPlayer, Target.Id);
    }
}