using System.Collections;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

// Code Review: Should be using a MonoBehaviour
public sealed class Trap : IDisposable
{
    public static readonly List<Trap> _traps = [];

    public readonly Dictionary<byte, float> _players = [];
    public TrapperRole? _owner;
    public Transform? _transform;
    private static float TrapSize => OptionGroupSingleton<TrapperOptions>.Instance.TrapSize;
    private static float MinAmountOfTimeInTrap => OptionGroupSingleton<TrapperOptions>.Instance.MinAmountOfTimeInTrap;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator FrameTimer()
    {
        while (_transform != null)
        {
            yield return 0;
            Update();
        }
    }

    public void Update()
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !TutorialManager.InstanceExists))
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.HasDied())
            {
                continue;
            }

            // PluginSingleton<TownOfUs>.Instance.Log.LogMessage($"player with byte {player.PlayerId} is {Vector2.Distance(transform.position, player.GetTruePosition())} away");
            if (Vector2.Distance(_transform!.position, player.GetTruePosition()) <
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

                var cachedMod = entry.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
                if (cachedMod != null)
                {
                    role = cachedMod.CachedRole;
                }

                // Logger<TownOfUsPlugin>.Error($"player with byte {entry.PlayerId} is logged with time {_players[entry.PlayerId]}");
                if (_players[entry.PlayerId] > MinAmountOfTimeInTrap && !_owner!.TrappedPlayers.Contains(role) &&
                    entry != _owner.Player)
                    // Logger<TownOfUsPlugin>.Error($"Trap.Updated add role: {role.NiceName}");
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
        Object.Destroy(trapPref.GetComponent<SphereCollider>());
        trapPref.GetComponent<MeshRenderer>().material = AuAvengersAnims.TrapMaterial.LoadAsset();
        trapPref.transform.position = location;

        var trap = new Trap
        {
            _owner = player,
            _transform = trapPref.transform
        };

        Coroutines.Start(trap.FrameTimer());

        _traps.Add(trap);
    }

    public static void Clear()
    {
        _traps.Do(x => x.Destroy());
        _traps.Clear();
    }

    public void Destroy()
    {
        Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_transform != null && _transform.gameObject != null)
            {
                Object.Destroy(_transform.gameObject);
            }

            Coroutines.Stop(FrameTimer());
        }
    }
}