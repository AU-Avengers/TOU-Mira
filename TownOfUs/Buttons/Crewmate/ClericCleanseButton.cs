using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class ClericCleanseButton : TownOfUsRoleButton<ClericRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleClericCleanse", "Cleanse");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Cleric;
    public override float Cooldown => OptionGroupSingleton<ClericOptions>.Instance.CleanseCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CleanseSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        if (Target.HasModifier<ClericCleanseModifier>())
        {
            Target.RpcRemoveModifier<ClericCleanseModifier>();
        }

        Target.RpcAddModifier<ClericCleanseModifier>(PlayerControl.LocalPlayer);

        if (ClericCleanseModifier.FindNegativeEffects(Target).Count > 0)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Cleric));
        }

        CustomButtonSingleton<ClericBarrierButton>.Instance.ResetCooldownAndOrEffect();
    }
}