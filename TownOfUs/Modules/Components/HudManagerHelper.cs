using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using TMPro;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class HudManagerHelper(nint cppPtr) : MonoBehaviour(cppPtr)
{
    #pragma warning disable S2325
    #pragma warning disable CA1822
    public void FixedUpdate()
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || !PlayerControl.LocalPlayer.Data.Role ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }

        UpdateCamouflageComms();
        UpdateRoleNameText();
    }
    #pragma warning restore CA1822
    #pragma warning restore S2325
    public static void UpdateCamouflageComms()
    {
        var isActive = HudManagerPatches.CommsSaboActive();
        if (PlayerControl.LocalPlayer.IsHysteria())
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var appearanceType = player.GetAppearanceType();
            if (isActive)
            {
                if (player.Data.Role is IGhostRole)
                {
                    continue;
                }

                if (appearanceType != TownOfUsAppearances.Swooper && appearanceType != TownOfUsAppearances.Camouflage)
                {
                    player.SetCamouflage();
                }
            }
            else
            {
                if (appearanceType == TownOfUsAppearances.Camouflage &&
                    !player.HasModifier<VenererCamouflageModifier>())
                {
                    player.SetCamouflage(false);
                }
            }
        }

        if (isActive)
        {
            HudManagerPatches.CamouflageCommsEnabled = true;

            foreach (var fakePlayer in FakePlayer.FakePlayers)
            {
                fakePlayer.Camo();
            }

            foreach (var fakePlayer in StonedPlayer.FakePlayers)
            {
                fakePlayer.Camo();
            }

            return;
        }

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            HudManagerPatches.CamouflageCommsEnabled = false;
            FakePlayer.FakePlayers.Do(x => x.UnCamo());
            StonedPlayer.FakePlayers.Do(x => x.UnCamo());
        }
    }

    public static void UpdateRoleNameText()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var taskOpt = OptionGroupSingleton<PostmortemOptions>.Instance;

        static PlayerControl GetDisguiseTargetOrSelf(PlayerControl player)
        {
            if (player.TryGetModifier<MorphlingMorphModifier>(out var morph) && morph.Target)
            {
                return morph.Target;
            }

            if (player.TryGetModifier<ShapeshifterShiftModifier>(out var shift) && shift.Target)
            {
                return shift.Target;
            }

            if (player.TryGetModifier<GlitchMimicModifier>(out var mimic) && mimic.Target)
            {
                return mimic.Target;
            }

            return player;
        }

        static string GetDiedR1ExtraNameTextForDisplayedIdentity(PlayerControl player)
        {
            var displayPlayer = GetDisguiseTargetOrSelf(player);
            var mod = displayPlayer.GetModifiers<BaseRevealModifier>()
                .FirstOrDefault(x => x.Visible && x is FirstRoundIndicator && x.ExtraNameText != string.Empty);
            return mod?.ExtraNameText ?? string.Empty;
        }

        var roleNameSize = HudManagerPatches.RoleIsSmall ? "80%" : "100%";
        var roleOnTop = HudManagerPatches.RoleOnTop;

        var colorPlayerNames = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ColorPlayerNameToggle.Value;
        var localDead = PlayerControl.LocalPlayer.HasDied();
        var localGhost = localDead && genOpt.TheDeadKnow;
        var localImp = PlayerControl.LocalPlayer.IsImpostorAligned() &&
                       genOpt is
                           { ImpsKnowRoles.Value: true, FFAImpostorMode: false };
        var localVamp = PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole;
        var useMiraApiChecks =
            !localDead && (!PlayerControl.LocalPlayer.IsImpostorAligned() || !genOpt.FFAImpostorMode);

        if (MeetingHud.Instance)
        {
            foreach (var playerVA in MeetingHud.Instance.playerStates)
            {
                if (!playerVA.gameObject.active)
                {
                    continue;
                }
                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId)!;
                playerVA.ColorBlindName.transform.localPosition = new Vector3(-0.93f, -0.2f, -0.1f);

                var curText = playerVA.NameText.text;
                if (!player || !player.Data || !player.Data.Role)
                {
                    var data = EndGamePatches.ContainedMeetingData.PlayerMeetingRecords.FirstOrDefault(x => x.PlayerId == playerVA.TargetPlayerId);
                    if (data != null)
                    {
                        EndGamePatches.ContainedMeetingData.DisplayRecordData(playerVA.NameText, data, colorPlayerNames, localGhost);
                    }
                    playerVA.NameText.fontSize = 2f;
                    playerVA.NameText.ForceMeshUpdate();
                    if (playerVA.NameText.m_lineNumber > 1)
                    {
                        playerVA.NameText.fontSize = 2f - playerVA.NameText.m_lineNumber * 0.075f;
                    }
                    continue;
                }

                var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

                var playerName = player.GetDefaultAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (colorPlayerNames && PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
                    !player.AmOwner && !genOpt.FFAImpostorMode)
                {
                    playerColor = Color.red;
                }

                playerColor = playerColor.UpdateTargetColor(player);
                playerName = playerName.UpdateTargetSymbols(player);
                playerName = playerName.UpdateProtectionSymbols(player);
                playerName = playerName.UpdateAllianceSymbols(player);
                playerName = playerName.UpdateStatusSymbols(player);

                var localSleuth = SleuthModifier.SleuthVisibilityFlag(player);
                var role = player.Data.Role;
                if (localSleuth || role.Role is RoleTypes.ImpostorGhost or RoleTypes.CrewmateGhost ||
                    role.Role == (RoleTypes)(RoleId.Get<NeutralGhostRole>()))
                {
                    role = player.GetRoleWhenAlive();
                }

                var customRole = role as ICustomRole;

                var color = Color.white;

                var roleName = "";
                var topText = "";
                var bottomText = "";

                var impostorBuddy = localImp && player.IsImpostorAligned();
                var vampBuddy = localVamp && role is VampireRole;
                var revealed = revealMods.Any(x => x.Visible && x.RevealRole);
                var localFairy = FairyRole.FairySeesRoleVisibilityFlag(player);
                if (player.AmOwner || vampBuddy || impostorBuddy || revealed || localGhost || localFairy || localSleuth || useMiraApiChecks && customRole != null && customRole.CanLocalPlayerSeeRole(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color></size>";

                    if (role.Role is RoleTypes.GuardianAngel)
                    {
                        roleName = $"<size={roleNameSize}>{color.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole != null)
                    {
                        color = revealedRole.ShownRole!.TeamColor;
                        roleName =
                            $"<size={roleNameSize}>{color.ToTextColor()}{MiscUtils.GetRoleTmpIcon(revealedRole.ShownRole!)}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole && (vampBuddy || localGhost))
                    {
                        roleName += $"<size={roleNameSize}><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && (impostorBuddy || localGhost))
                    {
                        roleName += $"<size={roleNameSize}><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        role.GetType() != cache.CachedRole.GetType())
                    {
                        var cachedName = cache.CachedRoleName == "" ? MiscUtils.GetRoleTmpIcon(cache.CachedRole) + cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size={roleNameSize}>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color>)</size>";
                    }

                    if (localDead &&
                        player.TryGetModifier<DeathHandlerModifier>(out var deathMod))
                    {
                        topText +=
                            $"<size=60%>『{Color.yellow.ToTextColor()}{deathMod.CauseOfDeath}</color>』</size>\n";
                    }
                }

                var revealedColorMod = revealMods.FirstOrDefault(x => x.Visible && x.NameColor != null);
                if (revealedColorMod != null)
                {
                    playerColor = (Color)revealedColorMod.NameColor!;
                    playerName = $"{playerColor.ToTextColor()}{playerName}</color>";
                }

                var addedRoleNameText = revealMods.FirstOrDefault(x => x.Visible && x.ExtraRoleText != string.Empty);
                if (addedRoleNameText != null)
                {
                    roleName += $"<size={roleNameSize}>{addedRoleNameText.ExtraRoleText}</size>";
                }

                if (HudManagerPatches.PlayerNameProgress == ProgressTracking.Always ||
                    HudManagerPatches.PlayerNameProgress == ProgressTracking.OnSelf && player.AmOwner ||
                    HudManagerPatches.PlayerNameProgress == ProgressTracking.OnOthers && !player.AmOwner)
                {
                    if (player.Data.Role is IProgressTally tally && tally.ProgressOnName(localDead, true, player.AmOwner, out var progress))
                    {
                        var placement = tally.TallyPlacement(true);
                        if (HudManagerPatches.RoleIsSmall && placement == TallyLocation.Auto || placement == TallyLocation.RoleName)
                        {
                            if (roleName != string.Empty)
                            {
                                roleName += " ";
                            }

                            roleName += $"<size={roleNameSize}>{progress}</size>";
                        }
                        else if (placement == TallyLocation.Auto || placement == TallyLocation.PlayerName)
                        {
                            playerName += $" {progress}";
                        }
                        else if (placement == TallyLocation.BelowName)
                        {
                            bottomText += $"\n{progress}";
                        }
                        else if (placement == TallyLocation.AboveName)
                        {
                            topText += $"{progress}\n";
                        }
                    }
                    else if ((player.AmOwner ||
                              (localDead && taskOpt.ShowTaskDead.Value)) &&
                             player.IsCrewmate())
                    {
                        if (HudManagerPatches.RoleIsSmall)
                        {
                            if (roleName != string.Empty)
                            {
                                roleName += " ";
                            }

                            roleName += $"<size={roleNameSize}>{player.TaskInfo()}</size>";
                        }
                        else
                        {
                            playerName += $" {player.TaskInfo()}";
                        }
                    }
                }

                if (player.TryGetModifier<OracleConfessModifier>(out var confess, x => x.ConfessToAll))
                {
                    var accuracy = OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage;
                    var revealText = confess.RevealedFaction switch
                    {
                        ModdedRoleTeams.Crewmate =>
                            $"\n<size=75%>{Palette.CrewmateBlue.ToTextColor()}({accuracy}% Crew) </color></size>",
                        ModdedRoleTeams.Custom =>
                            $"\n<size=75%>{TownOfUsColors.Neutral.ToTextColor()}({accuracy}% Neut) </color></size>",
                        ModdedRoleTeams.Impostor =>
                            $"\n<size=75%>{TownOfUsColors.ImpSoft.ToTextColor()}({accuracy}% Imp) </color></size>",
                        _ => string.Empty
                    };

                    bottomText += revealText;
                }

                var addedPlayerNameText = revealMods.FirstOrDefault(x =>
                    x.Visible && x.ExtraNameText != string.Empty && x is not FirstRoundIndicator);
                if (addedPlayerNameText != null)
                {
                    playerName += addedPlayerNameText.ExtraNameText;
                }

                var diedR1Text = GetDiedR1ExtraNameTextForDisplayedIdentity(player);
                if (!string.IsNullOrEmpty(diedR1Text))
                {
                    bottomText += diedR1Text;
                }

                if (player.Data?.Disconnected == true)
                {
                    EndGamePatches.ContainedMeetingData.AddPlayerData(player);
                    // don't wanna leak info!
                    continue;
                }

                if (HaunterRole.HaunterVisibilityFlag(player))
                {
                    playerColor = TownOfUsColors.HaunterRevealed;
                    color = TownOfUsColors.HaunterRevealed;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (roleOnTop)
                    {
                        if (colorPlayerNames)
                        {
                            playerName = $"{roleName}\n{color.ToTextColor()}{playerName}</color>";
                        }
                        else
                        {
                            playerName = $"{roleName}\n{playerName}";
                        }
                    }
                    else
                    {
                        if (colorPlayerNames)
                        {
                            playerName = $"{color.ToTextColor()}{playerName}</color>\n{roleName}";
                        }
                        else
                        {
                            playerName = $"{playerName}\n{roleName}";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(topText))
                {
                    playerName = $"{topText}{playerName}";
                }
                if (!string.IsNullOrEmpty(bottomText))
                {
                    playerName = $"{playerName}{bottomText}";
                }

                if (curText != playerName)
                {
                    playerVA.NameText.text = playerName;
                    playerVA.NameText.fontSize = 2f;
                    playerVA.NameText.ForceMeshUpdate();
                    if (playerVA.NameText.m_lineNumber > 1)
                    {
                        playerVA.NameText.fontSize = 2f - playerVA.NameText.m_lineNumber * 0.15f;
                    }
                }

                playerVA.NameText.color = playerColor;
            }
        }
        else
        {
            var isVisible = (PlayerControl.LocalPlayer.TryGetModifier<DeathHandlerModifier>(out var deathHandler) &&
                             !deathHandler.DiedThisRound) || TutorialManager.InstanceExists;
            if (localGhost)
            {
                localGhost = isVisible;
            }
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || !player.Data || !player.Data.Role)
                {
                    continue;
                }

                var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

                var playerName = player.GetAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (colorPlayerNames && PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
                    !player.AmOwner && !genOpt.FFAImpostorMode)
                {
                    playerColor = Color.red;
                }

                playerColor = playerColor.UpdateTargetColor(player);
                playerName = playerName.UpdateTargetSymbols(player, !isVisible);
                playerName = playerName.UpdateProtectionSymbols(player, !isVisible);
                playerName = playerName.UpdateAllianceSymbols(player, !isVisible);
                playerName = playerName.UpdateStatusSymbols(player, !isVisible);

                var localSleuth = SleuthModifier.SleuthVisibilityFlag(player);
                var role = player.Data.Role;
                if (localSleuth || role.Role is RoleTypes.ImpostorGhost or RoleTypes.CrewmateGhost ||
                    role.Role == (RoleTypes)(RoleId.Get<NeutralGhostRole>()))
                {
                    role = player.GetRoleWhenAlive();
                }
                var customRole = player.Data.Role as ICustomRole;
                var color = Color.white;

                var roleName = "";
                var topText = "";
                var bottomText = "";
                var impostorBuddy = localImp && player.IsImpostorAligned();
                var vampBuddy = localVamp && role is VampireRole;
                var revealed = revealMods.Any(x => x.Visible && x.RevealRole);
                var localFairy = FairyRole.FairySeesRoleVisibilityFlag(player);
                if (player.AmOwner || vampBuddy || impostorBuddy || revealed || localGhost || localFairy || localSleuth || useMiraApiChecks && customRole != null && customRole.CanLocalPlayerSeeRole(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color></size>";

                    if (role.Role is RoleTypes.GuardianAngel)
                    {
                        roleName = $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole != null)
                    {
                        color = revealedRole.ShownRole!.TeamColor;
                        roleName =
                            $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(revealedRole.ShownRole!)}{color.ToTextColor()}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole && (vampBuddy || localGhost))
                    {
                        roleName += $"<size={roleNameSize}><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && (impostorBuddy || localGhost))
                    {
                        roleName += $"<size={roleNameSize}><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        var cachedName = cache.CachedRoleName == "" ? MiscUtils.GetRoleTmpIcon(cache.CachedRole) + cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size={roleNameSize}>{MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size={roleNameSize}>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({MiscUtils.GetRoleTmpIcon(role)}{color.ToTextColor()}{role.GetRoleName()}</color>)</size>";
                    }

                    if (localDead && isVisible &&
                        player.TryGetModifier<DeathHandlerModifier>(out var deathMod))
                    {
                        topText +=
                            $"<size=75%>『{Color.yellow.ToTextColor()}{deathMod.CauseOfDeath}</color>』</size>\n";
                    }
                }

                var revealedColorMod = revealMods.FirstOrDefault(x => x.Visible && x.NameColor != null);
                if (revealedColorMod != null)
                {
                    playerColor = (Color)revealedColorMod.NameColor!;
                    playerName = $"{playerColor.ToTextColor()}{playerName}</color>";
                }

                var addedRoleNameText = revealMods.FirstOrDefault(x => x.Visible && x.ExtraRoleText != string.Empty);
                if (addedRoleNameText != null)
                {
                    roleName += $"<size={roleNameSize}>{addedRoleNameText.ExtraRoleText}</size>";
                }

                if (HudManagerPatches.PlayerNameProgress == ProgressTracking.Always ||
                    HudManagerPatches.PlayerNameProgress == ProgressTracking.OnSelf && player.AmOwner ||
                    HudManagerPatches.PlayerNameProgress == ProgressTracking.OnOthers && !player.AmOwner)
                {
                    if (player.Data.Role is IProgressTally tally && tally.ProgressOnName(localDead, false, player.AmOwner, out var progress))
                    {
                        var placement = tally.TallyPlacement(false);
                        if (HudManagerPatches.RoleIsSmall && placement == TallyLocation.Auto || placement == TallyLocation.RoleName)
                        {
                            if (roleName != string.Empty)
                            {
                                roleName += " ";
                            }

                            roleName += $"<size={roleNameSize}>{progress}</size>";
                        }
                        else if (placement == TallyLocation.Auto || placement == TallyLocation.PlayerName)
                        {
                            playerName += $" {progress}";
                        }
                        else if (placement == TallyLocation.BelowName)
                        {
                            bottomText += $"\n{progress}";
                        }
                        else if (placement == TallyLocation.AboveName)
                        {
                            topText += $"{progress}\n";
                        }
                    }
                    else if ((player.AmOwner || (taskOpt.ShowTaskDead.Value && isVisible && localDead)) &&
                             player.IsCrewmate())
                    {
                        if (HudManagerPatches.RoleIsSmall)
                        {
                            if (roleName != string.Empty)
                            {
                                roleName += " ";
                            }

                            roleName += $"<size={roleNameSize}>{player.TaskInfo()}</size>";
                        }
                        else
                        {
                            playerName += $" {player.TaskInfo()}";
                        }
                    }
                }

                if (player.AmOwner && player.TryGetModifier<ScatterModifier>(out var scatter) && !player.HasDied())
                {
                    roleName += $" - {scatter.GetDescription()}";
                }

                var addedPlayerNameText = revealMods.FirstOrDefault(x =>
                    x.Visible && x.ExtraNameText != string.Empty && x is not FirstRoundIndicator);
                if (addedPlayerNameText != null)
                {
                    playerName += addedPlayerNameText.ExtraNameText;
                }

                var diedR1Text = GetDiedR1ExtraNameTextForDisplayedIdentity(player);
                if (!string.IsNullOrEmpty(diedR1Text))
                {
                    bottomText += diedR1Text;
                }

                if (player.AmOwner && player.Data.Role is IGhostRole { GhostActive: true })
                {
                    playerColor = Color.clear;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (roleOnTop)
                    {
                        playerName = colorPlayerNames
                            ? $"{roleName}\n{color.ToTextColor()}{playerName}</color>"
                            : $"{roleName}\n{playerName}";
                    }
                    else
                    {
                        playerName = colorPlayerNames
                            ? $"{color.ToTextColor()}{playerName}</color>\n{roleName}"
                            : $"{playerName}\n{roleName}";
                    }
                }
                else
                {
                    playerName = colorPlayerNames
                        ? $"{color.ToTextColor()}{playerName}</color>"
                        : $"{playerName}";
                }

                if (!string.IsNullOrEmpty(topText))
                {
                    playerName = $"{topText}{playerName}";
                }
                if (!string.IsNullOrEmpty(bottomText))
                {
                    playerName = $"{playerName}{bottomText}";
                }

                player.cosmetics.nameText.text = playerName;
                player.cosmetics.nameText.color = playerColor;

                player.cosmetics.nameText.alignment = TextAlignmentOptions.Bottom;
            }
        }

        if (HudManager.Instance.TaskPanel)
        {
            var tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP")
                .GetComponent<TextMeshPro>();
            tabText.SetText($"{HudManagerPatches.StoredTasksText} {PlayerControl.LocalPlayer.TaskInfo()}");
        }
    }
}