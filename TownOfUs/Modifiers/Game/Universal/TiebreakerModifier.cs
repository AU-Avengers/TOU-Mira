using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class TiebreakerModifier : UniversalGameModifier, IWikiDiscoverable, IContinuesGame
{
    // If two or three players are present, then certain roles can stall the game.
    // Tiebreaker Jester, for example, can stall the game.
    // Solo Tiebreaker Crewmate can also stall the game and win.
    // Neutral Killers are unable to be counted here as their win condition allows them to handle it otherwise.
    public bool ContinuesGame
    {
        get
        {
            if (Player.HasDied() || Player.IsImpostorAligned())
            {
                return false;
            }
            if ((!Player.IsCrewmate() || Helpers.GetAlivePlayers().Count(x => x.IsCrewmate()) == 1) &&
                Player.Data.Role is ITownOfUsRole touRole &&
                touRole.RoleAlignment is not RoleAlignment.NeutralKilling && Helpers.GetAlivePlayers().Count < 4 &&
                Helpers.GetAlivePlayers().Count > 1)
            {
                return touRole.CanModifierContinueGame(this);
            }

            return false;
        }
    }
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Tiebreaker,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Tiebreaker.LoadAsset(),
            "TouMira.Modifier.Universal.Tiebreaker", 1.45f));
    public override string IdPart => "Tiebreaker";
    public override string ModifierName => MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Tiebreaker;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public override string GetDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return MiraLocaleManager.Get($"TownOfUsMira.Modifier.{IdPart}.WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerChance;
    }
}