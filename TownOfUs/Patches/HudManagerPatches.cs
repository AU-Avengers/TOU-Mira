using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.DraftMode;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Patches.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
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
    public static ProgressTracking PlayerNameProgress = ProgressTracking.Always;
    public static NameStyle RoleNameStyle = NameStyle.TopSmall;
    public static bool RoleOnTop => RoleNameStyle is NameStyle.Top or NameStyle.TopSmall;
    public static bool RoleIsSmall => RoleNameStyle is NameStyle.BottomSmall or NameStyle.TopSmall;
    public static bool IconOnRoleName;
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
        Camera.main?.orthographicSize = size;

        instance.UICamera?.orthographicSize = size;

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
        var isValid = MeetingHud.Instance && availableChats.Count > 0;

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
            player.cosmetics.nameText.alignment = TextAlignmentOptions.Bottom;
        }

        if (!RoleList)
        {
            var pingTracker = Object.FindObjectOfType<PingTracker>(true);
            RoleList = Object.Instantiate(pingTracker.gameObject, instance.transform);
            RoleList.GetComponent<PingTracker>().Destroy();
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
            if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek or TouGamemode.Other || roleAssignmentType is not RoleDistribution.RoleList && roleAssignmentType is not RoleDistribution.MinMaxList && roleAssignmentType is not RoleDistribution.Draft)
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
                case RoleDistribution.Draft:
                    if (!DraftSidebarManager.IsActive)
                    {
                        var draftOpts = OptionGroupSingleton<RoleDraftRoleListOptions>.Instance;
                        var draftCrewOpts = OptionGroupSingleton<RoleDraftCrewOptions>.Instance;
                        var draftImpOpts = OptionGroupSingleton<RoleDraftImpOptions>.Instance;
                        var draftNeutOpts = OptionGroupSingleton<RoleDraftNeutOptions>.Instance;
                        rolelistBuilder.Append(StoredDraftTitle);
                        rolelistBuilder.Append(":</color>\n");
                        if (list.UseRoleListForPool.Value)
                        {
                            for (var i = 0; i < maxSlots; i++)
                            {
                                var slotValue = i switch
                                {
                                    0 => draftOpts.Slot1.Value,
                                    1 => draftOpts.Slot2.Value,
                                    2 => draftOpts.Slot3.Value,
                                    3 => draftOpts.Slot4.Value,
                                    4 => draftOpts.Slot5.Value,
                                    5 => draftOpts.Slot6.Value,
                                    6 => draftOpts.Slot7.Value,
                                    7 => draftOpts.Slot8.Value,
                                    8 => draftOpts.Slot9.Value,
                                    9 => draftOpts.Slot10.Value,
                                    10 => draftOpts.Slot11.Value,
                                    11 => draftOpts.Slot12.Value,
                                    12 => draftOpts.Slot13.Value,
                                    13 => draftOpts.Slot14.Value,
                                    14 => draftOpts.Slot15.Value,
                                    _ => (RoleListOption)(-1)
                                };

                                rolelistBuilder.AppendLine(GetRoleForSlot(slotValue));
                            }
                        }
                        else
                        {
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {Palette.CrewmateBlue.ToTextColor()}Crew</color> Investigative: {draftCrewOpts.MaxCrewInvestigative.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {Palette.CrewmateBlue.ToTextColor()}Crew</color> Killing: {draftCrewOpts.MaxCrewKilling.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {Palette.CrewmateBlue.ToTextColor()}Crew</color> Power: {draftCrewOpts.MaxCrewPower.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {Palette.CrewmateBlue.ToTextColor()}Crew</color> Protective: {draftCrewOpts.MaxCrewProtective.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┗ {Palette.CrewmateBlue.ToTextColor()}Crew</color> Support: {draftCrewOpts.MaxCrewSupport.Value} Max");

                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"{TownOfUsColors.ImpSoft.ToTextColor()}Impostors</color>: {draftImpOpts.MaxImpostors.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {TownOfUsColors.ImpSoft.ToTextColor()}Imp</color> Concealing: {draftImpOpts.MaxImpConcealing.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {TownOfUsColors.ImpSoft.ToTextColor()}Imp</color> Killing: {draftImpOpts.MaxImpKilling.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┣ {TownOfUsColors.ImpSoft.ToTextColor()}Imp</color> Power: {draftImpOpts.MaxImpPower.Value} Max");
                            rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                $"┗ {TownOfUsColors.ImpSoft.ToTextColor()}Imp</color> Support: {draftImpOpts.MaxImpSupport.Value} Max");

                            if (draftNeutOpts.MaxNeutrals.Value > 0)
                            {
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"{TownOfUsColors.Neutral.ToTextColor()}Neutrals</color>: {draftNeutOpts.MaxNeutrals.Value} Max");
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"┣ {TownOfUsColors.Neutral.ToTextColor()}Neutral</color> Benign: {draftNeutOpts.MaxNeutBenign.Value} Max");
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"┣ {TownOfUsColors.Neutral.ToTextColor()}Neutral</color> Evil: {draftNeutOpts.MaxNeutEvil.Value} Max");
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"┣ {TownOfUsColors.Neutral.ToTextColor()}Neutral</color> Killing: {draftNeutOpts.MaxNeutKilling.Value} Max");
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"┗ {TownOfUsColors.Neutral.ToTextColor()}Neutral</color> Outlier: {draftNeutOpts.MaxNeutOutlier.Value} Max");
                            }
                            else
                            {
                                rolelistBuilder.AppendLine(TownOfUsPlugin.Culture,
                                    $"{TownOfUsColors.Neutral.ToTextColor()}Neutrals</color>: None");
                            }
                        }
                    }
                    else
                    {
                        DraftSidebarManager.DrawSidebar(RoleListTextComp);
                        RoleList.SetActive(true);
                        return;
                    }

                    break;
            }

            if (!IsHoveringRoleList)
            {
                RoleListTextComp.text = rolelistBuilder.ToString();
            }

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
            TouLocalTabButtons.SetUpButtonPositions();
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

                    TouLocalTabButtons.SetUpButtonPositions();
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

    public static Vector3 BelowOptionPos = new (0.435f, 1.25f, 0f);
    public static Vector3 FullTopPos = new (0.435f, 0.475f, 0f);
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
            var oldPos = settingsButton.transform.localPosition;
            settingsButton.transform.localPosition = new Vector3(oldPos.x, oldPos.y, -100);
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
            var iconContainer = new GameObject("iconContainer")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            iconContainer.transform.SetParent(chatButton.transform, false);
            iconContainer.transform.localPosition = new Vector3(0.1f, -0.1f, 0);
            instance.Chat.chatNotifyDot.transform.SetParent(iconContainer.transform, false);
            instance.Chat.chatNotifyDot = iconContainer.transform.GetChild(0).GetComponent<SpriteRenderer>();
            TeamChatPatches.PublicChatDot = instance.Chat.chatNotifyDot;
        }

        if (UiTopRight && UiGrid)
        {
            var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;
            instance.Chat.chatButton.gameObject.SetActive(isChatButtonVisible);
        }
    }
    public static void CreateNewUiRow(HudManager instance)
    {
        if (!ExtraUiTopRight && UiTopRight)
        {
            ExtraUiTopRight = new GameObject("ExtraUiTopRight")
            {
                layer = UiTopRight.layer
            };
            ExtraUiTopRight.transform.SetParent(instance.MapButton.transform.parent.parent, false);

            ExtraUiGrid = ExtraUiTopRight.AddComponent<GridArrange>();
            ExtraUiAspectPos = ExtraUiTopRight.AddComponent<AspectPosition>();

            ExtraUiGrid.Alignment = GridArrange.StartAlign.Left;
            ExtraUiGrid.CellSize = new Vector2(0.85f, 0.85f);
            ExtraUiAspectPos.Alignment = AspectPosition.EdgeAlignments.RightTop;
            ExtraUiAspectPos.DistanceFromEdge = BelowOptionPos;
            ExtraUiGrid.Start();
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
            TouLocalTabButtons.SetUpButtonPositions();
        }

        if (WikiButton)
        {
            WikiButton.SetActive(!GameSettingMenu.Instance &&
                                 (!Minigame.Instance || Minigame.Instance is IngameWikiMinigame));
        }
    }

    public static void AdjustModifierTab()
    {
        if (!ModifierDisplayObject && UiTopRight && ExtraUiTopRight && ModifierDisplayComponent.Instance)
        {
            ModifierDisplayObject = ModifierDisplayComponent.Instance?.gameObject ?? null!;
            ModifierDisplayOnRight = !LocalSettingsTabSingleton<MiraApiSettings>.Instance.ModifiersHudLeftSide.Value;
            if (ModifierDisplayOnRight)
            {
                ModifierDisplayObject.transform.SetParent(ExtraUiTopRight.transform, false);
                ModifierDisplayObject.GetComponent<AspectPosition>().Destroy();
                var oldPos = ModifierDisplayObject.transform.GetChild(0).localPosition;
                ModifierDisplayObject.transform.GetChild(0).localPosition = new Vector3(-1.1757f, -2.1633f, oldPos.z);
                oldPos = ModifierDisplayObject.transform.GetChild(1).localPosition;
                ModifierDisplayObject.transform.GetChild(1).localPosition = new Vector3(-0.45f, 0.3f, oldPos.z);
            }
            TouLocalTabButtons.SetUpButtonPositions();
        }
    }

    public static bool CanZoom =>
        ((PlayerControl.LocalPlayer.DiedOtherRound() &&
          (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } ||
           PlayerControl.LocalPlayer.Data.Role is not IGhostRole)) ||
         (TutorialManager.InstanceExists && LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ZoomingInPractice.Value) ||
         (GameStartManager.InstanceExists && LocalSettingsTabSingleton<TouLocalTabPractice>.Instance.ZoomingInLobby.Value)) && !(HudManager.Instance.GameMenu.IsOpen ||
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
    public static string StoredDraftTitle { get; private set; } = "Draft Mode";
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
        __instance.gameObject.AddComponent<HudManagerHelper>();
        RoleNameStyle = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.RoleNameStyle.Value;
        PlayerNameProgress = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.DisplayPlayerProgress.Value;
        IconOnRoleName = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance.ShowRoleIcons.Value;
        StoredHostLocale = TranslationController.Instance.GetString(StringNames.HostNounEmpty);
        StoredTasksText = TranslationController.Instance.GetString(StringNames.Tasks);
        StoredSpectatingLocale = TouLocale.Get("TouRoleSpectator");
        StoredRoleList = TouLocale.Get("SetRoleList");
        StoredFactionList = TouLocale.Get("NeutralFactionList");
        StoredDraftTitle = TouLocale.Get("StoredDraftTitle");
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
                x is not IWikiDiscoverable).ToList();
            foreach (var modifier in modifiers)
            {
                SoftWikiEntries.RegisterModifierEntry(modifier);
                MiscUtils.AllOverallWikiModifiers = MiscUtils.AllOverallWikiModifiers.AddItem(modifier);
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
            BucketTooltipData.RoleEntry[] roleEntry = [];
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

        TownOfUsColors.UseBasic = LocalSettingsTabSingleton<TouLocalTabPlayers>.Instance
            .UseCrewmateTeamColorToggle.Value;
        
        TouLocalTabButtons.OldButtonScaleFactor =
            LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ButtonUIFactorSlider.Value;
        Coroutines.Start(TouLocalTabButtons.CoResizeSettingsUI());
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
            text = $"AmongUs.Roles.{role.Role}";
        }

        return text;
    }
}
