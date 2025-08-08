using UnityEngine;

namespace TownOfUs;

public static class TownOfUsColors
{
    public static bool UseBasic { get; set; } = TownOfUsPlugin.UseCrewmateTeamColor.Value;
    public static Color Crewmate => Palette.CrewmateRoleBlue;
    public static Color Impostor => Palette.ImpostorRed;
    public static Color ImpSoft => new Color32(214, 64, 66, 255);
    public static Color Neutral => Color.gray;

    // Crew Colors
    public static Color Aurial => UseBasic ? Palette.CrewmateBlue : new Color32(179, 77, 153, 255);
    public static Color Detective => UseBasic ? Palette.CrewmateBlue : new Color32(77, 77, 255, 255);
    public static Color Haunter => UseBasic ? Palette.CrewmateBlue : new Color32(212, 212, 212, 255);
    public static Color Investigator => UseBasic ? Palette.CrewmateBlue : new Color32(0, 179, 179, 255);
    public static Color Lookout => UseBasic ? Palette.CrewmateBlue : new Color32(51, 255, 102, 255);
    public static Color Mystic => UseBasic ? Palette.CrewmateBlue : new Color32(77, 153, 230, 255);
    public static Color Psychic => UseBasic ? Palette.CrewmateBlue : new Color32(111, 105, 140, 255);
    public static Color Seer => UseBasic ? Palette.CrewmateBlue : new Color32(255, 204, 128, 255);
    public static Color Snitch => UseBasic ? Palette.CrewmateBlue : new Color32(212, 176, 56, 255);
    public static Color Spy => UseBasic ? Palette.CrewmateBlue : new Color32(204, 163, 204, 255);
    public static Color Tracker => UseBasic ? Palette.CrewmateBlue : new Color32(0, 153, 0, 255);
    public static Color Trapper => UseBasic ? Palette.CrewmateBlue : new Color32(166, 209, 179, 255);
    
    public static Color Deputy => UseBasic ? Palette.CrewmateBlue : new Color32(255, 204, 0, 255);
    public static Color Hunter => UseBasic ? Palette.CrewmateBlue : new Color32(41, 171, 135, 255);
    public static Color Sheriff => UseBasic ? Palette.CrewmateBlue : new Color32(255, 255, 0, 255);
    public static Color Veteran => UseBasic ? Palette.CrewmateBlue : new Color32(153, 128, 64, 255);
    public static Color Vigilante => UseBasic ? Palette.CrewmateBlue : new Color32(255, 255, 153, 255);
    
    public static Color Jailor => UseBasic ? Palette.CrewmateBlue : new Color32(166, 166, 166, 255);
    public static Color Mayor => UseBasic ? Palette.CrewmateBlue : new Color32(112, 79, 168, 255);
    public static Color Monarch => UseBasic ? Palette.CrewmateBlue : new Color32(97, 66, 199, 255);
    public static Color Politician => UseBasic ? Palette.CrewmateBlue : new Color32(102, 0, 153, 255);
    public static Color Prosecutor => UseBasic ? Palette.CrewmateBlue : new Color32(179, 128, 0, 255);
    public static Color Swapper => UseBasic ? Palette.CrewmateBlue : new Color32(102, 230, 102, 255);
    
    public static Color Altruist => UseBasic ? Palette.CrewmateBlue : new Color32(102, 0, 0, 255);
    public static Color Cleric => UseBasic ? Palette.CrewmateBlue : new Color32(0, 255, 179, 255);
    public static Color Medic => UseBasic ? Palette.CrewmateBlue : new Color32(0, 102, 0, 255);
    public static Color Mirrorcaster => UseBasic ? Palette.CrewmateBlue : new Color32(144, 162, 195, 255);
    public static Color Oracle => UseBasic ? Palette.CrewmateBlue : new Color32(191, 0, 191, 255);
    public static Color Warden => UseBasic ? Palette.CrewmateBlue : new Color32(153, 0, 255, 255);
    
    public static Color Barkeeper => UseBasic ? Palette.CrewmateBlue : new Color32(227, 212, 119, 255);
    public static Color Engineer => UseBasic ? Palette.CrewmateBlue : new Color32(255, 166, 10, 255);
    public static Color Imitator => UseBasic ? Palette.CrewmateBlue : new Color32(179, 217, 77, 255);
    public static Color Medium => UseBasic ? Palette.CrewmateBlue : new Color32(166, 128, 255, 255);
    public static Color Plumber => UseBasic ? Palette.CrewmateBlue : new Color32(204, 102, 0, 255);
    public static Color Transporter => UseBasic ? Palette.CrewmateBlue : new Color32(0, 237, 255, 255);

    // Neutral Colors
    public static Color Amnesiac => new Color32(128, 179, 255, 255);
    public static Color GuardianAngel => new Color32(179, 255, 255, 255);
    public static Color Mercenary => new Color32(140, 102, 153, 255);
    public static Color Survivor => new Color32(255, 230, 77, 255);
    
    
    public static Color Doomsayer => new Color32(0, 255, 128, 255);
    public static Color Executioner => new Color32(99, 59, 31, 255);
    public static Color Inquisitor = new Color32(217, 66, 145, 255);
    public static Color Jester => new Color32(255, 191, 204, 255);
    public static Color Phantom => new Color32(102, 41, 97, 255);
    
    public static Color Arsonist => new Color32(255, 77, 0, 255);
    public static Color Glitch => Color.green;
    public static Color Juggernaut => new Color32(140, 0, 77, 255);
    public static Color Plaguebearer => new Color32(230, 255, 179, 255);
    public static Color Pestilence => new Color32(77, 77, 77, 255);
    public static Color SerialKiller => new Color32(29, 77, 252, 255);
    public static Color Shroud => new Color32(102, 153, 255, 255);
    public static Color SoulCollector => new Color32(153, 255, 204, 255);
    public static Color Vampire => new Color32(163, 41, 41, 255);
    public static Color Werewolf => new Color32(168, 102, 41, 255);

    // Alliance Modifiers
    public static Color Egotist => new Color32(102, 153, 102, 255);
    public static Color Lover => new Color32(255, 102, 204, 255);
    // Universal Modifiers
    public static Color ButtonBarry => new Color32(179, 51, 204, 255);
    public static Color Drunk => new Color32(117, 128, 0, 255);
    public static Color Flash => new Color32(255, 128, 128, 255);
    public static Color Giant => new Color32(255, 179, 77, 255);
    public static Color Immovable => new Color32(230, 230, 204, 255);
    public static Color Mini => new Color32(204, 255, 230, 255);
    public static Color Radar => new Color32(255, 0, 128, 255);
    public static Color Satellite => new Color32(0, 153, 204, 255);
    public static Color Shy => new Color32(255, 179, 204, 255);
    public static Color SixthSense => new Color32(217, 255, 140, 255);
    public static Color Sleuth => new Color32(128, 51, 51, 255);
    public static Color Tiebreaker => new Color32(153, 230, 153, 255);
    // Crewmate Modifiers
    public static Color Aftermath => new Color32(166, 255, 166, 255);
    public static Color Bait => new Color32(51, 179, 179, 255);
    public static Color Celebrity => new Color32(255, 153, 153, 255);
    public static Color Diseased => Color.grey;
    public static Color Frosty => new Color32(153, 255, 255, 255);
    public static Color Multitasker => new Color32(255, 128, 77, 255);
    public static Color Noisemaker => new Color32(232, 105, 158, 255);
    public static Color Operative => new Color32(153, 8, 18, 255);
    public static Color Rotting => new Color32(171, 128, 105, 255);
    public static Color Scientist => new Color32(0, 199, 105, 255);
    public static Color Scout => new Color32(69, 97, 87, 255);
    public static Color Taskmaster => new Color32(148, 214, 237, 255);
    public static Color Torch => new Color32(255, 255, 153, 255);
    // Neutral Modifiers
    public static Color Camouflaged => Color.gray;
    // Assailiant Modifiers
    public static Color DoubleShot => new Color32(100, 63, 213, 255);

}