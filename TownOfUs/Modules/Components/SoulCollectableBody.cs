using HarmonyLib;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class SoulCollectibleBody(nint cppPtr) : MonoBehaviour(cppPtr)
{
    public static readonly List<SoulCollectibleBody> _collectableBodies = [];
    public DeadBody Body { get; set; }
    public PlayerControl Player { get; private set; }
    public bool WasCollected { get; set; }

    public void Awake()
    {
        Body = gameObject.GetComponent<DeadBody>();
    }
    public static void Clear()
    {
        _collectableBodies.Do(x =>
        {
            if (x != null && x.gameObject != null)
            {
                Destroy(x.gameObject);
            }
        });

        _collectableBodies.Clear();
    }
}
