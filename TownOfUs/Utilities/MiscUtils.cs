using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches.Misc;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Utilities;

public static class MiscUtils
{
    public static int KillersAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor() ||
        x.Is(RoleAlignment.NeutralKilling) ||
        (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame &&
         inquis is { CanVanquish: true, TargetsDead: false } && Helpers.GetAlivePlayers().Count <= 3) ||
        (x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
         !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
         OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue));

    public static int RealKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis &&
                                                                 OptionGroupSingleton<InquisitorOptions>.Instance
                                                                     .StallGame && inquis is
                                                                     { CanVanquish: true, TargetsDead: false }
                                                                 && Helpers.GetAlivePlayers().Count <= 3));

    public static int NKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Is(RoleAlignment.NeutralKilling) || (x.Data.Role is InquisitorRole inquis &&
                                               OptionGroupSingleton<InquisitorOptions>.Instance.StallGame &&
                                               inquis is { CanVanquish: true, TargetsDead: false }
                                               && Helpers.GetAlivePlayers().Count <= 3));

    public static int NonImpKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Is(RoleAlignment.NeutralKilling) ||
        (x.Data.Role is InquisitorRole inquis && OptionGroupSingleton<InquisitorOptions>.Instance.StallGame &&
         inquis is { CanVanquish: true, TargetsDead: false } && Helpers.GetAlivePlayers().Count <= 3) ||
        (x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
         !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
         OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue));

    public static int ImpAliveCount => Helpers.GetAlivePlayers().Count(x => x.IsImpostor());

    public static int CrewKillersAliveCount => Helpers.GetAlivePlayers().Count(x =>
        x.Data.Role is ITouCrewRole { IsPowerCrew: true } &&
        !(x.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.CrewContinuesGame) &&
        OptionGroupSingleton<GeneralOptions>.Instance.CrewKillersContinue);

    public static IEnumerable<BaseModifier> AllModifiers => ModifierManager.Modifiers;

    public static IEnumerable<RoleBehaviour> AllRoles => CustomRoleManager.CustomRoleBehaviours;

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

        var builder = new StringBuilder();
        builder.AppendLine(CultureInfo.InvariantCulture,
            $"\n<size=50%> \n</size><b>{TownOfUsColors.Vigilante.ToTextColor()}{TouLocale.Get("Options")}</color></b>");

        foreach (var option in options)
        {
            switch (option)
            {
                case ModdedToggleOption toggleOption:
                    if (!toggleOption.Visible())
                    {
                        continue;
                    }

                    builder.AppendLine(option.Title + ": " + toggleOption.Value);
                    break;
                case ModdedEnumOption enumOption:
                    if (!enumOption.Visible())
                    {
                        continue;
                    }

                    builder.AppendLine(enumOption.Title + ": " + enumOption.Values[enumOption.Value]);
                    break;
                case ModdedNumberOption numberOption:
                    if (!numberOption.Visible())
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

                    if (numberOption is { ZeroInfinity: true, Value: 0 })
                    {
                        builder.AppendLine(numberOption.Title + ": ∞");
                    }
                    else
                    {
                        builder.AppendLine(numberOption.Title + ": " + optionStr);
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
                role.RoleOptionsGroup.Name.Replace($" {TouLocale.Get("Roles")}", "") ==
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
                if (customRole.RoleOptionsGroup.Name.Replace(" Roles", "") == roleAlignment.ToDisplayString() || customRole.RoleOptionsGroup.Name.Replace($" {TouLocale.Get("Roles")}", "") == roleAlignment.ToDisplayString())
                {
                    return roleAlignment;
                }
            }
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
            return RoleAlignment.ImpostorSupport;
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
            foreach (var crewRole in AllRoles.Where(x => x.IsCrewmate()))
            {
                if (!isForCrew && gameMod.IsModifierValidOn(crewRole))
                {
                    isForCrew = true;
                    break;
                }
            }
            foreach (var neutRole in AllRoles.Where(x => x.IsNeutral()))
            {
                if (!isForNeut && gameMod.IsModifierValidOn(neutRole))
                {
                    isForNeut = true;
                    break;
                }
            }
            foreach (var impRole in AllRoles.Where(x => x.IsImpostor()))
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

    public static ModifierFaction GetModifierFaction(this AllianceGameModifier mod)
    {
        return mod.FactionType;
    }
    public static ModifierFaction GetModifierFaction(this TouGameModifier mod)
    {
        return mod.FactionType;
    }
    public static ModifierFaction GetModifierFaction(this UniversalGameModifier mod)
    {
        return mod.FactionType;
    }
    public static ModifierFaction GetModifierFaction(this GameModifier mod)
    {
        return GetModifierFaction(mod as BaseModifier);
    }
    public static ModifierFaction GetModifierFaction(this BaseModifier mod)
    {
        if (mod is TouGameModifier touMod)
        {
            return touMod.FactionType;
        }
        else if (mod is AllianceGameModifier allyMod)
        {
            return allyMod.FactionType;
        }
        else if (mod is UniversalGameModifier uniMod)
        {
            return uniMod.FactionType;
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
        var localizedName = TouLocale.Get(localeName);

        return localizedName;
    }
    public static string GetParsedRoleAlignment(ICustomRole role, bool coloredText = false)
    {
        var localeName = $"{role.GetRoleAlignment()}";
        var localizedName = TouLocale.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(TouLocale.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(TouLocale.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(TouLocale.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(TouLocale.Get("GameKeyword")))
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
        var localeName = $"{role.GetRoleAlignment()}";
        var localizedName = TouLocale.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(TouLocale.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(TouLocale.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(TouLocale.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(TouLocale.Get("GameKeyword")))
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
        var localizedName = TouLocale.Get(localeName);

        if (coloredText)
        {
            if (localizedName.Contains("Crewmate") || localizedName.Contains(TouLocale.Get("CrewmateKeyword")))
            {
                localizedName = $"<color=#68ACF4>{localizedName}";
            }
            else if (localizedName.Contains("Impostor") || localizedName.Contains(TouLocale.Get("ImpostorKeyword")))
            {
                localizedName = $"<color=#D63F42>{localizedName}";
            }
            else if (localizedName.Contains("Neutral") || localizedName.Contains(TouLocale.Get("NeutralKeyword")))
            {
                localizedName = $"<color=#8A8A8A>{localizedName}";
            }
            else if (localizedName.Contains("Game") || localizedName.Contains(TouLocale.Get("GameKeyword")))
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

    public static IEnumerable<RoleBehaviour> GetRegisteredRoles(RoleAlignment alignment)
    {
        var roles = AllRoles.Where(x => x.GetRoleAlignment() == alignment);

        var registeredRoles = roles.ToList();

        switch (alignment)
        {
            case RoleAlignment.CrewmateInvestigative:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                //registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Detective));
                break;
            case RoleAlignment.CrewmateSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Crewmate));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Scientist));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Noisemaker));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                break;
            case RoleAlignment.ImpostorSupport:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                //registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Viper));
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
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Engineer));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Tracker));
                break;
            case ModdedRoleTeams.Impostor:
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Impostor));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Shapeshifter));
                registeredRoles.Add(RoleManager.Instance.GetRole(RoleTypes.Phantom));
                break;
        }

        return registeredRoles;
    }

    public static IEnumerable<RoleBehaviour> GetRegisteredGhostRoles()
    {
        var baseGhostRoles = RoleManager.Instance.AllRoles.ToArray().Where(x => x.IsDead && AllRoles.All(y => y.Role != x.Role));
        var ghostRoles = AllRoles.Where(x => x.IsDead && !x.TryCast<SpectatorRole>()).Union(baseGhostRoles);

        return ghostRoles;
    }

    public static RoleBehaviour? GetRegisteredRole(RoleTypes roleType)
    {
        // we want to prioritize the custom roles because the role has the right RoleColour/TeamColor
        var role = AllRoles.FirstOrDefault(x => x.Role == roleType) ??
                   RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(x => x.Role == roleType);

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

    public static string RoleNameLookup(RoleTypes roleType)
    {
        var role = RoleManager.Instance.GetRole(roleType);
        return role?.GetRoleName() ?? TranslationController.Instance.GetString(roleType == RoleTypes.Crewmate ? StringNames.Crewmate : StringNames.Impostor);
    }

    public static IEnumerable<RoleBehaviour> GetPotentialRoles()
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;
        var assignmentData = RoleManager.Instance.AllRoles.ToArray().Select(role =>
            new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var roleList = assignmentData.Where(x => x is { Chance: > 0, Count: > 0, Role: ICustomRole }).Select(x => x.Role);

        /*if (OptionGroupSingleton<GeneralOptions>.Instance.GuessVanillaRoles)
        {
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Tracker) is { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Tracker)!);
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Noisemaker) is { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Noisemaker)!);
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Engineer) is { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Engineer)!);
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Scientist) is { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Scientist)!);
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Shapeshifter) is
                { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Shapeshifter)!);
            if (assignmentData.FirstOrDefault(x => x.Role.Role is RoleTypes.Phantom) is { Chance: > 0, Count: > 0 })
                roleList = roleList.AddItem(
                    RoleManager.Instance.AllRoles.FirstOrDefault(x => x.Role == RoleTypes.Phantom)!);
        }*/

        var crewmateRole = RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(x => x.Role == RoleTypes.Crewmate);
        roleList = roleList.AddItem(crewmateRole!);
        //Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - crewmateRole: '{crewmateRole?.GetRoleName()}'");

        var impostorRole = RoleManager.Instance.AllRoles.ToArray().FirstOrDefault(x => x.Role == RoleTypes.Impostor);
        roleList = roleList.AddItem(impostorRole!);

        //Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - impostorRole: '{impostorRole?.GetRoleName()}'");

        //roleList.Do(x => Logger<TownOfUsPlugin>.Error($"GetPotentialRoles - role: '{x.GetRoleName()}'"));

        return roleList;
    }

    public static void AddFakeChat(NetworkedPlayerInfo basePlayer, string nameText, string message,
        bool showHeadsup = false, bool altColors = false, bool onLeft = true)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();

        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
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
        if (altColors)
        {
            pooledBubble.Background.color = Color.black;
            pooledBubble.TextArea.color = Color.white;
        }

        pooledBubble.AlignChildren();
        var pos = pooledBubble.NameText.transform.localPosition;
        pooledBubble.NameText.transform.localPosition = pos;
        chat.AlignAllBubbles();
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
        bool showHeadsup = false, bool onLeft = true)
    {
        var chat = HudManager.Instance.Chat;

        var pooledBubble = chat.GetPooledBubble();

        pooledBubble.transform.SetParent(chat.scroller.Inner);
        pooledBubble.transform.localScale = Vector3.one;
        if (onLeft)
        {
            pooledBubble.SetLeft();
        }
        else
        {
            pooledBubble.SetRight();
        }

        pooledBubble.SetCosmetics(basePlayer);
        pooledBubble.NameText.text = nameText;
        pooledBubble.NameText.color = Color.white;
        pooledBubble.NameText.ForceMeshUpdate(true, true);
        pooledBubble.votedMark.enabled = false;
        pooledBubble.Xmark.enabled = false;
        pooledBubble.TextArea.text = message;
        pooledBubble.TextArea.ForceMeshUpdate(true, true);
        pooledBubble.Background.size = new Vector2(5.52f,
            0.2f + pooledBubble.NameText.GetNotDumbRenderedHeight() + pooledBubble.TextArea.GetNotDumbRenderedHeight());
        pooledBubble.MaskArea.size = pooledBubble.Background.size - new Vector2(0, 0.03f);

        pooledBubble.Background.color = new Color(0.2f, 0.2f, 0.27f, 1f);
        pooledBubble.TextArea.color = Color.white;

        pooledBubble.AlignChildren();
        var pos = pooledBubble.NameText.transform.localPosition;
        pooledBubble.NameText.transform.localPosition = pos;
        chat.AlignAllBubbles();
        if (chat is { IsOpenOrOpening: false, notificationRoutine: null })
        {
            chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
        }

        if (showHeadsup && !chat.IsOpenOrOpening)
        {
            SoundManager.Instance.PlaySound(chat.messageSound, false).pitch = 0.1f;
            chat.chatNotification.SetUp(PlayerControl.LocalPlayer, message);
        }
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

    public static List<(ushort RoleType, int Chance)> GetRolesToAssign(ModdedRoleTeams team,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(team);

        return GetRolesToAssign(roles, filter);
    }

    public static List<(ushort RoleType, int Chance)> GetRolesToAssign(RoleAlignment alignment,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment);

        return GetRolesToAssign(roles, filter);
    }

    private static List<(ushort RoleType, int Chance)> GetRolesToAssign(IEnumerable<RoleBehaviour> roles,
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
        var roles = GetRegisteredRoles(team);

        return GetMaxRolesToAssign(roles, max, filter);
    }

    public static List<ushort> GetMaxRolesToAssign(RoleAlignment alignment, int max,
        Func<RoleBehaviour, bool>? filter = null)
    {
        var roles = GetRegisteredRoles(alignment);

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

    private static List<(ushort RoleType, int Chance)> GetPossibleRoles(
        List<RoleManager.RoleAssignmentData> assignmentData,
        Func<RoleManager.RoleAssignmentData, bool>? predicate = null)
    {
        var roles = new List<(ushort, int)>();

        assignmentData.Where(x => predicate == null || predicate(x)).ToList().ForEach(x =>
        {
            for (var i = 0; i < x.Count; i++)
            {
                roles.Add(((ushort)x.Role.Role, x.Chance));
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

    public static PlayerControl? PlayerById(byte id)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == id)
            {
                return player;
            }
        }

        return null;
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

    public static IEnumerator CoFlash(Color color, float waitfor = 1f, float alpha = 0.3f)
    {
        color.a = alpha;
        if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
        {
            var fullscreen = HudManager.Instance.FullScreen;
            fullscreen.enabled = true;
            fullscreen.gameObject.SetActive(true);
            fullscreen.color = color;
        }

        yield return new WaitForSeconds(waitfor);

        if (HudManager.InstanceExists && HudManager.Instance.FullScreen)
        {
            var fullscreen = HudManager.Instance.FullScreen;
            if (!fullscreen.color.Equals(color))
            {
                yield break;
            }

            fullscreen.color = new Color(1f, 0f, 0f, 0.37254903f);
            fullscreen.enabled = false;
        }
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
                    && (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0 ||
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3 ||
                        GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4))
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

    public static List<ushort> ReadFromBucket(List<RoleListOption> buckets, List<(ushort RoleType, int Chance)> roles,
        RoleListOption roleType, RoleListOption replaceType)
    {
        var result = new List<ushort>();

        while (buckets.Contains(roleType))
        {
            if (roles.Count == 0)
            {
                var count = buckets.RemoveAll(x => x == roleType);
                buckets.AddRange(Enumerable.Repeat(replaceType, count));

                break;
            }

            var addedRole = SelectRole(roles);
            result.Add(addedRole.RoleType);
            roles.Remove(addedRole);

            buckets.Remove(roleType);
        }

        return result;
    }

    public static List<ushort> ReadFromBucket(List<RoleListOption> buckets, List<(ushort RoleType, int Chance)> roles,
        RoleListOption roleType)
    {
        var result = new List<ushort>();

        while (buckets.Contains(roleType))
        {
            if (roles.Count == 0)
            {
                buckets.RemoveAll(x => x == roleType);

                break;
            }

            var addedRole = SelectRole(roles);
            result.Add(addedRole.RoleType);
            roles.Remove(addedRole);

            buckets.Remove(roleType);
        }

        return result;
    }

    public static (ushort RoleType, int Chance) SelectRole(List<(ushort RoleType, int Chance)> roles)
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
        (ushort RoleType, int SpawnChance) selectedRole = default;

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
    public static T[] jsonToArray<T>(string json)
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
    ///     Gets a FakePlayer by comparing PlayerControl.
    /// </summary>
    /// <param name="player">The player themselves.</param>
    /// <returns>A fake player or null if its not found.</returns>
    public static FakePlayer? GetFakePlayer(PlayerControl player)
    {
        return FakePlayer.FakePlayers.FirstOrDefault(x => x.body?.name == $"Fake {player.gameObject.name}");
    }

    public static void SetForcedBodyType(this PlayerPhysics player, PlayerBodyTypes bodyType)
    {
        player.bodyType = bodyType;
        player.myPlayer.cosmetics.EnsureInitialized(bodyType);
        player.Animations.SetBodyType(bodyType, player.myPlayer.cosmetics.FlippedCosmeticOffset, player.myPlayer.cosmetics.NormalCosmeticOffset);
        player.Animations.PlayIdleAnimation();
    }

    public static bool IsMap(byte mapid)
    {
        return (GameOptionsManager.Instance != null &&
                GameOptionsManager.Instance.currentNormalGameOptions.MapId == mapid)
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

        var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            return true;
        }

        if (OptionGroupSingleton<GeneralOptions>.Instance.CamouflageComms)
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) ||
                commsSystem == null)
            {
                return false;
            }

            var isActive = false;
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq ||
                ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
            {
                var hqSystem = commsSystem.Cast<HqHudSystemType>();
                if (hqSystem != null)
                {
                    isActive = hqSystem.IsActive;
                }
            }
            else
            {
                var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
                if (hudSystem != null)
                {
                    isActive = hudSystem.IsActive;
                }
            }

            return isActive;
        }

        return false;
    }

    public static bool CanUseVent(this PlayerControl player, Vent vent)
    {
        var couldUse = (!player.MustCleanVent(vent.Id) || (player.inVent && Vent.currentVent == vent)) &&
                       !player.Data.IsDead && (player.CanMove || player.inVent);
        ISystemType systemType;
        if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out systemType))
        {
            var ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>();
            if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id))
            {
                couldUse = false;
            }
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
        var closePlayer = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, distance);

        var includePostors = genOpt.FFAImpostorMode ||
                             (PlayerControl.LocalPlayer.IsLover() &&
                              OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates) ||
                             (genOpt.KillDuringCamoComms &&
                              closePlayer?.GetAppearanceType() == TownOfUsAppearances.Camouflage);
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, distance, false, x => !x.IsLover());
        }
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, distance);
    }

    public static void SetSizeLimit(this SpriteRenderer sprite, float pixelSize)
    {
        sprite.drawMode = SpriteDrawMode.Sliced;
        float spriteWidth = sprite.sprite.rect.width;
        float spriteHeight = sprite.sprite.rect.height;

        if (spriteWidth < spriteHeight)
        {
            sprite.size = new Vector2(pixelSize * spriteWidth / spriteHeight, pixelSize);
        }
        else
        {
            sprite.size = new Vector2(pixelSize, pixelSize * spriteHeight / spriteWidth);
        }
    }

    public static void SetSizeLimit(this GameObject spriteObj, float pixelSize)
    {
        if (!spriteObj.TryGetComponent<SpriteRenderer>(out var sprite))
        {
            return;
        }

        sprite.SetSizeLimit(pixelSize);
    }
    public static bool DiedOtherRound(this PlayerControl player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && player.HasDied())
        {
            return !deathHandler.DiedThisRound;
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
        return ModifierManager.GetModifierTypeId(mod.GetType()) ?? throw new InvalidOperationException("Modifier is not registered.");
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

    public static void AddMiraTranslator(this GameObject obj, string stringName, bool parseInfo, string? defaultStr = null)
    {
        if (obj.TryGetComponent<TextTranslatorTMP>(out var amogTmp))
        {
            // we don't like innerscuff StringNames, im sorry
            Object.Destroy(amogTmp);
        }
        var translator = obj.AddComponent<TmpMiraTranslator>();
        translator.stringName = stringName;
        translator.parseStr = parseInfo;
        translator.defaultStr = defaultStr ?? string.Empty;
    }
    public static void AddMiraTranslator(this Transform obj, string stringName, bool parseInfo, string? defaultStr = null)
    {
        if (obj.TryGetComponent<TextTranslatorTMP>(out var amogTmp))
        {
            // we don't like innerscuff StringNames, im sorry
            Object.Destroy(amogTmp);
        }
        var translator = obj.gameObject.AddComponent<TmpMiraTranslator>();
        translator.stringName = stringName;
        translator.parseStr = parseInfo;
        translator.defaultStr = defaultStr ?? string.Empty;
    }

    public static string GetParsedRoleBucket(string bucket)
    {
        var text = TouLocale.Get(bucket);
        var crewmateKeyword = TouLocale.Get("CrewmateKeyword");
        var crewKeyword = TouLocale.Get("CrewKeyword");
        var impostorKeyword = TouLocale.Get("ImpostorKeyword");
        var impKeyword = TouLocale.Get("ImpKeyword");
        var neutralKeyword = TouLocale.Get("NeutralKeyword");
        var neutKeyword = TouLocale.Get("NeutKeyword");

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

    public static IEnumerable<T> Excluding<T>(this IEnumerable<T> source, Func<T, bool> predicate) => source.Where(x => !predicate(x)); // Added for easier inversion and reading
}