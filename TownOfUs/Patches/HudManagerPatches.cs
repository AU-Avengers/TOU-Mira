using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
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
using TownOfUs.Utilities;
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
    public static AspectPosition ZoomAspectPos;
    public static GameObject WikiButton;
    public static AspectPosition WikiAspectPos;
    public static GameObject RoleList;
    public static TextMeshPro RoleListTextComp;
    public static GameObject SubmergedFloorButton;

    public static bool Zooming;
    public static bool CamouflageCommsEnabled;

    private static readonly Dictionary<byte, Vector3> _colorBlindBasePos = new();

    private static void RefreshUIAnchors() // anchoring ui elements.
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
            Camera.main.orthographicSize = size; // setting size for the main camera

        if (instance.UICamera != null)
            instance.UICamera.orthographicSize =
                size; // setting size for the ui camera as well. thanku pietro for the help :)

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

        RefreshUIAnchors(); // new function. Don't ban me :sob:
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
        // Ensure built-in chats are registered
        TeamChatPatches.TeamChatManager.RegisterBuiltInChats();

        var availableChats = TeamChatPatches.TeamChatManager.GetAllAvailableChats();
        var isValid = MeetingHud.Instance != null && availableChats.Count > 0;

        // Don't show team chat button for lover chat (it's handled separately outside meetings)
        // Lover chat is always active when available and doesn't need the team chat button

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
        // Camo comms
        if (!TownOfUsMapOptions.IsCamoCommsOn())
        {
            return false;
        }

        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) ||
            commsSystem == null)
        {
            return false;
        }

        var isActive = false;

        if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq || ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
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
            /*if (!CamouflageFootsteps)
            {
                foreach (var steps in ModifierUtils.GetActiveModifiers<FootstepsModifier>()
                             .Select(mod => mod._currentSteps))
                {
                    if (steps != null && steps.Count > 0)
                    {
                        steps.DoIf(x => x.Key, x => x.Value.color = new Color(0.2f, 0.2f, 0.2f, x.Value.color.a));
                    }
                }
            }

            CamouflageFootsteps = true;*/
            CamouflageCommsEnabled = true;

            foreach (var fakePlayer in FakePlayer.FakePlayers)
            {
                fakePlayer.Camo();
            }

            return;
        }

        /*if (CamouflageFootsteps)
        {
            CamouflageFootsteps = false;

            foreach (var mod in ModifierUtils.GetActiveModifiers<FootstepsModifier>())
            {
                if (mod._currentSteps != null && mod._currentSteps.Count > 0)
                {
                    mod._currentSteps.DoIf(x => x.Key,
                        x => x.Value.color = new Color(mod._footstepColor.r, mod._footstepColor.g, mod._footstepColor.b,
                            x.Value.color.a));
                }
            }
        }*/

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

        if (MeetingHud.Instance)
        {
            foreach (var playerVA in MeetingHud.Instance.playerStates)
            {
                if (!playerVA.gameObject.active)
                {
                    // Spectators are hidden, and we try to avoid Linq methods for performance reasons
                    continue;
                }
                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
                playerVA.ColorBlindName.transform.localPosition = new Vector3(-0.93f, -0.2f, -0.1f);

                if (player == null || player.Data == null || player.Data.Role == null)
                {
                    continue;
                }

                var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

                var playerName = player.GetDefaultAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
                    PlayerControl.LocalPlayer != player && !genOpt.FFAImpostorMode)
                {
                    playerColor = Color.red;
                }

                playerColor = playerColor.UpdateTargetColor(player);
                playerName = playerName.UpdateTargetSymbols(player);
                playerName = playerName.UpdateProtectionSymbols(player);
                playerName = playerName.UpdateAllianceSymbols(player);
                playerName = playerName.UpdateStatusSymbols(player);

                var role = player.Data.Role;

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

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() && genOpt is
                        { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole && role is VampireRole) ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                    FairyRole.FairySeesRoleVisibilityFlag(player) ||
                    SleuthModifier.SleuthVisibilityFlag(player) ||
                    revealMods.Any(x => x.Visible && x.RevealRole))
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

                    if (!player.HasModifier<VampireBittenModifier>() && role is VampireRole)
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.GetRoleName()}</color>)</size>"
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.GetRoleName()}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole)
                    {
                        roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead &&
                                                                        role.Role is RoleTypes.CrewmateGhost
                                                                            or RoleTypes.ImpostorGhost))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.GetRoleName()}</color></size>";
                        if (PlayerControl.LocalPlayer.HasDied() && !player.HasModifier<VampireBittenModifier>() &&
                            roleWhenAlive is VampireRole)
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                        }

                        if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                        {
                            roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                        }
                    }

                    if (PlayerControl.LocalPlayer.HasDied() &&
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
                     (PlayerControl.LocalPlayer.HasDied() && taskOpt.ShowTaskDead)) &&
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
                    if (!((PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() && genOpt is
                              { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                          (PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole && role is VampireRole) ||
                          (!TutorialManager.InstanceExists &&
                           ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                            FairyRole.FairySeesRoleVisibilityFlag(player) ||
                            SleuthModifier.SleuthVisibilityFlag(player) ||
                            revealMods.Any(x => x.Visible && x.RevealRole)))))
                    {
                        roleName = "";
                        color = Color.white;
                        playerColor = Color.white;
                    }

                    var dash = "";
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        dash = " - ";
                    }

                    roleName = $"{roleName}<size=80%>{dash}Disconnected</size>";
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ColorPlayerNameToggle.Value)
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

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.Role == null)
                {
                    continue;
                }

                var revealMods = player.GetModifiers<BaseRevealModifier>().ToList();

                var playerName = player.GetAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() &&
                    PlayerControl.LocalPlayer != player && !genOpt.FFAImpostorMode)
                {
                    playerColor = Color.red;
                }

                playerColor = playerColor.UpdateTargetColor(player, !isVisible);
                playerName = playerName.UpdateTargetSymbols(player, !isVisible);
                playerName = playerName.UpdateProtectionSymbols(player, !isVisible);
                playerName = playerName.UpdateAllianceSymbols(player, !isVisible);
                playerName = playerName.UpdateStatusSymbols(player, !isVisible);

                var role = player.Data.Role;
                var color = Color.white;

                if (role == null)
                {
                    continue;
                }

                var roleName = "";
                var canSeeDeathReason = false;
                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostorAligned() && player.IsImpostorAligned() && genOpt is
                        { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole && role is VampireRole) ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && isVisible) ||
                    FairyRole.FairySeesRoleVisibilityFlag(player) ||
                    revealMods.Any(x => x.Visible && x.RevealRole))
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

                    if (!player.HasModifier<VampireBittenModifier>() && player.Data.Role is VampireRole)
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible &&
                        player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        roleName = cache.ShowCurrentRoleFirst
                            ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.GetRoleName()}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.GetRoleName()}</color>)</size>"
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.GetRoleName()}</color> ({color.ToTextColor()}{player.Data.Role.GetRoleName()}</color>)</size>";
                    }

                    if (player.HasModifier<AmbassadorRetrainedModifier>() && player.IsImpostorAligned())
                    {
                        roleName += "<size=80%><color=#FFFFFF> (<color=#D63F42>Retrained</color>)</color></size>";
                    }

                    if (player.Data.IsDead && role is GuardianAngelRole gaRole)
                    {
                        roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{TranslationController.Instance.GetString(StringNames.GuardianAngelRole)}</color></size>";
                    }

                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead &&
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

                    if (PlayerControl.LocalPlayer.HasDied() && isVisible &&
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

                if (((taskOpt.ShowTaskRound && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() &&
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
                    playerName = LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ColorPlayerNameToggle.Value
                        ? $"{roleName}\n{color.ToTextColor()}{playerName}</color>"
                        : $"{roleName}\n{playerName}";
                }

                player.cosmetics.nameText.text = playerName;
                player.cosmetics.nameText.color = playerColor;

                // Keep the name position consistent with vanilla/Tou defaults.
                player.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);

                // Avoid overlap between the "(Died R1)" line and the color-blind label (e.g. "Orange"),
                // especially when a player is morphing/mimicking a First-Round-Death player.
                // Avoid overlap between "(Died R1)" and the color-blind label (e.g. "Pink"/"Orange") WITHOUT drift:
                // we keep a stable baseline position per-player and apply an idempotent offset from it.
                var cbId = player.PlayerId;
                var cbCurrent = player.cosmetics.colorBlindText.transform.localPosition;
                var cbOffset = Vector3.down * 0.12f;

                if (!_colorBlindBasePos.TryGetValue(cbId, out var cbBase))
                {
                    // If we first see the player while DiedR1 is visible, infer baseline by reversing our offset.
                    cbBase = string.IsNullOrEmpty(diedR1Text) ? cbCurrent : cbCurrent - cbOffset;
                    _colorBlindBasePos[cbId] = cbBase;
                }
                else if (string.IsNullOrEmpty(diedR1Text))
                {
                    // If something else moved the colorblind label while DiedR1 is NOT visible, accept that as the new baseline.
                    // (But ignore our own previously-applied offset positions.)
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
                    rolelistBuilder.Append(StoredRoleList);
                    rolelistBuilder.Append(":</color>\n");
                    for (var i = 0; i < maxSlots; i++)
                    {
                        var slotValue = i switch
                        {
                            0 => list.Slot1,
                            1 => list.Slot2,
                            2 => list.Slot3,
                            3 => list.Slot4,
                            4 => list.Slot5,
                            5 => list.Slot6,
                            6 => list.Slot7,
                            7 => list.Slot8,
                            8 => list.Slot9,
                            9 => list.Slot10,
                            10 => list.Slot11,
                            11 => list.Slot12,
                            12 => list.Slot13,
                            13 => list.Slot14,
                            14 => list.Slot15,
                            _ => -1
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

            RoleListTextComp.text = rolelistBuilder.ToString();
            RoleList.SetActive(true);
        }
    }

    public static void CreateZoomButton(HudManager instance)
    {
        var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;

        if (!ZoomButton)
        {
            ZoomButton = Object.Instantiate(instance.MapButton.gameObject, instance.MapButton.transform.parent);
            ZoomButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
            ZoomButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(ButtonClickZoom));
            ZoomButton.name = "ZoomButton";
            ZoomButton.transform.Find("Background").localPosition = Vector3.zero;
            ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinus.LoadAsset();
            ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinusActive.LoadAsset();
            ZoomAspectPos = ZoomButton.GetComponentInChildren<AspectPosition>();
        }

        if (ZoomButton)
        {
            var distanceFromEdge = ZoomAspectPos.DistanceFromEdge;
            distanceFromEdge.x = isChatButtonVisible ? 2.73f : 2.15f;
            distanceFromEdge.y = 0.485f;
            ZoomAspectPos.DistanceFromEdge = distanceFromEdge;
            ZoomAspectPos.AdjustPosition();
        }
    }

    public static void UpdateSubmergedButtons(HudManager instance)
    {
        if (ModCompatibility.IsSubmerged())
        {
            if (!SubmergedFloorButton)
            {
                var transform = instance.MapButton.transform.parent.Find(instance.MapButton.name + "(Clone)");
                if (transform != null)
                {
                    SubmergedFloorButton = transform.gameObject;
                }
            }
            if (SubmergedFloorButton && PlayerControl.LocalPlayer.Data.Role is IGhostRole ghost)
            {
                SubmergedFloorButton.SetActive(ghost.Caught);
            }
        }
    }

    public static void CreateWikiButton(HudManager instance)
    {
        var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;

        if (!WikiButton)
        {
            WikiButton = Object.Instantiate(instance.MapButton.gameObject, instance.MapButton.transform.parent);
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

            WikiButton.transform.Find("Background").localPosition = Vector3.zero;
            WikiButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButton.LoadAsset();
            WikiButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButtonActive.LoadAsset();
            WikiAspectPos = WikiButton.GetComponentInChildren<AspectPosition>();
        }

        if (WikiButton)
        {
            var distanceFromEdge = WikiAspectPos.DistanceFromEdge;
            distanceFromEdge.x = isChatButtonVisible ? 2.73f : 2.15f;

            if ((ModCompatibility.IsWikiButtonOffset || ZoomButton.active) &&
                !MeetingHud.Instance /*  && Minigame.Instance == null */ &&
                (PlayerJoinPatch.SentOnce || TutorialManager.InstanceExists))
            {
                distanceFromEdge.x += 0.84f;
            }

            distanceFromEdge.y = 0.485f;
            WikiButton.SetActive(true);
            WikiAspectPos.DistanceFromEdge = distanceFromEdge;
            WikiAspectPos.AdjustPosition();
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

        CreateZoomButton(__instance);
        CreateWikiButton(__instance);

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
         (TutorialManager.InstanceExists && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ZoomingInPractice.Value) ||
         (GameStartManager.InstanceExists && LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.ZoomingInLobby.Value)) && !(HudManager.Instance.GameMenu.IsOpen ||
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
        List<string> roleBuckets =
        [
            "CommonCrew",
            "RandomCrew",
            "CrewInvestigative",
            "CrewKilling",
            "CrewProtective",
            "CrewPower",
            "CrewSupport",
            "SpecialCrew",
            "NonImp",
            "CommonNeutral",
            "SpecialNeutral",
            "WildcardNeutral",
            "RandomNeutral",
            "NeutralBenign",
            "NeutralEvil",
            "NeutralKilling",
            "NeutralOutlier",
            "CommonImp",
            "RandomImp",
            "ImpConcealing",
            "ImpKilling",
            "ImpPower",
            "ImpSupport",
            "SpecialImp",
            "Any"
        ];
        foreach (var bucket in roleBuckets)
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
    }
}
