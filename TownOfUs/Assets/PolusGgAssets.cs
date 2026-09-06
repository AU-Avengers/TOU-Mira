using UnityEngine;

namespace TownOfUs.Assets;

public static class PolusGgAssets
{
    internal const string ShortPath = "TownOfUs.Resources.PolusGg";
    private const string IconPath = "TownOfUs.Resources.PolusGg.Icons";
    private const string ButtonPath = "TownOfUs.Resources.PolusGg.Buttons";

    public static LoadableAsset<Sprite> IconCrewmate { get; } =
        new LoadableResourceAsset($"{IconPath}.Crewmate.png", 64);

    public static LoadableAsset<Sprite> IconCrewmateAlign { get; } =
        new LoadableResourceAsset($"{IconPath}.CrewmateAlign.png", 64);

    public static LoadableAsset<Sprite> IconEngineer { get; } =
        new LoadableResourceAsset($"{IconPath}.Engineer.png", 64);

    public static LoadableAsset<Sprite> IconGrenadier { get; } =
        new LoadableResourceAsset($"{IconPath}.Grenadier.png", 64);

    public static LoadableAsset<Sprite> IconImpervious { get; } =
        new LoadableResourceAsset($"{IconPath}.Impervious.png", 64);

    public static LoadableAsset<Sprite> IconImpostor { get; } =
        new LoadableResourceAsset($"{IconPath}.Impostor.png", 64);

    public static LoadableAsset<Sprite> IconImpostorAlign { get; } =
        new LoadableResourceAsset($"{IconPath}.ImpostorAlign.png", 64);

    public static LoadableAsset<Sprite> IconJester { get; } =
        new LoadableResourceAsset($"{IconPath}.Jester.png", 64);

    public static LoadableAsset<Sprite> IconLocksmith { get; } =
        new LoadableResourceAsset($"{IconPath}.Locksmith.png", 64);

    public static LoadableAsset<Sprite> IconNeutralAlign { get; } =
        new LoadableResourceAsset($"{IconPath}.NeutralAlign.png", 64);

    public static LoadableAsset<Sprite> IconOracle { get; } =
        new LoadableResourceAsset($"{IconPath}.Oracle.png", 64);

    public static LoadableAsset<Sprite> IconPhantom { get; } =
        new LoadableResourceAsset($"{IconPath}.Phantom.png", 64);

    public static LoadableAsset<Sprite> IconPoisoner { get; } =
        new LoadableResourceAsset($"{IconPath}.Poisoner.png", 64);

    public static LoadableAsset<Sprite> IconSerialKiller { get; } =
        new LoadableResourceAsset($"{IconPath}.SerialKiller.png", 64);

    public static LoadableAsset<Sprite> IconSheriff { get; } =
        new LoadableResourceAsset($"{IconPath}.Sheriff.png", 64);

    public static LoadableAsset<Sprite> IconSnitch { get; } =
        new LoadableResourceAsset($"{IconPath}.Snitch.png", 64);

    public static LoadableAsset<Sprite> ButtonClose { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Close.png", 80);

    public static LoadableAsset<Sprite> ButtonColorShift { get; } =
        new LoadableResourceAsset($"{ButtonPath}.ColorShift.png", 80);

    public static LoadableAsset<Sprite> ButtonEnchant { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Enchant.png", 80);

    public static LoadableAsset<Sprite> ButtonFix { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Fix.png", 80);

    public static LoadableAsset<Sprite> ButtonLight { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Light.png", 80);

    public static LoadableAsset<Sprite> ButtonMonitor { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Monitor.png", 80);

    public static LoadableAsset<Sprite> ButtonMorph { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Morph.png", 80);

    public static LoadableAsset<Sprite> ButtonNone { get; } =
        new LoadableResourceAsset($"{ButtonPath}.None.png", 80);

    public static LoadableAsset<Sprite> ButtonOpen { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Open.png", 80);

    public static LoadableAsset<Sprite> ButtonPoison { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Poison.png", 80);

    public static LoadableAsset<Sprite> ButtonPredict { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Predict.png", 80);

    public static LoadableAsset<Sprite> ButtonSample { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Sample.png", 80);

    public static LoadableAsset<Sprite> ButtonShoot { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Shoot.png", 80);

    public static LoadableAsset<Sprite> ButtonSteal { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Steal.png", 80);

    public static LoadableAsset<Sprite> ButtonSwoop { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Swoop.png", 80);

    public static LoadableAsset<Sprite> ButtonTeach { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Teach.png", 80);

    public static LoadableAsset<Sprite> ButtonThrow { get; } =
        new LoadableResourceAsset($"{ButtonPath}.Throw.png", 80);

    public static void Initialize()
    {
        AuAvengersAnims.Initialize();
    }
}
