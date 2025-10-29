using HarmonyLib;
using Reactor.Utilities.Extensions;
using Rewired;
using TownOfUs.Modules.Components;
using UnityEngine;

namespace TownOfUs.Patches.BetterMaps;

public static class DoorMinigamePatches
{
    [HarmonyPatch(typeof(DoorBreakerGame), nameof(DoorBreakerGame.FlipSwitch))]
    [HarmonyPrefix]
    public static bool PolusFlipSwitchPrefix(DoorBreakerGame __instance, SpriteRenderer button)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(__instance.FlipSound, false, 1f, null);
        }
        button.color = Color.gray;
        button.flipX = false;
        button.GetComponent<PassiveButton>().enabled = false;
        var doorSys = SystemTypes.Doors;
        var shipInstance = ShipStatus.Instance;
        if (shipInstance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            doorSys = SkeldDoorsSystemType.SystemType;
        }
        else if (shipInstance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            doorSys = ManualDoorsSystemType.SystemType;
        }
        if (__instance.Buttons.All((SpriteRenderer s) => !s.flipX))
        {
            shipInstance.RpcUpdateSystem(doorSys, (byte)(__instance.MyDoor.Id | 64));
            __instance.MyDoor.SetDoorway(true);
            __instance.StartCoroutine(__instance.CoStartClose(0.4f));
        }

        return false;
    }
    
    [HarmonyPatch(typeof(DoorCardSwipeGame), nameof(DoorCardSwipeGame.Update))]
    [HarmonyPrefix]
	public static bool AirshipDoorSwipeUpdatePrefix(DoorCardSwipeGame __instance)
	{
		if (__instance.amClosing != Minigame.CloseState.None)
		{
			return false;
		}
        var doorSys = SystemTypes.Doors;
        var shipInstance = ShipStatus.Instance;
        if (shipInstance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            doorSys = SkeldDoorsSystemType.SystemType;
        }
        else if (shipInstance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            doorSys = ManualDoorsSystemType.SystemType;
        }
		Vector3 localPosition = __instance.col.transform.localPosition;
		__instance.myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector = player.GetAxis2DRaw(13, 14);
			float magnitude = vector.magnitude;
			DoorCardSwipeGame.TaskStages state = __instance.State;
			if (state != DoorCardSwipeGame.TaskStages.Before)
			{
				if (state == DoorCardSwipeGame.TaskStages.Inserted)
				{
					if (magnitude > 0.9f)
					{
						vector = vector.normalized;
						if (__instance.hadPrev)
						{
							float num = __instance.prevStickInput.AngleSigned(vector);
							if (num > 180f)
							{
								num -= 360f;
							}
							if (num < -180f)
							{
								num += 360f;
							}
							float num2 = Mathf.Abs(num) * 0.025f;
							float y = localPosition.y;
							localPosition.y -= num2;
							if (num2 > 0.01f)
							{
								__instance.dragTime += Time.deltaTime;
								if (!__instance.moving)
								{
									__instance.moving = true;
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(__instance.CardMove.Random<AudioClip>(), false, 1f, null);
									}
								}
							}
							localPosition.y = __instance.YRange.Clamp(localPosition.y);
							float num3 = localPosition.y - y;
							float num4 = __instance.YRange.ReverseLerp(localPosition.y);
							float num5 = 0.8f * num3;
							VibrationManager.Vibrate(num5 * (1f - num4), num5 * num4, 0.01f, VibrationManager.VibrationFalloff.None, null, false, "");
						}
						else if ((double)__instance.YRange.ReverseLerp(localPosition.y) <= 0.05)
						{
							__instance.dragTime = 0f;
						}
						__instance.prevStickInput = vector;
						__instance.hadPrev = true;
					}
					else
					{
						if (__instance.hadPrev)
						{
							if (localPosition.y - __instance.YRange.min < 0.05f && !BoolRange.Next(0.01f))
							{
								if (__instance.dragTime > __instance.minAcceptedTime)
								{
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(__instance.AcceptSound, false, 1f, null);
									}
									__instance.State = DoorCardSwipeGame.TaskStages.After;
									__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
									__instance.StartCoroutine(__instance.PutCardBack());
									ShipStatus.Instance.RpcUpdateSystem(doorSys, (byte)(__instance.MyDoor.Id | 64));
									__instance.MyDoor.SetDoorway(true);
									__instance.StartCoroutine(__instance.CoStartClose(0.4f));
									__instance.confirmSymbol.sprite = __instance.AcceptSymbol;
								}
								else
								{
									if (Constants.ShouldPlaySfx())
									{
										SoundManager.Instance.PlaySound(__instance.DenySound, false, 1f, null);
									}
									__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
									__instance.confirmSymbol.sprite = __instance.RejectSymbol;
								}
							}
							else
							{
								if (Constants.ShouldPlaySfx())
								{
									SoundManager.Instance.PlaySound(__instance.DenySound, false, 1f, null);
								}
								__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
								__instance.confirmSymbol.sprite = __instance.RejectSymbol;
							}
						}
						localPosition.y = Mathf.Lerp(localPosition.y, __instance.YRange.max, Time.deltaTime * 4f);
						__instance.hadPrev = false;
						if ((double)__instance.YRange.ReverseLerp(localPosition.y) <= 0.05)
						{
							__instance.dragTime = 0f;
						}
					}
				}
			}
			else if (player.GetAnyButtonDown())
			{
				__instance.State = DoorCardSwipeGame.TaskStages.Animating;
				__instance.StartCoroutine(__instance.InsertCard());
			}
		}
		else
		{
			switch (__instance.myController.CheckDrag(__instance.col))
			{
			case DragState.NoTouch:
				if (__instance.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					localPosition.y = Mathf.Lerp(localPosition.y, __instance.YRange.max, Time.deltaTime * 4f);
				}
				break;
			case DragState.TouchStart:
				__instance.dragTime = 0f;
				break;
			case DragState.Dragging:
				if (__instance.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					Vector2 vector2 = __instance.myController.DragPosition - (Vector2)__instance.transform.position;
					vector2.y = __instance.YRange.Clamp(vector2.y);
					if (localPosition.y - vector2.y > 0.01f)
					{
						__instance.dragTime += Time.deltaTime;
						__instance.confirmSymbol.sprite = null;
						if (!__instance.moving)
						{
							__instance.moving = true;
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(__instance.CardMove.Random<AudioClip>(), false, 1f, null);
							}
						}
					}
					localPosition.y = vector2.y;
				}
				break;
			case DragState.Released:
				__instance.moving = false;
				if (__instance.State == DoorCardSwipeGame.TaskStages.Before)
				{
					__instance.State = DoorCardSwipeGame.TaskStages.Animating;
					__instance.StartCoroutine(__instance.InsertCard());
				}
				else if (__instance.State == DoorCardSwipeGame.TaskStages.Inserted)
				{
					if (localPosition.y - __instance.YRange.min < 0.05f && !BoolRange.Next(0.01f))
					{
						if (__instance.dragTime > __instance.minAcceptedTime)
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(__instance.AcceptSound, false, 1f, null);
							}
							__instance.State = DoorCardSwipeGame.TaskStages.After;
							__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardAccepted);
							__instance.StartCoroutine(__instance.PutCardBack());
							ShipStatus.Instance.RpcUpdateSystem(doorSys, (byte)(__instance.MyDoor.Id | 64));
							__instance.MyDoor.SetDoorway(true);
							__instance.StartCoroutine(__instance.CoStartClose(0.4f));
							__instance.confirmSymbol.sprite = __instance.AcceptSymbol;
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(__instance.DenySound, false, 1f, null);
							}
							__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardTooFast);
							__instance.confirmSymbol.sprite = __instance.RejectSymbol;
						}
					}
					else
					{
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(__instance.DenySound, false, 1f, null);
						}
						__instance.StatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SwipeCardBadRead);
						__instance.confirmSymbol.sprite = __instance.RejectSymbol;
					}
				}
				break;
			}
		}
		__instance.col.transform.localPosition = localPosition;
        return false;
    }

    [HarmonyPatch(typeof(MushroomDoorSabotageMinigame), nameof(MushroomDoorSabotageMinigame.FixDoorAndCloseMinigame))]
    [HarmonyPrefix]
    public static bool FungleDoorFixDoorPrefix(MushroomDoorSabotageMinigame __instance)
    {
        var doorSys = SystemTypes.Doors;
        var shipInstance = ShipStatus.Instance;
        if (shipInstance.Systems.ContainsKey(SkeldDoorsSystemType.SystemType))
        {
            doorSys = SkeldDoorsSystemType.SystemType;
        }
        else if (shipInstance.Systems.ContainsKey(ManualDoorsSystemType.SystemType))
        {
            doorSys = ManualDoorsSystemType.SystemType;
        }
        ShipStatus.Instance.RpcUpdateSystem(doorSys, (byte)(__instance.myDoor.Id | 64));
        __instance.myDoor.SetDoorway(true);
        __instance.Close();
        return false;
    }
}