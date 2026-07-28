using System.Text;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Patches;
using UnityEngine;


namespace TownOfUs.Modules.DraftMode
{
    public static class DraftSidebarManager
    {
        private static bool _active;
        private static GameObject    _bannerGo = null!;
        private static string _cachedStaticContent = null!;
        private static int    _cachedPickedCount   = -1;
        private static int    _cachedDisconnectedCount = -1;
        private static bool   _cachedDraftActive;

        public static void Activate()
        {
            _active = true;
            InvalidateCache();
            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftSidebar] Activated.");
        }

        public static void Deactivate()
        {
            if (!_active) return;
            _active = false;
            _cachedStaticContent = null!;
            _cachedPickedCount   = -1;
            _cachedDisconnectedCount = -1;
            _cachedDraftActive   = false;

            if (_bannerGo != null) _bannerGo.SetActive(false);

            var tmp = HudManagerPatches.RoleListTextComp;
            if (tmp != null)
                tmp.text = string.Empty;

            var roleList = HudManagerPatches.RoleList;
            if (roleList != null)
                roleList.SetActive(false);

            HudManagerPatches.IsHoveringRoleList = false;

            MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, "[DraftSidebar] Deactivated.");
        }
        public static void ClearBannerRef()
        {
            _bannerGo = null!;
        }

        public static bool IsActive => _active;
        public static void InvalidateCache()
        {
            _cachedStaticContent = null!;
            _cachedPickedCount   = -1;
            _cachedDisconnectedCount = -1;
            _cachedDraftActive   = false;
        }
        public static void DrawSidebar()
        {
            var roleList = HudManagerPatches.RoleList;
            var tmp      = HudManagerPatches.RoleListTextComp;
            if (roleList == null || tmp == null) return;

            roleList.SetActive(true);
            tmp.fontSize           = 3f;
            tmp.fontSizeMin        = 0.5f;
            tmp.fontSizeMax        = 3f;
            tmp.enableWordWrapping = false;
            tmp.text = AnimatedTitle() + GetStaticContent();
        }

        private static string GetStaticContent()
        {
            bool draftActive = DraftManager.IsDraftActive;

            if (!draftActive)
            {
                if (_cachedDraftActive == draftActive && _cachedStaticContent != null)
                    return _cachedStaticContent;
                _cachedDraftActive   = draftActive;
                _cachedPickedCount   = -1;
                _cachedDisconnectedCount = -1;
                _cachedStaticContent = $"\n\n<color=#7A8089><i>{TouLocale.GetParsed("TouDraftWaitingToStart", "Waiting to start...")}</i></color>";
                return _cachedStaticContent;
            }

            int total = 0, picked = 0, disconnected = 0;
            foreach (int slot in DraftManager.TurnOrder)
            {
                var s = DraftManager.GetStateForSlot(slot);
                if (s == null) continue;
                total++;
                if (s.HasPicked) picked++;
                if (DraftManager.IsPlayerDisconnected(s.PlayerId)) disconnected++;
            }
            if (draftActive == _cachedDraftActive && picked == _cachedPickedCount
                && disconnected == _cachedDisconnectedCount && _cachedStaticContent != null)
                return _cachedStaticContent;

            _cachedDraftActive = draftActive;
            _cachedPickedCount = picked;
            _cachedDisconnectedCount = disconnected;
            _cachedStaticContent = BuildStaticRows(total, picked);
            return _cachedStaticContent;
        }

        private static string BuildStaticRows(int total, int picked)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"<size=64%><color=#6B7178>{picked} / {total}  {TouLocale.GetParsed("TouDraftRolesPickedLabel", "ROLES PICKED")}</color></size>\n");
            sb.AppendLine();

            foreach (int slot in DraftManager.TurnOrder)
            {
                var state = DraftManager.GetStateForSlot(slot);
                if (state == null) continue;
                bool isMe = state.PlayerId == PlayerControl.LocalPlayer.PlayerId;
                sb.AppendLine(BuildRow(slot, state, isMe));
            }

            return sb.ToString().TrimEnd();
        }

        private static string AnimatedTitle()
        {
            float t = Time.time;
            var sb = new StringBuilder();
            var draftWord = TouLocale.GetParsed("TouDraftShimmerDraft", "DRAFT").ToUpperInvariant();
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.IconDraftMode.LoadAsset(),"TouMira.Gamemode.DraftMode",1.45f);
            var modeWord = TouLocale.GetParsed("TouDraftShimmerMode", "MODE").ToUpperInvariant();
            sb.Append("<size=105%><b>");
            sb.Append(Shimmer(draftWord, new Color(1f, 0.31f, 0.31f), t, 0));
            sb.Append(' ');
            sb.Append(Shimmer(modeWord, new Color(1f, 0.31f, 0.31f), t, draftWord.Length + 1));
            sb.Append("</b></size>"); 
            sb.Append(' ');
            sb.Append($"<sprite name=\"TouMira.Gamemode.DraftMode\">");
            return sb.ToString();
        }

        private static string Shimmer(string word, Color baseCol, float t, int startIdx)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < word.Length; i++)
            {
                float w = (Mathf.Sin(t * 2.2f - (startIdx + i) * 0.6f) + 1f) * 0.5f;
                w *= w;
                Color c = Color.Lerp(baseCol, Color.white, w * 0.8f);
                sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{word[i]}</color>");
            }
            return sb.ToString();
        }

        private static string BuildRow(int slot, DraftSlotState state, bool isMe)
        {
            string playerNumLabel = TouLocale.GetParsed("TouDraftPlayerNumberLabel", "Player #<num>").Replace("<num>", slot.ToString("D2", System.Globalization.CultureInfo.InvariantCulture));
            string you    = isMe ? $"  <color=#8BD5F9><b>({TouLocale.GetParsed("TouDraftYouLabel", "YOU")})</b></color>" : string.Empty;
            string numCol = isMe ? "#8BD5F9" : "#ffee00";

            if (DraftManager.IsPlayerDisconnected(state.PlayerId))
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <color=#FF5050>{TouLocale.GetParsed("TouDraftDisconnectedLabel", "DISCONNECTED")}</color>{you}";
            }

            if (state.IsPickingNow && !state.HasPicked)
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <b><color=#FFFFFF> {TouLocale.GetParsed("TouDraftIsPickingLabel", "is picking...")}</color></b>{you}";
            }

            string statusCol, statusTxt;
            if (state.HasPicked)
            {
                (statusTxt, statusCol) = GetStatusLabelForRole(state.ChosenRoleId);
            }
            else
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <color=#ffffff>{TouLocale.GetParsed("TouDraftIsWaitingLabel", "is waiting")}</color>";
            }

            string row = $"<color={numCol}><b>{playerNumLabel}</b></color> {TouLocale.GetParsed("TouDraftPickedLabel", "picked")} <b><color={statusCol}>{statusTxt}</color></b>{you}";
            if (isMe)
                return $"<mark=#8BD5F910>{row}</mark>";
            return row;
        }

        private static (string text, string colorHex) GetStatusLabelForRole(ushort roleId)
        {
            RoleBehaviour role = roleId != 0
                ? MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)roleId)
                  ?? RoleManager.Instance.GetRole((AmongUs.GameOptions.RoleTypes)roleId)
                : null!;

            if (role == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftSidebar] Could not resolve role for id {roleId}; falling back.");
                return (TouLocale.GetParsed("TouDraftUnknownRoleStatus", "UNKNOWN"), "#f7f7f7");
            }

            var faction = DraftUiManager.GetTeamLabel(role);
            string colorHex = "";
            var displayMode = OptionGroupSingleton<RoleOptions>.Instance.DraftSidebarDisplay.Value;
            string text = displayMode switch
            {
                DraftRecapMode.Alignment => $"{faction.ToUpperInvariant()} <sprite name=\"AmongUs.Role.{faction}\">",
                DraftRecapMode.Role      => $"{role.GetRoleName().ToUpperInvariant()} {MiscUtils.GetRoleTmpIcon(role)}",
                DraftRecapMode.Faction   => $"{faction.ToUpperInvariant()} <sprite name=\"AmongUs.Role.{faction}\">",
                _   => TouLocale.GetParsed("TouDraftARoleLabel", "a role"),
            };
            if(displayMode == DraftRecapMode.Role)
            {
                colorHex = role.TeamColor != default
                    ? "#" + ColorUtility.ToHtmlStringRGB(role.TeamColor)
                    : "#5BD7E4";
            } else if ( displayMode == DraftRecapMode.Nothing)
            {
                colorHex = "#f7f7f7";
            }
            else
            {
                colorHex =
                    faction switch
                    {
                        "Impostor" => "#FF5050",
                        "Neutral" => "#717171",
                        _ => "#5BD7E4",
                    };
            }

            return (text, colorHex);
    }

    [HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleList))]
    public static class DraftSidebarUpdateRoleListPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!DraftSidebarManager.IsActive) return;
            DraftSidebarManager.DrawSidebar();
        }
    }

    [HarmonyPatch(typeof(DraftRpcs), nameof(DraftRpcs.RpcStartDraft))]
    public static class DraftSidebarActivateOnClient
    {
        [HarmonyPostfix]
        public static void Postfix() => DraftSidebarManager.Activate();
    }

    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastRecap))]
    public static class DraftSidebarDeactivateOnRecap
    {
        [HarmonyPostfix]
        public static void Postfix() => DraftSidebarManager.Deactivate();
    }

    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastCancelDraft))]
    public static class DraftSidebarDeactivateOnCancel
    {
        [HarmonyPostfix]
        public static void Postfix() => DraftSidebarManager.Deactivate();
    }

    [HarmonyPatch(typeof(DraftStatusOverlay), nameof(DraftStatusOverlay.SetState))]
    public static class DraftSidebarDeactivateOnOverlayHidden
    {
        [HarmonyPostfix]
        public static void Postfix(OverlayState state)
        {
            if (state == OverlayState.Hidden && !DraftManager.IsDraftActive)
                DraftSidebarManager.Deactivate();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class DraftSidebarDeactivateOnIntro
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftSidebarManager.Deactivate();
            DraftSidebarManager.ClearBannerRef();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
    public static class DraftSidebarDeactivateOnDisconnect
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftSidebarManager.Deactivate();
            DraftSidebarManager.ClearBannerRef();
        }
    }

    [HarmonyPatch(typeof(RoleListHoverComponent), nameof(RoleListHoverComponent.Update))]
    public static class RoleListHoverSuppressUpdate
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (!DraftManager.IsDraftActive) return true;

            HudManagerPatches.IsHoveringRoleList = false;
            return false;
        }
    }
}
}