using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TrapperTrapButton : TownOfUsRoleButton<TrapperRole>
{
    public override string Name => TouLocale.Get("TouRoleTrapperTrap", "Trap");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Trapper;
    public override float Cooldown => OptionGroupSingleton<TrapperOptions>.Instance.TrapCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<TrapperOptions>.Instance.MaxTraps;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.TrapSprite;
    public int ExtraUses { get; set; }

    protected override void OnClick()
    {
        var role = PlayerControl.LocalPlayer.GetRole<TrapperRole>();

        if (role == null)
        {
            return;
        }

        var pos = PlayerControl.LocalPlayer.transform.position;
        pos.z += 0.001f;

        Trap.CreateTrap(role, pos);

        TouAudio.PlaySound(TouAudio.TrapperPlaceSound);
    }
}