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
using TownOfUs.Patches.ControlSystem;
using TownOfUs.Patches.Roles;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class HudManagerHelper(nint cppPtr) : MonoBehaviour(cppPtr)
{
    internal static Dictionary<int, string> PlatformAssociations = new();
    private static bool HasFetchedIcons;

    public static HudManagerHelper Instance { get; private set; }
    public float DeathTimer;
    public int CurrentRound { get; set; } = 1;
    public static void RefreshPlatformData()
    {
        PlatformAssociations.Clear();
        if (!HasFetchedIcons)
        {
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.BlankSprite.LoadAsset(),
                "Platform.Blank", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformEpic.LoadAsset(),
                "Platform.Epic", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformItch.LoadAsset(),
                "Platform.Itch", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformStarlight.LoadAsset(),
                "Platform.Starlight", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformSteam.LoadAsset(),
                "Platform.Steam", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformWindows.LoadAsset(),
                "Platform.Windows", 1.45f);
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.PlatformUnknown.LoadAsset(),
                "Platform.Unknown", 1.45f);
            HasFetchedIcons = true;
        }

        foreach (var client in AmongUsClient.Instance.allClients)
        {
            var platform = client.PlatformData.Platform;
            var icon = platform switch
            {
                Platforms.StandaloneEpicPC => "<sprite name=\"Platform.Epic\">",
                Platforms.StandaloneItch => "<sprite name=\"Platform.Itch\">",
                Platforms.Android or (Platforms)112 => "<sprite name=\"Platform.Starlight\">",
                Platforms.StandaloneSteamPC => "<sprite name=\"Platform.Steam\">",
                Platforms.StandaloneWin10 or Platforms.Xbox => "<sprite name=\"Platform.Windows\">",
                _ => "<sprite name=\"Platform.Unknown\">"
            };
            PlatformAssociations.Add(client.Id, icon);
        }
    }

    public void Awake()
    {
        Instance = this;
    }
#pragma warning disable S2325
    #pragma warning disable CA1822
    public void FixedUpdate()
    {
        if (!HudManager.InstanceExists || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        var instance = HudManager.Instance;

        HudManagerPatches.CreateUiRow(instance);
        HudManagerPatches.CreateNewUiRow(instance);

        HudManagerPatches.CreateWikiButton(instance);
        HudManagerPatches.CreateZoomButton(instance);
        HudManagerPatches.AdjustModifierTab();

        HudManagerPatches.UpdateRoleList(instance);
        HudManagerPatches.UpdateTeamChat();

        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || !PlayerControl.LocalPlayer.Data.Role ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }
        HudManagerPatches.UpdateSubmergedButtons(instance);
        
        if (!PlayerControl.LocalPlayer.Data.Role ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }

        UpdateCamouflageComms();
        UpdateRoleNameText();

        if (!TutorialManager.InstanceExists)
        {
            GameTimerPatch.UpdateGameTimer(instance);
        }
    }
    public void Update()
    {
        if (!HudManager.InstanceExists || !PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        if (DeathTimer > 0)
        {
            DeathTimer -= Time.deltaTime;
        }

        var instance = HudManager.Instance;
        Bindings.UpdateKeybinds(instance);

        if (HudManagerPatches.CanZoom)
        {
            HudManagerPatches.CheckForScrollZoom();
        }
        if (!PlayerControl.LocalPlayer.Data.Role ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }

        var role = PlayerControl.LocalPlayer.Data.Role;
        var ghostRole = role as IGhostRole;
        PuppeteerOverlayPatch.RunPuppetOverlay();
        ParasiteOverlayPatch.RunOvertakeOverlay();
        ControlledPlayerInteractionPatches.UpdateControlledUseButton(instance);
        SentryCameraPortablePatch.ApplyPortableBlinkState();
        TimeLordPatches.RecordTimeLordSnapshot(instance);
        if (PlayerControl.LocalPlayer.Data.IsDead && ghostRole != null)
        {
            GhostRolePatches.HandleGhostRoleVent(instance, ghostRole);
            SubmergedHudPatch.UpdateFloorButton(instance, ghostRole);
        }
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

    public static string GetDiedR1ExtraNameTextForDisplayedIdentity(PlayerControl player, bool useDisguise)
    {
        if (useDisguise && player.TryGetModifier<DisguisedModifier>(out var disguise) && disguise.Target != null)
        {
            player = disguise.Target;
        }
        var mod = player.GetModifiers<BaseRevealModifier>()
                .FirstOrDefault(x => x.Visible && x is FirstRoundIndicator && x.ExtraNameText != string.Empty);
        return mod?.ExtraNameText ?? string.Empty;
    }

    public static bool HasSetMeetingColorText;
    public static void UpdateRoleNameText()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var taskOpt = OptionGroupSingleton<PostmortemOptions>.Instance;

        var roleNameSize = HudManagerPatches.RoleIsSmall ? "80%" : "100%";
        var roleOnTop = HudManagerPatches.RoleOnTop;

        var colorPlayerNames = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.ColorPlayerNameToggle.Value;
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
                if (!HasSetMeetingColorText)
                {
                    playerVA.ColorBlindName.transform.localPosition = new Vector3(-0.93f, -0.2f, -0.1f);
                }

                var curText = playerVA.NameText.text;
                if (!player || !player.Data || !player.Data.Role)
                {
                    var data = EndGamePatches.ContainedMeetingData.PlayerMeetingRecords.FirstOrDefault(x => x.PlayerId == playerVA.TargetPlayerId);
                    if (data != null)
                    {
                        EndGamePatches.ContainedMeetingData.DisplayRecordData(
                            playerVA.NameText,
                            data,
                            LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.ColorPlayerNameToggle.Value,
                            PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow);
                    }
                    playerVA.NameText.fontSize = 2f;
                    playerVA.NameText.ForceMeshUpdate();
                    if (playerVA.NameText.m_lineNumber > 1)
                    {
                        playerVA.NameText.fontSize = 2f - playerVA.NameText.m_lineNumber * 0.075f;
                    }
                    continue;
                }

                if (player.Data?.Disconnected == true)
                {
                    EndGamePatches.ContainedMeetingData.AddPlayerData(player);
                    // don't wanna leak info!
                    continue;
                }

                var (playerColor, playerName) = GetRoleNameText(player, genOpt.FFAImpostorMode, taskOpt, roleNameSize, roleOnTop, colorPlayerNames, localDead, localGhost, localImp, localVamp, useMiraApiChecks, true);

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

            HasSetMeetingColorText = true;
        }
        else
        {
            HasSetMeetingColorText = false;
            var stats = GameHistory.PlayerStats[PlayerControl.LocalPlayer.PlayerId];
            var isVisible = (!stats.DiedThisRound) || TutorialManager.InstanceExists;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || !player.Data || !player.Data.Role)
                {
                    continue;
                }

                var (playerColor, playerName) = GetRoleNameText(player, genOpt.FFAImpostorMode, taskOpt, roleNameSize, roleOnTop, colorPlayerNames, localDead, localGhost, localImp, localVamp, useMiraApiChecks, false, isVisible);

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

    internal static (Color PlayerColor, string PlayerName) GetRoleNameText(PlayerControl player, bool isImpFfa, PostmortemOptions taskOpt, string roleNameSize, bool roleOnTop, bool colorPlayerNames, bool localDead, bool localGhost, bool localImp, bool localVamp, bool useMiraApiChecks, bool inMeeting, bool isVisible = true, bool removeCod = false)
    {
        // End Shared of loop
        if (!inMeeting && localGhost)
        {
            localGhost = isVisible;
        }

        var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

        var playerName = player.GetAppearance().PlayerName ?? "Unknown";
        var playerColor = Color.white;

        if (colorPlayerNames && PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
            !player.AmOwner && !isImpFfa)
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
            roleName = $"<size={roleNameSize}>{MiscUtils.GetToggledRoleTmpIcon(role, HudManagerPatches.IconOnRoleName)}{color.ToTextColor()}{role.GetRoleName()}</color></size>";

            if (role.Role is RoleTypes.GuardianAngel)
            {
                roleName = $"<size={roleNameSize}>{MiscUtils.GetToggledRoleTmpIcon(role, HudManagerPatches.IconOnRoleName)}{color.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
            }

            var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
            if (revealedRole != null)
            {
                color = revealedRole.ShownRole!.TeamColor;
                roleName =
                    $"<size={roleNameSize}>{MiscUtils.GetToggledRoleTmpIcon(revealedRole.ShownRole!, HudManagerPatches.IconOnRoleName)}{color.ToTextColor()}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
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
                cache.CanDisplayForRole(role))
            {
                var cachedName = cache.CachedRoleName == "" ? MiscUtils.GetToggledRoleTmpIcon(cache.CachedRole, HudManagerPatches.IconOnRoleName) + cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                roleName = cache.ShowCurrentRoleFirst
                    ? $"<size={roleNameSize}>{MiscUtils.GetToggledRoleTmpIcon(role, HudManagerPatches.IconOnRoleName)}{color.ToTextColor()}{role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                    : $"<size={roleNameSize}>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({MiscUtils.GetToggledRoleTmpIcon(role, HudManagerPatches.IconOnRoleName)}{color.ToTextColor()}{role.GetRoleName()}</color>)</size>";
            }

            var stats = GameHistory.PlayerStats[player.PlayerId];
            if (removeCod)
            {
                topText += "<cod>\n";
            }
            else if (localDead && isVisible &&
                stats.PlayerState != StoredPlayerState.Alive)
            {
                topText +=
                    $"<size={(inMeeting ? 60 : 75)}%>『{Color.yellow.ToTextColor()}{stats.DeathString}</color>』</size>\n";
            }
        }
        else if (removeCod)
        {
            topText += "<cod>\n";
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

        if (inMeeting)
        {
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
        }
        else
        {
            if (player.AmOwner && player.TryGetModifier<ScatterModifier>(out var scatter) && !player.HasDied())
            {
                roleName += $" - {scatter.GetDescription()}";
            }
        }

        var addedPlayerNameText = revealMods.FirstOrDefault(x =>
            x.Visible && x.ExtraNameText != string.Empty && x is not FirstRoundIndicator);
        if (addedPlayerNameText != null)
        {
            playerName += addedPlayerNameText.ExtraNameText;
        }

        var diedR1Text = GetDiedR1ExtraNameTextForDisplayedIdentity(player, true);
        if (!string.IsNullOrEmpty(diedR1Text))
        {
            bottomText += diedR1Text;
        }

        if (inMeeting)
        {
            if (HaunterRole.HaunterVisibilityFlag(player))
            {
                playerColor = TownOfUsColors.HaunterRevealed;
                color = TownOfUsColors.HaunterRevealed;
            }
        }
        else
        {
            if (player.AmOwner && player.Data.Role is IGhostRole { GhostActive: true })
            {
                playerColor = Color.clear;
            }
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

        return (playerColor, playerName);
    }
}