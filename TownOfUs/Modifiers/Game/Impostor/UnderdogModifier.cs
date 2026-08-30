using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Impostor;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class UnderdogModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Impostor,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Underdog.LoadAsset(),
            "TouMira.Modifier.Impostor.Underdog", 1.45f));
    public override string IdPart => "Underdog";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}.IntroBlurb");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Underdog;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;

    private static float KillCooldownIncrease => OptionGroupSingleton<UnderdogOptions>.Instance.KillCooldownIncrease;
    private static bool ExtraImpsKillCooldown => OptionGroupSingleton<UnderdogOptions>.Instance.ExtraImpsKillCooldown;

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.UnderdogAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }

    public static bool LastImpostor()
    {
        return MiscUtils.ImpAliveCount <= 1;
    }

    public static float GetKillCooldown(PlayerControl player)
    {
        var mod = player.GetModifier<UnderdogModifier>();

        if (mod == null)
        {
            return GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }

        var baseKillCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

        var lowerKc = baseKillCooldown - KillCooldownIncrease;
        var upperKc = baseKillCooldown + KillCooldownIncrease;

        var kc = ExtraImpsKillCooldown ? upperKc : baseKillCooldown;
        var timer = LastImpostor() ? lowerKc : kc;

        // Error($"GetKillCooldown({player.Data.PlayerName}) baseKillCooldown: {baseKillCooldown}, baseKillCooldown2: {baseKillCooldown2}, lowerKc {lowerKc}, upperKc {upperKc}, kc {kc}, timer {timer}");

        return timer;
    }
}