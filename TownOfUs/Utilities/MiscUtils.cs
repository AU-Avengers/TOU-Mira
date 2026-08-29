using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MiraAPI.GameModes;
using PowerTools;
using TMPro;
using TownOfUs.Events;
using TownOfUs.GameModes;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Patches;
using TownOfUs.Patches.Misc;
using TownOfUs.Patches.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Roles.TownOfPolus;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Utilities;

public static class MiscUtils
{
    /// <summary>
    /// Get all living players that aren't neutral benigns.
    /// </summary>
    /// <returns>A list of alive players.</returns>
    public static List<PlayerControl> GetImpactfulLivingPlayers()
    {
        return
        [
            .. GameData.Instance.AllPlayers.ToArray()
                .Where(x => !x.IsDead && !x.Disconnected && x.Object && !x.Object.Is(RoleAlignment.NeutralBenign))
                .Select(x => x.Object)
        ];
    }
    public static int GameHaltersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Data.Role is IContinuesGame gameHalt && gameHalt.ContinuesGame || x.GetModifiers<BaseModifier>()
            .Any(y => y is IContinuesGame gameHaltMod && gameHaltMod.ContinuesGame));

    public static int NonGameEndingNeutralCount => Helpers.GetAlivePlayers().Count(x => x.Is(RoleAlignment.NeutralBenign));

    public static int KillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor() ||
        x.Is(RoleAlignment.NeutralKilling) ||
        (x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
         !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
         OptionGroupSingleton<GameMechanicOptions>.Instance.CrewKillersContinue));

    public static int RealKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling));

    public static int NKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Is(RoleAlignment.NeutralKilling));

    public static int NonImpKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Is(RoleAlignment.NeutralKilling) ||
        (x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
         !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
         OptionGroupSingleton<GameMechanicOptions>.Instance.CrewKillersContinue));

    public static int ImpAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.IsImpostor() || x.GetModifiers<AllianceGameModifier>().Any(y => y.TrueFactionType is AlliedFaction.Impostor));

    public static int ImpostorHeadCount => Helpers.GetAlivePlayers().Count(x =>
        x.IsImpostor() || x.GetModifiers<AllianceGameModifier>().Any(y => y.TrueFactionType is AlliedFaction.Impostor && y.CountTowardsTrueFaction));

    public static int CrewKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
        !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
        OptionGroupSingleton<GameMechanicOptions>.Instance.CrewKillersContinue);

    /// <summary>
    /// Gets all registered <see cref="BaseModifier"/>s in MiraAPI.
    /// </summary>
    /// <returns>A list of <see cref="BaseModifier"/>s.</returns>
    public static IEnumerable<BaseModifier> AllModifiers { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="BaseModifier"/>s in MiraAPI that have the <see cref="IAssignableTargets"/> interface.
    /// </summary>
    /// <returns>A list of <see cref="IAssignableTargets"/>s.</returns>
    public static IEnumerable<IAssignableTargets> AssignableTargetModifiers { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="BaseModifier"/>s in MiraAPI that have the <see cref="IWikiDiscoverable"/> interface.
    /// </summary>
    /// <returns>A list of <see cref="BaseModifier"/>s.</returns>
    public static IEnumerable<BaseModifier> AllTouWikiModifiers { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="BaseModifier"/>s in MiraAPI that have the <see cref="IWikiDiscoverable"/> interface or are present in <see cref="SoftWikiEntries.RoleEntries"/>.
    /// </summary>
    /// <returns>A list of <see cref="BaseModifier"/>s.</returns>
    public static IEnumerable<BaseModifier> AllOverallWikiModifiers { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="TouBaseGameModifier"/>s in MiraAPI.
    /// </summary>
    /// <returns>A list of <see cref="TouBaseGameModifier"/>s.</returns>
    public static IEnumerable<TouBaseGameModifier> AllBaseGameModifiers { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="RoleBehaviour"/>s added through MiraAPI, excluding <see cref="NeutralGhostRole"/> and possibly any other registered basic roles.
    /// </summary>
    /// <returns>A list of <see cref="RoleBehaviour"/>s.</returns>
    public static IEnumerable<RoleBehaviour> AllRoles { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="ITownOfUsRole"/>s.
    /// </summary>
    /// <returns>A list of <see cref="ITownOfUsRole"/>s.</returns>
    public static IEnumerable<ITownOfUsRole> AllTouRoles { get; internal set; }
    /// <summary>
    /// Gets all registered <see cref="RoleBehaviour"/>s, excluding <see cref="CrewmateGhostRole"/>, <see cref="ImpostorGhostRole"/>, <see cref="NeutralGhostRole"/>, and possibly any other registered basic roles.
    /// </summary>
    /// <returns>A list of <see cref="RoleBehaviour"/>s.</returns>
    public static IEnumerable<RoleBehaviour> AllInGameRoles { get; internal set; }

    /// <summary>
    /// Gets all registered <see cref="RoleBehaviour"/>s that aren't blacklisted.
    /// </summary>
    /// <returns>A list of <see cref="RoleBehaviour"/>s.</returns>
    public static IEnumerable<RoleBehaviour> AllRegisteredRoles => AllInGameRoles.Where(x => !x.IsRoleBlacklisted());

    /// <summary>
    /// Gets all registered <see cref="RoleBehaviour"/>s that aren't blacklisted and spawn on the current mode.
    /// </summary>
    /// <returns>A list of <see cref="RoleBehaviour"/>s.</returns>
    public static IEnumerable<RoleBehaviour> SpawnableRoles =>
        AllInGameRoles.Where(x => !x.IsRoleBlacklisted() && CustomRoleUtils.CanSpawnOnCurrentMode(x));

    public static ReadOnlyCollection<IModdedOption>? GetModdedOptionsForRole(Type classType)
    {
        var optionGroups =
            AccessTools.Field(typeof(ModdedOptionsManager), "Groups").GetValue(null) as List<AbstractOptionGroup>;

        return optionGroups?.FirstOrDefault(x => x.OptionableType == classType)?.Children;
    }

    public static string AppendOptionsText(Type classType)
    {
        var options = GetModdedOptionsForRole(classType);
        if (options == null)
        {
            return string.Empty;
        }

        IWikiOptionsSummaryProvider? summaryProvider = null;
        IReadOnlySet<StringNames>? hiddenKeys = null;
        try
        {
            var optionGroups =
                AccessTools.Field(typeof(ModdedOptionsManager), "Groups").GetValue(null) as List<AbstractOptionGroup>;
            summaryProvider =
                optionGroups?.FirstOrDefault(x => x.OptionableType == classType) as IWikiOptionsSummaryProvider;
            hiddenKeys = summaryProvider?.WikiHiddenOptionKeys;
        }
        catch
        {
            summaryProvider = null;
            hiddenKeys = null;
        }

        var builder = new StringBuilder();
        builder.AppendLine(TownOfUsPlugin.Culture,
            $"\n<size=50%> \n</size><b>{TownOfUsColors.Vigilante.ToTextColor()}{MiraLocaleManager.Get("Options")}</color></b>");

        var insertedSummary = false;
        foreach (var option in options)
        {
            if (!insertedSummary && summaryProvider != null && hiddenKeys != null)
            {
                StringNames? key = option switch
                {
                    ModdedToggleOption t => t.StringName,
                    ModdedEnumOption e => e.StringName,
                    ModdedNumberOption n => n.StringName,
                    _ => null
                };
                if (key.HasValue && hiddenKeys.Contains(key.Value))
                {
                    foreach (var line in summaryProvider.GetWikiOptionSummaryLines())
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            builder.AppendLine(line);
                        }
                    }

                    insertedSummary = true;
                }
            }

            switch (option)
            {
                case ModdedToggleOption toggleOption:
                    if (!toggleOption.Visible())
                    {
                        continue;
                    }

                    if (hiddenKeys != null && hiddenKeys.Contains(toggleOption.StringName))
                    {
                        continue;
                    }

                    builder.AppendLine(TranslationController.Instance.GetString(toggleOption.StringName) + ": " +
                                       toggleOption.Value);
                    break;
                /*case ModdedMultiSelectOption<Enum> enumOption:
                    if (!enumOption.Visible())
                    {
                        continue;
                    }

                    builder.AppendLine(enumOption.Title + ": " + enumOption.Values[enumOption.Value]);
                    break;*/
                case ModdedEnumOption enumOption:
                    if (!enumOption.Visible())
                    {
                        continue;
                    }

                    if (hiddenKeys != null && hiddenKeys.Contains(enumOption.StringName))
                    {
                        continue;
                    }

                    builder.AppendLine(TranslationController.Instance.GetString(enumOption.StringName) + ": " +
                                       MiraLocaleManager.Get(enumOption.Values[enumOption.Value],
                                           enumOption.Values[enumOption.Value]));
                    break;
                case ModdedNumberOption numberOption:
                    if (!numberOption.Visible())
                    {
                        continue;
                    }

                    if (hiddenKeys != null && hiddenKeys.Contains(numberOption.StringName))
                    {
                        continue;
                    }

                    var optionStr = numberOption.Data.GetValueString(numberOption.Value);
                    if (optionStr.Contains(".000"))
                    {
                        optionStr = optionStr.Replace(".000", "");
                    }
                    else if (optionStr.Contains(".00"))
                    {
                        optionStr = optionStr.Replace(".00", "");
                    }
                    else if (optionStr.Contains(".0"))
                    {
                        optionStr = optionStr.Replace(".0", "");
                    }

                    var title = TranslationController.Instance.GetString(numberOption.StringName);
                    if (numberOption is { NegativeWordValue: not "#", Value: -1 })
                    {
                        builder.AppendLine(title + $": {numberOption.NegativeWordValue}");
                    }
                    else if (numberOption is { ZeroWordValue: not "#", Value: 0 })
                    {
                        builder.AppendLine(title + $": {numberOption.ZeroWordValue}");
                    }
                    else
                    {
                        builder.AppendLine(title + ": " + optionStr);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    public static RoleAlignment GetRoleAlignment(this ICustomRole role)
    {
        if (role is ITownOfUsRole touRole)
        {
            return touRole.RoleAlignment;
        }

        var alignments = Enum.GetValues<RoleAlignment>();
        foreach (var alignment in alignments)
        {
            var roleAlignment = alignment;
            if (role.RoleOptionsGroup.Name.Replace(" Roles", "") == roleAlignment.ToDisplayString() ||
                role.RoleOptionsGroup.Name.Replace($" {MiraLocaleManager.Get("Roles")}", "") ==
                roleAlignment.ToDisplayString())
            {
                return roleAlignment;
            }
        }

        var basicRole = role as RoleBehaviour;
        if (basicRole!.IsNeutral())
        {
            return RoleAlignment.NeutralOutlier;
        }
        else if (basicRole!.IsImpostor())
        {
            return RoleAlignment.ImpostorSupport;
        }
        else
        {
            return RoleAlignment.CrewmateSupport;
        }
    }

    public static RoleAlignment GetRoleAlignment(this RoleBehaviour role)
    {
        if (role is ITownOfUsRole touRole)
        {
            return touRole.RoleAlignment;
        }
        else if (role is ICustomRole customRole)
        {
            var alignments = Enum.GetValues<RoleAlignment>();
            foreach (var alignment in alignments)
            {
                var roleAlignment = alignment;
                if (customRole.RoleOptionsGroup.Name.Replace(" Roles", "") == roleAlignment.ToDisplayString() ||
                    customRole.RoleOptionsGroup.Name.Replace($" {MiraLocaleManager.Get("Roles")}", "") ==
                    roleAlignment.ToDisplayString())
                {
                    return roleAlignment;
                }
            }
        }

        if (role.IsDead)
        {
            if (role.IsNeutral())
            {
                return RoleAlignment.NeutralAfterlife;
            }
            if (role.IsImpostor())
            {
                return RoleAlignment.ImpostorAfterlife;
            }
            return RoleAlignment.CrewmateAfterlife;
        }

        if (role.Role is RoleTypes.Tracker or RoleTypes.Detective)
        {
            return RoleAlignment.CrewmateInvestigative;
        }

        if (role.Role is RoleTypes.Shapeshifter or RoleTypes.Phantom)
        {
            return RoleAlignment.ImpostorConcealing;
        }

        if (role.Role is RoleTypes.Viper)
        {
            return RoleAlignment.ImpostorKilling;
        }

        if (role.IsNeutral())
        {
            return RoleAlignment.NeutralOutlier;
        }
        else if (role.IsImpostor())
        {
            return RoleAlignment.ImpostorSupport;
        }
        else
        {
            return RoleAlignment.CrewmateSupport;
        }
    }

    public static ModifierFaction GetSoftModifierFaction(this BaseModifier mod)
    {
        if (mod is GameModifier gameMod)
        {
            var isForCrew = false;
            var isForNeut = false;
            var isForImp = false;
            foreach (var crewRole in AllRegisteredRoles.Where(x => x.IsCrewmate()))
            {
                if (!isForCrew && gameMod.IsModifierValidOn(crewRole))
                {
                    isForCrew = true;
                    break;
                }
            }

            foreach (var neutRole in AllRegisteredRoles.Where(x => x.IsNeutral()))
            {
                if (!isForNeut && gameMod.IsModifierValidOn(neutRole))
                {
                    isForNeut = true;
                    break;
                }
            }

            foreach (var impRole in AllRegisteredRoles.Where(x => x.IsImpostor()))
            {
                if (!isForImp && gameMod.IsModifierValidOn(impRole))
                {
                    isForImp = true;
                    break;
                }
            }

            if (isForCrew && isForNeut && isForImp)
            {
                return ModifierFaction.Universal;
            }
            else if (isForCrew && isForNeut)
            {
                return ModifierFaction.NonImpostor;
            }
            else if (isForNeut && isForImp)
            {
                return ModifierFaction.NonCrewmate;
            }
            else if (isForCrew && isForImp)
            {
                return ModifierFaction.NonNeutral;
            }
            else if (isForImp)
            {
                return ModifierFaction.Impostor;
            }
            else if (isForCrew)
            {
                return ModifierFaction.Crewmate;
            }
            else if (isForNeut)
            {
                return ModifierFaction.Neutral;
            }
        }

        return ModifierFaction.External;
    }

    public static ModifierFaction GetModifierFaction(this BaseModifier mod)
    {
        if (mod is TouBaseGameModifier touMod)
        {
            return touMod.FactionType;
        }

        if (SoftWikiEntries.ModifierEntries.ContainsKey(mod))
        {
            var name = SoftWikiEntries.ModifierEntries.FirstOrDefault(x => x.Key == mod).Value.TeamName;
            if (Enum.TryParse<ModifierFaction>(name, out var modFaction))
            {
                return modFaction;
            }
        }

        return ModifierFaction.External;
    }

    public static string GetParsedModifierFaction(BaseModifier modifier)
    {
        var localeName = $"{modifier.GetModifierFaction()}";
        var localizedName = MiraLocaleManager.Get(localeName);

        return localizedName;
    }

    public static string GetParsedModifierFaction(ModifierFaction faction, bool coloredText = false)
    {
        var localizedName = MiraLocaleManager.Get($"{faction}");

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(MiraLocaleManager.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(MiraLocaleManager.Get("GameKeyword")))
            {
                localizedName = $"<color=#888888>{localizedName}";
            }
            else
            {
                localizedName = $"<color=#FFFFFF>{localizedName}";
            }

            localizedName += "</color>";
        }

        return localizedName;
    }

    public static string GetColoredFactionString(string text)
    {
        if (text.Contains("Crewmate") || text.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
        {
            text = $"<color=#68ACF4>{text}";
        }
        else if (text.Contains("Impostor") || text.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
        {
            text = $"<color=#D63F42>{text}";
        }
        else if (text.Contains("Neutral") || text.Contains(MiraLocaleManager.Get("NeutralKeyword")))
        {
            text = $"<color=#8A8A8A>{text}";
        }
        else if (text.Contains("Game") || text.Contains(MiraLocaleManager.Get("GameKeyword")))
        {
            text = $"<color=#888888>{text}";
        }
        else
        {
            text = $"<color=#FFFFFF>{text}";
        }

        text += "</color>";

        return text;
    }

    public static string GetParsedRoleAlignment(ICustomRole role, bool coloredText = false)
    {
        var localeName = $"{role.GetRoleAlignment()}";
        var localizedName = MiraLocaleManager.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(MiraLocaleManager.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(MiraLocaleManager.Get("GameKeyword")))
            {
                localizedName = $"<color=#888888>{localizedName}";
            }
            else
            {
                localizedName = $"<color=#FFFFFF>{localizedName}";
            }

            localizedName += "</color>";
        }

        return localizedName;
    }

    public static string GetParsedRoleAlignment(RoleBehaviour role, bool coloredText = false)
    {
        var roleAlignment = role.GetRoleAlignment();
        var localeName = $"{roleAlignment}";
        if (roleAlignment is RoleAlignment.Crewmate or RoleAlignment.Impostor or RoleAlignment.Neutral)
        {
            localeName = $"{roleAlignment}Keyword";
        }
        var localizedName = MiraLocaleManager.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(MiraLocaleManager.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(MiraLocaleManager.Get("GameKeyword")))
            {
                localizedName = $"<color=#888888>{localizedName}";
            }
            else
            {
                localizedName = $"<color=#FFFFFF>{localizedName}";
            }

            localizedName += "</color>";
        }

        return localizedName;
    }

    public static string GetParsedRoleAlignment(RoleAlignment roleAlignment, bool coloredText = false)
    {
        var localeName = $"{roleAlignment}";
        if (roleAlignment is RoleAlignment.Crewmate or RoleAlignment.Impostor or RoleAlignment.Neutral)
        {
            localeName = $"{roleAlignment}Keyword";
        }

        var localizedName = MiraLocaleManager.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(MiraLocaleManager.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(MiraLocaleManager.Get("GameKeyword")))
            {
                localizedName = $"<color=#888888>{localizedName}";
            }
            else
            {
                localizedName = $"<color=#FFFFFF>{localizedName}";
            }

            localizedName += "</color>";
        }

        return localizedName;
    }

    public static Color GetRoleFactionColor(RoleBehaviour role, bool useAltColors = false)
    {
        if (role)
        {
            if (role.IsCrewmate())
            {
                return useAltColors ? TownOfUsColors.Crewmate : Palette.CrewmateBlue;
            }

            if (role.IsImpostor())
            {
                return useAltColors ? TownOfUsColors.ImpSoft : TownOfUsColors.Impostor;
            }
        }

        return TownOfUsColors.Neutral;
    }

    public static Color GetRoleFactionColor(RoleAlignment roleAlignment, bool useAltColors = false)
    {
        var localeName = $"{roleAlignment}";
        var localizedName = MiraLocaleManager.Get(localeName);

        if (localizedName.Contains("Crewmate") || localizedName.Contains(MiraLocaleManager.Get("CrewmateKeyword")))
        {
            return useAltColors ? TownOfUsColors.Crewmate : Palette.CrewmateBlue;
        }
        else if (localizedName.Contains("Impostor") || localizedName.Contains(MiraLocaleManager.Get("ImpostorKeyword")))
        {
            return useAltColors ? TownOfUsColors.ImpSoft : TownOfUsColors.Impostor;
        }

        return TownOfUsColors.Neutral;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredRoles(RoleAlignment alignment)
    {
        var roles = AllRoles.Where(x => x.GetRoleAlignment() == alignment);

        var registeredRoles = roles.ToList();

        switch (alignment)
        {
            case RoleAlignment.CrewmateInvestigative:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Detective));
                break;
            case RoleAlignment.CrewmateSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Crewmate));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Scientist));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Noisemaker));
                // registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                break;
            case RoleAlignment.CrewmateAfterlife:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.GuardianAngel));
                break;
            case RoleAlignment.ImpostorSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                break;
            case RoleAlignment.ImpostorKilling:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Viper));
                break;
            case RoleAlignment.ImpostorConcealing:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Shapeshifter));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Phantom));
                break;
        }

        return registeredRoles;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredRoles(ModdedRoleTeams team)
    {
        var roles = AllRoles.Where(x => x is ICustomRole role && role.Team == team);
        var registeredRoles = roles.ToList();

        switch (team)
        {
            case ModdedRoleTeams.Crewmate:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Crewmate));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Scientist));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Noisemaker));
                // registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Detective));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.GuardianAngel));
                break;
            case ModdedRoleTeams.Impostor:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Shapeshifter));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Viper));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Phantom));
                break;
        }

        return registeredRoles;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredGhostRoles()
    {
        var baseGhostRoles = AllRegisteredRoles
            .Where(x => x.IsDead && AllRoles.All(y => y.Role != x.Role));
        var ghostRoles = AllRoles.Where(x => x.IsDead && x is not SpectatorRole).Union(baseGhostRoles);

        return ghostRoles;
    }

    public static RoleBehaviour? GetRegisteredRole(RoleTypes roleType)
    {
        // we want to prioritize the custom roles because the role has the right RoleColour/TeamColor
        var role = AllRoles.FirstOrDefault(x => x.Role == roleType) ??
                   AllRegisteredRoles.FirstOrDefault(x => x.Role == roleType);

        return role;
    }

    public static T? GetRole<T>() where T : RoleBehaviour
    {
        return PlayerControl.AllPlayerControls.ToArray().ToList().Find(x => x.Data.Role is T)?.Data?.Role as T;
    }

    public static IEnumerable<RoleBehaviour> GetRoles(RoleAlignment alignment)
    {
        return CustomRoleUtils.GetActiveRoles()
            .Where(x => x.GetRoleAlignment() == alignment);
    }

    public static PlayerControl? GetPlayerWithModifier<T>() where T : BaseModifier
    {
        return ModifierUtils.GetPlayersWithModifier<T>().FirstOrDefault();
    }

    public static string GetIdPart(ITownOfUsRole role)
    {
        return role.IdPart;
    }

    public static string GetIdPart(ICustomRole role)
    {
        var name = role.RoleName;
        if (role is ITownOfUsRole touRole)
        {
            name = touRole.IdPart;
        }

        return name;
    }

    public static string GetIdPart(RoleBehaviour role)
    {
        var name = role.GetRoleName();
        if (role is ITownOfUsRole touRole)
        {
            name = touRole.IdPart;
        }

        return name;
    }

    public static string GetIdPart(BaseModifier modifier)
    {
        if (modifier is TouBaseGameModifier touMod)
        {
            return touMod.IdPart;
        }

        return modifier.ModifierName;
    }

    public static Color GetRoleColour(string name)
    {
        var pInfo = typeof(TownOfUsColors).GetProperty(name, BindingFlags.Public | BindingFlags.Static);

        if (pInfo == null)
        {
            return TownOfUsColors.Impostor;
        }

        var colour = (Color)pInfo.GetValue(null)!;

        return colour;
    }

    public static Color GetModifierColour(BaseModifier modifier)
    {
        if (modifier is TouBaseGameModifier touMod)
        {
            return touMod.Configuration.UiColor;
        }
        var color = GetRoleColour(GetIdPart(modifier).Replace(" ", string.Empty));
        if (modifier is IColoredModifier colorMod)
        {
            color = colorMod.ModifierColor;
        }

        return color;
    }

    public static string RoleNameLookup(RoleTypes roleType)
    {
        var role = RoleManager.Instance.GetRole(roleType);
        return role?.GetRoleName() ??
               TranslationController.Instance.GetString(roleType == RoleTypes.Crewmate
                   ? StringNames.Crewmate
                   : StringNames.Impostor);
    }

    public static IEnumerable<RoleBehaviour> GetPotentialRoles()
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;
        var assignmentData = SpawnableRoles.Select(role =>
            new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var roleList = assignmentData.Where(x => x is { Chance: > 0, Count: > 0, Role: ICustomRole })
            .Select(x => x.Role);
        var array = AllRegisteredRoles;
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Detective) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Detective)!);
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Tracker) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Tracker)!);
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Noisemaker) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Noisemaker)!);
        /*if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Engineer) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Engineer)!);*/
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Scientist) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Scientist)!);
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Shapeshifter) is
            { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Shapeshifter)!);
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Phantom) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Phantom)!);
        if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Viper) is { Chance: > 0, Count: > 0 })
            roleList = roleList.AddItem(
                array.FirstOrDefault(x => x.Role == RoleTypes.Viper)!);

        var crewmateRole = AllRegisteredRoles.FirstOrDefault(x => x.Role == RoleTypes.Crewmate);
        roleList = roleList.AddItem(crewmateRole!);
        //Error($"GetPotentialRoles - crewmateRole: '{crewmateRole?.GetRoleName()}'");

        var impostorRole = AllRegisteredRoles.FirstOrDefault(x => x.Role == RoleTypes.Impostor);
        roleList = roleList.AddItem(impostorRole!);

        if (TutorialManager.InstanceExists)
        {
            roleList = AllRegisteredRoles;
        }

        //Error($"GetPotentialRoles - impostorRole: '{impostorRole?.GetRoleName()}'");

        //roleList.Do(x => Error($"GetPotentialRoles - role: '{x.GetRoleName()}'"));

        return roleList;
    }

    public static void AddFakeChat(NetworkedPlayerInfo basePlayer, string nameText, string message,
        bool showHeadsup = false, bool altColors = false, bool onLeft = true)
    {
        if (!FakeChatHistory.IsReplaying)
        {
            FakeChatHistory.Record(nameText, message);
        }
        
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();
        var clonedBubble = chat.GetPooledBubble();
        clonedBubble.gameObject.name = TeamChatPatches.PublicBubbleName;

        pooledBubble.transform.SetParent(TeamChatPatches.PublicChatItems);
        clonedBubble.transform.SetParent(TeamChatPatches.MergedChatItems);
        pooledBubble.transform.localScale = Vector3.one;
        clonedBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
            clonedBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
            clonedBubble.SetRight();
        }

        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.color = Color.white;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.text = WikiHyperLinkPatches.CheckForTags(message, pooledBubble.TextArea);
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new Vector2(5.52f,
            0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);

        clonedBubble.SetCosmetics(basePlayer);
        clonedBubble.NameText.text = nameText;
        clonedBubble.NameText.color = Color.white;
        clonedBubble.NameText.ForceMeshUpdate(true, true);
        clonedBubble.votedMark.enabled = false;
        clonedBubble.Xmark.enabled = false;
        clonedBubble.TextArea.text = message;
        clonedBubble.TextArea.text = WikiHyperLinkPatches.CheckForTags(message, clonedBubble.TextArea);
        clonedBubble.TextArea.ForceMeshUpdate(true, true);
        clonedBubble.Background.size = new Vector2(5.52f,
            0.2f + clonedBubble.NameText.GetNotDumbRenderedHeight() + clonedBubble.TextArea.GetNotDumbRenderedHeight());
        clonedBubble.MaskArea.size = clonedBubble.Background.size - new Vector2(0, 0.03f);
        if (altColors)
        {
            pooledBubble.Background.color = Color.black;
            pooledBubble.TextArea.color = Color.white;
            clonedBubble.Background.color = Color.black;
            clonedBubble.TextArea.color = Color.white;
        }

        pooledBubble.AlignChildren();
        clonedBubble.AlignChildren();
        TeamChatPatches.PublicChatBubbles.Add(pooledBubble);
        TeamChatPatches.MergedChatBubbles.Add(new TeamChatPatches.MergedBubble(clonedBubble, true));
        TeamChatPatches.AlignAllChatBubbles(chat, ChatToCheck.Public);
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }

        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch =
                0.5f + PlayerControl.LocalPlayer.PlayerId / 15f;
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
    }

    public static void AddSystemChat(NetworkedPlayerInfo basePlayer, string nameText, string message,
        bool showHeadsup = false, bool altColors = false, bool onLeft = true)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();
        var clonedBubble = chat.GetPooledBubble();
        clonedBubble.gameObject.name = TeamChatPatches.PublicBubbleName;

        pooledBubble.transform.SetParent(TeamChatPatches.PublicChatItems);
        clonedBubble.transform.SetParent(TeamChatPatches.MergedChatItems);
        pooledBubble.transform.localScale = Vector3.one;
        clonedBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
            clonedBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
            clonedBubble.SetRight();
        }

        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.color = Color.white;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.text = WikiHyperLinkPatches.CheckForTags(message, pooledBubble.TextArea);
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new Vector2(5.52f,
            0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);

        clonedBubble.SetCosmetics(basePlayer);
        clonedBubble.NameText.text = nameText;
        clonedBubble.NameText.color = Color.white;
        clonedBubble.NameText.ForceMeshUpdate(true, true);
        clonedBubble.votedMark.enabled = false;
        clonedBubble.Xmark.enabled = false;
        clonedBubble.TextArea.text = message;
        clonedBubble.TextArea.text = WikiHyperLinkPatches.CheckForTags(message, clonedBubble.TextArea);
        clonedBubble.TextArea.ForceMeshUpdate(true, true);
        clonedBubble.Background.size = new Vector2(5.52f,
            0.2f + clonedBubble.NameText.GetNotDumbRenderedHeight() + clonedBubble.TextArea.GetNotDumbRenderedHeight());
        clonedBubble.MaskArea.size = clonedBubble.Background.size - new Vector2(0, 0.03f);

        if (altColors)
        {
            pooledBubble.Background.color = Color.black;
            pooledBubble.TextArea.color = Color.white;
            clonedBubble.Background.color = Color.black;
            clonedBubble.TextArea.color = Color.white;
        }

        pooledBubble.AlignChildren();
        clonedBubble.AlignChildren();
        TeamChatPatches.PublicChatBubbles.Add(pooledBubble);
        TeamChatPatches.MergedChatBubbles.Add(new TeamChatPatches.MergedBubble(clonedBubble, true));
        TeamChatPatches.AlignAllChatBubbles(chat, ChatToCheck.Public);
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }

        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch =
                0.5f + PlayerControl.LocalPlayer.PlayerId / 15f;
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
    }

    public static void AddTeamChat(NetworkedPlayerInfo basePlayer, string nameText, string message,
        bool showHeadsup = false, bool onLeft = true, bool blackoutText = true, BubbleType bubbleType = BubbleType.None)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();
        var clonedBubble = chat.GetPooledBubble();

        pooledBubble.transform.SetParent(blackoutText
            ? TeamChatPatches.PrivateChatItems
            : TeamChatPatches.PublicChatItems);
        clonedBubble.transform.SetParent(TeamChatPatches.MergedChatItems);
        pooledBubble.transform.localScale = Vector3.one;
        clonedBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
            clonedBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
            clonedBubble.SetRight();
        }

        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new Vector2(5.52f,
            0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);

        clonedBubble.SetCosmetics(basePlayer);
        clonedBubble.NameText.text = nameText;
        clonedBubble.NameText.ForceMeshUpdate(true, true);
        clonedBubble.votedMark.enabled = false;
        clonedBubble.Xmark.enabled = false;
        clonedBubble.TextArea.text = message;
        clonedBubble.TextArea.ForceMeshUpdate(true, true);
        clonedBubble.Background.size = new Vector2(5.52f,
            0.2f + clonedBubble.NameText.GetNotDumbRenderedHeight() + clonedBubble.TextArea.GetNotDumbRenderedHeight());
        clonedBubble.MaskArea.size = clonedBubble.Background.size - new Vector2(0, 0.03f);

        if (bubbleType is BubbleType.Jailor)
        {
            pooledBubble.ColorBlindName.gameObject.SetActive(false);
            clonedBubble.ColorBlindName.gameObject.SetActive(false);

            var pooledCos = pooledBubble.Player.cosmetics;
            pooledCos.skin.gameObject.SetActive(false);
            pooledCos.hat.gameObject.SetActive(false);
            pooledCos.skin.gameObject.SetActive(false);
            pooledCos.currentBodySprite.BodySprite.enabled = false;
            var jailedPlayer = Object.Instantiate(pooledCos.visor.gameObject, pooledCos.transform);
            jailedPlayer.GetComponent<VisorLayer>().Destroy();
            jailedPlayer.GetComponent<SpriteRenderer>().sprite = TouAssets.JailorPlayerSprite.LoadAsset();
            pooledCos.visor.gameObject.SetActive(false);

            var clonedCos = clonedBubble.Player.cosmetics;
            clonedCos.skin.gameObject.SetActive(false);
            clonedCos.hat.gameObject.SetActive(false);
            clonedCos.skin.gameObject.SetActive(false);
            clonedCos.currentBodySprite.BodySprite.enabled = false;
            var jailedPlayer2 = Object.Instantiate(clonedCos.visor.gameObject, clonedCos.transform);
            jailedPlayer2.GetComponent<VisorLayer>().Destroy();
            jailedPlayer2.GetComponent<SpriteRenderer>().sprite = TouAssets.JailorPlayerSprite.LoadAsset();
            clonedCos.visor.gameObject.SetActive(false);
        }
        if (blackoutText)
        {
            pooledBubble.Background.color = new Color(0.2f, 0.2f, 0.27f, 1f);
            pooledBubble.NameText.color = Color.white;
            pooledBubble.TextArea.color = Color.white;
            clonedBubble.Background.color = new Color(0.2f, 0.2f, 0.27f, 1f);
            clonedBubble.NameText.color = Color.white;
            clonedBubble.TextArea.color = Color.white;
        }

        // Tag *team/private* chat bubbles so the UI can reliably show/hide them.
        // Color-based filtering breaks when system/feedback messages use non-white/non-black backgrounds.
        // Note: Lovers chat intentionally uses `blackoutText: false` and should behave like regular chat.
        if (blackoutText && bubbleType != BubbleType.None)
        {
            clonedBubble.gameObject.name = TeamChatPatches.PrivateBubbleName;
        }
        else
        {
            clonedBubble.name = TeamChatPatches.PublicBubbleName;
        }

        pooledBubble.AlignChildren();
        clonedBubble.AlignChildren();
        if (blackoutText)
        {
            TeamChatPatches.PrivateChatBubbles.Add(pooledBubble);
        }
        else
        {
            TeamChatPatches.PublicChatBubbles.Add(pooledBubble);
        }
        TeamChatPatches.MergedChatBubbles.Add(new TeamChatPatches.MergedBubble(clonedBubble, !blackoutText));

        TeamChatPatches.AlignAllChatBubbles(chat, blackoutText
            ? ChatToCheck.Private
            : ChatToCheck.Public);
        // Only show the for incoming messages
        // Otherwise you get a notification when you message yourself (e.g. Lovers chat).
        // (I think this is the right way to do that...)
        if (onLeft && (!chat.IsOpenOrOpening || !TeamChatPatches.TeamChatActive))
        {
            Coroutines.Start(BouncePrivateChatDot(bubbleType));
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch = 0.1f;
        }

        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
    }

    private static IEnumerator BouncePrivateChatDot(BubbleType bubbleType)
    {
        if (TeamChatPatches.PrivateChatDot == null)
        {
            TeamChatPatches.CreateTeamChatBubble();
        }

        var sprite = TeamChatPatches.PrivateChatDot!.GetComponent<SpriteRenderer>();
        sprite.enabled = true;
        var actualSprite = bubbleType switch
        {
            BubbleType.None => TouChatAssets.NormalBubble.LoadAsset(),
            BubbleType.Impostor => TouChatAssets.ImpBubble.LoadAsset(),
            BubbleType.Vampire => TouChatAssets.VampBubble.LoadAsset(),
            BubbleType.Lover => TouChatAssets.LoveBubble.LoadAsset(),
            BubbleType.Jailor or BubbleType.Jailed => TouChatAssets.JailBubble.LoadAsset(),
            _ => null,
        };
        if (actualSprite != null)
        {
            sprite.sprite = actualSprite;
        }

        yield return Effects.Bounce(sprite.transform, 0.3f, 0.125f);
    }

    public static bool StartsWithVowel(this string word)
    {
        var vowels = new[] { 'a', 'e', 'i', 'o', 'u' };
        return vowels.Any(vowel => word.StartsWith(vowel.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    public static List<PlayerControl> GetCrewmates(List<PlayerControl> impostors)
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Where(player => impostors.All(imp => imp.PlayerId != player.PlayerId)).ToList();
    }

    public static List<PlayerControl> GetImpostors(List<NetworkedPlayerInfo> infected)
    {
        return infected.Select(impData => impData.Object).ToList();
    }

    public static List<(uint Id, ushort RoleType, int Chance)> GetRolesToAssign(ModdedRoleTeams team,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(team).Excluding(x => !CustomRoleUtils.CanSpawnOnCurrentMode(x));

        return GetRolesToAssign(roles, filter);
    }

    public static HashSet<(uint Id, ushort RoleType, int Chance)> GetRolesToAssign(RoleAlignment alignment,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment).Excluding(x => !CustomRoleUtils.CanSpawnOnCurrentMode(x));

        return GetRolesToAssign(roles, filter).ToHashSet();
    }

    private static List<(uint Id, ushort RoleType, int Chance)> GetRolesToAssign(IEnumerable<RoleBehaviour> roles,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var assignmentData = roles.Where(x => !x.IsDead && (filter == null || filter(x))).Select(role =>
            new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var chosenRoles = GetPossibleRoles(assignmentData);

        var rolesToKeep = chosenRoles.ToList();
        rolesToKeep.Shuffle();

        // Log.Message($"GetRolesToKeep Kept - Count: {rolesToKeep.Count}");
        return rolesToKeep;
    }

    public static List<ushort> GetMaxRolesToAssign(ModdedRoleTeams team, int max = 1,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(team).Excluding(x => !CustomRoleUtils.CanSpawnOnCurrentMode(x));

        return GetMaxRolesToAssign(roles, max, filter);
    }

    public static List<ushort> GetMaxRolesToAssign(RoleAlignment alignment, int max,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment).Excluding(x => !CustomRoleUtils.CanSpawnOnCurrentMode(x));

        return GetMaxRolesToAssign(roles, max, filter);
    }

    private static List<ushort> GetMaxRolesToAssign(IEnumerable<RoleBehaviour> roles, int max,
        Func<RoleBehaviour, bool>? filter = null)
    {
        if (max <= 0)
        {
            return [];
        }

        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var assignmentData = roles.Where(x => !x.IsDead && (filter == null || filter(x))).Select(role =>
            new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var chosenRoles = GetPossibleRoles(assignmentData, x => x.Chance == 100);

        // Shuffle to ensure that the same 100% roles do not appear in
        // every game if there are more than the maximum.
        chosenRoles.Shuffle();

        // Truncate the list if there are more 100% roles than the max.
        chosenRoles = chosenRoles.GetRange(0, Math.Min(max, chosenRoles.Count));

        if (chosenRoles.Count < max)
        {
            var potentialRoles = GetPossibleRoles(assignmentData, x => x.Chance < 100);

            // Determine which roles appear in this game.
            var optionalRoles = potentialRoles.Where(x => HashRandom.Next(101) < x.Chance).ToList();
            potentialRoles = potentialRoles.Where(x => !optionalRoles.Contains(x)).ToList();

            optionalRoles.Shuffle();
            chosenRoles.AddRange(optionalRoles.GetRange(0, Math.Min(max - chosenRoles.Count, optionalRoles.Count)));

            // If there are not enough roles after that, randomly add
            // ones which were previously eliminated, up to the max.
            if (chosenRoles.Count < max)
            {
                potentialRoles.Shuffle();
                chosenRoles.AddRange(
                    potentialRoles.GetRange(0, Math.Min(max - chosenRoles.Count, potentialRoles.Count)));
            }
        }

        var rolesToKeep = chosenRoles.Select(x => x.RoleType).ToList();
        rolesToKeep.Shuffle();

        // Log.Message($"GetMaxRolesToAssign Kept - Count: {rolesToKeep.Count}");
        return rolesToKeep;
    }

    private static List<(uint Id, ushort RoleType, int Chance)> GetPossibleRoles(
        List<RoleManager.RoleAssignmentData> assignmentData,
        Func<RoleManager.RoleAssignmentData, bool>? predicate = null)
    {
        var roles = new List<(uint, ushort, int)>();

        assignmentData.Where(x => predicate == null || predicate(x)).ToList().ForEach(x =>
        {
            for (uint i = 0; i < x.Count; i++)
            {
                roles.Add((i, (ushort)x.Role.Role, x.Chance));
            }
        });

        return roles;
    }

    public static RoleManager.RoleAssignmentData GetAssignData(RoleTypes roleType)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var role = GetRegisteredRole(roleType);
        var assignmentData = new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role!.Role),
            roleOptions.GetChancePerGame(role.Role));

        return assignmentData;
    }

    public static PlayerControl PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
            {
                return player;
            }
        }

        return null!;
    }

    public static IEnumerator PerformTimedAction(float duration, Action<float> action)
    {
        for (var t = 0f; t < duration; t += Time.deltaTime)
        {
            action(t / duration);
            yield return new WaitForEndOfFrame();
        }

        action(1f);
    }

    public static SpriteRenderer FlashRenderer;

    public static IEnumerator CoFlash(Color color, float waitfor = 1f, float alpha = 0.3f)
    {
        color.a = alpha;
        if (HudManager.InstanceExists && HudManager.Instance.FullScreen && !FlashRenderer)
        {
            FlashRenderer = Object.Instantiate(HudManager.Instance.FullScreen,
                HudManager.Instance.FullScreen.transform.parent);
            FlashRenderer.transform.localScale *= 10f;
        }

        FlashRenderer.enabled = true;
        FlashRenderer.gameObject.SetActive(true);
        FlashRenderer.color = color;

        yield return new WaitForSeconds(waitfor);

        if (HudManager.InstanceExists && HudManager.Instance.FullScreen && !FlashRenderer)
        {
            FlashRenderer = Object.Instantiate(HudManager.Instance.FullScreen,
                HudManager.Instance.FullScreen.transform.parent);
            FlashRenderer.transform.localScale *= 10f;
        }

        if (!FlashRenderer.color.Equals(color))
        {
            yield break;
        }

        FlashRenderer.color = new Color(1f, 0f, 0f, 0.37254903f);
        FlashRenderer.enabled = false;
    }

    public static IEnumerator FadeOut(SpriteRenderer? rend, float delay = 0.01f, float decrease = 0.01f)
    {
        if (rend == null)
        {
            yield break;
        }

        var alphaVal = rend.color.a;
        var tmp = rend.color;

        while (alphaVal > 0)
        {
            alphaVal -= decrease;
            tmp.a = alphaVal;
            rend.color = tmp;

            yield return new WaitForSeconds(delay);
        }
    }

    public static IEnumerator FadeIn(SpriteRenderer? rend, float delay = 0.01f, float increase = 0.01f)
    {
        if (rend == null)
        {
            yield break;
        }

        var tmp = rend.color;
        tmp.a = 0;
        rend.color = tmp;

        while (rend.color.a < 1)
        {
            tmp.a = Mathf.Min(rend.color.a + increase, 1f); // Ensure it doesn't go above 1
            rend.color = tmp;

            yield return new WaitForSeconds(delay);
        }
    }
    public static IEnumerator FadeInOutPair(SpriteRenderer? rendIn, SpriteRenderer? rendOut, float delay = 0.01f, float increase = 0.01f)
    {
        if (rendIn == null || rendOut == null)
        {
            yield break;
        }

        var tmpIn = rendIn.color;
        tmpIn.a = 0;
        rendIn.color = tmpIn;
        var tmpOut = rendOut.color;
        rendOut.color = tmpOut;

        while (tmpOut.a > 0)
        {
            tmpIn.a = Mathf.Min(rendIn.color.a + increase, 1f); // Ensure it doesn't go above 1
            rendIn.color = tmpIn;

            tmpOut.a = Mathf.Max(rendOut.color.a - increase, 0f); // Ensure it doesn't go below 0
            rendOut.color = tmpOut;

            yield return new WaitForSeconds(delay);
        }
    }

    public static IEnumerator FadeInOutMultiRenderers(SpriteRenderer[] rendsIn, SpriteRenderer[] rendsOut, float delay = 0.01f,
        float increase = 0.01f)
    {
        var tmpIn = rendsIn[0].color;
        tmpIn.a = 0;
        var tmpOut = rendsOut[0].color;

        while (tmpOut.a > 0 || tmpIn.a < 1)
        {
            tmpIn.a = Mathf.Min(tmpIn.a + increase, 1f); // Ensure it doesn't go above 1
            foreach (var rend in rendsIn)
            {
                rend.color = tmpIn;
            }

            tmpOut.a = Mathf.Max(tmpOut.a - increase, 0f); // Ensure it doesn't go below 0
            foreach (var rend in rendsOut)
            {
                rend.color = tmpOut;
            }

            yield return new WaitForSeconds(delay);
        }
    }

    public static IEnumerator FadeOutMultiRenderers(SpriteRenderer[] rends, float delay = 0.01f,
        float increase = 0.01f)
    {
        var tmp = rends[0].color;

        while (tmp.a > 0)
        {
            tmp.a = Mathf.Max(tmp.a - increase, 0f); // Ensure it doesn't go above 1
            foreach (var rend in rends)
            {
                rend.color = tmp;
            }

            yield return new WaitForSeconds(delay);
        }

    }

    public static IEnumerator FadeInDualRenderers(SpriteRenderer? rend, SpriteRenderer? rend2, float delay = 0.01f,
        float increase = 0.01f, float rend2Mult = 1f)
    {
        if (rend == null || rend2 == null)
        {
            yield break;
        }

        var tmp = rend.color;
        tmp.a = 0;
        rend.color = tmp;
        var tmp2 = rend2.color;
        tmp2.a = 0;
        rend2.color = tmp2;

        while (rend.color.a < 1)
        {
            tmp.a = Mathf.Min(rend.color.a + increase, 1f); // Ensure it doesn't go above 1
            rend.color = tmp;
            tmp2.a = Mathf.Min(rend2.color.a + increase * rend2Mult, 1f); // Ensure it doesn't go above 1
            rend2.color = tmp2;

            yield return new WaitForSeconds(delay);
        }

    }

    public static GameObject CreateSpherePrimitive(Vector3 location, float radius)
    {
        var spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        spherePrimitive.name = "Sphere Primitive";
        spherePrimitive.transform.localScale = new Vector3(
            radius * ShipStatus.Instance.MaxLightRadius * 2f,
            radius * ShipStatus.Instance.MaxLightRadius * 2f,
            radius * ShipStatus.Instance.MaxLightRadius * 2f);

        Object.Destroy(spherePrimitive.GetComponent<SphereCollider>());

        spherePrimitive.GetComponent<MeshRenderer>().material = AuAvengersAnims.BombMaterial.LoadAsset();
        spherePrimitive.transform.position = location;

        return spherePrimitive;
    }

    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input; // Return empty or null string if input is empty or null
        }

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return
            textInfo.ToTitleCase(
                input.ToLower(CultureInfo.CurrentCulture)); // Convert to lowercase first and then title case
    }


    public static ArrowBehaviour CreateArrow(Transform parent, Color color)
    {
        var gameObject = new GameObject("Arrow")
        {
            layer = 5,
            transform =
            {
                parent = parent
            }
        };

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.ArrowSprite.LoadAsset();
        renderer.color = color;

        var arrow = gameObject.AddComponent<ArrowBehaviour>();
        arrow.image = renderer;
        arrow.image.color = color;

        return arrow;
    }
    public static PingBehaviour CreatePing(Transform parent, Color color)
    {
        var gameObject = new GameObject("Ping")
        {
            layer = 5,
            transform =
            {
                parent = parent
            }
        };

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.ArrowSprite.LoadAsset();
        renderer.color = color;
        gameObject.AddComponent<Animator>();
        var spriteAnim = gameObject.AddComponent<SpriteAnim>();
        spriteAnim.Play(TouAssets.HeartbeatAnim.LoadAsset());

        var arrow = gameObject.AddComponent<PingBehaviour>();
        arrow.image = renderer;
        arrow.image.color = color;

        return arrow;
    }

    public static IEnumerator BetterBloop(Transform target, float delay = 0, float finalSize = 1f,
        float duration = 0.5f, float intensity = 1f)
    {
        for (var t = 0f; t < delay; t += Time.deltaTime)
        {
            yield return null;
        }

        var localScale = default(Vector3);
        for (var t = 0f; t < duration; t += Time.deltaTime)
        {
            var z = 1f + (Effects.ElasticOut(t, duration) - 1f) * intensity;
            z *= finalSize;
            localScale.x = localScale.y = localScale.z = z;
            target.localScale = localScale;
            yield return null;
        }

        localScale.z = localScale.y = localScale.x = finalSize;
        target.localScale = localScale;
    }

    public static void AdjustGhostTasks(PlayerControl player)
    {
        foreach (var task in player.myTasks)
        {
            if (task.TryCast<NormalPlayerTask>() != null)
            {
                var normalPlayerTask = task.Cast<NormalPlayerTask>();

                var updateArrow = normalPlayerTask.taskStep > 0;

                normalPlayerTask.taskStep = 0;
                normalPlayerTask.Initialize();
                if (normalPlayerTask.TaskType is TaskTypes.PickUpTowels)
                {
                    foreach (var console in Object.FindObjectsOfType<TowelTaskConsole>())
                    {
                        console.Image.color = Color.white;
                    }
                }

                normalPlayerTask.taskStep = 0;
                if (normalPlayerTask.TaskType == TaskTypes.UploadData)
                {
                    normalPlayerTask.taskStep = 1;
                }

                if (normalPlayerTask.TaskType is TaskTypes.EmptyGarbage or TaskTypes.EmptyChute
                    && (GameOptionsManager.Instance.currentGameOptions.MapId == 0 ||
                        GameOptionsManager.Instance.currentGameOptions.MapId == 3 ||
                        GameOptionsManager.Instance.currentGameOptions.MapId == 4))
                {
                    normalPlayerTask.taskStep = 1;
                }

                if (updateArrow)
                {
                    normalPlayerTask.UpdateArrowAndLocation();
                }

                var taskInfo = player.Data.FindTaskById(task.Id);
                taskInfo.Complete = false;
            }
        }
    }

    public static void UpdateLocalPlayerCamera(MonoBehaviour target, Transform lightParent)
    {
        HudManager.Instance.PlayerCam.SetTarget(target);
        PlayerControl.LocalPlayer.lightSource.transform.parent = lightParent;
        PlayerControl.LocalPlayer.lightSource.Initialize(PlayerControl.LocalPlayer.Collider.offset / 2);
    }

    public static void SnapPlayerCamera(MonoBehaviour target)
    {
        var cam = HudManager.Instance.PlayerCam;
        cam.SetTarget(target);
        cam.centerPosition = cam.Target.transform.position;
    }

    public static void AddFromBucket(this List<ushort> toApply, List<RoleListOption> buckets,
        HashSet<(uint Id, ushort RoleType, int Chance)> roles, HashSet<(uint Id, ushort RoleType, int Chance)> applied,
        RoleListOption roleType, RoleListOption replaceType = (RoleListOption)(-1), RoleListOption biggerType = (RoleListOption)(-1))
    {
        roles.ExceptWith(applied);

        while (buckets.Contains(roleType))
        {
            if (roles.Count == 0)
            {
                var count = buckets.RemoveAll(x => x == roleType);
                if ((int)replaceType != -1)
                {
                    buckets.AddRange(Enumerable.Repeat(replaceType, count));
                    if ((int)biggerType != -1) buckets.AddRange(Enumerable.Repeat(biggerType, count));
                }
                break;
            }

            var addedRole = SelectRole(roles);
            roles.Remove(addedRole);
            toApply.Add(addedRole.RoleType);
            applied.Add(addedRole);

            buckets.Remove(roleType);
        }
    }

    public static (uint Id, ushort RoleType, int Chance) SelectRole(HashSet<(uint Id, ushort RoleType, int Chance)> roles)
    {
        var chosenRoles = roles.Where(x => x.Chance == 100).ToList();
        if (chosenRoles.Count > 0)
        {
            chosenRoles.Shuffle();
            return chosenRoles.TakeFirst();
        }

        chosenRoles = roles.Where(x => x.Chance < 100).ToList();
        var total = chosenRoles.Sum(x => x.Chance);
        var random = Random.RandomRangeInt(1, total + 1);

        var cumulative = 0;
        (uint Id, ushort RoleType, int Chance) selectedRole = default;

        foreach (var role in chosenRoles)
        {
            cumulative += role.Chance;
            if (random <= cumulative)
            {
                selectedRole = role;
                break;
            }
        }

        return selectedRole;
    }

    public static string WithoutRichText(this string text)
    {
        // Regular expression to match any tag enclosed in < >
        var richTagRegex = new Regex(@"<[^>]*>");

        // Replace matched tags with an empty string
        return richTagRegex.Replace(text, string.Empty);
    }

    // Method to parse a JSON array string into an array of objects
    public static T[] JsonToArray<T>(string json)
    {
        // Wrap the JSON array in an object
        var newJson = "{ \"array\": " + json + "}";
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    public static string TaskInfo(this PlayerControl player)
    {
        var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
        var totalTasks = player.myTasks.ToArray()
            .Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
        var colorbase = Color.yellow;
        var color = Color.yellow;
        if (completed <= 0)
        {
            color = TownOfUsColors.ImpSoft;
        }
        else if (completed >= totalTasks)
        {
            color = TownOfUsColors.Doomsayer;
        }
        else if (completed > totalTasks / 2)
        {
            var fraction = ((completed * 0.4f) / totalTasks);
            Color color2 = TownOfUsColors.Doomsayer;
            color = new
            ((color2.r * fraction + colorbase.r * (1 - fraction)),
                (color2.g * fraction + colorbase.g * (1 - fraction)),
                (color2.b * fraction + colorbase.b * (1 - fraction)));
        }
        else if (completed < totalTasks / 2)
        {
            var fraction = ((completed * 0.9f) / totalTasks);
            Color color2 = TownOfUsColors.ImpSoft;
            color = new
            ((colorbase.r * fraction + color2.r * (1 - fraction)),
                (colorbase.g * fraction + color2.g * (1 - fraction)),
                (colorbase.b * fraction + color2.b * (1 - fraction)));
        }

        return $"{color.ToTextColor()}({completed}/{totalTasks})</color>";
    }

    /// <summary>
    /// Gets a <see cref="StonedPlayer"/> by its parent ID.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <returns>A <see cref="StonedPlayer"/> or <see langword="null"/> if its not found.</returns>
    public static StonedPlayer? GetStonedPlayerById(byte id)
    {
        var result = StonedPlayer.FakePlayers.FirstOrDefault(body => body.PlayerId == id);
        return result;
    }

    /// <summary>
    /// Gets a <see cref="StonedPlayer"/> by its parent ID if it is not permanent.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <returns>A <see cref="StonedPlayer"/> or <see langword="null"/> if its not found.</returns>
    public static StonedPlayer? GetFreshStonedPlayerById(byte id)
    {
        var result = GetStonedPlayerById(id);
        if (result != null && result.ProgressStage < StoneStage.Permanent)
        {
            return result;
        }

        return null;
    }

    /// <summary>
    ///     Gets a FakePlayer by comparing PlayerControl.
    /// </summary>
    /// <param name="player">The player themselves.</param>
    /// <returns>A fake player or null if its not found.</returns>
    public static FakePlayer? GetFakePlayer(PlayerControl player)
    {
        return FakePlayer.FakePlayers.FirstOrDefault(x => x.body?.name == $"Fake {player.gameObject.name}");
    }

    /// <summary>
    ///     Gets a FakePlayer by comparing a player id.
    /// </summary>
    /// <param name="playerId">The player id.</param>
    /// <returns>A fake player or null if its not found.</returns>
    public static FakePlayer? GetFakePlayer(int playerId)
    {
        return FakePlayer.FakePlayers.FirstOrDefault(x => x.PlayerId == playerId && x.body);
    }

    /// <summary>
    ///     Gets a FakePlayer by comparing a string.
    /// </summary>
    /// <param name="playerName">The player's name.</param>
    /// <returns>A fake player or null if its not found.</returns>
    public static FakePlayer? GetFakePlayer(string playerName)
    {
        return FakePlayer.FakePlayers.FirstOrDefault(x => x.body?.name == $"Fake {playerName}");
    }

    public static void SetForcedBodyType(this PlayerPhysics player, PlayerBodyTypes bodyType)
    {
        player.bodyType = bodyType;
        player.myPlayer.cosmetics.EnsureInitialized(bodyType);
        player.Animations.SetBodyType(bodyType, player.myPlayer.cosmetics.FlippedCosmeticOffset,
            player.myPlayer.cosmetics.NormalCosmeticOffset);
        player.Animations.PlayIdleAnimation();
    }

    public static bool IsMap(byte mapid)
    {
        return (GameOptionsManager.Instance != null &&
                GameOptionsManager.Instance.currentGameOptions.MapId == mapid)
               || (TutorialManager.InstanceExists && AmongUsClient.Instance.TutorialMapId == mapid);
    }

    public static bool IsConcealed(this PlayerControl player)
    {
        if (player.HasModifier<ConcealedModifier>() || !player.Visible ||
            (player.TryGetModifier<DisabledModifier>(out var mod) && !mod.IsConsideredAlive))
        {
            return true;
        }

        if (player.inVent)
        {
            return true;
        }

        if (VanillaSystemCheckPatches.ShroomSabotageSystem && VanillaSystemCheckPatches.ShroomSabotageSystem.IsActive)
        {
            return true;
        }

        if (TownOfUsMapOptions.IsCamoCommsOn())
        {
            var isActive = false;
            if (VanillaSystemCheckPatches.HudCommsSystem != null)
            {
                isActive = VanillaSystemCheckPatches.HudCommsSystem.IsActive;
            }
            else if (VanillaSystemCheckPatches.HqCommsSystem != null)
            {
                isActive = VanillaSystemCheckPatches.HqCommsSystem.IsActive;
            }

            return isActive;
        }

        return false;
    }

    public static bool CanUseVent(this PlayerControl player, Vent vent)
    {
        var couldUse = (!player.MustCleanVent(vent.Id) || (player.inVent && Vent.currentVent == vent)) &&
                       !player.Data.IsDead && (player.CanMove || player.inVent);
        if (VanillaSystemCheckPatches.VentSystem != null && VanillaSystemCheckPatches.VentSystem.IsVentCurrentlyBeingCleaned(vent.Id))
        {
            couldUse = false;
        }

        if (couldUse)
        {
            var center = player.Collider.bounds.center;
            var position = vent.transform.position;
            var num = Vector2.Distance(center, position);
            couldUse &= num <= vent.UsableDistance &&
                        !PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask,
                            false);
        }

        return couldUse;
    }

    public static PlayerControl? GetImpostorTarget(float distance)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var saboOpt = OptionGroupSingleton<AdvancedSabotageOptions>.Instance;
        var closePlayer = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, distance);

        var includePostors = genOpt.FFAImpostorMode ||
                             (PlayerControl.LocalPlayer.IsLover() &&
                              OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates) ||
                             (saboOpt.KillDuringCamoComms &&
                              closePlayer?.GetAppearanceType() == TownOfUsAppearances.Camouflage);
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, distance);
    }

    public static bool DiedOtherRound(this PlayerControl player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.HasDied())
        {
            var state = GameHistory.PlayerStats[player.PlayerId];
            return !state.DiedThisRound;
        }

        return false;
    }

    [Serializable]
    public class Wrapper<T>
    {
        public T[] array;
    }

    public static uint GetModifierTypeId(BaseModifier mod)
    {
        if (mod is IWikiDiscoverable wikiMod)
        {
            return wikiMod.FakeTypeId;
        }

        return ModifierManager.GetModifierTypeId(mod.GetType()) ??
               throw new InvalidOperationException("Modifier is not registered.");
    }

    public static string GetRoomName(Vector3 position)
    {
        PlainShipRoom? plainShipRoom = null;

        var allRooms2 = ShipStatus.Instance.FastRooms;
        foreach (var plainShipRoom2 in allRooms2.Values)
        {
            if (plainShipRoom2.roomArea && plainShipRoom2.roomArea.OverlapPoint(position))
            {
                plainShipRoom = plainShipRoom2;
            }
        }

        return plainShipRoom != null
            ? TranslationController.Instance.GetString(plainShipRoom.RoomId)
            : "Outside/Hallway";
    }

    public static void AddMiraTranslator(this GameObject obj, string stringName,
        string? defaultStr = null)
    {
        if (obj.TryGetComponent<TextTranslatorTMP>(out var amogTmp))
        {
            // we don't like innerscuff StringNames, im sorry
            Object.Destroy(amogTmp);
        }

        var translator = obj.AddComponent<TmpMiraTranslator>();
        translator.stringName = stringName;
        translator.defaultStr = defaultStr ?? string.Empty;
    }

    public static void AddMiraTranslator(this Transform obj, string stringName,
        string? defaultStr = null)
    {
        if (obj.TryGetComponent<TextTranslatorTMP>(out var amogTmp))
        {
            // we don't like innerscuff StringNames, im sorry
            Object.Destroy(amogTmp);
        }

        var translator = obj.gameObject.AddComponent<TmpMiraTranslator>();
        translator.stringName = stringName;
        translator.defaultStr = defaultStr ?? string.Empty;
    }

    public static string GetParsedRoleBucket(string bucket)
    {
        var text = MiraLocaleManager.Get(bucket);
        var crewmateKeyword = MiraLocaleManager.Get("CrewmateKeyword");
        var crewKeyword = MiraLocaleManager.Get("CrewKeyword");
        var impostorKeyword = MiraLocaleManager.Get("ImpostorKeyword");
        var impKeyword = MiraLocaleManager.Get("ImpKeyword");
        var neutralKeyword = MiraLocaleManager.Get("NeutralKeyword");
        var neutKeyword = MiraLocaleManager.Get("NeutKeyword");

        if (text.Contains(impostorKeyword))
        {
            text = text.Replace(impostorKeyword, $"<color=#FF0000FF>{impostorKeyword}</color>");
        }
        else if (text.Contains(crewmateKeyword))
        {
            text = text.Replace(crewmateKeyword, $"<color=#66FFFFFF>{crewmateKeyword}</color>");
        }
        else if (text.Contains(neutralKeyword))
        {
            text = text.Replace(neutralKeyword, $"<color=#999999FF>{neutralKeyword}</color>");
        }
        else if (text.Contains(impKeyword))
        {
            text = text.Replace(impKeyword, $"<color=#FF0000FF>{impKeyword}</color>");
        }
        else if (text.Contains(crewKeyword))
        {
            text = text.Replace(crewKeyword, $"<color=#66FFFFFF>{crewKeyword}</color>");
        }
        else if (text.Contains(neutKeyword))
        {
            text = text.Replace(neutKeyword, $"<color=#999999FF>{neutKeyword}</color>");
        }
        else if (text.Contains("Impostor"))
        {
            text = text.Replace("Impostor", "<color=#FF0000FF>Impostor</color>");
        }
        else if (text.Contains("Crewmate"))
        {
            text = text.Replace("Crewmate", "<color=#66FFFFFF>Crewmate</color>");
        }
        else if (text.Contains("Neutral"))
        {
            text = text.Replace("Neutral", "<color=#999999FF>Neutral</color>");
        }
        else if (text.Contains("Imp"))
        {
            text = text.Replace("Imp", "<color=#FF0000FF>Imp</color>");
        }
        else if (text.Contains("Crew"))
        {
            text = text.Replace("Crew", "<color=#66FFFFFF>Crew</color>");
        }
        else if (text.Contains("Neut"))
        {
            text = text.Replace("Neut", "<color=#999999FF>Neut</color>");
        }

        return text;
    }

    private static readonly List<SupportedLangs> _languagesToBold =
    [
        SupportedLangs.Russian,
        SupportedLangs.Japanese,
        SupportedLangs.SChinese,
        SupportedLangs.TChinese,
        SupportedLangs.Korean
    ];

    public static void AdjustNotification(this LobbyNotificationMessage notification)
    {
        if (!_languagesToBold.Contains(DataManager.Settings.Language.CurrentLanguage))
        {
            notification.Text.fontStyle = FontStyles.Bold;
            notification.Text.SetOutlineThickness(0.35f);
        }
    }

    public static IEnumerable<T> Excluding<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(x => !predicate(x)); // Added for easier inversion and reading

    public static bool CanSeeAdvancedLogs
    {
        get
        {
            if (!TownOfUsPlugin.IsDevBuild)
            {
                return false;
            }

            var logLevel = (LoggingLevel)OptionGroupSingleton<HostSpecificOptions>.Instance.BetaLoggingLevel.Value;
            if (PlayerControl.LocalPlayer.IsHost() && logLevel is LoggingLevel.LogForHost)
            {
                return true;
            }
            else if (logLevel is LoggingLevel.LogForEveryone)
            {
                return true;
            }

            return false;
        }
    }

    public static bool CanSeePostGameLogs
    {
        get
        {
            if (!TownOfUsPlugin.IsDevBuild)
            {
                return false;
            }

            var logLevel = (LoggingLevel)OptionGroupSingleton<HostSpecificOptions>.Instance.BetaLoggingLevel.Value;

            return logLevel is LoggingLevel.LogForEveryonePostGame;
        }
    }

    public static ExpandedMapNames GetCurrentMap
    {
        get
        {
            var mapId = (ExpandedMapNames)GameOptionsManager.Instance.currentGameOptions.MapId;
            if (TutorialManager.InstanceExists)
            {
                mapId = (ExpandedMapNames)AmongUsClient.Instance.TutorialMapId;
            }

            return mapId;
        }
    }

    public static bool IsBasicGhost(RoleBehaviour role)
    {
        return IsBasicGhost(role.Role);
    }

    public static bool IsBasicGhost(RoleTypes role)
    {
        return role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost ||
               role == (RoleTypes)RoleId.Get<NeutralGhostRole>() ||
               role == (RoleTypes)RoleId.Get<PolusGhostCrewRole>() ||
               role == (RoleTypes)RoleId.Get<PolusGhostImpRole>() ||
               role == (RoleTypes)RoleId.Get<PolusGhostNeutRole>();
    }

    public static TouGamemode CurrentGamemode()
    {
        if (CustomGameModeManager.IsHideNSeek() || GameOptionsManager.Instance.CurrentGameOptions.GameMode is AmongUs.GameOptions.GameModes.HideNSeek or AmongUs.GameOptions.GameModes.SeekFools)
            return TouGamemode.HideAndSeek;
        if (CustomGameModeManager.IsActiveGameMode<CultistMode>())
        {
            return TouGamemode.Cultist;
        }
        if (CustomGameModeManager.IsActiveGameMode<KillFrenzyMode>())
        {
            return TouGamemode.KillFrenzy;
        }
        if (CustomGameModeManager.IsActiveGameMode<TownOfPolusMode>())
        {
            return TouGamemode.TownOfPolus;
        }
        if (CustomGameModeManager.IsClassic())
        {
            return TouGamemode.Normal;
        }
        return TouGamemode.Other;
    }

    public static void LogInfo(TownOfUsEventHandlers.LogLevel logLevel, string text)
    {
        if (!CanSeeAdvancedLogs)
        {
            if (CanSeePostGameLogs)
            {
                TownOfUsEventHandlers.LogBuffer.Add(
                    new(logLevel, $"At {DateTime.UtcNow:T} -> " + text));
            }

            return;
        }

        switch (logLevel)
        {
            case TownOfUsEventHandlers.LogLevel.Error:
                Error(text);
                break;
            case TownOfUsEventHandlers.LogLevel.Warning:
                Warning(text);
                break;
            case TownOfUsEventHandlers.LogLevel.Debug:
                Debug(text);
                break;
            case TownOfUsEventHandlers.LogLevel.Info:
                Info(text);
                break;
            case TownOfUsEventHandlers.LogLevel.Message:
                Message(text);
                break;
        }

        TownOfUsEventHandlers.LogBuffer.Add(new(logLevel, $"At {DateTime.UtcNow:T} -> " + text));
    }


    /// <summary>
    ///     A Coroutine to be used for adjusting custom buttons to be ahead or behind the vent button.
    /// </summary>
    /// <param name="button">The custom button to move.</param>
    /// <param name="beforeVent">Determines whether the button appears before the vent button, as it is normally the very last button in the list.</param>
    public static IEnumerator CoMoveButtonIndex(CustomActionButton button, bool beforeVent = true)
    {
        yield return new WaitForEndOfFrame();
        if (button.Button == null)
        {
            yield break;
        }

        var bottomLeft = MiraAPI.Patches.HudManagerPatches.BottomLeft!;
        var bottomRight = MiraAPI.Patches.HudManagerPatches.BottomRight;
        var location = button.Location switch
        {
            ButtonLocation.BottomLeft => bottomLeft.transform,
            ButtonLocation.BottomRight => bottomRight,
            _ => null,
        };
        button.Button.transform.SetParent(null);
        button.Button.transform.SetParent(location);

        var index = HudManager.Instance.ImpostorVentButton.transform.GetSiblingIndex();
        button.Button.transform.SetSiblingIndex(index + (beforeVent ? -1 : 1));
    }

    //Submerged utils
    public static object? TryOtherCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type)
            .Invoke(self, []);
    }

    public static IList CreateList(Type myType)
    {
        Type genericListType = typeof(List<>).MakeGenericType(myType);
        return (IList)Activator.CreateInstance(genericListType)!;
    }

    public static void RemovePet(PlayerControl pc, PetHidden hidden = PetHidden.Remove)
    {
        if (pc == null || !pc.Data.IsDead || hidden is PetHidden.Never)
        {
            return;
        }

        if (!pc.cosmetics.currentPet)
        {
            return;
        }

        if (hidden is PetHidden.DuringRound)
        {
            pc.cosmetics.petHiddenByViper = true;
        }
        pc.cosmetics.TogglePet(false);
    }

    public static void LungeToPos(PlayerControl player, Vector2 pos)
    {
        var anim = player.KillAnimations.Random();

        Coroutines.Start(CoPerformAttack(player, anim!, pos));
    }

    public static IEnumerator CoPerformAttack(PlayerControl attacker, KillAnimation anim, Vector2 pos)
    {
        KillAnimation.SetMovement(attacker, false);

        var cam = Camera.main?.GetComponent<FollowerCamera>();

        if (attacker.AmOwner)
        {
            cam?.Locked = true;

            attacker.isKilling = true;
        }

        yield return attacker.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
        attacker.MyPhysics.Animations.PlayIdleAnimation();
        attacker.NetTransform.SnapTo(pos);

        KillAnimation.SetMovement(attacker, true);

        cam?.Locked = false;

        attacker.isKilling = false;
    }

    public static string GetHyperlinkText(RoleBehaviour role)
    {
        var name = role.GetRoleName();
        return $"#{name.Replace(" ", "-").RemoveAll(WikiHyperLinkPatches.RemovedCharacters)}";
    }

    public static string? GetHyperlinkText(BaseModifier modifier)
    {
        if (modifier is not IWikiDiscoverable || !SoftWikiEntries.ModifierEntries.ContainsKey(modifier))
        {
            return null;
        }
        var name = modifier.ModifierName;
        return $"&{name.Replace(" ", "-").RemoveAll(WikiHyperLinkPatches.RemovedCharacters)}";
    }

    public static string? GetHyperlinkText(GameModifier modifier)
    {
        if (modifier is not IWikiDiscoverable || !SoftWikiEntries.ModifierEntries.ContainsKey(modifier))
        {
            return null;
        }
        var name = modifier.ModifierName;
        return $"&{name.Replace(" ", "-").RemoveAll(WikiHyperLinkPatches.RemovedCharacters)}";
    }

    public static bool CanUseUtility(GameUtility utility, bool isPortable = false)
    {
        var opts = OptionGroupSingleton<AdvancedUtilityOptions>.Instance;
        if (isPortable && !opts.TasksOnPortables.Value)
        {
            return true;
        }
        var tasksNeeded = utility switch
        {
            GameUtility.Admin => (int)opts.TasksToUseAdmin.Value,
            GameUtility.Cams => (int)opts.TasksToUseCams.Value,
            GameUtility.Doorlog => (int)opts.TasksToUseDoorlog.Value,
            GameUtility.Vitals => (int)opts.TasksToUseVitals.Value,
            _ => 0,
        };
        var sprite = utility switch
        {
            GameUtility.Admin => TouRoleIcons.Spy,
            GameUtility.Cams => TouModifierIcons.Operative,
            GameUtility.Doorlog => TouRoleIcons.Investigator,
            GameUtility.Vitals => TouRoleIcons.Scientist,
            _ => TouModifierIcons.Operative,
        };
        if (tasksNeeded <= 0)
        {
            return true;
        }

        var playerCompleted = PlayerControl.LocalPlayer.Data.Tasks.Count - PlayerControl.LocalPlayer.GetTasksLeft();
        var cantUseCamera = tasksNeeded > playerCompleted;
        var tasksLeftToUnlock = tasksNeeded - playerCompleted;

        if (!cantUseCamera) return true;
        var notif1 = Helpers.CreateAndShowNotification(
            MiraLocaleManager.Get(tasksLeftToUnlock > 1 ? "TouUnavailableUtilityNotif" : "TouUnavailableUtilityNotifSingle").Replace("<amount>",
                $"<size=120%><b>\n{tasksLeftToUnlock.ToString(TownOfUsPlugin.Culture)}</b></size>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: sprite.LoadAsset());

        notif1.AdjustNotification();
        return false;
    }

    public static void RunAnticheatWarning(PlayerControl source)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(TownOfUsPlugin.Culture, $"{MiraLocaleManager.Get("AnticheatIllegalRpcMessage").Replace("<player>", source.Data.PlayerName)}");
        AddFakeChat(source.Data, $"<color=#D53F42>{MiraLocaleManager.Get("AnticheatChatTitle")}</color>", stringBuilder.ToString(), true, altColors:true);
    }

    public static string GetRegionName(IRegionInfo? region = null, bool shorten = true)
    {
        region ??= ServerManager.Instance.CurrentRegion;

        string name = region.Name;

        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            name = "Local Game";
            return name;
        }

        if (AmongUsClient.Instance.GameId == LobbyJoin.GameId && LobbyJoin.TempRegion != null)
        {
            region = LobbyJoin.TempRegion;
            name = LobbyJoin.TempRegion.Name;
        }

        if (shorten)
        {
            if (region.PingServer.EndsWith("among.us", StringComparison.Ordinal))
            {
                // Official Server
                if (name == "North America") name = "NA";
                else if (name == "Europe") name = "EU";
                else if (name == "Asia") name = "AS";

                return name;
            }

            var Ip = region.Servers.FirstOrDefault()?.Ip ?? string.Empty;

            if (Ip.Contains("aumods.us", StringComparison.Ordinal)
                || Ip.Contains("duikbo.at", StringComparison.Ordinal))
            {
                // Official Modded Server
                if (Ip.Contains("au-eu")) name = "MEU";
                else if (Ip.Contains("au-as")) name = "MAS";
                else if (Ip.Contains("www.")) name = "MNA";

                return name;
            }

            if (name.Contains("nikocat233", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Replace("nikocat233", "Niko233", StringComparison.OrdinalIgnoreCase);
            }
        }

        return name;
    }

    public static void DelayExile(this PlayerControl localPlayer)
    {
        Coroutines.Start(CoWaitExile(localPlayer));
    }

    private static IEnumerator CoWaitExile(PlayerControl player)
    {
        yield return new WaitForSeconds(1f);

        player.RpcPlayerExile();
    }

    public static string GetRoleTmpIcon(RoleTypes role)
    {
        return GetRoleTmpIcon(RoleManager.Instance.GetRole(role));
    }

    public static string GetRoleTmpIcon(ICustomRole role)
    {
        return role.Configuration.IconTmp ? $"<sprite name=\"{role.Configuration.IconTmp.name}\">" : $"<sprite name=\"AmongUs.Role.{role.Team}\">";
    }

    public static string GetRoleTmpIcon(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.Configuration.IconTmp ? $"<sprite name=\"{custom.Configuration.IconTmp.name}\">" : $"<sprite name=\"AmongUs.Role.{custom.Team}\">";
        }
        return $"<sprite name=\"AmongUs.Role.{role.Role}\">";
    }

    public static string GetToggledRoleTmpIcon(RoleBehaviour role, bool enabled)
    {
        if (!enabled)
        {
            return string.Empty;
        }
        return GetRoleTmpIcon(role);
    }
}

public enum GameUtility
{
    Admin,
    Cams,
    Doorlog,
    Vitals
}

public enum TouGamemode
{
    Normal,
    HideAndSeek,
    Cultist,
    KillFrenzy,
    TownOfPolus,
    Other,
    // Legacy
}
public enum ExpandedMapNames
{
    Skeld,
    MiraHq,
    Polus,
    Dleks,
    Airship,
    Fungle,
    Submerged,
    LevelImpostor
}

public enum BubbleType
{
    None,
    Other,
    Impostor,
    Vampire,
    Jailor,
    Jailed,
    Lover
}
