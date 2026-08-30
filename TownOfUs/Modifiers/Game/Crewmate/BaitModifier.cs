using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class BaitModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Bait,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Bait.LoadAsset(),
            "TouMira.Modifier.Crewmate.Bait", 1.45f));
    public override string IdPart => "Bait";
    public override string ModifierName => MiraLocaleManager.Get($"TouModifier{IdPart}");
    public override string IntroInfo => MiraLocaleManager.Get($"TouModifier{IdPart}.IntroBlurb");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TouModifier{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
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
        return 1;
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

        if (MeetingHud.Instance)
        {
            yield break;
        }

        if (killer.AmOwner)
        {
            killer.CmdReportDeadBody(target.Data);

            var text = MiraLocaleManager.Get("TouModifierBaitTriggeredNotif").Replace("<player>", target.Data.PlayerName);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{text.Replace("<modifier>", $"{TownOfUsColors.Bait.ToTextColor()}{MiraLocaleManager.Get("TouModifierBait")}</color>")}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Bait.LoadAsset());

            notif1.AdjustNotification();
        }
    }
}