using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules;

// Code Review: Should be using a MonoBehaviour
public sealed class Bomb : IDisposable
{
    private PlayerControl? _bomber;
    private GameObject? _obj;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Detonate()
    {
        Coroutines.Start(CoDetonate());
    }

    private IEnumerator CoDetonate()
    {
        yield return new WaitForSeconds(0.1f);

        var radius = OptionGroupSingleton<BomberOptions>.Instance.DetonateRadius * ShipStatus.Instance.MaxLightRadius;

        var affected = Helpers.GetClosestPlayers(_obj!.transform.position, radius);

        affected.Shuffle();

        while (affected.Count > OptionGroupSingleton<BomberOptions>.Instance.MaxKillsInDetonation)
        {
            affected.Remove(affected[^1]);
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            _obj.Destroy();
            yield break;
        }
        var targetList = affected.Where(x => !x.HasDied() && !(x.HasModifier<BaseShieldModifier>() && x.AmOwner) && !(x.HasModifier<FirstDeadShield>() && x.AmOwner)).ToList();
        _bomber?.RpcSpecialMultiMurder(targetList, true, teleportMurderer: false,
            causeOfDeath: "BomberBomb",
            inMeeting:false);

        _obj.Destroy();
    }

    public static Bomb CreateBomb(PlayerControl player, Vector3 location)
    {
        return new Bomb
        {
            _obj = MiscUtils.CreateSpherePrimitive(location,
                OptionGroupSingleton<BomberOptions>.Instance.DetonateRadius),
            _bomber = player
        };
    }

    public static IEnumerator BombShowTeammate(PlayerControl player, Vector3 location)
    {
        var bomb = CreateBomb(player, location);

        yield return new WaitForSeconds(OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay);

        try
        {
            bomb.Destroy();
        }
        catch
        {
            /* ignored */
        }
    }

    public void Destroy()
    {
        Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (disposing && _obj != null)
        {
            _obj.Destroy();
        }
    }
}