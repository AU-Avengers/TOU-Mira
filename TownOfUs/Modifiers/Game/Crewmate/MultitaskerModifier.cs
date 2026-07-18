using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class MultitaskerModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Multitasker,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Multitasker.LoadAsset(),
            "TouMira.Modifier.Crewmate.Multitasker", 1.45f));
    public override string LocaleKey => "Multitasker";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription");
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Multitasker;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount;
    }

    public override void Update()
    {
        if (!Player || Player.Data.IsDead || !Player.AmOwner)
        {
            return;
        }

        if (!Minigame.Instance || IsExemptTask())
        {
            return;
        }

        SpriteRenderer[] rends = Minigame.Instance.GetComponentsInChildren<SpriteRenderer>();

        foreach (var t in rends)
        {
            var oldColor1 = t.color[0];
            var oldColor2 = t.color[1];
            var oldColor3 = t.color[2];
            t.color = new Color(oldColor1, oldColor2, oldColor3, 0.5f);
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public static bool IsExemptTask()
    {
        return Minigame.Instance is VitalsMinigame ||
               Minigame.Instance is CollectShellsMinigame ||
               Minigame.Instance is MushroomDoorSabotageMinigame ||
               Minigame.Instance is ShapeshifterMinigame ||
               Minigame.Instance is FungleSurveillanceMinigame ||
               Minigame.Instance is SurveillanceMinigame ||
               Minigame.Instance is PlanetSurveillanceMinigame ||
               Minigame.Instance is IngameWikiMinigame ||
               Minigame.Instance is CustomPlayerMenu ||
               Minigame.Instance is GuesserMenu;
    }
}