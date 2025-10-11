using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class BaitModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Bait";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Bait;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    private static float MinDelay => OptionGroupSingleton<BaitOptions>.Instance.MinDelay;
    private static float MaxDelay => OptionGroupSingleton<BaitOptions>.Instance.MaxDelay;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BaitChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BaitAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public static IEnumerator CoReportDelay(PlayerControl killer, PlayerControl target)
    {
        if (!killer || !target || killer == target)
        {
            yield break;
        }

        yield return new WaitForSeconds(Random.RandomRange(MinDelay, MaxDelay));

        if (killer.AmOwner)
        {
            killer.CmdReportDeadBody(target.Data);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Bait.ToTextColor()}{target.Data.PlayerName} was a Bait, causing you to self report.</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Bait.LoadAsset());

            notif1.AdjustNotification();
        }
    }
}