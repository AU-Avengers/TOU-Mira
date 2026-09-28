using UnityEngine;

namespace TownOfUs;

public static class RoomHelpers
{
    public static string? GetRoomName(Vector2 position)
    {
        if (!ShipStatus.Instance) return null;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
            {
                return TranslationController.Instance.GetString(room.RoomId);
            }
        }

        return null;
    }
}
