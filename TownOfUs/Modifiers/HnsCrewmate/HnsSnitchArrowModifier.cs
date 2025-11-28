using MiraAPI.GameOptions;
using Reactor.Utilities;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Options.Roles.HnsCrewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsCrewmate;

public sealed class HnsSnitchArrowModifier(PlayerControl owner, Color color, float update)
    : ArrowTargetModifier(owner, color, update)
{
    public override string ModifierName => "Snitch Arrow";
    public override float Duration => OptionGroupSingleton<HnsSnitchOptions>.Instance.SnitchNotifyDuration.Value;
    public override bool AutoStart => true;

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        var r = Arrow.gameObject.AddComponent<BasicRainbowBehaviour>();

        r.AddRend(spr, Player.cosmetics.ColorId);

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch));
    }
}