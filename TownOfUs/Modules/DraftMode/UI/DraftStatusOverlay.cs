using Reactor.Utilities.Attributes;
using System.Collections;
using System.Globalization;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Roles.Other;

namespace TownOfUs.Modules.DraftMode
{
    public enum OverlayState
    {
        Hidden,
        Waiting,
        BackgroundOnly
    }

    [RegisterInIl2Cpp]
    public sealed class DraftStatusOverlay(IntPtr ip) : MonoBehaviour(ip)
    {
        private static DraftStatusOverlay _instance;

        private GameObject _root;
        private GameObject _bgOverlay;

        private GameObject _backdropArt;
        private SpriteRenderer _backdropHaloRenderer;
        private SpriteRenderer _backdropWashRenderer;
        private SpriteRenderer _backdropHorizonRenderer;
        private readonly List<SpriteRenderer> _backdropBeamRenderers = new();
        private readonly List<SpriteRenderer> _backdropParticleRenderers = new();
        private readonly List<Vector3> _backdropParticleBasePositions = new();
        private float _waitAnimTime;

        private TextMeshPro _yourNumberLabel;
        private TextMeshPro _yourNumberValue;
        private TextMeshPro _nowPickingLabel;
        private TextMeshPro _nowPickingValue;
        private GameObject _roleCardNewRoleObj;
        private GameObject _cardTooltipRoot;
        private TextMeshPro _cardTooltipText;

        private static GameObject _cachedRolePrefab;

        private ushort? _pendingRoleId = null!;
        private ushort? _shownRoleId = null!;
        private int _cachedMySlot = -1;
        private int _cachedPickerSlot = -1;
        private int _cachedPickerCount = -1;
        private bool _cachedIsMyTurn;
        private OverlayState _currentState = OverlayState.Hidden;

        private bool _cardHiddenForMenu;
        private bool _cardReady;

        private readonly List<GameObject> _hiddenHudChildren = new List<GameObject>();

        private float _menuCheckTimer;
        private const float MenuCheckInterval = 0.1f;
        private bool _lastMenuOpen;

        private float _slotCheckTimer;
        private const float SlotCheckInterval = 0.05f;

        private static GameStartManager _cachedGsm;
        private static LobbyInfoPane _cachedLobbyPane;

        private static readonly Color WaitingBgColor = new Color(0f, 0f, 0f, 1f);

        private static readonly Vector3 CardHudPos = new Vector3(2.0f, 0.3f, -21f);
        private const float CardScale = 0.55f;
        private const float CardTiltDeg = -8f;
        private const float TeamNameFontSize = 3.8f;

        public static void EnsureExists()
        {
            if (_instance != null) return;
            var go = new GameObject("DraftStatusOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DraftStatusOverlay>();
        }

        public static void SetState(OverlayState state)
        {
            EnsureExists();
            _instance._currentState = state;
            _instance.UpdateVisibility();
            if (state == OverlayState.Hidden && !DraftManager.IsDraftActive)
                DraftSidebarManager.Deactivate();
        }

        public static void Refresh()
        {
            if (_instance == null) return;
            _instance.UpdateContent();
        }

        public static void NotifyLocalPlayerPicked(ushort roleId)
        {
            EnsureExists();
            if (roleId != _instance._shownRoleId)
            {
                _instance._shownRoleId = roleId;
                _instance._pendingRoleId = null;
                _instance.ShowRoleCard(roleId);
            }
        }

        public static void ClearHudReferences()
        {
            if (_instance == null) return;
            _instance._hiddenHudChildren.Clear();
            _instance._root = null!;
            _instance._bgOverlay = null!;

            _instance._backdropArt = null!;
            _instance._backdropHaloRenderer = null!;
            _instance._backdropWashRenderer = null!;
            _instance._backdropHorizonRenderer = null!;
            _instance._backdropBeamRenderers.Clear();
            _instance._backdropParticleRenderers.Clear();
            _instance._backdropParticleBasePositions.Clear();

            _instance._yourNumberLabel = null!;
            _instance._yourNumberValue = null!;
            _instance._nowPickingLabel = null!;
            _instance._nowPickingValue = null!;
            _instance.DestroyRoleCardCore();
            _instance._cardTooltipRoot = null!;
            _instance._cardTooltipText = null!;
            _instance._pendingRoleId = null;
            _instance._shownRoleId = null;
            _instance._cachedMySlot = -1;
            _instance._cachedPickerSlot = -1;
            _instance._cachedPickerCount = -1;
            _instance._cachedIsMyTurn = false;
            _instance._rebuildPending = false;
            _cachedRolePrefab = null!;
            _cachedGsm = null!;
            _cachedLobbyPane = null!;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildUI();
        }

        private void OnDestroy()
        {
            DestroyCardTooltip();
            RestoreHudElements();
            if (_instance == this) _instance = null!;
        }

        private void BuildUI()
        {
            if (HudManager.Instance == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    "[DraftStatusOverlay] BuildUI: HudManager.Instance is null, deferring");
                return;
            }

            if (HudManager.Instance.TaskPanel == null || HudManager.Instance.TaskPanel.taskText == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    "[DraftStatusOverlay] BuildUI: TaskPanel/taskText not ready, deferring");
                return;
            }

            var font = HudManager.Instance.TaskPanel.taskText.font;
            var fontMat = HudManager.Instance.TaskPanel.taskText.fontMaterial;

            _bgOverlay = new GameObject("DraftWaitingBg");
            _bgOverlay.transform.SetParent(HudManager.Instance.transform, false);
            _bgOverlay.transform.localPosition = new Vector3(0f, 0f, 1f);

            var bgSr = _bgOverlay.AddComponent<SpriteRenderer>();
            bgSr.sprite = MakeWhiteSprite();
            bgSr.color = WaitingBgColor;
            bgSr.sortingLayerName = "UI";
            bgSr.sortingOrder = 42;

            var cam = Camera.main;
            float camH = cam != null ? cam.orthographicSize * 2f : 6f;
            float camW = camH * ((float)Screen.width / Screen.height);
            _bgOverlay.transform.localScale = new Vector3(camW, camH, 1f);
            _bgOverlay.SetActive(false);

            BuildBackdropArt(camW, camH);

            _root = new GameObject("DraftOverlayRoot");
            _root.transform.SetParent(HudManager.Instance.transform, false);
            _root.transform.localPosition = new Vector3(0f, 0.6f, -20f);

            _yourNumberLabel = MakeText(_root, "YourNumberLabel", font, fontMat,
                TouLocale.GetParsed("TouDraftYourNumberLabel", "YOUR NUMBER:"), 2.2f, new Color(0.6f, 0.9f, 1f),
                new Vector3(0f, 0.55f, 0f), bold: false);
            _yourNumberValue = MakeText(_root, "YourNumberValue", font, fontMat,
                "?", 5.5f, Color.white,
                new Vector3(0f, 0.05f, 0f), bold: true);
            _nowPickingLabel = MakeText(_root, "NowPickingLabel", font, fontMat,
                TouLocale.GetParsed("TouDraftNowPickingLabel", "NOW PICKING:"), 1.6f, new Color(1f, 0.85f, 0.1f),
                new Vector3(0f, -0.55f, 0f), bold: false);
            _nowPickingValue = MakeText(_root, "NowPickingValue", font, fontMat,
                "?", 3.0f, new Color(1f, 0.85f, 0.1f),
                new Vector3(0f, -1.05f, 0f), bold: true);

            _root.SetActive(false);
        }

        private void BuildBackdropArt(float camW, float camH)
        {
            if (HudManager.Instance == null) return;

            _backdropArt = new GameObject("DraftBackdropArt");
            _backdropArt.transform.SetParent(HudManager.Instance.transform, false);
            _backdropArt.transform.localPosition = new Vector3(0f, 0f, 0.85f);

            _backdropWashRenderer = MakeBackdropArtSprite(
                "DraftWaitingBackdropWash",
                new Vector3(0f, 0.02f, 0.03f),
                new Vector3(camW * 0.98f, camH * 0.42f, 1f),
                MakeSoftGlowSprite(),
                new Color(0f, 0.78f, 1f, 0.12f),
                43);

            var leftBeam = MakeBackdropArtSprite(
                "DraftWaitingBackdropBeamLeft",
                new Vector3(-camW * 0.34f, -0.02f, 0.01f),
                new Vector3(camW * 0.13f, camH * 0.82f, 1f),
                MakeSoftGlowSprite(),
                new Color(1f, 0.82f, 0.12f, 0.12f),
                44);
            leftBeam.transform.localRotation = Quaternion.Euler(0f, 0f, -13f);
            _backdropBeamRenderers.Add(leftBeam);

            var rightBeam = MakeBackdropArtSprite(
                "DraftWaitingBackdropBeamRight",
                new Vector3(camW * 0.34f, -0.02f, 0.01f),
                new Vector3(camW * 0.13f, camH * 0.82f, 1f),
                MakeSoftGlowSprite(),
                new Color(0.72f, 0.42f, 1f, 0.10f),
                44);
            rightBeam.transform.localRotation = Quaternion.Euler(0f, 0f, 13f);
            _backdropBeamRenderers.Add(rightBeam);

            _backdropHorizonRenderer = MakeBackdropArtSprite(
                "DraftWaitingBackdropHorizon",
                new Vector3(0f, -camH * 0.18f, 0f),
                new Vector3(camW * 0.82f, 0.055f, 1f),
                MakeSoftGlowSprite(),
                new Color(0f, 0.95f, 1f, 0.22f),
                45);

            const int particleCount = 6;
            for (int i = 0; i < particleCount; i++)
            {
                float x = Mathf.Sin(i * 1.77f) * camW * 0.42f;
                float y = Mathf.Cos(i * 1.31f) * camH * 0.28f;
                var particle = MakeBackdropArtSprite(
                    "DraftWaitingBackdropParticle",
                    new Vector3(x, y, -0.02f),
                    Vector3.one * (0.035f + (i % 3) * 0.012f),
                    MakeSoftGlowSprite(),
                    new Color(0.5f, 0.95f, 1f, 0.20f),
                    46);
                _backdropParticleRenderers.Add(particle);
                _backdropParticleBasePositions.Add(particle.transform.localPosition);
            }

            MakeBackdropHalo(camW, camH);

            _backdropArt.SetActive(false);
        }

        private SpriteRenderer MakeBackdropArtSprite(string name, Vector3 pos, Vector3 scale,
            Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_backdropArt.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = order;
            return sr;
        }

        private void MakeBackdropHalo(float camW, float camH)
        {
            if (_backdropArt == null) return;
            var halo = new GameObject("DraftBackdropHalo");
            halo.transform.SetParent(_backdropArt.transform, false);
            halo.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            halo.transform.localScale = new Vector3(camW * 0.72f, camH * 0.32f, 1f);

            _backdropHaloRenderer = halo.AddComponent<SpriteRenderer>();
            _backdropHaloRenderer.sprite = MakeSoftGlowSprite();
            _backdropHaloRenderer.color = new Color(0.18f, 0.9f, 1f, 0.075f);
            _backdropHaloRenderer.sortingLayerName = "UI";
            _backdropHaloRenderer.sortingOrder = 43;
        }

        private void UpdateBackdropMotion()
        {
            if (_backdropArt == null || !_backdropArt.activeSelf) return;

            float pulse = (Mathf.Sin(_waitAnimTime * 1.7f) + 1f) * 0.5f;
            _backdropArt.transform.localRotation =
                Quaternion.Euler(0f, 0f, Mathf.Sin(_waitAnimTime * 0.18f) * 0.45f);

            if (_backdropHaloRenderer != null)
            {
                var c = _backdropHaloRenderer.color;
                c.a = 0.055f + pulse * 0.025f;
                _backdropHaloRenderer.color = c;
            }

            if (_backdropWashRenderer != null)
            {
                var wash = new Color(0f, 0.78f, 1f, 0.11f + pulse * 0.08f);
                _backdropWashRenderer.color = wash;
                _backdropWashRenderer.transform.localScale =
                    new Vector3(8.7f + pulse * 0.55f, 2.9f + pulse * 0.24f, 1f);
            }

            for (int i = 0; i < _backdropBeamRenderers.Count; i++)
            {
                var beam = _backdropBeamRenderers[i];
                if (beam == null) continue;
                float side = i == 0 ? -1f : 1f;
                float sway = Mathf.Sin(_waitAnimTime * 0.55f + i * 1.7f) * 0.28f;
                beam.transform.localPosition = new Vector3(
                    side * (2.85f + sway),
                    Mathf.Cos(_waitAnimTime * 0.42f + i) * 0.14f, 0.01f);
                beam.transform.localRotation =
                    Quaternion.Euler(0f, 0f, side * (12f + pulse * 8f));
                var c = i == 0
                    ? new Color(1f, 0.82f, 0.12f, 0.14f + pulse * 0.06f)
                    : new Color(0.72f, 0.42f, 1f, 0.12f + pulse * 0.06f);
                beam.color = c;
            }

            if (_backdropHorizonRenderer != null)
            {
                var c = new Color(0f, 0.95f, 1f, 0.24f + pulse * 0.08f);
                _backdropHorizonRenderer.color = c;
                _backdropHorizonRenderer.transform.localScale =
                    new Vector3(7.4f + pulse * 0.28f, 0.055f + pulse * 0.018f, 1f);
            }

            for (int i = 0; i < _backdropParticleRenderers.Count; i++)
            {
                var particle = _backdropParticleRenderers[i];
                if (particle == null || i >= _backdropParticleBasePositions.Count) continue;
                Vector3 basePos = _backdropParticleBasePositions[i];
                float phase = i * 0.73f;
                particle.transform.localPosition = basePos + new Vector3(
                    Mathf.Sin(_waitAnimTime * 0.75f + phase) * 0.18f,
                    Mathf.Cos(_waitAnimTime * 0.52f + phase) * 0.14f, 0f);
                particle.transform.localScale =
                    Vector3.one * (0.035f + (i % 3) * 0.012f + pulse * 0.035f);
                particle.color = new Color(0.5f, 0.95f, 1f, 0.22f + pulse * 0.12f);
            }
        }

        private static bool EnsureRolePrefab()
        {
            if (_cachedRolePrefab != null) return true;
            try
            {
                var bundle = TouAssets.MainBundle;
                if (bundle == null) return false;
                var prefab = bundle.LoadAsset("SelectRoleGame")?.TryCast<GameObject>();
                if (prefab == null) return false;
                var holderGo = prefab.transform.Find("RoleCardHolder");
                if (holderGo == null) return false;
                _cachedRolePrefab = holderGo.gameObject;
                return true;
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftStatusOverlay] Prefab load failed: {ex.Message}");
                return false;
            }
        }

        private void ShowRoleCard(ushort roleId)
        {
            DestroyRoleCardCore();
            if (!EnsureRolePrefab() || HudManager.Instance == null) return;

            var role = DraftUiManager.ResolveRole(roleId);
            string roleName = role ? role.GetRoleName() : $"Role {roleId}";
            string teamName = MiscUtils.GetParsedRoleAlignment(role);
            Sprite icon = role ? role.GetRoleIcon() : TouRoleIcons.RandomAny.LoadAsset();
            Color color = role ? role.TeamColor : Color.white;
            string description = DraftUiManager.GetRoleDescription(role);

            _roleCardNewRoleObj = UnityEngine.Object.Instantiate(
                _cachedRolePrefab, HudManager.Instance.transform);
            _roleCardNewRoleObj.name = "DraftChosenRoleCard";

            if (_roleCardNewRoleObj.transform.childCount == 0)
            {
                DestroyRoleCardCore();
                return;
            }

            var actualCard = _roleCardNewRoleObj.transform.GetChild(0);
            if (actualCard.childCount < 3)
            {
                DestroyRoleCardCore();
                return;
            }

            var roleText = actualCard.GetChild(0).GetComponent<TextMeshPro>();
            var roleImage = actualCard.GetChild(1).GetComponent<SpriteRenderer>();
            var teamText = actualCard.GetChild(2).GetComponent<TextMeshPro>();
            var passiveButton = actualCard.GetComponent<PassiveButton>();
            var rollover = actualCard.GetComponent<ButtonRolloverHandler>();

            _roleCardNewRoleObj.transform.localPosition = CardHudPos;
            _roleCardNewRoleObj.transform.localScale = Vector3.one * CardScale;
            _roleCardNewRoleObj.transform.localRotation = Quaternion.Euler(0f, 0f, CardTiltDeg);

            if (roleText != null) roleText.text = roleName;
            if (teamText != null)
            {
                teamText.text = teamName;
                teamText.fontSizeMax = TeamNameFontSize;
                teamText.enableAutoSizing = true;
                teamText.color = GetTeamColor(teamName);
            }

            if (roleImage != null)
            {
                roleImage.sprite = icon;
                roleImage.SetSizeLimit(2.8f);
                roleImage.color = Color.white;
            }

            var cardBg = actualCard.GetComponent<SpriteRenderer>();
            if (cardBg != null) cardBg.color = color;
            if (rollover != null)
            {
                rollover.OutColor = color;
                rollover.OverColor = new Color(
                    Mathf.Min(color.r * 1.3f, 1f),
                    Mathf.Min(color.g * 1.3f, 1f),
                    Mathf.Min(color.b * 1.3f, 1f),
                    color.a);
            }

            if (roleText != null) roleText.color = color;

            foreach (var tmp in _roleCardNewRoleObj.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                var r = tmp.GetComponent<Renderer>();
                if (r != null)
                {
                    r.sortingLayerName = "UI";
                    r.sortingOrder = 1;
                }
            }

            foreach (var sr in _roleCardNewRoleObj.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingLayerName = "UI";
                sr.sortingOrder = 1;
            }

            var col = actualCard.GetComponent<Collider2D>()
                      ?? (Collider2D)actualCard.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                var box = actualCard.gameObject.AddComponent<BoxCollider2D>();
                box.size = new Vector2(4f, 6f);
                col = box;
            }

            if (passiveButton != null)
            {
                passiveButton.enabled = true;
                passiveButton.Colliders = new Collider2D[] { col };

                passiveButton.OnClick.RemoveAllListeners();
                ushort capturedId = roleId;
                passiveButton.OnClick.AddListener((Action)(() => OpenWiki(capturedId)));

                passiveButton.OnMouseOver.RemoveAllListeners();
                passiveButton.OnMouseOver.AddListener((Action)(() =>
                {
                    if (!_cardReady || _roleCardNewRoleObj == null) return;
                    _roleCardNewRoleObj.transform.localScale = Vector3.one * (CardScale * 1.08f);
                    ShowCardTooltip(roleName, teamName, description, color);
                }));
                passiveButton.OnMouseOut.RemoveAllListeners();
                passiveButton.OnMouseOut.AddListener((Action)(() =>
                {
                    if (_roleCardNewRoleObj != null)
                        _roleCardNewRoleObj.transform.localScale = Vector3.one * CardScale;
                    HideCardTooltip();
                }));
            }

            _roleCardNewRoleObj.SetActive(true);
            _cardHiddenForMenu = false;
            _cardReady = false;
            Coroutines.Start(CoPopInCard(_roleCardNewRoleObj.transform, this));
        }

        private void OpenWiki(ushort roleId)
        {
            try
            {
                var r = DraftUiManager.ResolveRole(roleId);
                if (r is not TownOfUs.Modules.Wiki.IWikiDiscoverable wikiTarget)
                {
                    MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                        $"[DraftStatusOverlay] Role {roleId} not IWikiDiscoverable");
                    return;
                }

                if (_roleCardNewRoleObj != null)
                    _roleCardNewRoleObj.SetActive(false);

                var wiki = TownOfUs.Modules.Wiki.IngameWikiMinigame.Create();
                wiki.Begin(null);
                wiki.OpenFor(wikiTarget);

                Coroutines.Start(CoWaitForWikiDestroyed(wiki));
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftStatusOverlay] Wiki open failed: {ex.Message}");
                if (_roleCardNewRoleObj != null)
                    _roleCardNewRoleObj.SetActive(true);
            }
        }

        [HideFromIl2Cpp]
        private IEnumerator CoWaitForWikiDestroyed(TownOfUs.Modules.Wiki.IngameWikiMinigame wiki)
        {
            while (wiki != null)
                yield return null;

            _lastMenuOpen = IsAnyMenuOpen();
            _menuCheckTimer = 0f;

            if (_roleCardNewRoleObj != null && !_lastMenuOpen)
            {
                _roleCardNewRoleObj.SetActive(true);
                _cardHiddenForMenu = false;
                _cardReady = true;
            }
        }

        private static bool IsAnyMenuOpen()
        {
            try
            {
                if (Minigame.Instance != null) return true;
                if (PlayerCustomizationMenu.Instance != null) return true;
                if (GameSettingMenu.Instance != null) return true;

                var hud = HudManager.Instance;
                if (hud != null)
                {
                    if (hud.GameMenu != null && hud.GameMenu.IsOpen) return true;
                    if (hud.Chat != null && hud.Chat.IsOpenOrOpening) return true;
                }

                if (FriendsListUI.Instance != null && FriendsListUI.Instance.IsOpen) return true;
            }
            catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }

            return false;
        }

        internal void DestroyRoleCardCore()
        {
            if (_roleCardNewRoleObj != null)
            {
                try
                {
                    MiraAPI.Utilities.Extensions.DeepDestroy(_roleCardNewRoleObj, true);
                }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }

                _roleCardNewRoleObj = null!;
            }

            HideCardTooltip();
            _cardHiddenForMenu = false;
            _cardReady = false;
        }

        public static void DestroyRoleCard()
        {
            _instance?.DestroyRoleCardCore();
        }

        private void EnsureCardTooltip()
        {
            if (_cardTooltipRoot != null || HudManager.Instance == null) return;
            if (HudManager.Instance.TaskPanel == null || HudManager.Instance.TaskPanel.taskText == null) return;

            _cardTooltipRoot = new GameObject("DraftWaitingCardTooltip");
            _cardTooltipRoot.transform.SetParent(HudManager.Instance.transform, false);
            _cardTooltipRoot.transform.localPosition = new Vector3(2.0f, -1.55f, -22f);

            _cardTooltipText = _cardTooltipRoot.AddComponent<TextMeshPro>();
            _cardTooltipText.font = HudManager.Instance.TaskPanel.taskText.font;
            _cardTooltipText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
            _cardTooltipText.fontSize = 1.35f;
            _cardTooltipText.alignment = TextAlignmentOptions.Center;
            _cardTooltipText.enableWordWrapping = true;
            _cardTooltipText.rectTransform.sizeDelta = new Vector2(3.5f, 1.3f);
            _cardTooltipText.text = string.Empty;

            var r = _cardTooltipRoot.GetComponent<Renderer>();
            if (r != null)
            {
                r.sortingLayerName = "UI";
                r.sortingOrder = 3;
            }

            _cardTooltipRoot.SetActive(false);
        }

        private void ShowCardTooltip(string roleName, string teamName, string description, Color color)
        {
            EnsureCardTooltip();
            if (_cardTooltipRoot == null || _cardTooltipText == null) return;
            string hex = ColorUtility.ToHtmlStringRGB(color);
            string desc = string.IsNullOrWhiteSpace(description) ? string.Empty : $"\n<size=70%>{description}</size>";
            _cardTooltipText.text =
                $"<b><color=#{hex}>{roleName}</color></b>  <size=60%><color=#BBBBBB>{teamName}</color></size>{desc}";
            _cardTooltipRoot.SetActive(true);
        }

        private void HideCardTooltip()
        {
            if (_cardTooltipRoot != null) _cardTooltipRoot.SetActive(false);
        }

        private void DestroyCardTooltip()
        {
            if (_cardTooltipRoot != null)
            {
                try
                {
                    MiraAPI.Utilities.Extensions.DeepDestroy(_cardTooltipRoot, true);
                }
                catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
            }

            _cardTooltipRoot = null!;
            _cardTooltipText = null!;
        }

        private static IEnumerator CoPopInCard(Transform holder, DraftStatusOverlay owner)
        {
            holder.localScale = Vector3.zero;
            float duration = 0.25f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                if (holder == null) yield break;
                float s = Mathf.LerpUnclamped(0f, CardScale, EaseOutBack(t / duration));
                holder.localScale = Vector3.one * s;
                yield return null;
            }

            if (holder != null)
                holder.localScale = Vector3.one * CardScale;

            if (owner != null)
                owner._cardReady = true;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static Color GetTeamColor(string teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return Color.white;
            string lower = teamName.ToLowerInvariant();
            if (lower.Contains("crewmate")) return new Color32(0, 255, 255, 255);
            if (lower.Contains("impostor") || lower.Contains("imposter")) return new Color32(255, 0, 0, 255);
            if (lower.Contains("neutral")) return new Color32(180, 180, 180, 255);
            return Color.white;
        }

        private static (int pickerSlot, int pickerCount, bool isMyTurn) ComputePickerStatus()
        {
            int pickerSlot = -1;
            int pickerCount = 0;
            bool isMyTurn = false;
            bool isLocalGame = AmongUsClient.Instance?.NetworkMode == NetworkModes.LocalGame || AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;

            foreach (var s in DraftManager.GetActivePickerStatesNonAlloc())
            {
                if (s == null || !s.IsPickingNow) continue;
                pickerCount++;
                if (pickerSlot < 0) pickerSlot = s.SlotNumber;
                if (s.PlayerId == PlayerControl.LocalPlayer.PlayerId) isMyTurn = true;
                else if (isLocalGame)
                {
                    var p = MiscUtils.PlayerById(s.PlayerId);
                    if (p != null && AmongUsClient.Instance?.GetClient(p.OwnerId) == null) isMyTurn = true;
                }
            }

            return (pickerSlot, pickerCount, isMyTurn);
        }

        private void Update()
        {
            if (_currentState == OverlayState.Hidden) return;

            float dt = Time.deltaTime;

            _menuCheckTimer += dt;
            if (_menuCheckTimer >= MenuCheckInterval)
            {
                _menuCheckTimer = 0f;
                _lastMenuOpen = IsAnyMenuOpen();
            }

            if (_roleCardNewRoleObj != null)
            {
                if (_lastMenuOpen && _roleCardNewRoleObj.activeSelf)
                {
                    _roleCardNewRoleObj.SetActive(false);
                    _cardHiddenForMenu = true;
                    HideCardTooltip();
                }
                else if (!_lastMenuOpen && _cardHiddenForMenu && !_roleCardNewRoleObj.activeSelf)
                {
                    _roleCardNewRoleObj.SetActive(true);
                    _cardHiddenForMenu = false;
                    _cardReady = true;
                }
            }

            _waitAnimTime += dt;
            UpdateBackdropMotion();

            if (_currentState == OverlayState.Waiting)
            {
                if (_root == null) BuildUI();
                if (_root == null) return;

                if (DraftManager.IsDraftActive)
                {
                    _slotCheckTimer += dt;
                    if (_slotCheckTimer >= SlotCheckInterval)
                    {
                        _slotCheckTimer = 0f;

                        int mySlot = DraftManager.GetSlotForPlayer(PlayerControl.LocalPlayer.PlayerId);
                        var (pickerSlot, pickerCount, isMyTurn) = ComputePickerStatus();

                        if (mySlot != _cachedMySlot || pickerSlot != _cachedPickerSlot ||
                            pickerCount != _cachedPickerCount || isMyTurn != _cachedIsMyTurn)
                        {
                            _cachedMySlot = mySlot;
                            _cachedPickerSlot = pickerSlot;
                            _cachedPickerCount = pickerCount;
                            _cachedIsMyTurn = isMyTurn;
                            UpdateContent();
                        }
                    }
                }
            }

            if (_pendingRoleId.HasValue && _pendingRoleId != _shownRoleId)
            {
                _shownRoleId = _pendingRoleId;
                _pendingRoleId = null;
                ShowRoleCard(_shownRoleId.Value);
            }
        }

        private void UpdateContent()
        {
            if (_root == null) return;

            int mySlot = DraftManager.GetSlotForPlayer(PlayerControl.LocalPlayer.PlayerId);
            var (pickerSlot, pickerCount, isMyTurn) = ComputePickerStatus();

            _cachedIsMyTurn = isMyTurn;

            string mySlotText = mySlot > 0 ? mySlot.ToString(CultureInfo.InvariantCulture) : "?";
            string mySlotLabelText = TouLocale.GetParsed("TouDraftYourNumberLabel", "YOUR NUMBER:");
            bool isSpectating = SpectatorRole.TrackedSpectators.Contains(PlayerControl.LocalPlayer.Data.PlayerName);
            if (isSpectating)
            {
                mySlotLabelText = TouLocale.GetParsed("TouDraftYouAreLabel", "YOU ARE");
                mySlotText = TouLocale.GetParsed("TouDraftSpectatingValue", "SPECTATING");
            }

            string pickerText = "?";
            if (pickerCount > 1) pickerText = TouLocale.GetParsed("TouDraftMultiLabel", "MULTI");
            else if (pickerSlot > 0) pickerText = pickerSlot.ToString(CultureInfo.InvariantCulture);

            string labelText = TouLocale.GetParsed("TouDraftNowPickingLabel", "NOW PICKING:");
            if (isMyTurn) labelText = TouLocale.GetParsed("TouDraftYourTurnLabel", "YOUR TURN!");
            else if (pickerCount > 1) labelText = TouLocale.GetParsed("TouDraftNowPickingMultiLabel", "NOW PICKING (MULTI):");

            if (_yourNumberLabel != null)
                _yourNumberLabel.text = mySlotLabelText;
            if (_yourNumberValue != null)
                _yourNumberValue.text = mySlotText;
            if (_nowPickingValue != null)
                _nowPickingValue.text = pickerText;
            if (_nowPickingValue != null)
                _nowPickingValue.color = isMyTurn ? new Color(0.1f, 1f, 0.4f) : new Color(1f, 0.85f, 0.1f);
            if (_nowPickingLabel != null)
                _nowPickingLabel.text = labelText;

        }

        private bool _rebuildPending;

        private void UpdateVisibility()
        {
            if (_root == null && _currentState != OverlayState.Hidden)
            {
                BuildUI();
                if (_root == null && !_rebuildPending)
                {

                    _rebuildPending = true;
                    Coroutines.Start(CoRetryBuildUI());
                }
            }

            if (_root == null) return;

            if (_currentState == OverlayState.Hidden)
            {
                _root.SetActive(false);
                if (_bgOverlay != null) _bgOverlay.SetActive(false);
                if (_backdropArt != null) _backdropArt.SetActive(false);
                DestroyRoleCardCore();
                _pendingRoleId = null;
                _shownRoleId = null;
                _waitAnimTime = 0f;
                _menuCheckTimer = 0f;
                _slotCheckTimer = 0f;
                _lastMenuOpen = false;
                RestoreHudElements();
            }
            else if (_currentState == OverlayState.Waiting)
            {
                _root.SetActive(true);
                if (_bgOverlay != null) _bgOverlay.SetActive(true);
                if (_backdropArt != null) _backdropArt.SetActive(true);
                HideHudElements();
            }
            else if (_currentState == OverlayState.BackgroundOnly)
            {
                _root.SetActive(false);
                if (_bgOverlay != null) _bgOverlay.SetActive(true);
                if (_backdropArt != null) _backdropArt.SetActive(true);
                HideHudElements();
            }
        }

        [HideFromIl2Cpp]
        private IEnumerator CoRetryBuildUI()
        {

            for (int i = 0; i < 60 && _root == null && _currentState != OverlayState.Hidden; i++)
            {
                yield return null;
                BuildUI();
            }

            _rebuildPending = false;
            if (_root != null) UpdateVisibility();
        }

        private void HideHudElements()
        {
            _hiddenHudChildren.RemoveAll(go => go == null);

            if (_cachedGsm == null)
                _cachedGsm = UnityEngine.Object.FindObjectOfType<GameStartManager>();
            if (_cachedGsm != null && _cachedGsm.gameObject.activeSelf)
            {
                _cachedGsm.gameObject.SetActive(false);
                if (!_hiddenHudChildren.Contains(_cachedGsm.gameObject))
                    _hiddenHudChildren.Add(_cachedGsm.gameObject);
            }

            if (_cachedLobbyPane == null)
                _cachedLobbyPane = UnityEngine.Object.FindObjectOfType<LobbyInfoPane>();
            if (_cachedLobbyPane != null && _cachedLobbyPane.gameObject.activeSelf)
            {
                _cachedLobbyPane.gameObject.SetActive(false);
                if (!_hiddenHudChildren.Contains(_cachedLobbyPane.gameObject))
                    _hiddenHudChildren.Add(_cachedLobbyPane.gameObject);
            }
        }

        private void RestoreHudElements()
        {
            foreach (var go in _hiddenHudChildren)
                if (go != null)
                    try
                    {
                        go.SetActive(true);
                    }
                    catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }

            _hiddenHudChildren.Clear();
        }

        private static TextMeshPro MakeText(
            GameObject parent, string name,
            TMP_FontAsset font, Material fontMat,
            string text, float fontSize, Color color,
            Vector3 offset, bool bold)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = offset;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.fontMaterial = fontMat;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.text = text;

            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.sortingLayerName = "UI";
                r.sortingOrder = 50;
            }

            return tmp;
        }

        private static Sprite _softGlow;

        private static Sprite MakeSoftGlowSprite()
        {
            if (_softGlow != null) return _softGlow;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var px = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center) / radius;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            _softGlow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _softGlow.hideFlags = HideFlags.HideAndDontSave;
            return _softGlow;
        }

        private static Sprite _white;

        private static Sprite MakeWhiteSprite()
        {
            if (_white != null) return _white;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            _white.hideFlags = HideFlags.HideAndDontSave;
            return _white;
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerDraftPrefix
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            return !DraftManager.IsDraftActive;
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerDraftPostfix
    {
        [HarmonyPostfix]
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(PingTracker __instance)
        {
            if (!DraftManager.IsDraftActive) return;
            if (__instance.text != null)
                __instance.text.text = string.Empty;
        }
    }
}