using HarmonyLib;
using Reactor.Utilities.Attributes;
using UnityEngine;


namespace TownOfUs.Modules.DraftMode
{
    [RegisterInIl2Cpp]
    public class DraftTicker(IntPtr ip) : MonoBehaviour(ip)
    {
        private static DraftTicker _instance;

        public static void EnsureExists()
        {
            if (_instance != null) return;
            var go = new GameObject("DraftTicker");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DraftTicker>();
        }

        public static void DestroyIfExists()
        {
            if (_instance == null) return;
            MiraAPI.Utilities.Extensions.DeepDestroy(_instance.gameObject, true);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                MiraAPI.Utilities.Extensions.DeepDestroy(gameObject, true);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null!;
        }

#pragma warning disable S2325, CA1822
        private void Update()
        {
            if (!DraftManager.IsDraftActive) return;
            DraftManager.TurnTimeLeft = Mathf.Max(0f, DraftManager.TurnTimeLeft - Time.deltaTime);
        }
#pragma warning restore S2325, CA1822
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.OnDestroy))]
    public static class DraftTickerHudDestroyPatch
    {
        [HarmonyPostfix]
        public static void Postfix() => DraftTicker.DestroyIfExists();
    }
}


