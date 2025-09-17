using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class OracleBlessButton : TownOfUsRoleButton<OracleRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleOracleBless", "Bless");
    public override Color TextOutlineColor => TownOfUsColors.Oracle;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override float Cooldown => OptionGroupSingleton<OracleOptions>.Instance.BlessCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.BlessSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<OracleBlessedModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        var players = ModifierUtils.GetPlayersWithModifier<OracleBlessedModifier>(x => x.Oracle.AmOwner);
        players.Do(x => x.RpcRemoveModifier<OracleBlessedModifier>());

        Target.RpcAddModifier<OracleBlessedModifier>(PlayerControl.LocalPlayer);
    }
}