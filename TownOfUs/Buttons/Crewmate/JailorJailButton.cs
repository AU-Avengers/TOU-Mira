using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class JailorJailButton : TownOfUsRoleButton<JailorRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleJailorJail", "Jail");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Jailor;
    public override float Cooldown => OptionGroupSingleton<JailorOptions>.Instance.JailCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.JailSprite;

    public bool ExecutedACrew { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !ExecutedACrew;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: player => !player.HasModifier<JailedModifier>() && !player.HasModifier<JailSparedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        ModifierUtils.GetPlayersWithModifier<JailedModifier>().Do(x => x.RpcRemoveModifier<JailedModifier>());
        Target?.RpcAddModifier<JailedModifier>(PlayerControl.LocalPlayer.PlayerId);
        TouAudio.PlaySound(TouAudio.JailSound);
    }
}