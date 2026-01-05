using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class RevealerTrapButton : TownOfUsRoleButton<RevealerRole>
{
    public override string Name => TouLocale.GetParsed("TouRoleRevealerTrap", "Trap");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Revealer;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<RevealerOptions>.Instance.TrapCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<RevealerOptions>.Instance.MaxTraps;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.TrapSprite;
    public int ExtraUses { get; set; }

    protected override void OnClick()
    {
        var role = PlayerControl.LocalPlayer.GetRole<RevealerRole>();

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