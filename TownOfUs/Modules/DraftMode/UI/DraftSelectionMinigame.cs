using System.Collections;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace TownOfUs.Modules.DraftMode
{
    public class DraftScreenController : MonoBehaviour
    {
        public static DraftScreenController Instance { get; private set; }

        private sealed class DraftCardIdleCache
        {
            public Transform Card;
            public Transform Holder;
            public Vector3 BasePosition;
            public Quaternion BaseRotation;
        }

        private GameObject _screenRoot;
        private ushort[] _offeredRoleIds;
        private bool _hasPicked;
        private TextMeshPro _timerText;
        private GameObject _tooltipRoot;
        private TextMeshPro _tooltipText;
        private const string PickPrompt = "<color=#FFFFFF><size=200%><b>Pick Your Role!</b></size></color>";
        private GameObject _timerRoot;
        private GameObject _timerTrack;
        private GameObject _timerFill;
        private SpriteRenderer _timerFillRenderer;

        private GameObject _selectionBackdrop;
        private SpriteRenderer _selectionBackdropWash;

        private SpriteRenderer _selectionBackdropHorizon;
        private SpriteRenderer _selectionBackdropFlash;
        private readonly List<SpriteRenderer> _selectionBackdropParticles = new();
        private readonly List<Vector3> _selectionBackdropParticleBase = new();
        private readonly List<SpriteRenderer> _selectionBackdropBeams = new();

        private readonly List<DraftCardIdleCache> _idleCardCaches = new();
        private readonly Dictionary<Transform, DraftCardIdleCache> _idleCardCacheByTransform = new();
        private Transform _hoveredCard;
        private readonly Color _backdropPulseColor = new Color(0f, 0.95f, 1f, 0.4f);
        private float _backdropPulse;
        private float _backdropTime;

        private const string PrefabName = "SelectRoleGame";
        private const float DraftReadyGateSeconds = 0.92f;
        private const float DraftCardRevealStaggerSeconds = 0.045f;
        private const float TimerProgressWidth = 4.2f;

        private const float CardScaleForCount = 0.55f;

        private static float SpacingForCount(int count) => count switch
        {
            <= 5 => -1f,
            _ => 0f,
        };


        private static Color GetTeamColor(DraftFaction faction)
        {
            switch (faction)
            {
                case DraftFaction.Crewmate:
                    return TownOfUsColors.Crewmate;
                case DraftFaction.Impostor:
                    return TownOfUsColors.ImpSoft;
                case DraftFaction.Neutral:
                    return TownOfUsColors.Neutral;
            }
            return Color.white;
        }

        private void BuildBottomTimer()
        {
            if (HudManager.Instance == null) return;

            _timerRoot = new GameObject("DraftBottomTimer");
            _timerRoot.transform.SetParent(HudManager.Instance.transform, false);
            _timerRoot.transform.localPosition = new Vector3(0f, -2.6f, -25f);

            _timerText = _timerRoot.AddComponent<TextMeshPro>();
            _timerText.font = HudManager.Instance.TaskPanel.taskText.font;
            _timerText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
            _timerText.fontSize = 2.5f;
            _timerText.alignment = TextAlignmentOptions.Center;
            _timerText.enableWordWrapping = false;
            _timerText.text = string.Empty;

            var r = _timerRoot.GetComponent<Renderer>();
            if (r != null) { r.sortingLayerName = "UI"; r.sortingOrder = 82; }

            _timerTrack = new GameObject("DraftBottomTimerTrack");
            _timerTrack.transform.SetParent(_timerRoot.transform, false);
            _timerTrack.transform.localPosition = new Vector3(0f, -0.32f, 0.02f);
            _timerTrack.transform.localScale = new Vector3(TimerProgressWidth, 0.045f, 1f);
            var trackRenderer = _timerTrack.AddComponent<SpriteRenderer>();
            trackRenderer.sprite = MakeWhiteSprite();
            trackRenderer.color = new Color(1f, 1f, 1f, 0.16f);
            trackRenderer.sortingLayerName = "UI";
            trackRenderer.sortingOrder = 81;

            _timerFill = new GameObject("DraftBottomTimerFill");
            _timerFill.transform.SetParent(_timerRoot.transform, false);
            _timerFill.transform.localPosition = new Vector3(-TimerProgressWidth * 0.5f, -0.32f, -0.01f);
            _timerFill.transform.localScale = new Vector3(TimerProgressWidth, 0.055f, 1f);
            _timerFillRenderer = _timerFill.AddComponent<SpriteRenderer>();
            _timerFillRenderer.sprite = MakeWhiteSprite();
            _timerFillRenderer.color = new Color(1f, 0.82f, 0.12f, 0.92f);
            _timerFillRenderer.sortingLayerName = "UI";
            _timerFillRenderer.sortingOrder = 83;
        }

        private void DestroyBottomTimer()
        {
            if (_timerRoot != null)
            {
                try { MiraAPI.Utilities.Extensions.DeepDestroy(_timerRoot, false); } catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
                _timerRoot = null!;
                _timerText = null!;
                _timerTrack = null!;
                _timerFill = null!;
                _timerFillRenderer = null!;
            }
        }

        private void BuildTooltip(Transform parent)
        {
            if (HudManager.Instance == null || parent == null) return;

            _tooltipRoot = new GameObject("DraftCardTooltip");
            _tooltipRoot.transform.SetParent(parent, false);
            _tooltipRoot.transform.localPosition = new Vector3(0f, 2.45f, -1f);

            _tooltipText = _tooltipRoot.AddComponent<TextMeshPro>();
            _tooltipText.font = HudManager.Instance.TaskPanel.taskText.font;
            _tooltipText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
            _tooltipText.fontSize = 1.6f;
            _tooltipText.alignment = TextAlignmentOptions.Center;
            _tooltipText.enableWordWrapping = true;
            _tooltipText.rectTransform.sizeDelta = new Vector2(6.4f, 1.0f);
            _tooltipText.text = string.Empty;

            var r = _tooltipRoot.GetComponent<Renderer>();
            if (r != null) { r.sortingLayerName = "UI"; r.sortingOrder = 252; }

            _tooltipText.text = PickPrompt;
            _tooltipRoot.SetActive(true);
        }

        private void ShowTooltip(string roleName, string teamName, string description, Color color)
        {
            if (_tooltipRoot == null || _tooltipText == null) return;
            string hex = ColorUtility.ToHtmlStringRGB(color);
            string desc = string.IsNullOrWhiteSpace(description)
                ? string.Empty
                : $"\n<size=68%>{description}</size>";
            string teamcolor = "#BBBBBB";
            if(teamName.Contains("crewmate", StringComparison.OrdinalIgnoreCase)) teamcolor= "#5BD7E4";
            if(teamName.Contains("impostor", StringComparison.OrdinalIgnoreCase)) teamcolor= "#FF5050";
            if(teamName.Contains("neutral", StringComparison.OrdinalIgnoreCase)) teamcolor= "#7c7c7c";
            _tooltipText.text =
                $"<b><color=#{hex}>{roleName}</color></b>  <size=58%><color={teamcolor}>{teamName}</color></size>{desc}";
            _tooltipRoot.SetActive(true);
        }

        private void HideTooltip()
        {
            if (_tooltipText != null) {
                _tooltipText.text = PickPrompt;
                _tooltipText.fontSize = 2f;
            }
        }

        private void DestroyTooltip()
        {
            if (_tooltipRoot != null)
            {
                try { MiraAPI.Utilities.Extensions.DeepDestroy(_tooltipRoot, false); } catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
                _tooltipRoot = null!;
                _tooltipText = null!;
            }
        }

        private void HideTimerProgressLine()
        {
            if (_timerTrack != null) _timerTrack.SetActive(false);
            if (_timerFill != null) _timerFill.SetActive(false);
        }

        private void UpdateTimerProgressLine(float secondsLeft, bool urgent)
        {
            if (_timerTrack == null || _timerFill == null) return;
            _timerTrack.SetActive(true);
            _timerFill.SetActive(true);

            float duration = Mathf.Max(1f, DraftManager.TurnDuration);
            float progress = Mathf.Clamp01(secondsLeft / duration);
            float width = Mathf.Max(0.03f, TimerProgressWidth * progress);
            _timerFill.transform.localScale = new Vector3(width, urgent ? 0.075f : 0.055f, 1f);
            _timerFill.transform.localPosition = new Vector3(
                -TimerProgressWidth * 0.5f + width * 0.5f, -0.32f, -0.01f);
            if (_timerFillRenderer != null)
            {
                _timerFillRenderer.color = urgent
                    ? new Color(1f, 0.22f, 0.22f, 0.96f)
                    : new Color(1f, 0.82f, 0.12f, 0.92f);
            }
        }

        public void CacheOfferedRoles(ushort[] offeredRoleIds)
        {
            _offeredRoleIds = offeredRoleIds ?? Array.Empty<ushort>();
        }

        public static void Show(ushort[] roleIds)
        {
            Hide();
            if (HudManager.Instance?.FullScreen != null)
                HudManager.Instance.FullScreen.color = Color.clear;
            var go = new GameObject("DraftScreenController");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<DraftScreenController>();
            Instance._offeredRoleIds = roleIds ?? Array.Empty<ushort>();
            Instance._hasPicked = false;
            Instance._cardsReady = false;
            Instance._localTimeLeft = -1f;
            DraftStatusOverlay.SetState(OverlayState.BackgroundOnly);
            Instance.BuildScreen();
        }

        public static void Hide()
        {
            if (Instance == null) return;

            DraftStatusOverlay.SetState(DraftManager.IsDraftActive ? OverlayState.Waiting : OverlayState.Hidden);
            Instance.DestroyBottomTimer();
            Instance.DestroySelectionBackdrop();
            Instance.DestroyTooltip();

            if (Instance._screenRoot != null)
                MiraAPI.Utilities.Extensions.DeepDestroy(Instance._screenRoot, false);

            if (HudManager.Instance != null)
            {
                var hud = HudManager.Instance.transform;
                for (int i = hud.childCount - 1; i >= 0; i--)
                {
                    var child = hud.GetChild(i);
                    if (child != null && child.name.StartsWith("DraftCard_", StringComparison.Ordinal))
                        MiraAPI.Utilities.Extensions.DeepDestroy(child.gameObject, false);
                }
            }

            Instance._idleCardCaches.Clear();
            Instance._idleCardCacheByTransform.Clear();
            var go = Instance.gameObject;
            Instance = null!;

            if (go != null)
                MiraAPI.Utilities.Extensions.DeepDestroy(go, false);

            try
            {
                MiraAPI.Utilities.Extensions.ClearGarbageCollector();
            }
            catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
        }

        private void BuildScreen()
        {
            if (HudManager.Instance == null) return;
            if (HudManager.Instance.FullScreen != null)
                HudManager.Instance.FullScreen.color = Color.clear;

            BuildBottomTimer();
            BuildSelectionBackdrop();

            GameObject? prefab = null;
            try
            {
                var bundle = TouAssets.MainBundle;
                if (bundle != null)
                    prefab = bundle.LoadAsset(PrefabName) as GameObject;
            }
            catch (Exception ex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error,$"[DraftScreenController] Bundle load failed: {ex.Message}");
            }

            if (prefab == null)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftScreenController] SelectRoleGame prefab not found.");
                DestroyBottomTimer();
                DestroySelectionBackdrop();
                MiraAPI.Utilities.Extensions.DeepDestroy(gameObject, false); Instance = null!; return;
            }

            _screenRoot = Instantiate(prefab);
            _screenRoot.name = "DraftRoleSelectScreen";
            DontDestroyOnLoad(_screenRoot);

            if (HudManager.Instance != null)
            {
                _screenRoot.transform.SetParent(HudManager.Instance.transform, false);
                _screenRoot.transform.localPosition = Vector3.zero;
            }

            var holderGo = _screenRoot.transform.Find("RoleCardHolder");
            var statusGo = _screenRoot.transform.Find("Status");
            var rolesHolder = _screenRoot.transform.Find("Roles");

            HidePrefabBackdrop(_screenRoot.transform, rolesHolder, statusGo, holderGo);

            if (statusGo != null)
            {
                var statusText = statusGo.GetComponent<TextMeshPro>();
                if (statusText != null)
                {
                    statusText.font = HudManager.Instance!.TaskPanel.taskText.font;
                    statusText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
                    statusText.text = string.Empty;
                    statusGo.gameObject.SetActive(false);
                }
            }

            if (holderGo == null)
            {
                DestroyBottomTimer();
                DestroySelectionBackdrop();
                MiraAPI.Utilities.Extensions.DeepDestroy(_screenRoot, false);
                MiraAPI.Utilities.Extensions.DeepDestroy(gameObject, false); Instance = null!; return;
            }

            var rolePrefab = holderGo.gameObject;

            BuildTooltip(_screenRoot.transform);

            var idList = new List<ushort>();
            if (_offeredRoleIds != null) idList.AddRange(_offeredRoleIds);
            var cards = DraftUiManager.BuildCards(idList);

            int totalCards = cards.Count;
            float cardScale = CardScaleForCount;
            float spacing = SpacingForCount(totalCards);
            bool useGrid = totalCards > 5;

            if (useGrid)
            {
                var hLayout = rolesHolder?.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (hLayout != null) hLayout.enabled = false;
                var rt = rolesHolder?.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 12f);
            }
            else
            {
                var layoutGroup = rolesHolder?.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (layoutGroup != null) layoutGroup.spacing = spacing;
            }

            for (int i = 0; i < totalCards; i++)
            {
                var card = cards[i];
                int capturedIdx = card.Index;

                var btn = CreateCard(
                    rolePrefab, rolesHolder!,
                    card.RoleName, card.TeamName,
                    card.Icon,
                    i, totalCards, card.Color, card.Faction,
                    cardScale, useGrid, spacing, card.Description);

                btn.OnClick.RemoveAllListeners();
                btn.OnClick.AddListener((UnityAction)(() => OnCardClicked(capturedIdx)));
            }

            Coroutines.Start(CoAnimateCards(rolesHolder!, cardScale, useGrid, totalCards));
        }

        private static void HidePrefabBackdrop(Transform root, Transform rolesHolder, Transform statusGo, Transform holderGo)
        {
            if (root == null) return;
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr == null || sr.gameObject == null) continue;
                var tr = sr.transform;
                if (IsChildOf(tr, rolesHolder) || IsChildOf(tr, statusGo) || IsChildOf(tr, holderGo)) continue;
                string n = sr.gameObject.name.ToLowerInvariant();
                Vector3 size = sr.bounds.size;
                if (n.Contains("background") || n.Contains("backdrop") || n.Contains("bg") || size.x > 4f || size.y > 3f)
                    sr.enabled = false;
            }
        }

        private static bool IsChildOf(Transform child, Transform parent)
        {
            if (child == null || parent == null) return false;
            var cur = child;
            while (cur != null)
            {
                if (cur == parent) return true;
                cur = cur.parent;
            }
            return false;
        }

        private void BuildSelectionBackdrop()
        {
            if (HudManager.Instance == null) return;
            DestroySelectionBackdrop();
            _selectionBackdropParticles.Clear();
            _selectionBackdropParticleBase.Clear();
            _selectionBackdropBeams.Clear();

            var cam = Camera.main;
            float camH = cam != null ? cam.orthographicSize * 2f : 6f;
            float camW = camH * ((float)Screen.width / Screen.height);

            _selectionBackdrop = new GameObject("DraftSelectionBackdropRoot");
            _selectionBackdrop.transform.SetParent(HudManager.Instance.transform, false);
            _selectionBackdrop.transform.localPosition = new Vector3(0f, 0f, 0.7f);

            MakeBackdropSprite("DraftSelectionBackdropBase", Vector3.zero,
                new Vector3(camW, camH, 1f), MakeWhiteSprite(),
                new Color(0.010f, 0.006f, 0.035f, 1f), 46);

            _selectionBackdropWash = MakeBackdropSprite("DraftSelectionBackdropWash",
                new Vector3(0f, 0.05f, -0.02f),
                new Vector3(camW * 1.05f, camH * 0.55f, 1f), MakeSoftGlowSprite(),
                new Color(0f, 0.95f, 1f, 0.18f), 47);

            var selectionBackdropBeam = MakeBackdropSprite("DraftSelectionBackdropBeam",
                new Vector3(-camW * 0.36f, 0f, -0.04f),
                new Vector3(camW * 0.18f, camH * 0.9f, 1f), MakeSoftGlowSprite(),
                new Color(1f, 0.84f, 0.2f, 0.16f), 49);
            selectionBackdropBeam.transform.localRotation = Quaternion.Euler(0f, 0f, -13f);
            _selectionBackdropBeams.Add(selectionBackdropBeam);

            var beam2 = MakeBackdropSprite("DraftSelectionBackdropBeam",
                new Vector3(camW * 0.34f, 0f, -0.04f),
                new Vector3(camW * 0.14f, camH * 0.85f, 1f), MakeSoftGlowSprite(),
                new Color(0.72f, 0.42f, 1f, 0.12f), 49);
            beam2.transform.localRotation = Quaternion.Euler(0f, 0f, 14f);
            _selectionBackdropBeams.Add(beam2);

            _selectionBackdropHorizon = MakeBackdropSprite("DraftSelectionBackdropHorizon",
                new Vector3(0f, -0.58f, -0.03f),
                new Vector3(camW * 0.9f, 0.06f, 1f), MakeSoftGlowSprite(),
                new Color(0f, 0.95f, 1f, 0.28f), 50);

            _selectionBackdropFlash = MakeBackdropSprite("DraftSelectionBackdropFlash",
                Vector3.zero, new Vector3(camW, camH, 1f), MakeSoftGlowSprite(),
                new Color(1f, 1f, 1f, 0f), 53);

            for (int i = 0; i < 18; i++)
            {
                float x = Mathf.Sin(i * 1.91f) * camW * 0.46f;
                float y = Mathf.Cos(i * 1.37f) * camH * 0.37f;
                var p = MakeBackdropSprite("DraftSelectionBackdropParticle",
                    new Vector3(x, y, -0.05f),
                    Vector3.one * (0.035f + (i % 4) * 0.012f), MakeSoftGlowSprite(),
                    new Color(0.45f, 0.95f, 1f, 0.24f), 52);
                _selectionBackdropParticles.Add(p);
                _selectionBackdropParticleBase.Add(p.transform.localPosition);
            }
        }

        private SpriteRenderer MakeBackdropSprite(string name, Vector3 pos, Vector3 scale,
            Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_selectionBackdrop.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingLayerName = "UI";
            sr.sortingOrder = order;
            return sr;
        }

        private void DestroySelectionBackdrop()
        {
            if (_selectionBackdrop != null)
            {
                try { MiraAPI.Utilities.Extensions.DeepDestroy(_selectionBackdrop, true); } catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"Ignored Exception: {e.Message}"); }
                _selectionBackdrop = null!;
                _selectionBackdropWash = null!;
                _selectionBackdropHorizon = null!;
                _selectionBackdropFlash = null!;
                _selectionBackdropParticles.Clear();
                _selectionBackdropParticleBase.Clear();
                _selectionBackdropBeams.Clear();
            }
        }

        private void UpdateSelectionBackdrop()
        {
            if (_selectionBackdrop == null) return;

            _backdropTime += Time.deltaTime;
            _backdropPulse = Mathf.MoveTowards(_backdropPulse, 0f, Time.deltaTime * 1.35f);

            Color idle = new Color(0.0f, 0.55f, 0.95f, 0.24f);
            Color wash = Color.Lerp(idle, _backdropPulseColor, Mathf.Clamp01(_backdropPulse));
            if (_selectionBackdropWash != null)
            {
                wash.a = 0.16f + _backdropPulse * 0.26f + (Mathf.Sin(_backdropTime * 0.9f) + 1f) * 0.025f;
                _selectionBackdropWash.color = wash;
                _selectionBackdropWash.transform.localScale = new Vector3(
                    9.4f + _backdropPulse * 0.8f, 3.35f + _backdropPulse * 0.4f, 1f);
            }

            for (int i = 0; i < _selectionBackdropBeams.Count; i++)
            {
                var beam = _selectionBackdropBeams[i];
                if (beam == null) continue;
                var c = Color.Lerp(new Color(0f, 0.95f, 1f, 0.12f),
                    _backdropPulseColor, Mathf.Clamp01(_backdropPulse));
                c.a = 0.14f + _backdropPulse * 0.38f;
                beam.color = c;
                float dir = i == 0 ? -1f : 1f;
                beam.transform.localPosition = new Vector3(
                    dir * (2.8f + Mathf.Sin(_backdropTime * 0.55f + i) * 0.35f),
                    Mathf.Cos(_backdropTime * 0.42f + i) * 0.18f, -0.04f);
            }

            if (_selectionBackdropHorizon != null)
            {
                var c = Color.Lerp(new Color(0f, 0.95f, 1f, 0.28f),
                    _backdropPulseColor, Mathf.Clamp01(_backdropPulse));
                c.a = 0.24f + _backdropPulse * 0.32f;
                _selectionBackdropHorizon.color = c;
                _selectionBackdropHorizon.transform.localScale = new Vector3(
                    8.2f + Mathf.Sin(_backdropTime * 0.65f) * 0.22f,
                    0.06f + _backdropPulse * 0.04f, 1f);
            }

            if (_selectionBackdropFlash != null)
            {
                var c = _backdropPulseColor;
                c.a = _backdropPulse * 0.12f;
                _selectionBackdropFlash.color = c;
            }

            for (int i = 0; i < _selectionBackdropParticles.Count; i++)
            {
                var p = _selectionBackdropParticles[i];
                if (p == null || i >= _selectionBackdropParticleBase.Count) continue;
                Vector3 b = _selectionBackdropParticleBase[i];
                float phase = i * 0.73f;
                p.transform.localPosition = b + new Vector3(
                    Mathf.Sin(_backdropTime * 0.8f + phase) * 0.22f,
                    Mathf.Cos(_backdropTime * 0.55f + phase) * 0.18f, 0f);
                p.transform.localScale = Vector3.one *
                    (0.035f + (i % 4) * 0.012f + _backdropPulse * 0.045f);
                var c = Color.Lerp(new Color(0.5f, 0.95f, 1f, 0.22f),
                    _backdropPulseColor, Mathf.Clamp01(_backdropPulse));
                c.a = 0.18f + (Mathf.Sin(_backdropTime * 1.4f + phase) + 1f) * 0.10f
                    + _backdropPulse * 0.28f;
                p.color = c;
            }
        }

        private void RegisterIdleCard(Transform card, Transform holder)
        {
            if (card == null) return;
            if (!_idleCardCacheByTransform.TryGetValue(card, out var cache))
            {
                cache = new DraftCardIdleCache { Card = card, Holder = holder };
                _idleCardCacheByTransform[card] = cache;
                _idleCardCaches.Add(cache);
            }
            cache.BasePosition = card.localPosition;
            cache.BaseRotation = card.localRotation;
        }

        private void UpdateCardIdleMotion()
        {
            if (!_cardsReady || _hasPicked) return;
            for (int i = 0; i < _idleCardCaches.Count; i++)
            {
                var cache = _idleCardCaches[i];
                var card = cache.Card;
                if (card == null) continue;
                if (card == _hoveredCard) continue;

                float phase = i * 0.79f;
                float bob = Mathf.Sin(_backdropTime * 1.15f + phase) * 0.035f;
                float sway = Mathf.Sin(_backdropTime * 0.62f + phase) * 0.016f;
                card.localPosition = cache.BasePosition + new Vector3(sway, bob, 0f);
                card.localRotation = cache.BaseRotation *
                    Quaternion.Euler(0f, 0f, Mathf.Sin(_backdropTime * 0.9f + phase) * 1.35f);
            }
        }
        private static PassiveButton CreateCard(
            GameObject rolePrefab,
            Transform rolesHolder,
            string roleName,
            string teamName,
            Sprite icon,
            int cardIndex,
            int totalCards,
            Color color,
            DraftFaction faction,
            float cardScale,
            bool useGrid = false,
            float spacing = 0f,
            string description = "")
        {
            var newRoleObj = UnityEngine.Object.Instantiate(rolePrefab, rolesHolder);
            var actualCard = newRoleObj!.transform.GetChild(0);
            var roleText = actualCard.GetChild(0).GetComponent<TextMeshPro>();
            var roleImage = actualCard.GetChild(1).GetComponent<SpriteRenderer>();
            var teamText = actualCard.GetChild(2).GetComponent<TextMeshPro>();
            var passiveButton = actualCard.GetComponent<PassiveButton>();
            var rollover = actualCard.GetComponent<ButtonRolloverHandler>();

            newRoleObj.name = $"DraftCard_{cardIndex}";

            int tiltIndex = useGrid ? (cardIndex % Mathf.CeilToInt(totalCards / 2f)) : cardIndex;
            float tiltScale = Mathf.Lerp(1f, 0.25f, Mathf.InverseLerp(3f, 9f, totalCards));
            float randZ = (-10f + tiltIndex * 5f) * tiltScale
                          + UnityEngine.Random.Range(-1.5f, 1.5f) * tiltScale;

            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (Instance == null) return;
                Instance._hoveredCard = actualCard;
                newRoleObj.transform.localScale = Vector3.one * (cardScale * 1.075f);
                Instance.ShowTooltip(roleName, teamName, description, color);
            }));

            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (Instance == null) return;
                if (Instance._hoveredCard == actualCard)
                    Instance._hoveredCard = null!;
                newRoleObj.transform.localScale = Vector3.one * cardScale;
                Instance.HideTooltip();
            }));

            newRoleObj.transform.localRotation = Quaternion.Euler(0f, 0f, -randZ);

            if (useGrid)
            {
                int cols = Mathf.CeilToInt(totalCards / 2f);
                int row = cardIndex / cols;
                int col = cardIndex % cols;
                float cardW = 2.5f * cardScale;
                float cardH = 3.7f * cardScale;
                float xGap = cardW + spacing;
                float yGap = cardH + spacing * 0.5f;
                float totalW = cols * xGap - spacing;
                float startX = -totalW / 2f + cardW / 2f;
                float startY = yGap / 2f;
                newRoleObj.transform.localPosition = new Vector3(
                    startX + col * xGap, startY - row * yGap, cardIndex);
            }
            else
            {
                newRoleObj.transform.localPosition = new Vector3(
                    newRoleObj.transform.localPosition.x, 0f, cardIndex);
            }

            newRoleObj.transform.localScale = Vector3.one * cardScale;

            roleText.text = roleName;
            teamText.text = teamName;
            roleImage.sprite = icon;
            roleImage.SetSizeLimit(2.8f);
            var cardBgRenderer = actualCard.GetComponent<SpriteRenderer>();
            if (cardBgRenderer != null) cardBgRenderer.color = color;
            roleImage.color = Color.white;

            teamText.fontSizeMax = Mathf.Lerp(4f, 2f, Mathf.InverseLerp(3f, 9f, totalCards));
            teamText.enableAutoSizing = true;

            rollover.OutColor = color;
            rollover.OverColor = new Color(
                Mathf.Min(color.r * 1.3f, 1f),
                Mathf.Min(color.g * 1.3f, 1f),
                Mathf.Min(color.b * 1.3f, 1f),
                color.a);
            roleText.color = color;
            teamText.fontSizeMax = 3.8f;
            teamText.color = GetTeamColor(faction);

            foreach (var sr in newRoleObj.GetComponentsInChildren<SpriteRenderer>(true))
            { sr.sortingLayerName = "UI"; sr.sortingOrder = 70; }
            foreach (var tmp in newRoleObj.GetComponentsInChildren<TMPro.TMP_Text>(true))
            {
                var rr = tmp.GetComponent<Renderer>();
                if (rr != null) { rr.sortingLayerName = "UI"; rr.sortingOrder = 74; }
            }

            return passiveButton;
        }

        private static IEnumerator CoAnimateCards(Transform rolesHolder, float cardScale, bool useGrid, int totalCards)
        {
            if (rolesHolder == null) yield break;
            int cols = Mathf.CeilToInt(totalCards / 2f);

            for (int i = 0; i < rolesHolder.childCount; i++)
            {
                if (rolesHolder == null) yield break;
                Transform card = rolesHolder.GetChild(i);
                if (card == null || card.childCount == 0) continue;
                Transform child = card.GetChild(0);
                if (child == null) continue;
                int animIndex = useGrid ? (i % cols) : i;
                float stagger = Mathf.Min(i, 7) * DraftCardRevealStaggerSeconds;
                Coroutines.Start(CoAnimateCardRevealSequence(child, card, animIndex, totalCards, cardScale, stagger));
            }

            yield return CoWaitForSharedReadyGate(totalCards);
            if (Instance != null) Instance._cardsReady = true;
            DraftNetworkHelper.NotifyPickerReady();
        }

        private static IEnumerator CoWaitForSharedReadyGate(int totalCards)
        {
            float revealBudget = 0.48f + Mathf.Min(Mathf.Max(0, totalCards - 1), 7) * DraftCardRevealStaggerSeconds;
            yield return new WaitForSeconds(Mathf.Max(0f, Mathf.Max(DraftReadyGateSeconds, revealBudget)));
        }

        private static IEnumerator CoAnimateCardRevealSequence(Transform child, Transform holder, int animIndex, int totalCards, float cardScale, float stagger)
        {
            if (stagger > 0f)
                yield return new WaitForSeconds(stagger);
            if (child == null) yield break;

            yield return CoAnimateCardIn(child, animIndex, totalCards, cardScale);
            if (child == null) yield break;

            Instance?.RegisterIdleCard(child, holder);

            try
            {
                Coroutines.Start(MiscUtils.BetterBloop(child, finalSize: cardScale, duration: 0.22f, intensity: 0.16f));
            }
            catch (Exception bex)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Error, $"[DraftScreen] BetterBloop failed: {bex.Message}");
            }

        }

        private static IEnumerator CoAnimateCardIn(Transform card, int currentCard, int totalCards, float cardScale)
        {
            if (card == null) yield break;

            float randY = (currentCard * currentCard * 0.5f - currentCard) * 0.05f
                          + UnityEngine.Random.Range(-0.08f, 0f);
            float randZ = -10f + currentCard * 5f + UnityEngine.Random.Range(-1.5f, 0f);
            if (currentCard == 0) { randY = 0f; randZ = -2f; }

            float center = (totalCards - 1) * 0.5f;
            float sideDirection = currentCard <= center ? -1f : 1f;
            float side = totalCards <= 1 ? 0f : sideDirection;
            Vector3 targetPos = new Vector3(card.localPosition.x, randY, card.localPosition.z);
            Vector3 startPos = targetPos + new Vector3(side * 2.7f, -2.35f, 0f);
            Quaternion targetRot = Quaternion.Euler(0f, 0f, -randZ);
            Quaternion startRot = Quaternion.Euler(0f, 0f, -randZ + side * 26f + 10f);

            if (card == null) yield break;
            card.localPosition = startPos;
            card.localRotation = startRot;
            card.localScale = Vector3.one * (cardScale * 0.62f);
            card.parent.gameObject.SetActive(true);

            const float duration = 0.46f;
            for (float timer = 0f; timer < duration; timer += Time.deltaTime)
            {
                if (card == null) yield break;
                float t = Mathf.Clamp01(timer / duration);
                float eased = EaseOutBack(t);
                card.localPosition = Vector3.LerpUnclamped(startPos, targetPos, eased);
                card.localRotation = Quaternion.Lerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
                card.localScale = Vector3.one * Mathf.Lerp(cardScale * 0.62f, cardScale, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            if (card == null) yield break;
            card.localPosition = targetPos;
            card.localRotation = targetRot;
            card.localScale = Vector3.one * cardScale;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        private float _localTimeLeft = -1f;
        private bool _cardsReady;

        private void Update()
        {
            if (HudManager.Instance?.FullScreen != null)
                HudManager.Instance.FullScreen.color = Color.clear;

            UpdateSelectionBackdrop();
            UpdateCardIdleMotion();

            if (_timerText != null)
            {
                if (_hasPicked || !DraftManager.IsDraftActive || !_cardsReady)
                {
                    _timerText.text = string.Empty;
                    HideTimerProgressLine();
                }
                else
                {
                    int secs;
                    if (AmongUsClient.Instance.AmHost)
                    {
                        secs = Mathf.Max(0, Mathf.CeilToInt(DraftManager.TurnTimeLeft));
                    }
                    else
                    {
                        if (_localTimeLeft < -0.5f) _localTimeLeft = DraftManager.TurnDuration;
                        if (_localTimeLeft > 0f) _localTimeLeft -= Time.deltaTime;
                        secs = Mathf.Max(0, Mathf.CeilToInt(_localTimeLeft));
                    }

                    bool urgent = secs <= 5;
                    string color = urgent ? "#FF5555" : "#FFD700";
                    float timerPulse = urgent ? 1f + Mathf.Sin(Time.time * 10f) * 0.08f : 1f;
                    _timerText.transform.localScale = Vector3.one * timerPulse;
                    _timerText.text =
                        $"<color={color}><b>{secs} Second{(secs != 1 ? "s" : "")} Remaining</b></color>";

                    float timeForBar = AmongUsClient.Instance.AmHost
                        ? DraftManager.TurnTimeLeft
                        : _localTimeLeft;
                    UpdateTimerProgressLine(timeForBar, urgent);
                }
            }
        }

        private void OnCardClicked(int index)
        {
            if (_hasPicked) return;

            _hasPicked = true;
            DraftNetworkHelper.SendPickToHost(index);
            Invoke(nameof(DestroySelf), 1.2f);
        }

        private static void DestroySelf() => Hide();
        private static Sprite _softGlowSprite;
        private static Sprite MakeSoftGlowSprite()
        {
            if (_softGlowSprite != null) return _softGlowSprite;
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
            _softGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _softGlowSprite.hideFlags = HideFlags.HideAndDontSave;
            return _softGlowSprite;
        }

        private static Sprite _whiteSprite;
        private static Sprite MakeWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            _whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return _whiteSprite;
        }
    }
}

