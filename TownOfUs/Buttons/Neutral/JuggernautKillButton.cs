using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class JuggernautKillButton : TownOfUsRoleButton<JuggernautRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Juggernaut;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.JuggKillSprite;
    public override float Cooldown => GetCooldown();

    public static float BaseCooldown => OptionGroupSingleton<JuggernautOptions>.Instance.KillCooldown + MapCooldown;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Juggernaut Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public static float GetCooldown()
    {
        var juggernaut = PlayerControl.LocalPlayer.Data.Role as JuggernautRole;

        if (juggernaut == null)
        {
            return BaseCooldown;
        }

        var options = OptionGroupSingleton<JuggernautOptions>.Instance;

        return Math.Max(BaseCooldown - options.KillCooldownReduction * juggernaut.KillCount, 0);
    }
}