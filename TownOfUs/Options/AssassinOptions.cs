using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;

namespace TownOfUs.Options;

public sealed class AssassinOptions : AbstractOptionGroup<AssassinModifier>, IWikiOptionsSummaryProvider
{
    public override string GroupName => "Assassin Options";
    public override uint GroupPriority => 7;
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;

    public ModdedNumberOption NumberOfImpostorAssassins { get; } =
        new("Number Of Impostor Assassins", 1, 0, 4, 1, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ImpAssassinChance { get; } =
        new("Impostor Assassin Chance", 100f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins.Value > 0
        };

    public ModdedNumberOption NumberOfNeutralAssassins { get; } =
        new("Number Of Neutral Assassins", 1, 0, 4, 1, MiraNumberSuffixes.None, "0");

    public ModdedNumberOption NeutAssassinChance { get; } =
        new("Neutral Assassin Chance", 100f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins.Value > 0
        };

    public ModdedNumberOption AssassinKills { get; } =
        new("Number Of Assassin Kills", 5, 1, 15, 1, MiraNumberSuffixes.None, "0");

    public ModdedToggleOption AssassinMultiKill { get; } =
        new("Assassin Can Kill More Than Once Per Meeting", true)
    {
        Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinKills.Value > 1
    };

    /*
    public ModdedToggleOption GuessVanillaRoles { get; } =
        new("Non-Basic Vanilla Roles Are Guessable", true);*/

    public ModdedToggleOption AssassinCrewmateGuess { get; } =
        new("Assassin Can Guess \"Crewmate\"", false);

    public ModdedToggleOption AssassinGuessInvest { get; } =
        new("Assassin Can Guess Crew Investigative Roles", false);

    public ModdedToggleOption AssassinGuessNeutralBenign { get; } =
        new("Assassin Can Guess Neutral Benign Roles", true);

    public ModdedToggleOption AssassinGuessNeutralEvil { get; } =
        new("Assassin Can Guess Neutral Evil Roles", true);

    public ModdedToggleOption AssassinGuessNeutralKilling { get; } =
        new("Assassin Can Guess Neutral Killing Roles", true);

    public ModdedToggleOption AssassinGuessNeutralOutlier { get; } =
        new("Assassin Can Guess Neutral Outlier Roles", true);

    public ModdedToggleOption AssassinGuessImpostors { get; } =
        new("Assassin Can Guess Impostor Roles", true);

    public ModdedToggleOption AssassinGuessCrewModifiers { get; } =
        new("Assassin Can Guess Crewmate Modifiers", true);

    public ModdedToggleOption AssassinGuessUtilityModifiers { get; } =
        new("Assassin Can Guess Crew Utility Modifiers", false)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers.Value
        };

    public ModdedToggleOption AssassinGuessNonCrewModifiers { get; } =
        new("Assassin Can Guess Other Faction Modifiers", true);

    public ModdedToggleOption AssassinGuessAlliances { get; } =
        new("Assassin Can Guess Alliances", true);

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            NumberOfImpostorAssassins.StringName,
            ImpAssassinChance.StringName,
            NumberOfNeutralAssassins.StringName,
            NeutAssassinChance.StringName,

            AssassinKills.StringName,
            AssassinMultiKill.StringName,

            // GuessVanillaRoles.StringName,
            AssassinCrewmateGuess.StringName,
            AssassinGuessInvest.StringName,

            AssassinGuessNeutralBenign.StringName,
            AssassinGuessNeutralEvil.StringName,
            AssassinGuessNeutralKilling.StringName,
            AssassinGuessNeutralOutlier.StringName,

            AssassinGuessImpostors.StringName,

            AssassinGuessCrewModifiers.StringName,
            AssassinGuessNonCrewModifiers.StringName,
            AssassinGuessUtilityModifiers.StringName,
            AssassinGuessAlliances.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var all = TouLocale.Get("TouOptionAssassinAll");
        var none = TouLocale.Get("TouOptionAssassinNone");
        var cult = TownOfUsPlugin.Culture;
        var impCount = (int)NumberOfImpostorAssassins.Value;
        var impChance = (int)ImpAssassinChance.Value;
        var impText = TouLocale.Get("TouOptionAssassinImpAssassinTitle") +
                      (impCount > 0 && impChance > 0
                          ? TouLocale.GetParsed("TouOptionAssassinSetAssassins").Replace("<amount>",
                              impCount.ToString(TownOfUsPlugin.Culture)).Replace("<chance>",
                              impChance.ToString(TownOfUsPlugin.Culture))
                          : TouLocale.Get("TouOptionAssassinNoAssassins"));
        var neutCount = (int)NumberOfNeutralAssassins.Value;
        var neutChance = (int)NeutAssassinChance.Value;
        var neutText = TouLocale.Get("TouOptionAssassinNeutAssassinTitle") +
                       (neutCount > 0 && neutChance > 0
                           ? TouLocale.GetParsed("TouOptionAssassinSetAssassins").Replace("<amount>",
                               neutCount.ToString(TownOfUsPlugin.Culture)).Replace("<chance>",
                               neutChance.ToString(TownOfUsPlugin.Culture))
                           : TouLocale.Get("TouOptionAssassinNoAssassins"));
        var assassinShots = $"{((int)AssassinKills.Value).ToString(cult)}";
        if ((int)AssassinKills.Value > 1)
        {
            assassinShots += AssassinMultiKill.Value ? " (Any Per Meeting)" : " (1 Per Meeting)";
        }

        var crewRoles = none;
        var neutRoles = none;
        var impRoles = AssassinGuessImpostors.Value ? all : none;
        var modifiers = all;

        if (!AssassinGuessInvest.Value && !AssassinCrewmateGuess.Value)
        {
            crewRoles = TouLocale.Get("TouOptionAssassinBasicCrew") + ", " + TouLocale.Get("TouOptionAssassinInvestCrew");
        }
        else if (!AssassinCrewmateGuess.Value)
        {
            crewRoles = TouLocale.Get("TouOptionAssassinBasicCrew");
        }
        else if (!AssassinGuessInvest.Value)
        {
            crewRoles = TouLocale.Get("TouOptionAssassinInvestCrew");
        }

        if (AssassinGuessNeutralBenign.Value || AssassinGuessNeutralEvil.Value || AssassinGuessNeutralKilling.Value || AssassinGuessNeutralOutlier.Value)
        {
            if (AssassinGuessNeutralBenign.Value && AssassinGuessNeutralEvil.Value &&
                AssassinGuessNeutralKilling.Value && AssassinGuessNeutralOutlier.Value)
            {
                neutRoles = none;
            }
            else
            {
                string[] neutArray = Array.Empty<string>();

                if (!AssassinGuessNeutralBenign.Value)
                {
                    neutArray = neutArray.AddToArray(TouLocale.Get("TouOptionAssassinNeutBenign"));
                }

                if (!AssassinGuessNeutralEvil.Value)
                {
                    neutArray = neutArray.AddToArray(TouLocale.Get("TouOptionAssassinNeutEvil"));
                }

                if (!AssassinGuessNeutralKilling.Value)
                {
                    neutArray = neutArray.AddToArray(TouLocale.Get("TouOptionAssassinNeutKilling"));
                }

                if (!AssassinGuessNeutralOutlier.Value)
                {
                    neutArray = neutArray.AddToArray(TouLocale.Get("TouOptionAssassinNeutOutlier"));
                }

                neutRoles = string.Join(", ", neutArray);
            }
        }

        if (AssassinGuessCrewModifiers.Value || AssassinGuessNonCrewModifiers.Value || AssassinGuessAlliances.Value)
        {
            if (AssassinGuessCrewModifiers.Value && AssassinGuessUtilityModifiers.Value &&
                AssassinGuessNonCrewModifiers.Value && AssassinGuessAlliances.Value)
            {
                modifiers = TouLocale.Get("TouOptionAssassinUniversalMods");
            }
            else
            {
                var modArray = new[]
                {
                    TouLocale.Get("TouOptionAssassinUniversalMods")
                };
                if (!AssassinGuessCrewModifiers.Value)
                {
                    modArray = modArray.AddToArray(TouLocale.Get("TouOptionAssassinCrewMods"));
                }
                else if (!AssassinGuessUtilityModifiers.Value)
                {
                    modArray = modArray.AddToArray(TouLocale.Get("TouOptionAssassinUtilityCrewMods"));
                }

                if (!AssassinGuessNonCrewModifiers.Value)
                {
                    modArray = modArray.AddToArray(TouLocale.Get("TouOptionAssassinNonCrewMods"));
                }

                if (!AssassinGuessAlliances.Value)
                {
                    modArray = modArray.AddToArray(TouLocale.Get("TouOptionAssassinAllianceMods"));
                }

                modifiers = string.Join(", ", modArray);
            }
        }
        var newArray = new[]
        {
            impText,
            neutText,
            "Assassin Shots: " + assassinShots,
            TouLocale.Get("TouOptionAssassinGuessableCrewRolesTitle") + crewRoles,
            TouLocale.Get("TouOptionAssassinGuessableNeutRolesTitle") + neutRoles,
            TouLocale.Get("TouOptionAssassinGuessableImpRolesTitle") + impRoles,
            TouLocale.Get("TouOptionAssassinGuessableModifiersTitle") + modifiers,
        };
        return newArray;
    }
}