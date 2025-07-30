using UnityEngine;

namespace TownOfUs;

public static class TownOfUsColors
{
    public static Color Inquisitor = new(0.85f, 0.26f, 0.57f, 1f);

    public static bool UseBasic { get; set; } = TownOfUsPlugin.UseCrewmateTeamColor.Value;

    // Crew Colors
    public static Color Crewmate => Palette.CrewmateRoleBlue;
    public static Color Mayor => UseBasic ? Palette.CrewmateBlue : new Color(0.44f, 0.31f, 0.66f, 1f);
    public static Color Sheriff => UseBasic ? Palette.CrewmateBlue : Color.yellow;
    public static Color Engineer => UseBasic ? Palette.CrewmateBlue : new Color(1f, 0.65f, 0.04f, 1f);
    public static Color Swapper => UseBasic ? Palette.CrewmateBlue : new Color(0.4f, 0.9f, 0.4f, 1f);
    public static Color Investigator => UseBasic ? Palette.CrewmateBlue : new Color(0f, 0.7f, 0.7f, 1f);
    public static Color Medic => UseBasic ? Palette.CrewmateBlue : new Color(0f, 0.4f, 0f, 1f);
    public static Color Seer => UseBasic ? Palette.CrewmateBlue : new Color(1f, 0.8f, 0.5f, 1f);
    public static Color Spy => UseBasic ? Palette.CrewmateBlue : new Color(0.8f, 0.64f, 0.8f, 1f);
    public static Color Snitch => UseBasic ? Palette.CrewmateBlue : new Color(0.83f, 0.69f, 0.22f, 1f);
    public static Color Altruist => UseBasic ? Palette.CrewmateBlue : new Color(0.4f, 0f, 0f, 1f);
    public static Color Vigilante => UseBasic ? Palette.CrewmateBlue : new Color(1f, 1f, 0.6f, 1f);
    public static Color Veteran => UseBasic ? Palette.CrewmateBlue : new Color(0.6f, 0.5f, 0.25f, 1f);
    public static Color Haunter => UseBasic ? Palette.CrewmateBlue : new Color(0.83f, 0.83f, 0.83f, 1f);
    public static Color Transporter => UseBasic ? Palette.CrewmateBlue : new Color(0f, 0.93f, 1f, 1f);
    public static Color Medium => UseBasic ? Palette.CrewmateBlue : new Color(0.65f, 0.5f, 1f, 1f);
    public static Color Mystic => UseBasic ? Palette.CrewmateBlue : new Color(0.3f, 0.6f, 0.9f, 1f);
    public static Color Trapper => UseBasic ? Palette.CrewmateBlue : new Color(0.65f, 0.82f, 0.7f, 1f);
    public static Color Detective => UseBasic ? Palette.CrewmateBlue : new Color(0.3f, 0.3f, 1f, 1f);
    public static Color Imitator => UseBasic ? Palette.CrewmateBlue : new Color(0.7f, 0.85f, 0.3f, 1f);
    public static Color Prosecutor => UseBasic ? Palette.CrewmateBlue : new Color(0.7f, 0.5f, 0f, 1f);
    public static Color Oracle => UseBasic ? Palette.CrewmateBlue : new Color(0.75f, 0f, 0.75f, 1f);
    public static Color Aurial => UseBasic ? Palette.CrewmateBlue : new Color(0.7f, 0.3f, 0.6f, 1f);
    public static Color Politician => UseBasic ? Palette.CrewmateBlue : new Color(0.4f, 0f, 0.6f, 1f);
    public static Color Warden => UseBasic ? Palette.CrewmateBlue : new Color(0.6f, 0f, 1f, 1f);
    public static Color Jailor => UseBasic ? Palette.CrewmateBlue : new Color(0.65f, 0.65f, 0.65f, 1f);
    public static Color Hunter => UseBasic ? Palette.CrewmateBlue : new Color(0.16f, 0.67f, 0.53f, 1f);
    public static Color Tracker => UseBasic ? Palette.CrewmateBlue : new Color(0f, 0.6f, 0f, 1f);
    public static Color Lookout => UseBasic ? Palette.CrewmateBlue : new Color(0.2f, 1f, 0.4f, 1f);
    public static Color Deputy => UseBasic ? Palette.CrewmateBlue : new Color(1f, 0.8f, 0f, 1f);
    public static Color Plumber => UseBasic ? Palette.CrewmateBlue : new Color(0.8f, 0.4f, 0f, 1f);
    public static Color Cleric => UseBasic ? Palette.CrewmateBlue : new Color(0f, 1f, 0.7f, 1f);
    public static Color Mirrorcaster => UseBasic ? Palette.CrewmateBlue : new Color32(144, 162, 195, 255);
    public static Color Marshal => UseBasic ? Palette.CrewmateBlue : new Color32(40, 48, 125, 255);

    // Neutral Colors
    public static Color Neutral => Color.gray;
    public static Color Jester => new(1f, 0.75f, 0.8f, 1f);
    public static Color Executioner => new(0.39f, 0.23f, 0.12f, 1f);
    public static Color Glitch => Color.green;
    public static Color Arsonist => new(1f, 0.3f, 0f);
    public static Color Amnesiac => new(0.5f, 0.7f, 1f, 1f);
    public static Color Juggernaut => new(0.55f, 0f, 0.3f, 1f);
    public static Color Survivor => new(1f, 0.9f, 0.3f, 1f);
    public static Color Protector => new(0.7f, 1f, 1f, 1f);
    public static Color Plaguebearer => new(0.9f, 1f, 0.7f, 1f);
    public static Color Pestilence => new(0.3f, 0.3f, 0.3f, 1f);
    public static Color Werewolf => new(0.66f, 0.4f, 0.16f, 1f);
    public static Color Doomsayer => new(0f, 1f, 0.5f, 1f);
    public static Color Vampire => new(0.64f, 0.16f, 0.16f, 1f);
    public static Color SoulCollector => new(0.6f, 1f, 0.8f, 1f);
    public static Color GuardianAngel => new(0.7f, 1f, 1f, 1f);
    public static Color Phantom => new(0.4f, 0.16f, 0.38f, 1f);
    public static Color Mercenary => new(0.55f, 0.4f, 0.6f, 1f);

    // Impostor Colors
    public static Color Impostor => Palette.ImpostorRed;
    public static Color ImpSoft => new(0.84f, 0.25f, 0.26f, 1f);

    // Modifiers
    public static Color Bait => new(0.2f, 0.7f, 0.7f, 1f);
    public static Color Aftermath => new(0.65f, 1f, 0.65f, 1f);
    public static Color Diseased => Color.grey;
    public static Color Torch => new(1f, 1f, 0.6f, 1f);
    public static Color ButtonBarry => new(0.7f, 0.2f, 0.8f, 1f);
    public static Color Flash => new(1f, 0.5f, 0.5f, 1f);
    public static Color Giant => new(1f, 0.7f, 0.3f, 1f);
    public static Color Lover => new(1f, 0.4f, 0.8f, 1f);
    public static Color Sleuth => new(0.5f, 0.2f, 0.2f, 1f);
    public static Color Tiebreaker => new(0.6f, 0.9f, 0.6f, 1f);
    public static Color Radar => new(1f, 0f, 0.5f, 1f);
    public static Color Multitasker => new(1f, 0.5f, 0.3f, 1f);
    public static Color Frosty => new(0.6f, 1f, 1f, 1f);
    public static Color SixthSense => new(0.85f, 1f, 0.55f, 1f);
    public static Color Shy => new(1f, 0.7f, 0.8f, 1f);
    public static Color Mini => new(0.8f, 1f, 0.9f, 1f);
    public static Color Camouflaged => Color.gray;
    public static Color Satellite => new(0f, 0.6f, 0.8f, 1f);
    public static Color Egotist => new(0.4f, 0.6f, 0.4f, 1f);
    public static Color Taskmaster => new(0.58f, 0.84f, 0.93f, 1f);
    public static Color Celebrity => new(1f, 0.6f, 0.6f, 1f);
    public static Color Immovable => new(0.9f, 0.9f, 0.8f, 1f);
    public static Color Rotting => new(0.67f, 0.5f, 0.41f, 1f);
    public static Color Noisemaker => new(0.91f, 0.41f, 0.62f, 1f);
    public static Color Scientist => new(0f, 0.78f, 0.41f, 1f);
    public static Color Operative => new(0.6f, 0.03f, 0.07f, 1f);
    public static Color Scout => new(0.27f, 0.38f, 0.34f, 1f);
}