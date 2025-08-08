using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouRoleIcons
{
    private static readonly string iconPath = "TownOfUs.Resources.RoleIcons";

    // THIS FILE SHOULD ONLY HOLD ROLE ICONS
    public static LoadableAsset<Sprite> Aurial { get; } = new LoadableResourceAsset($"{iconPath}.Aurial.png");
    public static LoadableAsset<Sprite> Detective { get; } = new LoadableResourceAsset($"{iconPath}.Detective.png");
    public static LoadableAsset<Sprite> Haunter { get; } = new LoadableResourceAsset($"{iconPath}.Haunter.png");

    public static LoadableAsset<Sprite> Investigator { get; } =
        new LoadableResourceAsset($"{iconPath}.Investigator.png");

    public static LoadableAsset<Sprite> Lookout { get; } = new LoadableResourceAsset($"{iconPath}.Lookout.png");
    public static LoadableAsset<Sprite> Mystic { get; } = new LoadableResourceAsset($"{iconPath}.Mystic.png");
    public static LoadableAsset<Sprite> Psychic { get; } = new LoadableResourceAsset($"{iconPath}.Psychic.png");
    public static LoadableAsset<Sprite> Seer { get; } = new LoadableResourceAsset($"{iconPath}.Seer.png");
    public static LoadableAsset<Sprite> Snitch { get; } = new LoadableResourceAsset($"{iconPath}.Snitch.png");
    public static LoadableAsset<Sprite> Spy { get; } = new LoadableResourceAsset($"{iconPath}.Spy.png");
    public static LoadableAsset<Sprite> Tracker { get; } = new LoadableResourceAsset($"{iconPath}.Tracker.png");
    public static LoadableAsset<Sprite> Trapper { get; } = new LoadableResourceAsset($"{iconPath}.Trapper.png");

    public static LoadableAsset<Sprite> Deputy { get; } = new LoadableResourceAsset($"{iconPath}.Deputy.png");
    public static LoadableAsset<Sprite> Hunter { get; } = new LoadableResourceAsset($"{iconPath}.Hunter.png");
    public static LoadableAsset<Sprite> Sheriff { get; } = new LoadableResourceAsset($"{iconPath}.Sheriff.png");
    public static LoadableAsset<Sprite> Veteran { get; } = new LoadableResourceAsset($"{iconPath}.Veteran.png");
    public static LoadableAsset<Sprite> Vigilante { get; } = new LoadableResourceAsset($"{iconPath}.Vigilante.png");
    
    
    public static LoadableAsset<Sprite> Agent { get; } = new LoadableResourceAsset($"{iconPath}.Agent.png");
    public static LoadableAsset<Sprite> Jailor { get; } = new LoadableResourceAsset($"{iconPath}.Jailor.png");
    public static LoadableAsset<Sprite> Politician { get; } = new LoadableResourceAsset($"{iconPath}.Politician.png");
    public static LoadableAsset<Sprite> Mayor { get; } = new LoadableResourceAsset($"{iconPath}.Mayor.png");
    public static LoadableAsset<Sprite> Monarch { get; } = new LoadableResourceAsset($"{iconPath}.Monarch.png");
    public static LoadableAsset<Sprite> Prosecutor { get; } = new LoadableResourceAsset($"{iconPath}.Prosecutor.png");
    public static LoadableAsset<Sprite> Swapper { get; } = new LoadableResourceAsset($"{iconPath}.Swapper.png");

    public static LoadableAsset<Sprite> Altruist { get; } = new LoadableResourceAsset($"{iconPath}.Altruist.png");
    public static LoadableAsset<Sprite> Cleric { get; } = new LoadableResourceAsset($"{iconPath}.Cleric.png");
    public static LoadableAsset<Sprite> Medic { get; } = new LoadableResourceAsset($"{iconPath}.Medic.png");
    public static LoadableAsset<Sprite> Mirrorcaster { get; } = new LoadableResourceAsset($"{iconPath}.Mirrorcaster.png");
    public static LoadableAsset<Sprite> Oracle { get; } = new LoadableResourceAsset($"{iconPath}.Oracle.png");
    public static LoadableAsset<Sprite> Warden { get; } = new LoadableResourceAsset($"{iconPath}.Warden.png");

    public static LoadableAsset<Sprite> Barkeeper { get; } = new LoadableResourceAsset($"{iconPath}.Barkeeper.png");
    public static LoadableAsset<Sprite> Engineer { get; } = new LoadableResourceAsset($"{iconPath}.Engineer.png");
    public static LoadableAsset<Sprite> Imitator { get; } = new LoadableResourceAsset($"{iconPath}.Imitator.png");
    public static LoadableAsset<Sprite> Medium { get; } = new LoadableResourceAsset($"{iconPath}.Medium.png");
    public static LoadableAsset<Sprite> Plumber { get; } = new LoadableResourceAsset($"{iconPath}.Plumber.png");
    public static LoadableAsset<Sprite> Transporter { get; } = new LoadableResourceAsset($"{iconPath}.Transporter.png");

    public static LoadableAsset<Sprite> Amnesiac { get; } = new LoadableResourceAsset($"{iconPath}.Amnesiac.png");

    public static LoadableAsset<Sprite> GuardianAngel { get; } =
        new LoadableResourceAsset($"{iconPath}.GuardianAngel.png");

    public static LoadableAsset<Sprite> Mercenary { get; } = new LoadableResourceAsset($"{iconPath}.Mercenary.png");
    public static LoadableAsset<Sprite> Survivor { get; } = new LoadableResourceAsset($"{iconPath}.Survivor.png");

    public static LoadableAsset<Sprite> Doomsayer { get; } = new LoadableResourceAsset($"{iconPath}.Doomsayer.png");
    public static LoadableAsset<Sprite> Executioner { get; } = new LoadableResourceAsset($"{iconPath}.Executioner.png");
    public static LoadableAsset<Sprite> Inquisitor { get; } = new LoadableResourceAsset($"{iconPath}.Inquisitor.png");
    public static LoadableAsset<Sprite> Jester { get; } = new LoadableResourceAsset($"{iconPath}.Jester.png");
    public static LoadableAsset<Sprite> Phantom { get; } = new LoadableResourceAsset($"{iconPath}.Phantom.png");

    public static LoadableAsset<Sprite> Arsonist { get; } = new LoadableResourceAsset($"{iconPath}.Arsonist.png");
    public static LoadableAsset<Sprite> Glitch { get; } = new LoadableResourceAsset($"{iconPath}.Glitch.png");
    public static LoadableAsset<Sprite> Juggernaut { get; } = new LoadableResourceAsset($"{iconPath}.Juggernaut.png");
    public static LoadableAsset<Sprite> SerialKiller { get; } = new LoadableResourceAsset($"{iconPath}.SerialKiller.png");
    public static LoadableAsset<Sprite> Shroud { get; } = new LoadableResourceAsset($"{iconPath}.Shroud.png");

    public static LoadableAsset<Sprite> Plaguebearer { get; } =
        new LoadableResourceAsset($"{iconPath}.Plaguebearer.png");

    public static LoadableAsset<Sprite> Pestilence { get; } = new LoadableResourceAsset($"{iconPath}.Pestilence.png");

    public static LoadableAsset<Sprite> SoulCollector { get; } =
        new LoadableResourceAsset($"{iconPath}.SoulCollector.png");

    public static LoadableAsset<Sprite> Vampire { get; } = new LoadableResourceAsset($"{iconPath}.Vampire.png");
    public static LoadableAsset<Sprite> Werewolf { get; } = new LoadableResourceAsset($"{iconPath}.Werewolf.png");

    public static LoadableAsset<Sprite> Eclipsal { get; } = new LoadableResourceAsset($"{iconPath}.Eclipsal.png");
    public static LoadableAsset<Sprite> Escapist { get; } = new LoadableResourceAsset($"{iconPath}.Escapist.png");
    public static LoadableAsset<Sprite> Grenadier { get; } = new LoadableResourceAsset($"{iconPath}.Grenadier.png");
    public static LoadableAsset<Sprite> Morphling { get; } = new LoadableResourceAsset($"{iconPath}.Morphling.png");
    public static LoadableAsset<Sprite> Swooper { get; } = new LoadableResourceAsset($"{iconPath}.Swooper.png");
    public static LoadableAsset<Sprite> Venerer { get; } = new LoadableResourceAsset($"{iconPath}.Venerer.png");
    

    public static LoadableAsset<Sprite> Ambusher { get; } = new LoadableResourceAsset($"{iconPath}.Ambusher.png");
    public static LoadableAsset<Sprite> Bomber { get; } = new LoadableResourceAsset($"{iconPath}.Bomber.png");
    public static LoadableAsset<Sprite> Infestor { get; } = new LoadableResourceAsset($"{iconPath}.Infestor.png");
    public static LoadableAsset<Sprite> Scavenger { get; } = new LoadableResourceAsset($"{iconPath}.Scavenger.png");
    public static LoadableAsset<Sprite> Warlock { get; } = new LoadableResourceAsset($"{iconPath}.Warlock.png");
    
    public static LoadableAsset<Sprite> Ambassador { get; } = new LoadableResourceAsset($"{iconPath}.Ambassador.png");
    public static LoadableAsset<Sprite> Spellslinger { get; } = new LoadableResourceAsset($"{iconPath}.Spellslinger.png");
    public static LoadableAsset<Sprite> Traitor { get; } = new LoadableResourceAsset($"{iconPath}.Traitor.png");

    public static LoadableAsset<Sprite> Blackmailer { get; } = new LoadableResourceAsset($"{iconPath}.Blackmailer.png");
    public static LoadableAsset<Sprite> Bootlegger { get; } = new LoadableResourceAsset($"{iconPath}.Bootlegger.png");
    public static LoadableAsset<Sprite> Herbalist { get; } = new LoadableResourceAsset($"{iconPath}.Herbalist.png");
    public static LoadableAsset<Sprite> Hypnotist { get; } = new LoadableResourceAsset($"{iconPath}.Hypnotist.png");
    public static LoadableAsset<Sprite> Janitor { get; } = new LoadableResourceAsset($"{iconPath}.Janitor.png");
    public static LoadableAsset<Sprite> Miner { get; } = new LoadableResourceAsset($"{iconPath}.Miner.png");
    public static LoadableAsset<Sprite> Undertaker { get; } = new LoadableResourceAsset($"{iconPath}.Undertaker.png");

    public static LoadableAsset<Sprite> RandomAny { get; } = new LoadableResourceAsset($"{iconPath}.RandomAny.png");
    public static LoadableAsset<Sprite> RandomCrew { get; } = new LoadableResourceAsset($"{iconPath}.RandomCrew.png");
    public static LoadableAsset<Sprite> RandomNeut { get; } = new LoadableResourceAsset($"{iconPath}.RandomNeut.png");
    public static LoadableAsset<Sprite> RandomImp { get; } = new LoadableResourceAsset($"{iconPath}.RandomImp.png");
}