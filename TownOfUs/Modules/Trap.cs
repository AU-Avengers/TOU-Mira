using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class Trap(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public static readonly List<Trap> _traps = [];

    public readonly Dictionary<byte, float> _players = [];
    public TrapperRole _owner;
    public Transform _transform;
    private static float TrapSize => OptionGroupSingleton<TrapperOptions>.Instance.TrapSize;
    private static float MinAmountOfTimeInTrap => OptionGroupSingleton<TrapperOptions>.Instance.MinAmountOfTimeInTrap;

    public void Update()
    {
        if (_transform == null ||
            !PlayerControl.LocalPlayer ||
            !PlayerControl.LocalPlayer.Data ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !TutorialManager.InstanceExists))
        {
            return;
        }

        foreach (var player in Helpers.GetAlivePlayers())
        {
            // PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"player with byte {player.PlayerId} is {Vector2.Distance(transform.position, player.GetTruePosition())} away");
            if (Vector2.Distance(_transform.position, player.GetTruePosition()) <
                (TrapSize + 0.01f) * ShipStatus.Instance.MaxLightRadius)
            {
                _players.TryAdd(player.PlayerId, 0f);
            }
            else
            {
                _players.Remove(player.PlayerId);
            }

            var entry = player;
            if (_players.ContainsKey(entry.PlayerId))
            {
                _players[entry.PlayerId] += Time.deltaTime;

                var role = entry.Data.Role;

                if (entry.TryGetModifierOfType<ICachedRole>(out var cachedMod))
                {
                    role = cachedMod.CachedRole;
                }

                // Error($"player with byte {entry.PlayerId} is logged with time {_players[entry.PlayerId]}");
                if (_players[entry.PlayerId] > MinAmountOfTimeInTrap && !_owner.TrappedPlayers.Contains(role) &&
                    entry != _owner.Player)
                // Error($"Trap.Updated add role: {role.GetRoleName()}");
                {
                    _owner.TrappedPlayers.Add(role);
                }
            }
        }
    }

    public static void CreateTrap(TrapperRole player, Vector3 location)
    {
        var trapSize = OptionGroupSingleton<TrapperOptions>.Instance.TrapSize * ShipStatus.Instance.MaxLightRadius * 2f;

        var trapPref = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        trapPref.name = "Trap";
        trapPref.transform.localScale = new Vector3(trapSize, trapSize, trapSize);
        Destroy(trapPref.GetComponent<SphereCollider>());
        trapPref.GetComponent<MeshRenderer>().material = AuAvengersAnims.TrapMaterial;
        trapPref.transform.position = location;

        var trap = trapPref.AddComponent<Trap>();
        trap._owner = player;
        trap._transform = trapPref.transform;

        _traps.Add(trap);
    }

    public static void Clear()
    {
        _traps.ProcessAndClear(trap => trap.Destroy());
    }

    public void Destroy()
    {
        if (_transform.gameObject != null)
        {
            Destroy(_transform.gameObject);
        }
        _traps.Remove(this);
    }
}