using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsGame.Crewmate;

public sealed class HnsMultitaskerModifier : HnsGameModifier
{
    public override string LocaleKey => "Multitasker";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Multitasker;
    public override ModifierFaction FactionType => ModifierFaction.CrewmateVisibility;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }


    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MultitaskerChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<HnsCrewmateModifierOptions>.Instance.MultitaskerAmount;
    }

    public override void Update()
    {
        if (!Player || Player.Data.IsDead || !Player.AmOwner)
        {
            return;
        }

        if (Minigame.Instance == null || IsExemptTask())
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

    public static bool IsExemptTask()
    {
        return Minigame.Instance.TryCast<VitalsMinigame>() ||
               Minigame.Instance.TryCast<CollectShellsMinigame>() ||
               Minigame.Instance.TryCast<MushroomDoorSabotageMinigame>() ||
               Minigame.Instance.TryCast<ShapeshifterMinigame>() ||
               Minigame.Instance.TryCast<FungleSurveillanceMinigame>() ||
               Minigame.Instance.TryCast<SurveillanceMinigame>() ||
               Minigame.Instance.TryCast<PlanetSurveillanceMinigame>() ||
               Minigame.Instance is IngameWikiMinigame ||
               Minigame.Instance is CustomPlayerMenu ||
               Minigame.Instance is GuesserMenu;
    }
}