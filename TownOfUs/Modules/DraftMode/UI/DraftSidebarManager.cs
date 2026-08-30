using System.Text;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
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
        public static void DrawSidebar(TextMeshPro tmp)
        {
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
                _cachedStaticContent = $"\n\n<color=#7A8089><i>{MiraLocaleManager.Get("TouDraftWaitingToStart", "Waiting to start...")}</i></color>";
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
            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"<size=64%><color=#6B7178>{picked} / {total}  {MiraLocaleManager.Get("TouDraftRolesPickedLabel", "ROLES PICKED")}</color></size>\n");
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
            var draftWord = MiraLocaleManager.Get("TouDraftShimmerDraft", "DRAFT").ToUpperInvariant();
            TmpSpriteUtils.CreateSpriteAsset(TouAssets.IconDraftMode.LoadAsset(),"TouMira.Gamemode.DraftMode",1.45f);
            var modeWord = MiraLocaleManager.Get("TouDraftShimmerMode", "MODE").ToUpperInvariant();
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
            string playerNumLabel = MiraLocaleManager.Get("TouDraftPlayerNumberLabel", "Player #<num>").Replace("<num>", slot.ToString("D2", System.Globalization.CultureInfo.InvariantCulture));
            string you    = isMe ? $"  <color=#8BD5F9><b>({MiraLocaleManager.Get("TouDraftYouLabel", "YOU")})</b></color>" : string.Empty;
            string numCol = isMe ? "#8BD5F9" : "#ffee00";

            if (DraftManager.IsPlayerDisconnected(state.PlayerId))
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <color=#FF5050>{MiraLocaleManager.Get("TouDraftDisconnectedLabel", "DISCONNECTED")}</color>{you}";
            }

            if (state.IsPickingNow && !state.HasPicked)
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <b><color=#FFFFFF> {MiraLocaleManager.Get("TouDraftIsPickingLabel", "is picking...")}</color></b>{you}";
            }

            string statusCol, statusTxt;
            if (state.HasPicked)
            {
                (statusTxt, statusCol) = GetStatusLabelForRole(state.ChosenRoleId);
            }
            else
            {
                return $"<color={numCol}><b>{playerNumLabel}</b></color> <color=#ffffff>{MiraLocaleManager.Get("TouDraftIsWaitingLabel", "is waiting")}</color>";
            }

            string row = $"<color={numCol}><b>{playerNumLabel}</b></color> {MiraLocaleManager.Get("TouDraftPickedLabel", "picked")} <b><color={statusCol}>{statusTxt}</color></b>{you}";
            if (isMe)
                return $"<mark=#8BD5F910>{row}</mark>";
            return row;
        }

        private static (string text, string colorHex) GetStatusLabelForRole(ushort roleId)
        {
            if (roleId == 0)
            {
                return (TouLocale.GetParsed("TouDraftARoleLabel", "a role"), "#f7f7f7");
            }

            var role = MiscUtils.GetRegisteredRole((AmongUs.GameOptions.RoleTypes)roleId);

            if (role == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftSidebar] Could not resolve role for id {roleId}; falling back.");
                return (MiraLocaleManager.Get("TouDraftUnknownRoleStatus", "UNKNOWN"), "#f7f7f7");
            }

            var faction = DraftUiManager.GetTeamLabel(role);
            var actualFaction = role is ICustomRole custom ? custom.Team.ToString() : role.TeamType.ToString();
            string colorHex = "";
            var displayMode = OptionGroupSingleton<RoleOptions>.Instance.DraftSidebarDisplay.Value;
            string text = displayMode switch
            {
                DraftRecapMode.Alignment => $"{MiscUtils.GetParsedRoleAlignment(role).ToUpperInvariant()} <sprite name=\"AmongUs.Role.{actualFaction}\">",
                DraftRecapMode.Role      => $"{role.GetRoleName().ToUpperInvariant()} {MiscUtils.GetRoleTmpIcon(role)}",
                DraftRecapMode.Faction   => $"{faction.ToUpperInvariant()} <sprite name=\"AmongUs.Role.{actualFaction}\">",
                _   => MiraLocaleManager.Get("TouDraftARoleLabel", "a role"),
            };
            if(displayMode == DraftRecapMode.Nothing)
            {
                colorHex = "#f7f7f7";

            } else if(displayMode == DraftRecapMode.Role)
            {
                colorHex = "#" + role.TeamColor.ToHtmlStringRGBA();
            } 
            else
            {
                colorHex ="#" + MiscUtils.GetRoleFactionColor(role).ToHtmlStringRGBA();
            }
            return (text, colorHex);
    }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
    public static class DraftSidebarDeactivateOnDisconnect
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!DraftManager.IsDraftActive) return;

            DraftCancelButton.Hide();
            DraftShuffleButton.HideAndReset();
            DraftSidebarManager.Deactivate();
            DraftSidebarManager.ClearBannerRef();
        }
    }
}