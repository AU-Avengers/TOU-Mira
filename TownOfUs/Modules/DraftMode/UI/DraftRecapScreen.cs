using Reactor.Utilities;
using System.Collections;
using TownOfUs.Options;
using TMPro;
using UnityEngine;


namespace TownOfUs.Modules.DraftMode
{

    public sealed class DraftRecapScreen : MonoBehaviour
    {
        private const float DisplaySeconds = 5f;
        private const float FadeSeconds    = 0.35f;

        private static DraftRecapScreen _instance;
        private Action _onComplete;

        private GameObject      _bgOverlay;
        private GameObject      _backdropArt;
        private SpriteRenderer  _backdropWashRenderer;
        private SpriteRenderer  _backdropHorizonRenderer;
        private readonly List<SpriteRenderer> _backdropBeamRenderers     = new();
        private readonly List<SpriteRenderer> _backdropParticleRenderers = new();
        private readonly List<Vector3>        _backdropParticleBasePos   = new();
        private float _animTime;

        private GameObject  _textRoot;
        private TextMeshPro _headerText;
        private readonly List<TextMeshPro> _rowTexts = new();
        private float _camW = 8f;

        public static void Show(
            List<(int slot, string label, string colorHex)> entries,
            DraftRecapMode mode,
            Action onComplete = null!)
        {
            if (entries == null || entries.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            EnsureExists();
            _instance._onComplete = onComplete;
            _instance.BuildContent(entries, mode);
            Coroutines.Start(_instance.CoDisplay());
        }

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null!;
        }

        private void Update()
        {
            if (_backdropArt == null || !_backdropArt.activeSelf) return;

            _animTime += Time.deltaTime;
            float pulse = (Mathf.Sin(_animTime * 1.7f) + 1f) * 0.5f;

            if (_backdropWashRenderer != null)
            {
                _backdropWashRenderer.color = new Color(0f, 0.78f, 1f,
                    0.09f + pulse * 0.07f);
            }

            for (int i = 0; i < _backdropBeamRenderers.Count; i++)
            {
                var beam = _backdropBeamRenderers[i];
                if (beam == null) continue;
                float dir = (i == 0) ? -1f : 1f;
                beam.color = new Color(beam.color.r, beam.color.g, beam.color.b,
                    0.08f + pulse * 0.06f);
                beam.transform.localPosition = new Vector3(
                    dir * (2.8f + Mathf.Sin(_animTime * 0.55f + i) * 0.35f),
                    Mathf.Cos(_animTime * 0.42f + i) * 0.18f,
                    beam.transform.localPosition.z);
            }

            if (_backdropHorizonRenderer != null)
                _backdropHorizonRenderer.color = new Color(0f, 0.95f, 1f, 0.18f + pulse * 0.08f);

            for (int i = 0; i < _backdropParticleRenderers.Count; i++)
            {
                var p = _backdropParticleRenderers[i];
                if (p == null) continue;
                var basePos = _backdropParticleBasePos[i];
                float spd = 0.2f + (i % 3) * 0.15f;
                p.transform.localPosition = basePos + new Vector3(
                    Mathf.Sin(_animTime * spd + i) * 0.08f,
                    Mathf.Cos(_animTime * spd * 1.2f + i) * 0.08f,
                    0f);
            }
        }

        private static void EnsureExists()
        {
            if (_instance != null) return;
            var go = new GameObject("DraftRecapScreen");
            DontDestroyOnLoad(go);
            go.AddComponent<DraftRecapScreen>();
        }

        private void BuildUI()
        {
            if (HudManager.Instance == null) return;

            var cam   = Camera.main;
            float camH = cam != null ? cam.orthographicSize * 2f : 6f;
            float camW = camH * ((float)Screen.width / Screen.height);
            _camW = camW;

            _bgOverlay = new GameObject("DraftRecapBg");
            _bgOverlay.transform.SetParent(HudManager.Instance.transform, false);
            _bgOverlay.transform.localPosition = new Vector3(0f, 0f, 1f);

            var bgSr              = _bgOverlay.AddComponent<SpriteRenderer>();
            bgSr.sprite           = MakeWhiteSprite();
            bgSr.color            = new Color(0f, 0f, 0f, 1f);
            bgSr.sortingLayerName = "UI";
            bgSr.sortingOrder     = 500;
            _bgOverlay.transform.localScale = new Vector3(camW, camH, 1f);
            _bgOverlay.SetActive(false);

            BuildBackdropArt(camW, camH);

            _textRoot = new GameObject("DraftRecapTextRoot");
            _textRoot.transform.SetParent(HudManager.Instance.transform, false);
            _textRoot.transform.localPosition = new Vector3(0f, 0f, -20f);

            var headerGo = new GameObject("RecapHeader");
            headerGo.transform.SetParent(_textRoot.transform, false);
            headerGo.transform.localPosition = new Vector3(0f, 2.1f, 0f);

            _headerText = headerGo.AddComponent<TextMeshPro>();
            CopyFont(_headerText);
            _headerText.fontSize           = 4f;
            _headerText.alignment          = TextAlignmentOptions.Center;
            _headerText.enableWordWrapping = false;
            _headerText.color              = new Color(0.2f, 0.85f, 1f);
            _headerText.text               = "<b>DRAFT RECAP</b>";
            ApplySortingOrder(_headerText, 520);

            _textRoot.SetActive(false);
        }

        private void BuildBackdropArt(float camW, float camH)
        {
            _backdropArt = new GameObject("DraftRecapBackdropArt");
            _backdropArt.transform.SetParent(HudManager.Instance.transform, false);
            _backdropArt.transform.localPosition = new Vector3(0f, 0f, 0.85f);

            _backdropWashRenderer = MakeArtSprite(
                "RecapWash",
                new Vector3(0f, 0.02f, 0.03f),
                new Vector3(camW * 0.98f, camH * 0.42f, 1f),
                MakeSoftGlowSprite(),
                new Color(0f, 0.78f, 1f, 0.12f), 501);

            var leftBeam = MakeArtSprite(
                "RecapBeamLeft",
                new Vector3(-camW * 0.34f, -0.02f, 0.01f),
                new Vector3(camW * 0.13f, camH * 0.82f, 1f),
                MakeSoftGlowSprite(),
                new Color(1f, 0.82f, 0.12f, 0.12f), 502);
            leftBeam.transform.localRotation = Quaternion.Euler(0f, 0f, -13f);
            _backdropBeamRenderers.Add(leftBeam);

            var rightBeam = MakeArtSprite(
                "RecapBeamRight",
                new Vector3(camW * 0.34f, -0.02f, 0.01f),
                new Vector3(camW * 0.13f, camH * 0.82f, 1f),
                MakeSoftGlowSprite(),
                new Color(0.72f, 0.42f, 1f, 0.10f), 502);
            rightBeam.transform.localRotation = Quaternion.Euler(0f, 0f, 13f);
            _backdropBeamRenderers.Add(rightBeam);

            _backdropHorizonRenderer = MakeArtSprite(
                "RecapHorizon",
                new Vector3(0f, -camH * 0.18f, 0f),
                new Vector3(camW * 0.82f, 0.055f, 1f),
                MakeSoftGlowSprite(),
                new Color(0f, 0.95f, 1f, 0.22f), 503);

            for (int i = 0; i < 6; i++)
            {
                float x = Mathf.Sin(i * 1.77f) * camW * 0.42f;
                float y = Mathf.Cos(i * 1.31f) * camH * 0.28f;
                var p = MakeArtSprite(
                    "RecapParticle",
                    new Vector3(x, y, -0.02f),
                    Vector3.one * (0.035f + (i % 3) * 0.012f),
                    MakeSoftGlowSprite(),
                    new Color(0.5f, 0.95f, 1f, 0.20f), 504);
                _backdropParticleRenderers.Add(p);
                _backdropParticleBasePos.Add(p.transform.localPosition);
            }

            _backdropArt.SetActive(false);
        }

        private SpriteRenderer MakeArtSprite(string name, Vector3 pos, Vector3 scale,
            Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_backdropArt.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale    = scale;
            var sr              = go.AddComponent<SpriteRenderer>();
            sr.sprite           = sprite;
            sr.color            = color;
            sr.sortingLayerName = "UI";
            sr.sortingOrder     = order;
            return sr;
        }

        private void BuildContent(
            List<(int slot, string label, string colorHex)> entries,
            DraftRecapMode mode)
        {

            if (_textRoot == null) BuildUI();
            if (_textRoot == null) return;

            foreach (var t in _rowTexts)
                if (t != null) MiraAPI.Utilities.Extensions.DeepDestroy(t.gameObject, false);
            _rowTexts.Clear();
            try { MiraAPI.Utilities.Extensions.ClearGarbageCollector(); } catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"GC Error: {e.Message}"); }

            string modeLabel = mode == DraftRecapMode.Role ? "Role" : "Faction";
            _headerText.text = $"<b>DRAFT RECAP</b>  <size=60%><color=#88FFFF>({modeLabel})</color></size>";
            var sorted = entries.OrderBy(e => e.slot).ToList();
            int count  = sorted.Count;
            const int maxSingleColumn = 8;
            bool twoColumns = count > maxSingleColumn;
            int rowsPerColumn = twoColumns ? (count + 1) / 2 : count;
            rowsPerColumn = Math.Max(1, rowsPerColumn);
            float rowHeight  = Mathf.Min(0.7f, 3.6f / rowsPerColumn);
            float startY     = (rowsPerColumn - 1) * rowHeight * 0.5f;
            float colOffsetX = twoColumns ? _camW * 0.23f : 0f;

            var slotToName = new Dictionary<int, string>();
            if (PlayerControl.AllPlayerControls != null)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null) continue;
                    var state = DraftManager.GetStateForPlayer(pc.PlayerId);
                    if (state != null)
                        slotToName[state.SlotNumber] = pc.Data?.PlayerName ?? $"Player {pc.PlayerId}";
                }
            }

            for (int i = 0; i < sorted.Count; i++)
            {
                var (slot, label, colorHex) = sorted[i];

                int rowIndex     = twoColumns ? i / 2 : i;
                bool inLeftColumn = !twoColumns || i % 2 == 0;
                float x = 0f;
                if (twoColumns) x = inLeftColumn ? -colOffsetX : colOffsetX;
                float y = startY - rowIndex * rowHeight;

                slotToName.TryGetValue(slot, out var playerName);
                playerName ??= $"Slot {slot}";

                var rowGo = new GameObject($"RecapRow_{i}");
                rowGo.transform.SetParent(_textRoot.transform, false);
                rowGo.transform.localPosition = new Vector3(x, y - 0.05f, 0f);

                var tmp = rowGo.AddComponent<TextMeshPro>();
                CopyFont(tmp);
                tmp.fontSize           = Mathf.Clamp(rowHeight * 3.4f, 1.6f, 2.8f);
                tmp.alignment          = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.text = $"<color=#CCCCCC>Player {slot}</color>  <color=#{colorHex}><b>{label}</b></color>";
                ApplySortingOrder(tmp, 520);

                _rowTexts.Add(tmp);
            }
        }

        private IEnumerator CoDisplay()
        {
            _bgOverlay?.SetActive(true);
            _backdropArt?.SetActive(true);
            _textRoot?.SetActive(true);

            yield return CoFadeText(0f, 1f, FadeSeconds);
            yield return new WaitForSeconds(DisplaySeconds);
            yield return CoFadeText(1f, 0f, FadeSeconds);

            _bgOverlay?.SetActive(false);
            _backdropArt?.SetActive(false);
            _textRoot?.SetActive(false);

            CleanupUI();

            var callback = _onComplete;
            _onComplete = null!;

            MiraAPI.Utilities.Extensions.DeepDestroy(gameObject, true);
            callback?.Invoke();
        }

        private IEnumerator CoFadeText(float from, float to, float duration)
        {

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float alpha = Mathf.Lerp(from, to, t);
                if (_headerText != null) _headerText.alpha = alpha;
                foreach (var row in _rowTexts) if (row != null) row.alpha = alpha;

                yield return null;
            }
            float final = to;
            if (_headerText != null) _headerText.alpha = final;
            foreach (var row in _rowTexts) if (row != null) row.alpha = final;
        }

        private void CleanupUI()
        {
            foreach (var t in _rowTexts)
                if (t != null) MiraAPI.Utilities.Extensions.DeepDestroy(t.gameObject, false);
            _rowTexts.Clear();

            if (_backdropArt != null) { MiraAPI.Utilities.Extensions.DeepDestroy(_backdropArt, false); _backdropArt = null!; }
            if (_bgOverlay   != null) { MiraAPI.Utilities.Extensions.DeepDestroy(_bgOverlay,   false); _bgOverlay   = null!; }
            if (_textRoot    != null) { MiraAPI.Utilities.Extensions.DeepDestroy(_textRoot,     false); _textRoot    = null!; }

            _backdropBeamRenderers.Clear();
            _backdropParticleRenderers.Clear();
            _backdropParticleBasePos.Clear();
            _backdropWashRenderer    = null!;
            _backdropHorizonRenderer = null!;
            _headerText              = null!;

            try { MiraAPI.Utilities.Extensions.ClearGarbageCollector(); } catch (Exception e) { MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Info, $"GC Error: {e.Message}"); }
        }

        private static void CopyFont(TextMeshPro tmp)
        {
            if (tmp == null) return;

            var sourceText = HudManager.Instance?.TaskPanel?.taskText;
            if (sourceText == null) return;

            try
            {
                var font = sourceText.font;
                if (font != null)
                    tmp.font = font;

                var material = sourceText.fontMaterial;
                if (material != null)
                    tmp.fontMaterial = material;
            }
            catch (Exception e)
            {
                MiscUtils.LogInfo(Events.TownOfUsEventHandlers.LogLevel.Warning,
                    $"[DraftRecapScreen] CopyFont failed, falling back to default font: {e.Message}");
            }
        }

        private static void ApplySortingOrder(TextMeshPro tmp, int order)
        {
            var r = tmp.GetComponent<Renderer>();
            if (r != null) { r.sortingLayerName = "UI"; r.sortingOrder = order; }
        }

        private static Sprite _softGlowSprite;
        private static Sprite MakeSoftGlowSprite()
        {
            if (_softGlowSprite != null) return _softGlowSprite;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var px = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
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
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            _whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return _whiteSprite;
        }
    }
}

