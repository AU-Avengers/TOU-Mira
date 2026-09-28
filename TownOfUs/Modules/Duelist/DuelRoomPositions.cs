using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs.Modules.Duelist;
public static class DuelRoomPositions
{
    private static readonly Dictionary<int, Dictionary<SystemTypes, Vector2>> Maps = new()
    {
        // Skeld
        [0] = new()
        {
            { SystemTypes.Cafeteria, new Vector2(-0.73f, -1.96f) },
            { SystemTypes.Admin, new Vector2(3.00f, -8.82f) },
            { SystemTypes.Storage, new Vector2(-1.64f, -16.66f) },
            { SystemTypes.Electrical, new Vector2(-7.60f, -8.61f) },
            { SystemTypes.LowerEngine, new Vector2(-17.19f, -13.54f) },   // Lower Engine
            { SystemTypes.Reactor, new Vector2(-20.15f, -5.68f) },
            { SystemTypes.Security, new Vector2(-12.79f, -3.46f) },
            { SystemTypes.UpperEngine, new Vector2(-17.25f, 2.17f) },     // Upper Engine
            { SystemTypes.MedBay, new Vector2(-8.95f, -3.63f) },
            { SystemTypes.Weapons, new Vector2(9.17f, 2.62f) },
            { SystemTypes.LifeSupp, new Vector2(6.87f, -3.91f) },         // O2
            { SystemTypes.Nav, new Vector2(16.73f, -5.01f) },            // Navigation
            { SystemTypes.Shields, new Vector2(9.38f, -12.47f) },
            { SystemTypes.Comms, new Vector2(3.08f, -16.47f) },          // Communications
        },

        // MIRA HQ
        [1] = new()
        {
            { SystemTypes.Launchpad, new Vector2(-4.40f, 1.98f) },
            { SystemTypes.MedBay, new Vector2(16.39f, 0.03f) },
            { SystemTypes.LockerRoom, new Vector2(9.74f, 2.13f) },       // Locker Room
            { SystemTypes.Comms, new Vector2(15.40f, 3.97f) },          // Communications
            { SystemTypes.Decontamination, new Vector2(6.02f, 6.10f) },
            { SystemTypes.Reactor, new Vector2(2.52f, 10.97f) },
            { SystemTypes.Laboratory, new Vector2(9.11f, 12.48f) },
            { SystemTypes.Office, new Vector2(14.73f, 19.98f) },
            { SystemTypes.Admin, new Vector2(21.09f, 20.10f) },
            { SystemTypes.Greenhouse, new Vector2(17.79f, 23.53f) },
            { SystemTypes.Cafeteria, new Vector2(23.32f, 4.16f) },
            { SystemTypes.Storage, new Vector2(19.51f, 4.28f) },
            { SystemTypes.Balcony, new Vector2(19.99f, -2.07f) },
        },

        // Polus
        [2] = new()
        {
            { SystemTypes.Dropship, new Vector2(16.64f, -1.88f) },
            { SystemTypes.Storage, new Vector2(20.73f, -12.20f) },
            { SystemTypes.Electrical, new Vector2(7.53f, -10.09f) },
            { SystemTypes.Security, new Vector2(2.78f, -12.36f) },
            { SystemTypes.LifeSupp, new Vector2(1.76f, -17.01f) },           // O2
            { SystemTypes.BoilerRoom, new Vector2(2.15f, -24.19f) },         // Boiler Room
            { SystemTypes.Comms, new Vector2(11.40f, -16.15f) },            // Communications
            { SystemTypes.Weapons, new Vector2(10.64f, -23.38f) },
            { SystemTypes.Office, new Vector2(19.44f, -17.93f) },
            { SystemTypes.Admin, new Vector2(22.82f, -21.67f) },
            { SystemTypes.Decontamination3, new Vector2(24.31f, -25.09f) },  // Lower Decontamination
            { SystemTypes.Specimens, new Vector2(36.45f, -21.66f) },         // Specimen Room
            { SystemTypes.Decontamination2, new Vector2(39.06f, -10.16f) },  // Upper Decontamination
            { SystemTypes.Laboratory, new Vector2(40.39f, -7.09f) },
        },

        // Dleks (reverse Skeld)
        [3] = new()
        {
            { SystemTypes.Cafeteria, new Vector2(0.86f, -2.20f) },
            { SystemTypes.MedBay, new Vector2(7.29f, -5.21f) },
            { SystemTypes.UpperEngine, new Vector2(16.81f, 2.28f) },     // Upper Engine
            { SystemTypes.Reactor, new Vector2(20.80f, -5.57f) },
            { SystemTypes.Security, new Vector2(12.66f, -3.69f) },
            { SystemTypes.LowerEngine, new Vector2(17.09f, -13.29f) },   // Lower Engine
            { SystemTypes.Electrical, new Vector2(8.44f, -8.52f) },
            { SystemTypes.Storage, new Vector2(0.88f, -15.91f) },
            { SystemTypes.Admin, new Vector2(-3.16f, -8.87f) },
            { SystemTypes.Comms, new Vector2(-4.61f, -16.36f) },         // Communications
            { SystemTypes.Shields, new Vector2(-9.24f, -12.67f) },
            { SystemTypes.Nav, new Vector2(-16.58f, -4.70f) },          // Navigation
            { SystemTypes.LifeSupp, new Vector2(-6.48f, -3.84f) },       // O2
            { SystemTypes.Weapons, new Vector2(-9.46f, 2.79f) },
        },

        // Airship
        [4] = new()
        {
            { SystemTypes.ViewingDeck, new Vector2(-13.92f, -16.75f) },     // Viewing Deck
            { SystemTypes.Kitchen, new Vector2(-5.85f, -11.43f) },
            { SystemTypes.HallOfPortraits, new Vector2(1.43f, -12.68f) },   // Hall of Portraits
            { SystemTypes.Security, new Vector2(6.95f, -12.68f) },
            { SystemTypes.Electrical, new Vector2(13.34f, -8.88f) },
            { SystemTypes.MainHall, new Vector2(12.11f, -0.38f) },          // Main Hall
            { SystemTypes.Showers, new Vector2(19.40f, -0.38f) },
            { SystemTypes.Records, new Vector2(19.80f, 8.17f) },
            { SystemTypes.Lounge, new Vector2(25.87f, 7.03f) },
            { SystemTypes.CargoBay, new Vector2(34.16f, -1.34f) },          // Cargo Bay
            { SystemTypes.Ventilation, new Vector2(28.80f, -1.85f) },
            { SystemTypes.GapRoom, new Vector2(10.67f, 8.40f) },            // Gap Room
            { SystemTypes.Engine, new Vector2(-3.21f, -1.36f) },            // Engine Room
            { SystemTypes.Comms, new Vector2(-13.11f, 1.57f) },             // Communications
            { SystemTypes.Armory, new Vector2(-12.26f, -3.83f) },
            { SystemTypes.Cockpit, new Vector2(-21.34f, -1.92f) },
            { SystemTypes.Brig, new Vector2(-0.89f, 8.43f) },
            { SystemTypes.VaultRoom, new Vector2(-7.76f, 8.43f) },          // Vault
            { SystemTypes.MeetingRoom, new Vector2(7.28f, 14.91f) },        // Meeting Room
            { SystemTypes.Medical, new Vector2(25.29f, -8.93f) },           // Medical
        },

        // The Fungle
        [5] = new()
        {
            { SystemTypes.Cafeteria, new Vector2(-15.52f, 6.74f) },
            { SystemTypes.RecRoom, new Vector2(-17.46f, -0.47f) },          // Splash Zone
            { SystemTypes.Kitchen, new Vector2(-16.34f, -7.92f) },
            { SystemTypes.FishingDock, new Vector2(-22.55f, -7.56f) },      // Dock
            { SystemTypes.MeetingRoom, new Vector2(-3.95f, -1.15f) },       // Meeting Room
            { SystemTypes.Dropship, new Vector2(-8.12f, 9.38f) },
            { SystemTypes.Storage, new Vector2(0.18f, 4.13f) },
            { SystemTypes.SleepingQuarters, new Vector2(1.64f, -2.00f) },   // The Dorm
            { SystemTypes.Laboratory, new Vector2(-4.38f, -9.04f) },
            { SystemTypes.Greenhouse, new Vector2(9.35f, -11.94f) },
            { SystemTypes.Reactor, new Vector2(21.48f, -8.54f) },
            { SystemTypes.UpperEngine, new Vector2(22.72f, 3.35f) },        // Upper Engine
            { SystemTypes.Lookout, new Vector2(7.24f, 0.35f) },
            { SystemTypes.MiningPit, new Vector2(12.52f, 9.27f) },          // Mining Pit
            { SystemTypes.Comms, new Vector2(22.15f, 13.44f) },             // Communications
        },
    };

    private static readonly Dictionary<int, (SystemTypes A, SystemTypes B)> FarRooms = new()
    {
        [0] = (SystemTypes.Reactor, SystemTypes.Nav),         // Skeld
        [3] = (SystemTypes.Reactor, SystemTypes.Nav),         // Dleks
        [1] = (SystemTypes.Launchpad, SystemTypes.Greenhouse), // MIRA HQ
        [2] = (SystemTypes.Laboratory, SystemTypes.BoilerRoom), // Polus
        [4] = (SystemTypes.Armory, SystemTypes.Records),       // Airship
        [5] = (SystemTypes.Kitchen, SystemTypes.Comms),        // The Fungle
    };

    private static int CurrentMapId =>
        GameOptionsManager.Instance == null ? 0 : GameOptionsManager.Instance.CurrentGameOptions.MapId;

    public static bool TryGetFarRooms(out SystemTypes a, out SystemTypes b)
    {
        a = b = default;
        if (!FarRooms.TryGetValue(CurrentMapId, out var pair))
        {
            return false;
        }
        a = pair.A;
        b = pair.B;
        return true;
    }

    public static List<SystemTypes> GetAllRooms()
    {
        return Maps.TryGetValue(CurrentMapId, out var table)
            ? new List<SystemTypes>(table.Keys)
            : new List<SystemTypes>();
    }

    public static bool TryGet(SystemTypes room, out Vector2 pos)
    {
        pos = Vector2.zero;
        if (Maps.TryGetValue(CurrentMapId, out var table) &&
            table.TryGetValue(room, out var p) && p != Vector2.zero)
        {
            pos = p;
            return true;
        }
        return false;
    }

    public static SystemTypes? GetRoomAt(Vector2 worldPos)
    {
        if (ShipStatus.Instance == null)
        {
            return null;
        }

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room != null && room.roomArea != null && room.roomArea.OverlapPoint(worldPos))
            {
                return room.RoomId;
            }
        }
        return null;
    }
}
