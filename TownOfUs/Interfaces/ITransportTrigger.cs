using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace TownOfUs.Interfaces;

public interface ITransportTrigger
{
    [HideFromIl2Cpp]
    MonoBehaviour? OnTransport();
}