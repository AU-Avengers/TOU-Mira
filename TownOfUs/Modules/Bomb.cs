using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Achievements;
using TownOfUs.Modifiers;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class Bomb(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private PlayerControl? _bomber;
    private GameObject _obj;

    public void Detonate()
    {
        Coroutines.Start(CoDetonate());
    }

    private IEnumerator CoDetonate()
    {
        yield return new WaitForSeconds(0.1f);

        if (!_obj)
        {
            yield break;
        }
        var radius = OptionGroupSingleton<BomberOptions>.Instance.DetonateRadius * ShipStatus.Instance.MaxLightRadius;

        var affected = Helpers.GetClosestPlayers(_obj.transform.position, radius);

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
        TouAudio.PlaySound(TouAudio.BombExplode);
        HudManager.Instance.StartCoroutine(HudManager.Instance.PlayerCam.CoShakeScreen(0.4f, 1.5f));
        var targetList = affected.Where(x =>
                !x.HasDied() && (!x.AmOwner || x.AmOwner && !x.HasModifier<BaseShieldModifier>() &&
                    !x.HasModifier<FirstDeadShield>()))
            .ToList();
        if (_bomber != null)
        {
            TouAchievementEvents.OnBombDetonate(_bomber, targetList);
        }

        _bomber?.RpcSpecialMultiMurder(targetList, MeetingCheck.OutsideMeeting, true, teleportMurderer: false,
            causeOfDeath: "BomberBomb");

        _obj.Destroy();
    }

    public static Bomb CreateBomb(PlayerControl player, Vector3 location)
    {
        var obj = MiscUtils.CreateSpherePrimitive(location,
                OptionGroupSingleton<BomberOptions>.Instance.DetonateRadius);
        var bomb = obj.AddComponent<Bomb>();
        bomb._obj = obj;
        bomb._bomber = player;
        return bomb;
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
        if (_obj)
        {
            _obj.Destroy();
        }
    }
}