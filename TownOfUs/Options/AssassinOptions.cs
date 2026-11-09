using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Assailant;

namespace TownOfUs.Options;

public sealed class AssassinOptions : AbstractTouModifierOptionGroup<AssassinModifier>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.Assassin");
    public override uint GroupPriority => 7;
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;

    public AmountChanceOption NumberOfImpostorAssassins { get; } =
        new("TouOptionNumberOfImpostorAssassins", 1, 0, 4, 1,
            color: TownOfUsColors.Impostor, asset: TouModifierIcons.Assassin,
            assetName: "TouMira.Modifier.Assailant.Assassin", assetScale: 1.45f)
        {
            ChangedEvent = _impAssassinNotif
        };

    public AmountChanceOption ImpAssassinChance { get; } =
        new("TouOptionImpAssassinChance", 100f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: TownOfUsColors.Impostor, asset: TouModifierIcons.Assassin,
            assetName: "TouMira.Modifier.Assailant.Assassin", assetScale: 1.45f)
        {
            ChangedEvent = _impAssassinNotif,
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins > 0
        };

    public ModdedNumberOption ImpAssassinKills { get; } =
        new("TouOptionImpAssassinKills", 3, 1, 15, 1, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins > 0 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance > 0
        };

    public ModdedToggleOption ImpAssassinMultiKill { get; } =
        new("TouOptionImpAssassinMultiKill", true)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinKills > 1 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins > 0 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance > 0
        };

    public AmountChanceOption NumberOfNeutralAssassins { get; } =
        new("TouOptionNumberOfNeutralAssassins", 1, 0, 4, 1,
            color: TownOfUsColors.Neutral, asset: TouModifierIcons.Assassin,
            assetName: "TouMira.Modifier.Assailant.Assassin", assetScale: 1.45f)
        {
            ChangedEvent = _neutAssassinNotif
        };

    public AmountChanceOption NeutAssassinChance { get; } =
        new("TouOptionNeutAssassinChance", 100f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: TownOfUsColors.Neutral, asset: TouModifierIcons.Assassin,
            assetName: "TouMira.Modifier.Assailant.Assassin", assetScale: 1.45f)
        {
            ChangedEvent = _neutAssassinNotif,
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins > 0
        };

    public ModdedNumberOption NeutAssassinKills { get; } =
        new("TouOptionNeutAssassinKills", 5, 1, 15, 1, MiraNumberSuffixes.None, "0")
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins > 0 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinChance > 0
        };

    public ModdedToggleOption NeutAssassinMultiKill { get; } =
        new("TouOptionNeutAssassinMultiKill", true)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinKills > 1 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins > 0 &&
                            OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinChance > 0
        };

    /*
    public ModdedToggleOption GuessVanillaRoles { get; } =
        new("TouOptionGuessVanillaRoles", true);
    */

    public ModdedToggleOption AssassinCrewmateGuess { get; } =
        new("TouOptionAssassinCrewmateGuess", false);

    public ModdedToggleOption AssassinGuessInvest { get; } =
        new("TouOptionAssassinGuessInvest", false);

    public ModdedToggleOption AssassinGuessNeutralBenign { get; } =
        new("TouOptionAssassinGuessNeutralBenign", true);

    public ModdedToggleOption AssassinGuessNeutralEvil { get; } =
        new("TouOptionAssassinGuessNeutralEvil", true);

    public ModdedToggleOption AssassinGuessNeutralKilling { get; } =
        new("TouOptionAssassinGuessNeutralKilling", true);

    public ModdedToggleOption AssassinGuessNeutralOutlier { get; } =
        new("TouOptionAssassinGuessNeutralOutlier", true);

    public ModdedToggleOption AssassinGuessImpostors { get; } =
        new("TouOptionAssassinGuessImpostors", true);

    public ModdedToggleOption AssassinGuessCrewModifiers { get; } =
        new("TouOptionAssassinGuessCrewModifiers", true);

    public ModdedToggleOption AssassinGuessUtilityModifiers { get; } =
        new("TouOptionAssassinGuessUtilityModifiers", false)
        {
            Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers
        };

    public ModdedToggleOption AssassinGuessImpostorModifiers { get; } =
        new("TouOptionAssassinGuessImpostorModifiers", true);

    public ModdedToggleOption AssassinGuessNonCrewModifiers { get; } =
        new("TouOptionAssassinGuessNonCrewModifiers", true);

    public ModdedToggleOption AssassinGuessAlliances { get; } =
        new("TouOptionAssassinGuessAlliances", true);
    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        new HashSet<StringNames>
        {
            NumberOfImpostorAssassins.StringName,
            ImpAssassinChance.StringName,
            NumberOfNeutralAssassins.StringName,
            NeutAssassinChance.StringName,

            NeutAssassinKills.StringName,
            NeutAssassinMultiKill.StringName,
            ImpAssassinKills.StringName,
            ImpAssassinMultiKill.StringName,

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
            AssassinGuessImpostorModifiers.StringName,
            AssassinGuessUtilityModifiers.StringName,
            AssassinGuessAlliances.StringName,
        };

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        var all = MiraLocaleManager.Get("TouOptionAssassinAll");
        var none = MiraLocaleManager.Get("TouOptionAssassinNone");
        var cult = TownOfUsPlugin.Culture;
        int impCount = NumberOfImpostorAssassins;
        int impChance = ImpAssassinChance;
        var impText = MiraLocaleManager.Get("TouOptionAssassinImpTitleNone");
        if (impCount == 1 && impChance > 0)
        {
            impText = MiraLocaleManager.Get("TouOptionAssassinImpTitleSingle").Replace("<chance>",
                impChance.ToString(TownOfUsPlugin.Culture));
        }
        else if (impCount > 0 && impChance > 0)
        {
            impText = MiraLocaleManager.Get("TouOptionAssassinImpTitleFull").Replace("<amount>",
                impCount.ToString(TownOfUsPlugin.Culture)).Replace("<chance>",
                impChance.ToString(TownOfUsPlugin.Culture));
        }

        if (impCount > 0 && impChance > 0)
        {
            int impKills = ImpAssassinKills;
            impText += " " + MiraLocaleManager.Get("TouOptionAssassinShots")
                .Replace("<amount>", impKills.ToString(cult));

            if (impKills > 1)
            {
                impText += ImpAssassinMultiKill
                    ? " " + MiraLocaleManager.Get("TouOptionAssassinOverall")
                    : " " + MiraLocaleManager.Get("TouOptionAssassinOnePerMeeting");
            }
        }

        int neutCount = NumberOfNeutralAssassins;
        int neutChance = NeutAssassinChance;
        var neutText = MiraLocaleManager.Get("TouOptionAssassinNeutTitleNone");
        if (neutCount == 1 && neutChance > 0)
        {
            neutText = MiraLocaleManager.Get("TouOptionAssassinNeutTitleSingle").Replace("<chance>",
                neutChance.ToString(TownOfUsPlugin.Culture));
        }
        else if (neutCount > 0 && neutChance > 0)
        {
            neutText = MiraLocaleManager.Get("TouOptionAssassinNeutTitleFull").Replace("<amount>",
                neutCount.ToString(TownOfUsPlugin.Culture)).Replace("<chance>",
                neutChance.ToString(TownOfUsPlugin.Culture));
        }

        if (neutCount > 0 && neutChance > 0)
        {
            int neutKills = NeutAssassinKills;
            neutText += " " + MiraLocaleManager.Get("TouOptionAssassinShots")
                .Replace("<amount>", neutKills.ToString(cult));

            if (neutKills > 1)
            {
                neutText += NeutAssassinMultiKill
                    ? " " + MiraLocaleManager.Get("TouOptionAssassinOverall")
                    : " " + MiraLocaleManager.Get("TouOptionAssassinOnePerMeeting");
            }
        }

        var crewRoles = none;
        var neutRoles = none;
        var impRoles = AssassinGuessImpostors ? none : all;
        var modifiers = all;

        if (!AssassinGuessInvest && !AssassinCrewmateGuess)
        {
            crewRoles = MiraLocaleManager.Get("TouOptionAssassinBasicCrew") + ", " + MiraLocaleManager.Get("TouOptionAssassinInvestCrew");
        }
        else if (!AssassinCrewmateGuess)
        {
            crewRoles = MiraLocaleManager.Get("TouOptionAssassinBasicCrew");
        }
        else if (!AssassinGuessInvest)
        {
            crewRoles = MiraLocaleManager.Get("TouOptionAssassinInvestCrew");
        }

        if (AssassinGuessNeutralBenign || AssassinGuessNeutralEvil ||
            AssassinGuessNeutralKilling || AssassinGuessNeutralOutlier)
        {
            if (AssassinGuessNeutralBenign && AssassinGuessNeutralEvil &&
                AssassinGuessNeutralKilling && AssassinGuessNeutralOutlier)
            {
                neutRoles = none;
            }
            else
            {
                string[] neutArray = [];

                if (!AssassinGuessNeutralBenign)
                {
                    neutArray = neutArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinNeutBenign"));
                }

                if (!AssassinGuessNeutralEvil)
                {
                    neutArray = neutArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinNeutEvil"));
                }

                if (!AssassinGuessNeutralKilling)
                {
                    neutArray = neutArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinNeutKilling"));
                }

                if (!AssassinGuessNeutralOutlier)
                {
                    neutArray = neutArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinNeutOutlier"));
                }

                neutRoles = string.Join(", ", neutArray);
            }
        }

        if (AssassinGuessCrewModifiers || AssassinGuessNonCrewModifiers ||
            AssassinGuessImpostorModifiers || AssassinGuessAlliances)
        {
            if (AssassinGuessCrewModifiers && AssassinGuessUtilityModifiers &&
                AssassinGuessNonCrewModifiers && AssassinGuessAlliances &&
                AssassinGuessImpostorModifiers)
            {
                modifiers = MiraLocaleManager.Get("TouOptionAssassinUniversalMods");
            }
            else
            {
                var modArray = new[]
                {
                    MiraLocaleManager.Get("TouOptionAssassinUniversalMods")
                };

                if (!AssassinGuessCrewModifiers)
                {
                    modArray = modArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinCrewMods"));
                }
                else if (!AssassinGuessUtilityModifiers)
                {
                    modArray = modArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinUtilityCrewMods"));
                }

                if (!AssassinGuessImpostorModifiers)
                {
                    modArray = modArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinImpMods"));
                }

                if (!AssassinGuessNonCrewModifiers)
                {
                    modArray = modArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinNonCrewMods"));
                }

                if (!AssassinGuessAlliances)
                {
                    modArray = modArray.AddToArray(MiraLocaleManager.Get("TouOptionAssassinAllianceMods"));
                }

                modifiers = string.Join(", ", modArray);
            }
        }

        var newArray = new[]
        {
            impText,
            neutText,
            MiraLocaleManager.Get("TouOptionAssassinGuessableCrewRolesTitle") + crewRoles,
            MiraLocaleManager.Get("TouOptionAssassinGuessableNeutRolesTitle") + neutRoles,
            MiraLocaleManager.Get("TouOptionAssassinGuessableImpRolesTitle") + impRoles,
            MiraLocaleManager.Get("TouOptionAssassinGuessableModifiersTitle") + modifiers,
        };

        return newArray;
    }

    private static Action<float> _impAssassinNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins;
        var opt = OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance;
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            MiraLocaleManager.Get("TownOfUsMira.Modifier.Assassin"),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    };

    private static Action<float> _neutAssassinNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins;
        var opt = OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinChance;
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            MiraLocaleManager.Get("TownOfUsMira.Modifier.Assassin"),
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    };
}