using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using ModCompatibility = TownOfUs.Modules.ModCompatibility;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class HudManagerPatches
{
    public static GameObject ZoomButton;
    public static GameObject WikiButton;
    public static GameObject ModifierDisplayObject;
    public static bool ModifierDisplayOnRight;
    public static GameObject ClonedChatButton;
    public static GameObject ExtraUiTopRight;
    public static GridArrange ExtraUiGrid;
    public static AspectPosition ExtraUiAspectPos;
    public static GameObject UiTopRight;
    public static GridArrange UiGrid;
    public static AspectPosition UiAspectPos;
    public static GameObject RoleList;
    public static string RoleListPrefixText = string.Empty;
    public static TextMeshPro RoleListTextComp;
    public static GameObject SubmergedFloorButton;
    public static SpriteRenderer SubmergedFloorButtonRenderer;
    public static SpriteRenderer SubmergedFloorButtonRendererHover;
    public static bool IsHoveringRoleList;

    public static bool Zooming;
    public static bool CamouflageCommsEnabled;

    private static readonly Dictionary<byte, Vector3> _colorBlindBasePos = new();

    private static void RefreshUIAnchors()
    {
        ResolutionManager.ResolutionChanged.Invoke(
            (float)Screen.width / Screen.height,
            Screen.width,
            Screen.height,
            Screen.fullScreen
        );

        foreach (var ap in Object.FindObjectsOfType<AspectPosition>())
            ap.AdjustPosition();
    }

    public static void AdjustCameraSize(float size)
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        var instance = HudManager.Instance;
        if (Camera.main != null)
            Camera.main.orthographicSize = size;

        if (instance.UICamera != null)
            instance.UICamera.orthographicSize = size;

        if (size <= 3f)
        {
            Zooming = false;
            instance.ShadowQuad.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead);
        }
        else
        {
            Zooming = true;
            instance.ShadowQuad.gameObject.SetActive(false);
        }

        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
            Zooming ? TouAssets.ZoomPlus.LoadAsset() : TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
            Zooming ? TouAssets.ZoomPlusActive.LoadAsset() : TouAssets.ZoomMinusActive.LoadAsset();

        RefreshUIAnchors();
    }

    public static void ButtonClickZoom()
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ZoomButton.SetActive(false);
            return;
        }

        AdjustCameraSize(!Zooming ? 12f : 3f);
    }

    public static void ScrollZoom(bool zoomOut = false)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ZoomButton.SetActive(false);
            return;
        }

        var size = Camera.main!.orthographicSize;
        size = zoomOut ? size * 1.25f : size / 1.25f;
        size = Mathf.Clamp(size, 3, 15);
        if (Camera.main!.orthographicSize == size)
        {
            return;
        }

        AdjustCameraSize(size);
    }

    public static void ResetZoom()
    {
        ZoomButton.SetActive(false);
        AdjustCameraSize(3f);
    }

    public static void CheckForScrollZoom()
    {
        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        var axisRaw = ConsoleJoystick.player.GetAxisRaw(55);

        if (scrollWheel == 0 && Input.touchCount < 2 && axisRaw == 0)
        {
            return;
        }
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentTouchDeltaMag = (touch0.position - touch1.position).magnitude;
            float deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

            switch (deltaMagnitudeDiff)
            {
                case > 0:
                {
                    ScrollZoom();
                    break;
                }
                case < 0:
                {
                    ScrollZoom(true);
                    break;
                }
            }
        }

        if (scrollWheel > 0 || axisRaw > 0)
        {
            ScrollZoom();
        }
        else if (scrollWheel < 0 || axisRaw < 0)
        {
            ScrollZoom(true);
        }
    }

    public static void UpdateTeamChat()
    {
        TeamChatPatches.TeamChatManager.RegisterBuiltInChats();

        var availableChats = TeamChatPatches.TeamChatManager.GetAllAvailableChats();
        var isValid = MeetingHud.Instance != null && availableChats.Count > 0;

        if (!TeamChatPatches.TeamChatButton)
        {
            return;
        }

        if (TeamChatPatches.TeamChatActive && !isValid && HudManager.Instance.Chat.IsOpenOrOpening)
        {
            TeamChatPatches.TeamChatActive = false;
            TeamChatPatches.CurrentChatIndex = -1;
            TeamChatPatches.UpdateChat();
        }

        TeamChatPatches.TeamChatButton.SetActive(isValid);
    }

    public static bool CommsSaboActive()
    {
        if (!TownOfUsMapOptions.IsCamoCommsOn())
        {
            return false;
        }

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

    public static void UpdateCamouflageComms()
    {
        var isActive = CommsSaboActive();
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
            CamouflageCommsEnabled = true;

            foreach (var fakePlayer in FakePlayer.FakePlayers)
            {
                fakePlayer.Camo();
            }

            return;
        }

        if (CamouflageCommsEnabled)
        {
            CamouflageCommsEnabled = false;
            FakePlayer.FakePlayers.Do(x => x.UnCamo());
        }
    }

    public static void UpdateRoleNameText()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var taskOpt = OptionGroupSingleton<TaskTrackingOptions>.Instance;

        static PlayerControl GetDisguiseTargetOrSelf(PlayerControl player)
        {
            if (player.TryGetModifier<MorphlingMorphModifier>(out var morph) && morph.Target != null)
            {
                return morph.Target;
            }

            if (player.TryGetModifier<GlitchMimicModifier>(out var mimic) && mimic.Target != null)
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
                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
                playerVA.ColorBlindName.transform.localPosition = new Vector3(-0.93f, -0.2f, -0.1f);

                if (player == null || player.Data == null || player.Data.Role == null)
                {
                    var data = EndGamePatches.ContainedMeetingData.PlayerMeetingRecords.FirstOrDefault(x => x.PlayerId == playerVA.TargetPlayerId);
                    if (data != null)
                    {
                        EndGamePatches.ContainedMeetingData.DisplayRecordData(playerVA.NameText, data, colorPlayerNames, localGhost);
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

                var role = player.Data.Role;
                var customRole = player.Data.Role as ICustomRole;

                if (role == null)
                {
                    continue;
                }

                var color = role.TeamColor;

                if (HaunterRole.HaunterVisibilityFlag(player))
                {
                    playerColor = color;
                }

                color = Color.white;

                var roleName = "";

                var impostorBuddy = localImp && player.IsImpostorAligned();
                var vampBuddy = localVamp && role is VampireRole;
                var revealed = revealMods.Any(x => x.Visible && x.RevealRole);
                var localFairy = FairyRole.FairySeesRoleVisibilityFlag(player);
                var localSleuth = SleuthModifier.SleuthVisibilityFlag(player);
                if (player.AmOwner || vampBuddy || impostorBuddy || revealed || localGhost || localFairy || localSleuth || useMiraApiChecks && customRole != null && customRole.CanLocalPlayerSeeRole(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color></size>";

                    var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole != null)
                    {
                        color = revealedRole.ShownRole!.TeamColor;
                        roleName =
                            $"<size=80%>{color.ToTextColor()}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole && (vampBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && (impostorBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        var cachedName = cache.CachedRoleName == "" ? cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole)
                    {
                        roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (localSleuth || (player.Data.IsDead &&
                                        role.Role is RoleTypes.CrewmateGhost
                                            or RoleTypes.ImpostorGhost))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.GetRoleName()}</color></size>";
                        if (localDead && !player.HasModifier<VampireBittenModifier>() &&
                            roleWhenAlive is VampireRole)
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                        }

                        if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                        }
                    }

                    if (localDead &&
                        player.TryGetModifier<DeathHandlerModifier>(out var deathMod))
                    {
                        var deathReason =
                            $"<size=60%>『{Color.yellow.ToTextColor()}{deathMod.CauseOfDeath}</color>』</size>\n";

                        roleName = $"{deathReason}{roleName}";
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
                    roleName += $"<size=80%>{addedRoleNameText.ExtraRoleText}</size>";
                }

                if (((taskOpt.ShowTaskInMeetings && player.AmOwner) ||
                     (localDead && taskOpt.ShowTaskDead)) &&
                    (player.IsCrewmate() || player.Data.Role is SpectreRole))
                {
                    if (roleName != string.Empty)
                    {
                        roleName += " ";
                    }

                    roleName += $"<size=80%>{player.TaskInfo()}</size>";
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

                    playerName += revealText;
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
                    playerName += diedR1Text;
                }

                if (player.Data?.Disconnected == true)
                {
                    EndGamePatches.ContainedMeetingData.AddPlayerData(player);
                    // don't wanna leak info!
                    continue;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (colorPlayerNames)
                    {
                        playerName = $"{roleName}\n{color.ToTextColor()}<size=92%>{playerName}</size></color>";
                    }
                    else
                    {
                        playerName = $"{roleName}\n<size=92%>{playerName}</size>";
                    }
                }

                playerVA.NameText.text = playerName;
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
                if (player == null || player.Data == null || player.Data.Role == null)
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

                playerColor = playerColor.UpdateTargetColor(player, !isVisible);
                playerName = playerName.UpdateTargetSymbols(player, !isVisible);
                playerName = playerName.UpdateProtectionSymbols(player, !isVisible);
                playerName = playerName.UpdateAllianceSymbols(player, !isVisible);
                playerName = playerName.UpdateStatusSymbols(player, !isVisible);

                var role = player.Data.Role;
                var customRole = player.Data.Role as ICustomRole;
                var color = Color.white;

                if (role == null)
                {
                    continue;
                }

                var roleName = "";
                var canSeeDeathReason = false;
                var impostorBuddy = localImp && player.IsImpostorAligned();
                var vampBuddy = localVamp && role is VampireRole;
                var revealed = revealMods.Any(x => x.Visible && x.RevealRole);
                var localFairy = FairyRole.FairySeesRoleVisibilityFlag(player);
                var localSleuth = SleuthModifier.SleuthVisibilityFlag(player);
                if (player.AmOwner || vampBuddy || impostorBuddy || revealed || localGhost || localFairy || localSleuth || useMiraApiChecks && customRole != null && customRole.CanLocalPlayerSeeRole(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color></size>";

                    var revealedRole = revealMods.FirstOrDefault(x => x.Visible && x.RevealRole && x.ShownRole != null);
                    if (revealedRole != null)
                    {
                        color = revealedRole.ShownRole!.TeamColor;
                        roleName =
                            $"<size=80%>{color.ToTextColor()}{revealedRole.ShownRole!.GetRoleName()}</color></size>";
                    }

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole && (vampBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && (impostorBuddy || localGhost))
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        var cachedName = cache.CachedRoleName == "" ? cache.CachedRole.GetRoleName() : cache
                            .CachedRoleName;
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color>)</size>"
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cachedName}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole)
                    {
                        roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (localSleuth || (player.Data.IsDead &&
                                        role.Role is RoleTypes.CrewmateGhost
                                            or RoleTypes.ImpostorGhost))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.GetRoleName()}</color></size>";
                        if (!player.HasModifier<VampireBittenModifier>() && roleWhenAlive is VampireRole)
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                        }

                        if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                        }
                    }

                    if (localDead && isVisible &&
                        player.TryGetModifier<DeathHandlerModifier>(out var deathMod))
                    {
                        var deathReason =
                            $"<size=75%>『{Color.yellow.ToTextColor()}{deathMod.CauseOfDeath}</color>』</size>\n";

                        roleName = $"{deathReason}{roleName}";
                        canSeeDeathReason = true;
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
                    roleName += $"<size=80%>{addedRoleNameText.ExtraRoleText}</size>";
                }

                if (((taskOpt.ShowTaskRound && player.AmOwner) || (localDead &&
                                                                   taskOpt.ShowTaskDead && isVisible)) &&
                    (player.IsCrewmate() ||
                     player.Data.Role is SpectreRole))
                {
                    if (roleName != string.Empty)
                    {
                        roleName += " ";
                    }

                    roleName += $"<size=80%>{player.TaskInfo()}</size>";
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
                    playerName += diedR1Text;
                }

                if (canSeeDeathReason)
                {
                    playerName += $"\n<size=75%> </size>";
                }

                if (player.AmOwner && player.Data.Role is IGhostRole { GhostActive: true })
                {
                    playerColor = Color.clear;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    playerName = colorPlayerNames
                        ? $"{roleName}\n{color.ToTextColor()}{playerName}</color>"
                        : $"{roleName}\n{playerName}";
                }

                player.cosmetics.nameText.text = playerName;
                player.cosmetics.nameText.color = playerColor;

                player.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);

                var cbId = player.PlayerId;
                var cbCurrent = player.cosmetics.colorBlindText.transform.localPosition;
                var cbOffset = Vector3.down * 0.12f;

                if (!_colorBlindBasePos.TryGetValue(cbId, out var cbBase))
                {
                    cbBase = string.IsNullOrEmpty(diedR1Text) ? cbCurrent : cbCurrent - cbOffset;
                    _colorBlindBasePos[cbId] = cbBase;
                }
                else if (string.IsNullOrEmpty(diedR1Text))
                {
                    var cbExpectedNoR1 = cbBase;
                    var cbExpectedR1 = cbBase + cbOffset;
                    if ((cbCurrent - cbExpectedNoR1).sqrMagnitude > 0.0001f &&
                        (cbCurrent - cbExpectedR1).sqrMagnitude > 0.0001f)
                    {
                        cbBase = cbCurrent;
                        _colorBlindBasePos[cbId] = cbBase;
                    }
                }

                player.cosmetics.colorBlindText.transform.localPosition =
                    string.IsNullOrEmpty(diedR1Text) ? cbBase : cbBase + cbOffset;
            }
        }

        if (HudManager.Instance.TaskPanel != null)
        {
            var tabText = HudManager.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP")
                .GetComponent<TextMeshPro>();
            tabText.SetText($"{StoredTasksText} {PlayerControl.LocalPlayer.TaskInfo()}");
        }
    }

    public static string GetRoleForSlot(RoleListOption slotValue)
    {
        var newVal = (int)slotValue;
        var roleListText = RoleOptions.OptionStrings;
        if (newVal >= 0 && newVal < roleListText.Length)
        {
            return roleListText[newVal];
        }

        return "<color=#696969>???</color>";
    }

    public static string GetRoleForSlot(int slotValue)
    {
        var roleListText = RoleOptions.OptionStrings;
        if (slotValue >= 0 && slotValue < roleListText.Length)
        {
            return roleListText[slotValue];
        }

        return "<color=#696969>???</color>";
    }

    public static void UpdateRoleList(HudManager instance)
    {
        if (!LobbyBehaviour.Instance)
        {
            if (RoleList)
            {
                RoleList.SetActive(false);
            }

            return;
        }

        var roleAssignmentType = OptionGroupSingleton<RoleOptions>.Instance.CurrentRoleDistribution();

        if (CancelCountdownStart.CancelStartButton && AmongUsClient.Instance.AmHost)
        {
            CancelCountdownStart.CancelStartButton.gameObject.SetActive(
                GameStartManager.Instance.startState is GameStartManager.StartingStates.Countdown);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null)
            {
                continue;
            }

            var playerName = player.Data.PlayerName ?? "Unknown";
            
            var isHost = player.IsHost();
            var isTrackedSpectator = SpectatorRole.TrackedSpectators.Contains(playerName);

            if (isHost || isTrackedSpectator)
            {
                var playerInfoSb = new StringBuilder("<size=80%>");

                if (isHost)
                {
                    playerInfoSb.Append(TownOfUsColors.Jester.ToTextColor());
                    playerInfoSb.Append(StoredHostLocale);
                    if (isTrackedSpectator)
                    {
                        playerInfoSb.Append(' ');
                    }
                }

                if (isTrackedSpectator)
                {
                    playerInfoSb.Append(Color.yellow.ToTextColor());
                    playerInfoSb.Append('(');
                    playerInfoSb.Append(StoredSpectatingLocale);
                    playerInfoSb.Append(")</color>");
                }

                if (isHost)
                { 
                    playerInfoSb.Append("</color>");
                }
                playerInfoSb.Append("</size>\n");
                playerInfoSb.Append(playerName);

                playerName = playerInfoSb.ToString();
            }

            var playerColor = Color.white;

            player.cosmetics.nameText.text = playerName;
            player.cosmetics.nameText.color = playerColor;
            player.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);
        }

        if (!RoleList)
        {
            var pingTracker = Object.FindObjectOfType<PingTracker>(true);
            RoleList = Object.Instantiate(pingTracker.gameObject, instance.transform);
            RoleList.name = "RoleListText";
            var pos = RoleList.gameObject.GetComponent<AspectPosition>();
            pos.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            pos.DistanceFromEdge = new Vector3(0.43f, 0.1f, 1f);

            RoleListTextComp = RoleList.GetComponent<TextMeshPro>();
            RoleListTextComp.alignment = TextAlignmentOptions.TopLeft;
            RoleListTextComp.verticalAlignment = VerticalAlignmentOptions.Top;
            RoleListTextComp.fontSize = RoleListTextComp.fontSizeMin = RoleListTextComp.fontSizeMax = 3f;
            RoleList.SetActive(false);
            var hoverComp = instance.gameObject.GetComponent<RoleListHoverComponent>()
                         ?? instance.gameObject.AddComponent<RoleListHoverComponent>();
            hoverComp.TextTarget = RoleListTextComp;
        }
        else
        {
            RoleList.SetActive(false);
            if (roleAssignmentType is not RoleDistribution.RoleList && roleAssignmentType is not RoleDistribution.MinMaxList)
            {
                return;
            }

            var rolelistBuilder = new StringBuilder("<color=#FFD700>");
            var players = GameData.Instance.PlayerCount - SpectatorRole.TrackedSpectators.Count;
            var maxSlots = players < 15 ? players : 15;

            var list = OptionGroupSingleton<RoleOptions>.Instance;
            switch (roleAssignmentType)
            {
                case RoleDistribution.RoleList:
                    rolelistBuilder.Append(RoleListPrefixText);
                    rolelistBuilder.Append(StoredRoleList);
                    rolelistBuilder.Append(":</color>\n");
                    for (var i = 0; i < maxSlots; i++)
                    {
                        var slotValue = i switch
                        {
                            0 => list.Slot1.Value,
                            1 => list.Slot2.Value,
                            2 => list.Slot3.Value,
                            3 => list.Slot4.Value,
                            4 => list.Slot5.Value,
                            5 => list.Slot6.Value,
                            6 => list.Slot7.Value,
                            7 => list.Slot8.Value,
                            8 => list.Slot9.Value,
                            9 => list.Slot10.Value,
                            10 => list.Slot11.Value,
                            11 => list.Slot12.Value,
                            12 => list.Slot13.Value,
                            13 => list.Slot14.Value,
                            14 => list.Slot15.Value,
                            _ => (RoleListOption)(-1)
                        };

                        rolelistBuilder.AppendLine(GetRoleForSlot(slotValue));
                    }

                    break;
                case RoleDistribution.MinMaxList:
                    rolelistBuilder.Append(StoredFactionList);
                    rolelistBuilder.Append(":</color>\n");
                    var minMaxData = new (string Label, float Min, float Max)[]
                    {
                        (NeutralBenigns, list.MinNeutralBenign.Value, list.MaxNeutralBenign.Value),
                        (NeutralEvils, list.MinNeutralEvil.Value, list.MaxNeutralEvil.Value),
                        (NeutralKillers, list.MinNeutralKiller.Value, list.MaxNeutralKiller.Value),
                        (NeutralOutliers, list.MinNeutralOutlier.Value, list.MaxNeutralOutlier.Value)
                    };
                    
                    foreach (var (label, min, max) in minMaxData)
                    {
                        rolelistBuilder.Append(label);
                        rolelistBuilder.Append(": ");
                        rolelistBuilder.Append(min);
                        rolelistBuilder.Append(' ');
                        rolelistBuilder.Append(StoredMinimum);
                        rolelistBuilder.Append(", ");
                        rolelistBuilder.Append(max);
                        rolelistBuilder.Append(' ');
                        rolelistBuilder.AppendLine(StoredMaximum);
                    }
                    break;
            }

            if (!IsHoveringRoleList)
                RoleListTextComp.text = rolelistBuilder.ToString();

            RoleList.SetActive(true);
        }
    }

    public static void CreateZoomButton(HudManager instance)
    {
        if (!ZoomButton && UiTopRight && ExtraUiTopRight)
        {
            ZoomButton = Object.Instantiate(instance.MapButton.gameObject, ExtraUiTopRight.transform);
            ZoomButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
            ZoomButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(ButtonClickZoom));
            ZoomButton.name = "ZoomButton";
            var inactive = ZoomButton.transform.Find("Inactive");
                inactive.GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinus.LoadAsset();
                inactive.localPosition = new Vector3(0, 0.021f, -0.1f);
                var active = ZoomButton.transform.Find("Active");
                active.GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinusActive.LoadAsset();
                active.localPosition = new Vector3(0, 0.021f, -0.1f);
            ZoomButton.GetComponentInChildren<AspectPosition>().Destroy();
        }
    }

    public static void UpdateSubmergedButtons(HudManager instance)
    {
        if (ModCompatibility.IsSubmerged())
        {
            if (!SubmergedFloorButton && ExtraUiTopRight)
            {
                var transform = instance.MapButton.transform.parent.Find(instance.MapButton.name + "(Clone)");
                if (transform != null)
                {
                    SubmergedFloorButton = transform.gameObject;
                    SubmergedFloorButton.transform.SetParent(ExtraUiTopRight.transform, false);

                    SubmergedFloorButtonRenderer =
                        SubmergedFloorButton.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                    SubmergedFloorButtonRendererHover =
                        SubmergedFloorButton.transform.Find("Active").GetComponent<SpriteRenderer>();
                    PassiveButton buttonBehavior = SubmergedFloorButton.GetComponent<PassiveButton>();
                    buttonBehavior.OnClick.RemoveAllListeners();
                    buttonBehavior.OnClick = new Button.ButtonClickedEvent();
                    buttonBehavior.OnClick.AddListener(new Action(ChangeSubFloor));

                    TownOfUsLocalSettings.SetUpButtonPositions();
                }
            }
            if (SubmergedFloorButton && PlayerControl.LocalPlayer.Data.Role is IGhostRole ghost)
            {
                SubmergedFloorButton.SetActive(ghost.Caught);
            }
        }
    }

    private static void ChangeSubFloor()
    {
        ModCompatibility.ChangeFloor(PlayerControl.LocalPlayer.transform.position.y <= -5);
    }

    public static Vector3 BelowOptionPos = new Vector3(0.435f, 1.25f, 65f);
    public static Vector2 FullTopPos = new Vector2(0.435f, 0.475f);
    public static void CreateUiRow(HudManager instance)
    {
        if (!UiTopRight)
        {
            UiTopRight = instance.MapButton.transform.parent.gameObject;

            UiGrid = UiTopRight.AddComponent<GridArrange>();
            UiAspectPos = UiTopRight.AddComponent<AspectPosition>();

            UiGrid.Alignment = GridArrange.StartAlign.Left;
            UiGrid.CellSize = new Vector2(0.85f, 0.85f);
            UiGrid.MaxColumns = 6;
            UiAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
            UiGrid.Start();
            UiAspectPos.DistanceFromEdge = FullTopPos;
            var mapButton = instance.MapButton.gameObject;
            mapButton.GetComponent<AspectPosition>().Destroy();
            var settingsButton = instance.SettingsButton;
            settingsButton.GetComponent<AspectPosition>().Destroy();
            var chatButton = instance.Chat.chatButton.gameObject;
            ClonedChatButton = Object.Instantiate(chatButton, chatButton.transform.parent);
            ClonedChatButton.SetActive(false);
            instance.Chat.chatButtonAspectPosition = ClonedChatButton.GetComponent<AspectPosition>();
            chatButton.GetComponent<AspectPosition>().Destroy();
            var inactivePos = settingsButton.transform.GetChild(1).transform.localPosition;
            var bg = settingsButton.transform.GetChild(2).gameObject;
            var bgPos = bg.transform.localPosition;
            var bgSprite = bg.GetComponent<SpriteRenderer>().sprite;
            var activePos = settingsButton.transform.GetChild(3).transform.localPosition;
            var selectedPos = settingsButton.transform.GetChild(4).transform.localPosition;
            chatButton.transform.GetChild(2).transform.localPosition = inactivePos;
            var chatBg = chatButton.transform.GetChild(3);
            chatBg.transform.localPosition = bgPos;
            chatBg.GetComponent<SpriteRenderer>().sprite = bgSprite;
            chatButton.transform.GetChild(4).transform.localPosition = activePos;
            chatButton.transform.GetChild(5).transform.localPosition = selectedPos;
            var collider = chatButton.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(0.4354f, 0.4003f);
            collider.offset = new Vector2(0.0025f, 0.0254f);
            if (FriendsListManager.InstanceExists && !TutorialManager.InstanceExists)
            {
                var listButton = FriendsListManager.Instance.FriendsListButton.transform.GetChild(0);
                listButton.transform.SetParent(UiTopRight.transform, false);
                FriendsListManager.Instance.FriendsListButton = listButton.GetComponent<FriendsListButton>();
                listButton.GetComponent<AspectPosition>().Destroy();
                listButton.localPosition = new Vector3(0, 0, 0);
            }
            settingsButton.transform.SetAsLastSibling();
            chatButton.transform.SetParent(UiTopRight.transform, false);
            instance.Chat.chatButton = chatButton.GetComponent<PassiveButton>();
            var iconContainer = new GameObject("iconContainer");
            iconContainer.layer = LayerMask.NameToLayer("UI");
            iconContainer.transform.SetParent(chatButton.transform, false);
            iconContainer.transform.localPosition = new Vector3(0.1f, -0.1f, 0);
            instance.Chat.chatNotifyDot.transform.SetParent(iconContainer.transform, false);
            instance.Chat.chatNotifyDot = iconContainer.transform.GetChild(0).GetComponent<SpriteRenderer>();
            TeamChatPatches.PublicChatDot = instance.Chat.chatNotifyDot;
            UiAspectPos.updateAlways = true;
        }

        if (UiTopRight && UiGrid)
        {
            UiGrid.ArrangeChilds();
        }
    }
    public static void CreateNewUiRow(HudManager instance)
    {
        if (!ExtraUiTopRight && UiTopRight)
        {
            ExtraUiTopRight = new GameObject("ExtraUiTopRight");
            ExtraUiTopRight.transform.SetParent(instance.MapButton.transform.parent.parent, false);

            ExtraUiGrid = ExtraUiTopRight.AddComponent<GridArrange>();
            ExtraUiAspectPos = ExtraUiTopRight.AddComponent<AspectPosition>();

            ExtraUiGrid.Alignment = GridArrange.StartAlign.Left;
            ExtraUiGrid.CellSize = new Vector2(0.85f, 0.85f);
            ExtraUiAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
            ExtraUiAspectPos.DistanceFromEdge = BelowOptionPos;
            ExtraUiGrid.Start();
            ExtraUiAspectPos.updateAlways = true;
        }

        if (ExtraUiTopRight && ExtraUiGrid)
        {
            var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;
            instance.Chat.chatButton.gameObject.SetActive(isChatButtonVisible);
            ExtraUiGrid.ArrangeChilds();
        }
    }

    public static void CreateWikiButton(HudManager instance)
    {
        if (!WikiButton && UiTopRight && ExtraUiTopRight)
        {
            WikiButton = Object.Instantiate(instance.MapButton.gameObject, ExtraUiTopRight.transform);
            WikiButton.name = "WikiButton";
            WikiButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
            WikiButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
            {
                if (Minigame.Instance)
                {
                    return;
                }

                IngameWikiMinigame.Create().Begin(null);
            }));

            var inactive = WikiButton.transform.Find("Inactive");
            inactive.GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButton.LoadAsset();
            inactive.localPosition = new Vector3(0, 0.021f, -0.1f);
            var active = WikiButton.transform.Find("Active");
            active.GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButtonActive.LoadAsset();
            active.localPosition = new Vector3(0, 0.021f, -0.1f);

            WikiButton.GetComponentInChildren<AspectPosition>().Destroy();
        }

        if (WikiButton)
        {
            WikiButton.SetActive(true);
        }
    }

    public static void AdjustModifierTab(HudManager instance)
    {
        if (!ModifierDisplayObject && UiTopRight && ExtraUiTopRight && ModifierDisplayComponent.Instance != null)
        {
            ModifierDisplayObject = ModifierDisplayComponent.Instance?.gameObject ?? null!;
            ModifierDisplayOnRight = !LocalSettingsTabSingleton<MiraApiSettings>.Instance.ModifiersHudLeftSide.Value;
            if (ModifierDisplayOnRight)
            {
                ModifierDisplayObject.transform.SetParent(ExtraUiTopRight.transform, false);
                ModifierDisplayObject.GetComponent<AspectPosition>().Destroy();
                ModifierDisplayObject.transform.GetChild(0).localPosition = new Vector3(-1.1757f, -2.1633f, -80f);
                ModifierDisplayObject.transform.GetChild(1).localPosition = new Vector3(-0.45f, 0.3f, -80f);
            }
            TownOfUsLocalSettings.SetUpButtonPositions();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
        {
            return;
        }

        CreateUiRow(__instance);
        CreateNewUiRow(__instance);

        CreateWikiButton(__instance);
        CreateZoomButton(__instance);
        AdjustModifierTab(__instance);

        UpdateRoleList(__instance);
        UpdateTeamChat();

        if (CanZoom)
        {
            CheckForScrollZoom();
        }

        if (PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }
        
        UpdateCamouflageComms();
        UpdateRoleNameText();
        UpdateSubmergedButtons(__instance);
    }

    public static bool CanZoom =>
        ((PlayerControl.LocalPlayer.DiedOtherRound() &&
          (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } ||
           PlayerControl.LocalPlayer.Data.Role is not IGhostRole)) ||
         (TutorialManager.InstanceExists && LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.ZoomingInPractice.Value) ||
         (GameStartManager.InstanceExists && LocalSettingsTabSingleton<TownOfUsLocalMiscSettings>.Instance.ZoomingInLobby.Value)) && !(HudManager.Instance.GameMenu.IsOpen ||
                                                 HudManager.Instance.Chat.IsOpenOrOpening ||
                                                 MeetingHud.Instance || Minigame.Instance ||
                                                 PlayerCustomizationMenu.Instance ||
                                                 FriendsListUI.Instance && FriendsListUI.Instance.IsOpen ||
                                                 GameStartManager.InstanceExists &&
                                                 (GameStartManager.Instance.RulesViewPanel &&
                                                  GameStartManager.Instance.RulesViewPanel.active ||
                                                  GameSettingMenu.Instance));

    private static bool _registeredSoftModifiers;
    public static string StoredTasksText { get; private set; } = "Tasks";
    public static string StoredHostLocale { get; private set; } = "Host";
    public static string StoredSpectatingLocale { get; private set; } = "Spectator";
    public static string StoredRoleList { get; private set; } = "Set Role List";
    public static string StoredFactionList { get; private set; } = "Neutral Faction List";
    public static string NeutralBenigns { get; private set; } = "Neutral Benigns";
    public static string NeutralEvils { get; private set; } = "Neutral Evils";
    public static string NeutralOutliers { get; private set; } = "Neutral Outliers";
    public static string NeutralKillers { get; private set; } = "Neutral Killers";
    public static string StoredMinimum { get; private set; } = "Min";
    public static string StoredMaximum { get; private set; } = "Max";
    internal static List<string> StoredRoleBuckets =
    [
        "CrewInvestigative",
        "CrewKilling",
        "CrewProtective",
        "CrewPower",
        "CrewSupport",

        "CommonCrew",
        "SpecialCrew",
        "RandomCrew",

        "NeutralBenign",
        "NeutralEvil",
        "NeutralKilling",
        "NeutralOutlier",

        "CommonNeutral",
        "SpecialNeutral",
        "WildcardNeutral",
        "RandomNeutral",

        "ImpConcealing",
        "ImpKilling",
        "ImpPower",
        "ImpSupport",

        "CommonImp",
        "SpecialImp",
        "RandomImp",

        "NonImp",
        "Any"
    ];

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void HudManagerStartPatch(HudManager __instance)
    {
        StoredHostLocale = TranslationController.Instance.GetString(StringNames.HostNounEmpty);
        StoredTasksText = TranslationController.Instance.GetString(StringNames.Tasks);
        StoredSpectatingLocale = TouLocale.Get("TouRoleSpectator");
        StoredRoleList = TouLocale.Get("SetRoleList");
        StoredFactionList = TouLocale.Get("NeutralFactionList");
        List<string> lists =
        [
            TouLocale.Get("NeutralBenigns"),
            TouLocale.Get("NeutralEvils"),
            TouLocale.Get("NeutralOutliers"),
            TouLocale.Get("NeutralKillers")
        ];
        List<string> listsNew = [];
        var neutKeyword = TouLocale.Get("NeutralKeyword");
        foreach (var alignment in lists)
        {
            var text = alignment;
            if (text.Contains(neutKeyword))
            {
                text = text.Replace(neutKeyword, $"<color=#8A8A8A>{neutKeyword}</color>");
            }
            else if (alignment.Contains("Neutral"))
            {
                text = text.Replace("Neutral", "<color=#8A8A8A>Neutral</color>");
            }

            listsNew.Add(text);
        }

        NeutralBenigns = listsNew[0];
        NeutralEvils = listsNew[1];
        NeutralOutliers = listsNew[2];
        NeutralKillers = listsNew[3];
        StoredMinimum = TouLocale.Get("MinimumShort");
        StoredMaximum = TouLocale.Get("MaximumShort");
        List<string> localizedRoleList = [];
        foreach (var bucket in StoredRoleBuckets)
        {
            localizedRoleList.Add(MiscUtils.GetParsedRoleBucket(bucket));
        }

        RoleOptions.OptionStrings = localizedRoleList.ToArray();
        if (!_registeredSoftModifiers)
        {
            var modifiers = MiscUtils.AllModifiers.Where(x =>
                x.ParentMod != MiraPluginManager.GetPluginByGuid("auavengers.tou.mira") && x is GameModifier &&
                x is not IWikiDiscoverable);
            foreach (var modifier in modifiers)
            {
                SoftWikiEntries.RegisterModifierEntry(modifier);
            }

            _registeredSoftModifiers = true;
        }

        MiraApiSettings.OldButtonScaleFactor =
            LocalSettingsTabSingleton<MiraApiSettings>.Instance.ButtonUIFactorSlider.Value;

        TownOfUsColors.UseBasic = false;
        BucketTooltipData.AllRoles.Clear();
        foreach (var pair in TooltipAlignments)
        {
            var allRoles = MiscUtils.GetRegisteredRoles(pair.Value).ToList();
            BucketTooltipData.RoleEntry[] roleEntry = Array.Empty<BucketTooltipData.RoleEntry>();
            foreach (var role in allRoles)
            {
                if (role.Role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost ||
                    role.Role == (RoleTypes)RoleId.Get<NeutralGhostRole>())
                {
                    continue;
                }
                // Warning($"Adding: {role.GetRoleName()}, {role.Role}, {role.GetNamespace()}");
                roleEntry = roleEntry.AddToArray(new(role.GetRoleName(), role.Role, role.GetNamespace(), role.TeamColor));
            }
            BucketTooltipData.AllRoles.Add(pair.Key, roleEntry);
        }

        TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance
            .UseCrewmateTeamColorToggle.Value;
        
        TownOfUsLocalSettings.OldButtonScaleFactor =
            LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ButtonUIFactorSlider.Value;
        Coroutines.Start(TownOfUsLocalSettings.CoResizeSettingsUI());
    }

    internal static readonly Dictionary<RoleListOption, RoleAlignment> TooltipAlignments = new()
    {
        { RoleListOption.CrewInvest, RoleAlignment.CrewmateInvestigative },
        { RoleListOption.CrewKilling, RoleAlignment.CrewmateKilling },
        { RoleListOption.CrewProtective, RoleAlignment.CrewmateProtective },
        { RoleListOption.CrewPower, RoleAlignment.CrewmatePower },
        { RoleListOption.CrewSupport, RoleAlignment.CrewmateSupport },
                
        { RoleListOption.NeutBenign, RoleAlignment.NeutralBenign },
        { RoleListOption.NeutEvil, RoleAlignment.NeutralEvil },
        { RoleListOption.NeutKilling, RoleAlignment.NeutralKilling },
        { RoleListOption.NeutOutlier, RoleAlignment.NeutralOutlier },

        { RoleListOption.ImpConceal, RoleAlignment.ImpostorConcealing },
        { RoleListOption.ImpKilling, RoleAlignment.ImpostorKilling },
        { RoleListOption.ImpPower, RoleAlignment.ImpostorPower },
        { RoleListOption.ImpSupport, RoleAlignment.ImpostorSupport },
    };

    public static string GetNamespace(this RoleBehaviour role)
    {
        if (role is ICustomRole customRole)
        {
            return customRole.GetType().FullName!;
        }

        var text = role.GetType().FullName!;
        if (Enum.IsDefined(role.Role))
        {
            text = $"AmongUs.Roles.{role.Role.ToString()}";
        }

        return text;
    }
}
