using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouRoleIcons
{
    // THIS FILE SHOULD ONLY HOLD ROLE ICONS
    public static LoadableAsset<Sprite> Aurial { get; } = new LoadableBundleAsset<Sprite>("Aurial", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Detective { get; } = new LoadableBundleAsset<Sprite>("Detective", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Haunter { get; } = new LoadableBundleAsset<Sprite>("Haunter", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Investigator { get; } = new LoadableBundleAsset<Sprite>("Investigator", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Lookout { get; } = new LoadableBundleAsset<Sprite>("Lookout", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Mystic { get; } = new LoadableBundleAsset<Sprite>("Mystic", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Seer { get; } = new LoadableBundleAsset<Sprite>("Seer", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Snitch { get; } = new LoadableBundleAsset<Sprite>("Snitch", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Spy { get; } = new LoadableBundleAsset<Sprite>("Spy", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Tracker { get; } = new LoadableBundleAsset<Sprite>("Tracker", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Trapper { get; } = new LoadableBundleAsset<Sprite>("Trapper", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Deputy { get; } = new LoadableBundleAsset<Sprite>("Deputy", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Hunter { get; } = new LoadableBundleAsset<Sprite>("Hunter", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Sheriff { get; } = new LoadableBundleAsset<Sprite>("Sheriff", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Veteran { get; } = new LoadableBundleAsset<Sprite>("Veteran", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Vigilante { get; } = new LoadableBundleAsset<Sprite>("Vigilante", TouAssets.MainBundle);
    
    
    public static LoadableAsset<Sprite> Agent { get; } = new LoadableBundleAsset<Sprite>("Agent", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Jailor { get; } = new LoadableBundleAsset<Sprite>("Jailor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Politician { get; } = new LoadableBundleAsset<Sprite>("Politician", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Mayor { get; } = new LoadableBundleAsset<Sprite>("Mayor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Prosecutor { get; } = new LoadableBundleAsset<Sprite>("Prosecutor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Swapper { get; } = new LoadableBundleAsset<Sprite>("Swapper", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Altruist { get; } = new LoadableBundleAsset<Sprite>("Altruist", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Cleric { get; } = new LoadableBundleAsset<Sprite>("Cleric", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Medic { get; } = new LoadableBundleAsset<Sprite>("Medic", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Mirrorcaster { get; } = new LoadableBundleAsset<Sprite>("Mirrorcaster", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Oracle { get; } = new LoadableBundleAsset<Sprite>("Oracle", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Warden { get; } = new LoadableBundleAsset<Sprite>("Warden", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Engineer { get; } = new LoadableBundleAsset<Sprite>("Engineer", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Imitator { get; } = new LoadableBundleAsset<Sprite>("Imitator", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Medium { get; } = new LoadableBundleAsset<Sprite>("Medium", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Plumber { get; } = new LoadableBundleAsset<Sprite>("Plumber", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Transporter { get; } = new LoadableBundleAsset<Sprite>("Transporter", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Amnesiac { get; } = new LoadableBundleAsset<Sprite>("Amnesiac", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> GuardianAngel { get; } =
        new LoadableBundleAsset<Sprite>("GuardianAngel", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Mercenary { get; } = new LoadableBundleAsset<Sprite>("Mercenary", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Survivor { get; } = new LoadableBundleAsset<Sprite>("Survivor", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Doomsayer { get; } = new LoadableBundleAsset<Sprite>("Doomsayer", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Executioner { get; } = new LoadableBundleAsset<Sprite>("Executioner", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Inquisitor { get; } = new LoadableBundleAsset<Sprite>("Inquisitor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Jester { get; } = new LoadableBundleAsset<Sprite>("Jester", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Phantom { get; } = new LoadableBundleAsset<Sprite>("Phantom", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Arsonist { get; } = new LoadableBundleAsset<Sprite>("Arsonist", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Glitch { get; } = new LoadableBundleAsset<Sprite>("Glitch", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Juggernaut { get; } = new LoadableBundleAsset<Sprite>("Juggernaut", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Plaguebearer { get; } =
        new LoadableBundleAsset<Sprite>("Plaguebearer", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Pestilence { get; } = new LoadableBundleAsset<Sprite>("Pestilence", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> SoulCollector { get; } =
        new LoadableBundleAsset<Sprite>("SoulCollector", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Vampire { get; } = new LoadableBundleAsset<Sprite>("Vampire", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Werewolf { get; } = new LoadableBundleAsset<Sprite>("Werewolf", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Eclipsal { get; } = new LoadableBundleAsset<Sprite>("Eclipsal", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Escapist { get; } = new LoadableBundleAsset<Sprite>("Escapist", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Grenadier { get; } = new LoadableBundleAsset<Sprite>("Grenadier", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Morphling { get; } = new LoadableBundleAsset<Sprite>("Morphling", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Swooper { get; } = new LoadableBundleAsset<Sprite>("Swooper", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Venerer { get; } = new LoadableBundleAsset<Sprite>("Venerer", TouAssets.MainBundle);
    

    public static LoadableAsset<Sprite> Ambusher { get; } = new LoadableBundleAsset<Sprite>("Ambusher", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Bomber { get; } = new LoadableBundleAsset<Sprite>("Bomber", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Infestor { get; } = new LoadableBundleAsset<Sprite>("Infestor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Scavenger { get; } = new LoadableBundleAsset<Sprite>("Scavenger", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Warlock { get; } = new LoadableBundleAsset<Sprite>("Warlock", TouAssets.MainBundle);
    
    public static LoadableAsset<Sprite> Ambassador { get; } = new LoadableBundleAsset<Sprite>("Ambassador", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Traitor { get; } = new LoadableBundleAsset<Sprite>("Traitor", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> Blackmailer { get; } = new LoadableBundleAsset<Sprite>("Blackmailer", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Herbalist { get; } = new LoadableBundleAsset<Sprite>("Herbalist", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Hypnotist { get; } = new LoadableBundleAsset<Sprite>("Hypnotist", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Janitor { get; } = new LoadableBundleAsset<Sprite>("Janitor", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Miner { get; } = new LoadableBundleAsset<Sprite>("Miner", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> Undertaker { get; } = new LoadableBundleAsset<Sprite>("Undertaker", TouAssets.MainBundle);

    public static LoadableAsset<Sprite> RandomAny { get; } = new LoadableBundleAsset<Sprite>("RandomAny", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> RandomCrew { get; } = new LoadableBundleAsset<Sprite>("RandomCrew", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> RandomNeut { get; } = new LoadableBundleAsset<Sprite>("RandomNeut", TouAssets.MainBundle);
    public static LoadableAsset<Sprite> RandomImp { get; } = new LoadableBundleAsset<Sprite>("RandomImp", TouAssets.MainBundle);
}